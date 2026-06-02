using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrainingCertification.API.Data;
using TrainingCertification.API.Models;
using TrainingCertification.API.Services;
using TrainingCertification.MVC.ViewModels;

namespace TrainingCertification.MVC.Controllers;

[Authorize]
public class SessionsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ISchedulingService _scheduling;
    public SessionsController(ApplicationDbContext db, ISchedulingService scheduling) { _db = db; _scheduling = scheduling; }
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = _db.CourseSessions.Include(s => s.Course).Include(s => s.Classroom).Include(s => s.InstructorProfile)!.ThenInclude(i => i!.ApplicationUser).Include(s => s.Enrollments).AsQueryable();
        if (User.IsInRole(Roles.Instructor)) query = query.Where(s => s.InstructorProfile!.ApplicationUserId == userId);
        if (User.IsInRole(Roles.Trainee)) query = query.Where(s => s.Enrollments.Any(e => e.TraineeId == userId));
        return View(await query.OrderBy(s => s.StartDateTime).ToListAsync());
    }

    [Authorize(Roles = Roles.TrainingCoordinator)]
    public async Task<IActionResult> Create() => View(await BuildForm(new CourseSession { StartDateTime = DateTime.Today.AddDays(7).AddHours(9), EndDateTime = DateTime.Today.AddDays(7).AddHours(13) }));

    [HttpPost, Authorize(Roles = Roles.TrainingCoordinator), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseSession session)
    {
        var check = await _scheduling.ValidateSessionAsync(session);
        if (!check.Success) ModelState.AddModelError("", check.Message);
        if (!ModelState.IsValid) return View(await BuildForm(session));
        _db.CourseSessions.Add(session);
        var instructor = await _db.InstructorProfiles.FindAsync(session.InstructorProfileId);
        if (instructor != null)
        {
            _db.Notifications.Add(new Notification { UserId = instructor.ApplicationUserId, Title = "New session scheduled", Message = "A new course session has been assigned to you.", Type = NotificationType.NewSessionScheduled });
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Session scheduled successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.TrainingCoordinator)]
    public async Task<IActionResult> Edit(int id)
    {
        var session = await _db.CourseSessions.FindAsync(id);
        return session == null ? NotFound() : View("Create", await BuildForm(session));
    }

    [HttpPost, Authorize(Roles = Roles.TrainingCoordinator), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CourseSession form)
    {
        var session = await _db.CourseSessions.Include(s => s.Enrollments).FirstOrDefaultAsync(s => s.Id == id);
        if (session == null) return NotFound();

        session.CourseId = form.CourseId;
        session.InstructorProfileId = form.InstructorProfileId;
        session.ClassroomId = form.ClassroomId;
        session.StartDateTime = form.StartDateTime;
        session.EndDateTime = form.EndDateTime;
        session.Capacity = form.Capacity;
        session.Status = form.Status;

        var check = await _scheduling.ValidateSessionAsync(session);
        if (!check.Success) ModelState.AddModelError("", check.Message);
        if (session.Enrollments.Count(e => e.Status != EnrollmentStatus.Dropped) > session.Capacity)
        {
            ModelState.AddModelError("", "Capacity cannot be lower than current active enrollments.");
        }
        if (!ModelState.IsValid) return View("Create", await BuildForm(session));

        foreach (var enrollment in session.Enrollments.Where(e => e.Status != EnrollmentStatus.Dropped))
        {
            _db.Notifications.Add(new Notification { UserId = enrollment.TraineeId, Title = "Session updated", Message = "One of your scheduled training sessions has changed.", Type = NotificationType.ScheduleChange });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Session updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Roles = Roles.TrainingCoordinator), ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var session = await _db.CourseSessions.Include(s => s.Course).Include(s => s.InstructorProfile).Include(s => s.Enrollments).FirstOrDefaultAsync(s => s.Id == id);
        if (session == null) return NotFound();
        session.Status = CourseSessionStatus.Cancelled;
        foreach (var enrollment in session.Enrollments.Where(e => e.Status != EnrollmentStatus.Dropped))
        {
            _db.Notifications.Add(new Notification { UserId = enrollment.TraineeId, Title = "Session cancelled", Message = $"{session.Course?.Title} has been cancelled.", Type = NotificationType.ScheduleChange });
        }
        if (session.InstructorProfile != null)
        {
            _db.Notifications.Add(new Notification { UserId = session.InstructorProfile.ApplicationUserId, Title = "Session cancelled", Message = $"{session.Course?.Title} has been cancelled.", Type = NotificationType.ScheduleChange });
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Session cancelled.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<SessionFormViewModel> BuildForm(CourseSession session) => new()
    {
        Session = session,
        Courses = await _db.Courses.Where(c => c.IsActive).OrderBy(c => c.Title).ToListAsync(),
        Instructors = await _db.InstructorProfiles.Include(i => i.ApplicationUser).Include(i => i.Expertises).ThenInclude(e => e.CourseCategory).Include(i => i.Availability).OrderBy(i => i.FullName).ToListAsync(),
        Classrooms = await _db.Classrooms.Include(c => c.Equipment).OrderBy(c => c.RoomName).ToListAsync()
    };
}

[Authorize]
public class EnrollmentsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IEnrollmentService _service;
    private readonly ICertificationService _certificationService;
    public EnrollmentsController(ApplicationDbContext db, IEnrollmentService service, ICertificationService certificationService) { _db = db; _service = service; _certificationService = certificationService; }
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = _db.Enrollments.Include(e => e.Trainee).Include(e => e.AssessmentResult).Include(e => e.CourseSession)!.ThenInclude(s => s!.Course).Include(e => e.CourseSession)!.ThenInclude(s => s!.InstructorProfile).AsQueryable();
        if (User.IsInRole(Roles.Trainee)) query = query.Where(e => e.TraineeId == userId);
        if (User.IsInRole(Roles.Instructor)) query = query.Where(e => e.CourseSession!.InstructorProfile!.ApplicationUserId == userId);
        return View(await query.OrderByDescending(e => e.EnrolledAt).ToListAsync());
    }
    [HttpPost, Authorize(Roles = Roles.Trainee), ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int sessionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.EnrollAsync(userId, sessionId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction("Index", "Sessions");
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Drop(int id)
    {
        if (User.IsInRole(Roles.Trainee))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var enrollment = await _db.Enrollments.Include(e => e.CourseSession).FirstOrDefaultAsync(e => e.Id == id);
            if (enrollment == null) return NotFound();
            if (enrollment.TraineeId != userId) return Forbid();
            if (enrollment.Status != EnrollmentStatus.Enrolled && enrollment.Status != EnrollmentStatus.Confirmed)
            {
                TempData["Error"] = "Only enrolled or confirmed enrollments can be dropped.";
                return RedirectToAction(nameof(Index));
            }
            if (enrollment.CourseSession!.StartDateTime <= DateTime.Now.AddHours(24))
            {
                TempData["Error"] = "Enrollments can only be dropped at least 24 hours before the session starts.";
                return RedirectToAction(nameof(Index));
            }
        }
        var result = await _service.DropAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Roles = Roles.TrainingCoordinator + "," + Roles.Instructor), ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, EnrollmentStatus status, string? notes)
    {
        if (!await CanManageEnrollment(id)) return Forbid();
        var result = await _service.ChangeStatusAsync(id, status, notes);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Roles = Roles.TrainingCoordinator + "," + Roles.Instructor), ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordAssessment(int enrollmentId, AssessmentStatus result, decimal score, string? remarks)
    {
        if (result == AssessmentStatus.Pending)
        {
            TempData["Error"] = "Assessment result must be Pass or Fail.";
            return RedirectToAction(nameof(Index));
        }

        if (!await CanManageEnrollment(enrollmentId)) return Forbid();
        var enrollment = await _db.Enrollments.Include(e => e.CourseSession).FirstOrDefaultAsync(e => e.Id == enrollmentId);
        if (enrollment == null) return NotFound();
        if (enrollment.Status != EnrollmentStatus.Completed)
        {
            TempData["Error"] = "Assessments can only be recorded for completed enrollments.";
            return RedirectToAction(nameof(Index));
        }
        if (enrollment.CourseSession!.EndDateTime > DateTime.Now && enrollment.CourseSession.Status != CourseSessionStatus.Completed)
        {
            TempData["Error"] = "Assessments can only be recorded after the session ends.";
            return RedirectToAction(nameof(Index));
        }

        var assessment = await _db.AssessmentResults.FirstOrDefaultAsync(x => x.EnrollmentId == enrollmentId);
        if (assessment == null)
        {
            assessment = new AssessmentResult { EnrollmentId = enrollmentId };
            _db.AssessmentResults.Add(assessment);
        }

        assessment.Result = result;
        assessment.Score = score;
        assessment.Remarks = remarks ?? string.Empty;
        assessment.RecordedAt = DateTime.UtcNow;
        _db.Notifications.Add(new Notification { UserId = enrollment.TraineeId, Title = "Assessment recorded", Message = $"Your assessment result is {result}.", Type = NotificationType.AssessmentRecorded });
        await _db.SaveChangesAsync();

        if (result == AssessmentStatus.Pass)
        {
            await _certificationService.CheckEligibilityAsync(enrollment.TraineeId);
        }

        TempData["Success"] = "Assessment recorded.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CanManageEnrollment(int enrollmentId)
    {
        if (User.IsInRole(Roles.TrainingCoordinator)) return true;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await _db.Enrollments.AnyAsync(e => e.Id == enrollmentId && e.CourseSession!.InstructorProfile!.ApplicationUserId == userId);
    }
}
