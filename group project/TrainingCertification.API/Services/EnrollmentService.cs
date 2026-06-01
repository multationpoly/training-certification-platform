using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TrainingCertification.API.Data;
using TrainingCertification.API.Hubs;
using TrainingCertification.API.Models;

namespace TrainingCertification.API.Services;

public interface IEnrollmentService
{
    Task<(bool Success, string Message, Enrollment? Enrollment)> EnrollAsync(string traineeId, int sessionId);
    Task<(bool Success, string Message)> DropAsync(int enrollmentId);
    Task<(bool Success, string Message)> ChangeStatusAsync(int enrollmentId, EnrollmentStatus newStatus, string? notes);
    Task<int> RemainingSeatsAsync(int sessionId);
}

public class EnrollmentService : IEnrollmentService
{
    private static readonly Dictionary<EnrollmentStatus, EnrollmentStatus[]> ValidTransitions = new()
    {
        [EnrollmentStatus.Enrolled] = new[] { EnrollmentStatus.Confirmed, EnrollmentStatus.Dropped },
        [EnrollmentStatus.Confirmed] = new[] { EnrollmentStatus.Attending, EnrollmentStatus.Dropped },
        [EnrollmentStatus.Attending] = new[] { EnrollmentStatus.Completed, EnrollmentStatus.Dropped }
    };

    private readonly ApplicationDbContext _db;
    private readonly IHubContext<EnrollmentHub> _hub;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(ApplicationDbContext db, IHubContext<EnrollmentHub> hub, ILogger<EnrollmentService> logger)
    {
        _db = db;
        _hub = hub;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, Enrollment? Enrollment)> EnrollAsync(string traineeId, int sessionId)
    {
        var session = await _db.CourseSessions
            .Include(s => s.Course)
            .Include(s => s.Enrollments)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null) return (false, "Session was not found.", null);
        if (session.Status != CourseSessionStatus.Scheduled) return (false, "Cannot enroll unless the session is scheduled.", null);

        var traineeExists = await _db.Users.AnyAsync(u => u.Id == traineeId);
        if (!traineeExists) return (false, "Trainee was not found.", null);

        if (await _db.Enrollments.AnyAsync(e => e.TraineeId == traineeId && e.CourseSessionId == sessionId && e.Status != EnrollmentStatus.Dropped))
            return (false, "Trainee is already enrolled in this session.", null);

        if (session.Enrollments.Count(e => e.Status != EnrollmentStatus.Dropped) >= session.Capacity)
            return (false, "No seats are available for this session.", null);

        var prerequisiteIds = await _db.CoursePrerequisites
            .Where(p => p.CourseId == session.CourseId)
            .Select(p => p.PrerequisiteCourseId)
            .ToListAsync();

        if (prerequisiteIds.Any())
        {
            var passedCourses = await _db.Enrollments
                .Where(e => e.TraineeId == traineeId
                    && e.Status == EnrollmentStatus.Completed
                    && e.AssessmentResult != null
                    && e.AssessmentResult.Result == AssessmentStatus.Pass)
                .Select(e => e.CourseSession!.CourseId)
                .Distinct()
                .ToListAsync();

            if (prerequisiteIds.Except(passedCourses).Any())
                return (false, "Prerequisite course must be completed with a Pass result before enrolling.", null);
        }

        var enrollment = new Enrollment { TraineeId = traineeId, CourseSessionId = sessionId, Status = EnrollmentStatus.Enrolled };
        _db.Enrollments.Add(enrollment);
        _db.Notifications.Add(new Notification { UserId = traineeId, Title = "Enrollment received", Message = $"Your enrollment for {session.Course!.Title} has been created.", Type = NotificationType.EnrollmentConfirmation });
        var instructorUserId = await _db.CourseSessions.Where(s => s.Id == sessionId).Select(s => s.InstructorProfile!.ApplicationUserId).FirstOrDefaultAsync();
        if (!string.IsNullOrWhiteSpace(instructorUserId))
        {
            _db.Notifications.Add(new Notification { UserId = instructorUserId, Title = "New trainee enrollment", Message = $"A trainee enrolled in {session.Course!.Title}.", Type = NotificationType.EnrollmentConfirmation });
        }

        var trackIds = await _db.CertificationTrackCourses
            .Where(x => x.CourseId == session.CourseId)
            .Select(x => x.CertificationTrackId)
            .Distinct()
            .ToListAsync();

        foreach (var trackId in trackIds)
        {
            if (!await _db.TraineeCertifications.AnyAsync(x => x.TraineeId == traineeId && x.CertificationTrackId == trackId))
            {
                _db.TraineeCertifications.Add(new TraineeCertification { TraineeId = traineeId, CertificationTrackId = trackId });
            }
        }

        await _db.SaveChangesAsync();
        await PublishSeatsAsync(sessionId);
        return (true, "Enrollment created successfully.", enrollment);
    }

    public async Task<(bool Success, string Message)> DropAsync(int enrollmentId)
    {
        return await ChangeStatusAsync(enrollmentId, EnrollmentStatus.Dropped, "Dropped by user/coordinator");
    }

    public async Task<(bool Success, string Message)> ChangeStatusAsync(int enrollmentId, EnrollmentStatus newStatus, string? notes)
    {
        var enrollment = await _db.Enrollments.Include(x => x.CourseSession).ThenInclude(x => x!.Course).FirstOrDefaultAsync(x => x.Id == enrollmentId);
        if (enrollment == null) return (false, "Enrollment was not found.");

        if (enrollment.Status == newStatus) return (true, "Enrollment status is already up to date.");
        if (!ValidTransitions.TryGetValue(enrollment.Status, out var allowed) || !allowed.Contains(newStatus))
            return (false, $"Invalid enrollment status transition from {enrollment.Status} to {newStatus}.");

        var old = enrollment.Status;
        enrollment.Status = newStatus;
        _db.EnrollmentStatusHistories.Add(new EnrollmentStatusHistory { EnrollmentId = enrollment.Id, OldStatus = old, NewStatus = newStatus, Notes = notes ?? string.Empty });
        _db.Notifications.Add(new Notification { UserId = enrollment.TraineeId, Title = "Enrollment status changed", Message = $"Your enrollment for {enrollment.CourseSession!.Course!.Title} is now {newStatus}.", Type = NotificationType.EnrollmentStatusChanged });

        await _db.SaveChangesAsync();
        await PublishSeatsAsync(enrollment.CourseSessionId);
        return (true, "Enrollment status updated.");
    }

    public async Task<int> RemainingSeatsAsync(int sessionId)
    {
        var session = await _db.CourseSessions.Include(s => s.Enrollments).FirstAsync(s => s.Id == sessionId);
        return session.Capacity - session.Enrollments.Count(e => e.Status != EnrollmentStatus.Dropped);
    }

    private async Task PublishSeatsAsync(int sessionId)
    {
        try
        {
            var remaining = await RemainingSeatsAsync(sessionId);
            await EnrollmentHub.UpdateEnrollmentCount(_hub, sessionId, remaining);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to publish seat update for session {SessionId}", sessionId);
        }
    }
}
