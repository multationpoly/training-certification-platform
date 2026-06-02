using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCertification.API.Data;
using TrainingCertification.API.DTOs;
using TrainingCertification.API.Models;
using TrainingCertification.API.Services;

namespace TrainingCertification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ISchedulingService _scheduling;
    public SessionsController(ApplicationDbContext db, ISchedulingService scheduling) { _db = db; _scheduling = scheduling; }

    /// <summary>Returns all scheduled sessions.</summary>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<SessionDto>>>> GetSessions()
    {
        var data = await _db.CourseSessions.Include(s => s.Course).Include(s => s.Classroom).Include(s => s.InstructorProfile)!.ThenInclude(i => i!.ApplicationUser).Include(s => s.Enrollments)
            .Where(s => s.Course != null && s.InstructorProfile != null && s.InstructorProfile.ApplicationUser != null && s.Classroom != null)
            .Select(s => new SessionDto(s.Id, s.CourseId, s.Course!.Title, s.InstructorProfile!.ApplicationUser!.FullName, s.Classroom!.RoomName, s.StartDateTime, s.EndDateTime, s.Capacity, s.Enrollments.Count(e => e.Status != EnrollmentStatus.Dropped), s.Capacity - s.Enrollments.Count(e => e.Status != EnrollmentStatus.Dropped)))
            .ToListAsync();
        return Ok(new ApiResponse<IEnumerable<SessionDto>>(true, data, "Sessions loaded."));
    }

    /// <summary>Creates a new course session after instructor and room conflict checks.</summary>
    [Authorize(Roles = Roles.TrainingCoordinator)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateSessionDto dto)
    {
        var session = new CourseSession
        {
            CourseId = dto.CourseId,
            InstructorProfileId = dto.InstructorProfileId,
            ClassroomId = dto.ClassroomId,
            StartDateTime = dto.StartDateTime,
            EndDateTime = dto.EndDateTime,
            Capacity = dto.Capacity
        };

        var validation = await _scheduling.ValidateSessionAsync(session);
        if (!validation.Success) return BadRequest(new { message = validation.Message });

        _db.CourseSessions.Add(session);
        var instructor = await _db.InstructorProfiles.FirstOrDefaultAsync(x => x.Id == dto.InstructorProfileId);
        if (instructor != null)
        {
            _db.Notifications.Add(new Notification { UserId = instructor.ApplicationUserId, Title = "New session scheduled", Message = "A new course session has been assigned to you.", Type = NotificationType.NewSessionScheduled });
        }

        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetSessions), new { id = session.Id }, new { message = "Session created.", sessionId = session.Id });
    }

    /// <summary>Cancels a session and notifies enrolled trainees and the instructor.</summary>
    [Authorize(Roles = Roles.TrainingCoordinator)]
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var session = await _db.CourseSessions.Include(s => s.Enrollments).Include(s => s.Course).Include(s => s.InstructorProfile).FirstOrDefaultAsync(s => s.Id == id);
        if (session == null) return NotFound(new { message = "Session was not found." });

        session.Status = CourseSessionStatus.Cancelled;
        foreach (var enrollment in session.Enrollments.Where(e => e.Status != EnrollmentStatus.Dropped))
        {
            _db.Notifications.Add(new Notification { UserId = enrollment.TraineeId, Title = "Session cancelled", Message = $"{session.Course!.Title} session has been cancelled.", Type = NotificationType.ScheduleChange });
        }

        _db.Notifications.Add(new Notification { UserId = session.InstructorProfile!.ApplicationUserId, Title = "Session cancelled", Message = $"{session.Course!.Title} session has been cancelled.", Type = NotificationType.ScheduleChange });
        await _db.SaveChangesAsync();
        return Ok(new { message = "Session cancelled." });
    }
}
