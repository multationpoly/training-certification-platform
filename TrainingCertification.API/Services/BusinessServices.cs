using Microsoft.EntityFrameworkCore;
using TrainingCertification.API.Data;
using TrainingCertification.API.Models;

namespace TrainingCertification.API.Services;

public interface ISchedulingService
{
    Task<(bool Success, string Message)> ValidateSessionAsync(CourseSession session);
}

public class SchedulingService : ISchedulingService
{
    private readonly ApplicationDbContext _db;
    public SchedulingService(ApplicationDbContext db) => _db = db;

    public async Task<(bool Success, string Message)> ValidateSessionAsync(CourseSession session)
    {
        if (session.EndDateTime <= session.StartDateTime) return (false, "End date must be after start date.");
        var room = await _db.Classrooms.Include(r => r.Equipment).FirstOrDefaultAsync(r => r.Id == session.ClassroomId);
        if (room == null || room.Capacity < session.Capacity) return (false, "Classroom capacity is not enough.");

        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == session.CourseId);
        if (course == null) return (false, "Course was not found.");
        if (session.Capacity > course.Capacity) return (false, "Session capacity cannot exceed the course capacity.");

        var hasExpertise = await _db.InstructorExpertises.AnyAsync(e =>
            e.InstructorProfileId == session.InstructorProfileId && e.CourseCategoryId == course.CourseCategoryId);
        if (!hasExpertise) return (false, "Instructor does not have expertise for this course category.");

        var missingEquipment = RequiredEquipment(course)
            .Where(required => !room.Equipment.Any(e => EquipmentMatches(e.Name, required)))
            .ToList();
        if (missingEquipment.Any())
            return (false, $"Selected classroom is missing required equipment: {string.Join(", ", missingEquipment)}.");

        var available = await _db.InstructorAvailabilities.AnyAsync(a => a.InstructorProfileId == session.InstructorProfileId
            && a.DayOfWeek == session.StartDateTime.DayOfWeek
            && a.StartTime <= session.StartDateTime.TimeOfDay
            && a.EndTime >= session.EndDateTime.TimeOfDay);
        if (!available) return (false, "Instructor is not available at this time.");

        var instructorBooked = await _db.CourseSessions.AnyAsync(s => s.Id != session.Id
            && s.Status != CourseSessionStatus.Cancelled
            && s.InstructorProfileId == session.InstructorProfileId
            && session.StartDateTime < s.EndDateTime && session.EndDateTime > s.StartDateTime);
        if (instructorBooked) return (false, "Instructor is already booked.");

        var roomBooked = await _db.CourseSessions.AnyAsync(s => s.Id != session.Id
            && s.Status != CourseSessionStatus.Cancelled
            && s.ClassroomId == session.ClassroomId
            && session.StartDateTime < s.EndDateTime && session.EndDateTime > s.StartDateTime);
        if (roomBooked) return (false, "Classroom is already booked.");

        return (true, "Session is valid.");
    }

    private static IEnumerable<string> RequiredEquipment(Course course)
    {
        if (!string.IsNullOrWhiteSpace(course.RequiredEquipment))
        {
            return course.RequiredEquipment.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        var text = course.Title + " " + course.Description;
        var inferred = new List<string>();
        if (text.Contains("projector", StringComparison.OrdinalIgnoreCase)) inferred.Add("Projector");
        if (text.Contains("lab", StringComparison.OrdinalIgnoreCase)
            || text.Contains("computer", StringComparison.OrdinalIgnoreCase)
            || text.Contains("workstation", StringComparison.OrdinalIgnoreCase))
        {
            inferred.Add("Workstations");
        }

        return inferred;
    }

    private static bool EquipmentMatches(string roomEquipment, string requiredEquipment)
    {
        return roomEquipment.Contains(requiredEquipment, StringComparison.OrdinalIgnoreCase)
            || requiredEquipment.Contains(roomEquipment, StringComparison.OrdinalIgnoreCase)
            || (requiredEquipment.Contains("computer", StringComparison.OrdinalIgnoreCase) && roomEquipment.Contains("workstation", StringComparison.OrdinalIgnoreCase))
            || (requiredEquipment.Contains("workstation", StringComparison.OrdinalIgnoreCase) && roomEquipment.Contains("computer", StringComparison.OrdinalIgnoreCase));
    }
}

public interface ICertificationService
{
    Task<Certificate?> GenerateIfEligibleAsync(string traineeId, int trackId);
    Task CheckEligibilityAsync(string traineeId);
}

public class CertificationService : ICertificationService
{
    private readonly ApplicationDbContext _db;
    public CertificationService(ApplicationDbContext db) => _db = db;

    public async Task<Certificate?> GenerateIfEligibleAsync(string traineeId, int trackId)
    {
        if (await _db.Certificates.AnyAsync(c => c.TraineeId == traineeId && c.CertificationTrackId == trackId && !c.IsRevoked))
            return await _db.Certificates.FirstAsync(c => c.TraineeId == traineeId && c.CertificationTrackId == trackId && !c.IsRevoked);

        var requiredCourseIds = await _db.CertificationTrackCourses.Where(x => x.CertificationTrackId == trackId).Select(x => x.CourseId).ToListAsync();
        var passedCourseIds = await _db.Enrollments
            .Where(e => e.TraineeId == traineeId && e.AssessmentResult != null && e.AssessmentResult.Result == AssessmentStatus.Pass)
            .Select(e => e.CourseSession!.CourseId).Distinct().ToListAsync();

        if (!requiredCourseIds.Any() || requiredCourseIds.Except(passedCourseIds).Any()) return null;

        var certificate = new Certificate
        {
            TraineeId = traineeId,
            CertificationTrackId = trackId,
            ReferenceNumber = $"CERT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}"
        };
        _db.Certificates.Add(certificate); // Generate certificate after all required courses are completed
        var traineeCertification = await _db.TraineeCertifications.FirstOrDefaultAsync(x => x.TraineeId == traineeId && x.CertificationTrackId == trackId);
        if (traineeCertification == null)
        {
            traineeCertification = new TraineeCertification { TraineeId = traineeId, CertificationTrackId = trackId };
            _db.TraineeCertifications.Add(traineeCertification);
        }

        traineeCertification.Status = TraineeCertificationStatus.Issued;
        traineeCertification.IssuedAt = certificate.IssueDate;
        traineeCertification.CertificateReferenceNumber = certificate.ReferenceNumber;
        _db.Notifications.Add(new Notification { UserId = traineeId, Title = "Certificate generated", Message = "Congratulations, your certificate is ready.", Type = NotificationType.CertificationCompletion });
        await _db.SaveChangesAsync();
        return certificate;
    }

    public async Task CheckEligibilityAsync(string traineeId)
    {
        var pursued = await _db.TraineeCertifications
            .Where(x => x.TraineeId == traineeId && x.Status == TraineeCertificationStatus.InProgress)
            .ToListAsync();

        var passedCourseIds = await _db.Enrollments
            .Where(e => e.TraineeId == traineeId
                && e.Status == EnrollmentStatus.Completed
                && e.AssessmentResult != null
                && e.AssessmentResult.Result == AssessmentStatus.Pass)
            .Select(e => e.CourseSession!.CourseId)
            .Distinct()
            .ToListAsync();

        foreach (var traineeCertification in pursued)
        {
            var requiredCourseIds = await _db.CertificationTrackCourses
                .Where(x => x.CertificationTrackId == traineeCertification.CertificationTrackId)
                .Select(x => x.CourseId)
                .ToListAsync();

            if (requiredCourseIds.Any() && !requiredCourseIds.Except(passedCourseIds).Any())
            {
                traineeCertification.Status = TraineeCertificationStatus.Eligible;
                _db.Notifications.Add(new Notification { UserId = traineeId, Title = "Certification eligibility reached", Message = "You are now eligible for a certification track.", Type = NotificationType.CertificationEligible });
            }
        }

        if (_db.ChangeTracker.HasChanges())
        {
            await _db.SaveChangesAsync();
        }
    }
}

public interface IPaymentReminderService
{
    Task<int> CreateOverduePaymentRemindersAsync(CancellationToken cancellationToken = default);
}

public class PaymentReminderService : IPaymentReminderService
{
    private readonly ApplicationDbContext _db;

    public PaymentReminderService(ApplicationDbContext db) => _db = db;

    public async Task<int> CreateOverduePaymentRemindersAsync(CancellationToken cancellationToken = default)
    {
        var overdue = await _db.Enrollments
            .Include(e => e.Payments)
            .Include(e => e.CourseSession)!.ThenInclude(s => s!.Course)
            .Where(e => e.Status != EnrollmentStatus.Dropped
                && e.CourseSession != null
                && e.CourseSession.StartDateTime.Date < DateTime.Today
                && e.CourseSession.Course != null)
            .ToListAsync(cancellationToken);

        var created = 0;
        foreach (var enrollment in overdue)
        {
            var fee = enrollment.CourseSession?.Course?.EnrollmentFee ?? 0;
            var paid = enrollment.Payments.Sum(p => p.Amount);
            if (paid >= fee) continue;

            var title = $"Payment overdue for {enrollment.CourseSession?.Course?.Title}";
            var hasUnreadReminder = await _db.Notifications.AnyAsync(n =>
                n.UserId == enrollment.TraineeId && n.Title == title && !n.IsRead, cancellationToken);
            if (hasUnreadReminder) continue;

            _db.Notifications.Add(new Notification
            {
                UserId = enrollment.TraineeId,
                Title = title,
                Message = $"Outstanding balance: {fee - paid:0.00}.",
                Type = NotificationType.PaymentReminder
            });
            created++;
        }

        if (created > 0) await _db.SaveChangesAsync(cancellationToken);
        return created;
    }
}

public class PaymentReminderHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentReminderHostedService> _logger;

    public PaymentReminderHostedService(IServiceScopeFactory scopeFactory, ILogger<PaymentReminderHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IPaymentReminderService>();
                await service.CreateOverduePaymentRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to create overdue payment reminders.");
            }

            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
}
