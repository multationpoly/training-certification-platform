using System.Security.Claims;
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
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IEnrollmentService _service;
    private readonly ICertificationService _certificationService;

    public EnrollmentsController(ApplicationDbContext db, IEnrollmentService service, ICertificationService certificationService)
    {
        _db = db;
        _service = service;
        _certificationService = certificationService;
    }

    /// <summary>Creates an enrollment after prerequisite, capacity, and duplicate checks.</summary>
    [HttpPost]
    public async Task<IActionResult> Enroll(CreateEnrollmentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (User.IsInRole(Roles.Trainee) && dto.TraineeId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return Forbid();
        }

        var result = await _service.EnrollAsync(dto.TraineeId, dto.CourseSessionId);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return CreatedAtAction(nameof(MyEnrollments), new { id = result.Enrollment!.Id }, new { message = result.Message, enrollmentId = result.Enrollment.Id });
    }

    /// <summary>Returns the authenticated trainee's enrollments.</summary>
    [HttpGet("my")]
    [Authorize(Roles = Roles.Trainee)]
    public async Task<ActionResult<ApiResponse<IEnumerable<EnrollmentListDto>>>> MyEnrollments()
    {
        var traineeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var enrollments = await _db.Enrollments
            .Include(e => e.CourseSession)!.ThenInclude(s => s!.Course)
            .Where(e => e.TraineeId == traineeId)
            .Select(e => new EnrollmentListDto(e.Id, e.CourseSessionId, e.CourseSession!.Course!.Title, e.CourseSession.StartDateTime, e.Status.ToString()))
            .ToListAsync();
        return Ok(new ApiResponse<IEnumerable<EnrollmentListDto>>(true, enrollments, "Enrollments loaded."));
    }

    /// <summary>Returns enrolled trainees for one session.</summary>
    [HttpGet("session/{sessionId:int}")]
    [Authorize(Roles = Roles.TrainingCoordinator + "," + Roles.Instructor)]
    public async Task<ActionResult<ApiResponse<IEnumerable<SessionEnrollmentDto>>>> SessionEnrollments(int sessionId)
    {
        var enrollments = await _db.Enrollments
            .Include(e => e.Trainee)
            .Where(e => e.CourseSessionId == sessionId && e.Status != EnrollmentStatus.Dropped)
            .Select(e => new SessionEnrollmentDto(e.Id, e.TraineeId, e.Trainee!.FullName, e.Status.ToString()))
            .ToListAsync();
        return Ok(new ApiResponse<IEnumerable<SessionEnrollmentDto>>(true, enrollments, "Session enrollments loaded."));
    }

    /// <summary>Updates enrollment status through the allowed lifecycle transitions.</summary>
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = Roles.TrainingCoordinator + "," + Roles.Instructor)]
    public async Task<IActionResult> ChangeStatus(int id, UpdateEnrollmentStatusDto dto)
    {
        if (!Enum.TryParse<EnrollmentStatus>(dto.Status, true, out var newStatus)) return BadRequest(new { message = "Enrollment status is invalid." });
        if (!await CanManageEnrollment(id)) return Forbid();
        var result = await _service.ChangeStatusAsync(id, newStatus, dto.Notes);
        return result.Success ? Ok(new { message = result.Message }) : BadRequest(new { message = result.Message });
    }

    /// <summary>Drops an enrollment when the current state allows it.</summary>
    [HttpPost("{id:int}/drop")]
    public async Task<IActionResult> Drop(int id)
    {
        var enrollment = await _db.Enrollments.Include(e => e.CourseSession)!.ThenInclude(s => s!.InstructorProfile).FirstOrDefaultAsync(e => e.Id == id);
        if (enrollment == null) return NotFound(new { message = "Enrollment was not found." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (User.IsInRole(Roles.Trainee))
        {
            if (enrollment.TraineeId != userId) return Forbid();
            if (enrollment.Status != EnrollmentStatus.Enrolled && enrollment.Status != EnrollmentStatus.Confirmed)
                return BadRequest(new { message = "Only enrolled or confirmed enrollments can be dropped." });
            if (enrollment.CourseSession!.StartDateTime <= DateTime.Now.AddHours(24))
                return BadRequest(new { message = "Enrollments can only be dropped at least 24 hours before the session starts." });
        }
        else if (!await CanManageEnrollment(id))
        {
            return Forbid();
        }

        var result = await _service.DropAsync(id);
        return result.Success ? Ok(new { message = result.Message }) : BadRequest(new { message = result.Message });
    }

    /// <summary>Records an assessment result and updates certification eligibility.</summary>
    [HttpPost("assessment-results")]
    [Authorize(Roles = Roles.TrainingCoordinator + "," + Roles.Instructor)]
    public async Task<IActionResult> RecordAssessment(RecordAssessmentDto dto)
    {
        if (!Enum.TryParse<AssessmentStatus>(dto.Result, true, out var result) || result == AssessmentStatus.Pending)
            return BadRequest(new { message = "Assessment result must be Pass or Fail." });

        if (!await CanManageEnrollment(dto.EnrollmentId)) return Forbid();

        var enrollment = await _db.Enrollments.Include(e => e.CourseSession)!.ThenInclude(s => s!.Course).FirstOrDefaultAsync(e => e.Id == dto.EnrollmentId);
        if (enrollment == null) return NotFound(new { message = "Enrollment was not found." });
        if (enrollment.Status != EnrollmentStatus.Completed)
            return BadRequest(new { message = "Assessments can only be recorded for completed enrollments." });
        if (enrollment.CourseSession!.EndDateTime > DateTime.Now && enrollment.CourseSession.Status != CourseSessionStatus.Completed)
            return BadRequest(new { message = "Assessments can only be recorded after the session ends." });

        var assessment = await _db.AssessmentResults.FirstOrDefaultAsync(x => x.EnrollmentId == dto.EnrollmentId);
        if (assessment == null)
        {
            assessment = new AssessmentResult { EnrollmentId = dto.EnrollmentId };
            _db.AssessmentResults.Add(assessment);
        }

        assessment.Result = result;
        assessment.Score = dto.Score;
        assessment.Remarks = dto.InstructorNotes ?? string.Empty;
        assessment.RecordedAt = DateTime.UtcNow;
        _db.Notifications.Add(new Notification { UserId = enrollment.TraineeId, Title = "Assessment recorded", Message = $"Your assessment for {enrollment.CourseSession!.Course!.Title} was recorded as {result}.", Type = NotificationType.AssessmentRecorded });
        await _db.SaveChangesAsync();

        if (result == AssessmentStatus.Pass)
        {
            await _certificationService.CheckEligibilityAsync(enrollment.TraineeId);
        }

        return Ok(new { message = "Assessment result recorded." });
    }

    [HttpGet("sessions/{sessionId:int}/remaining-seats")]
    [AllowAnonymous]
    public async Task<IActionResult> RemainingSeats(int sessionId) => Ok(new { sessionId, remainingSeats = await _service.RemainingSeatsAsync(sessionId) });

    private async Task<bool> CanManageEnrollment(int enrollmentId)
    {
        if (User.IsInRole(Roles.TrainingCoordinator)) return true;
        if (!User.IsInRole(Roles.Instructor)) return false;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await _db.Enrollments.AnyAsync(e => e.Id == enrollmentId && e.CourseSession!.InstructorProfile!.ApplicationUserId == userId);
    }
}
