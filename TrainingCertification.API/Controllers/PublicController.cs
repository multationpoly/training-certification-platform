using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCertification.API.Data;
using TrainingCertification.API.DTOs;
using TrainingCertification.API.Models;

namespace TrainingCertification.API.Controllers;

[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public PublicController(ApplicationDbContext db) => _db = db;

    /// <summary>Looks up a trainee certification by trainee identifier and certificate reference.</summary>
    [HttpGet("certification")]
    public async Task<ActionResult<ApiResponse<PublicCertificationDto>>> Certification([FromQuery] string traineeId, [FromQuery] string certificateRef)
    {
        var certification = await _db.TraineeCertifications
            .Include(x => x.Trainee)
            .Include(x => x.CertificationTrack)
            .FirstOrDefaultAsync(x =>
                (x.TraineeId == traineeId || (x.Trainee != null && x.Trainee.TraineeNumber == traineeId))
                && x.CertificateReferenceNumber == certificateRef);

        if (certification == null)
        {
            return NotFound(new { message = "Certification was not found or the reference does not match the trainee." });
        }

        var completedCourses = await _db.Enrollments
            .Include(e => e.CourseSession)!.ThenInclude(s => s!.Course)
            .Include(e => e.AssessmentResult)
            .Where(e => e.TraineeId == certification.TraineeId && e.Status == EnrollmentStatus.Completed && e.AssessmentResult != null && e.CourseSession != null && e.CourseSession.Course != null)
            .Select(e => new CompletedCourseDto(e.CourseSession!.Course!.Title, e.AssessmentResult!.Result.ToString(), e.AssessmentResult.RecordedAt))
            .ToListAsync();

        var data = new PublicCertificationDto(
            certification.Trainee!.FullName,
            certification.CertificationTrack!.Name,
            certification.Status.ToString(),
            certification.IssuedAt,
            completedCourses);

        return Ok(new ApiResponse<PublicCertificationDto>(true, data, "Certification loaded."));
    }
}
