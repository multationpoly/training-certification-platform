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
public class CertificatesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICertificationService _service;
    public CertificatesController(ApplicationDbContext db, ICertificationService service) { _db = db; _service = service; }

    /// <summary>Verifies a certificate reference for public lookup callers.</summary>
    [HttpGet("verify")]
    public async Task<ActionResult<CertificateInfoDto>> Verify([FromQuery] string traineeId, [FromQuery] string referenceNumber)
    {
        var cert = await _db.Certificates.Include(c => c.Trainee).Include(c => c.CertificationTrack)
            .FirstOrDefaultAsync(c => c.Trainee != null && c.Trainee.TraineeNumber == traineeId && c.ReferenceNumber == referenceNumber);
        if (cert == null) return NotFound(new { message = "Certificate was not found." });
        var completed = await _db.Enrollments.Include(e => e.CourseSession)!.ThenInclude(s => s!.Course)
            .Where(e => e.TraineeId == cert.TraineeId && e.AssessmentResult != null && e.AssessmentResult.Result == AssessmentStatus.Pass && e.CourseSession != null && e.CourseSession.Course != null)
            .Select(e => e.CourseSession!.Course!.Title).Distinct().ToListAsync();
        return Ok(new CertificateInfoDto(cert.Trainee!.FullName, cert.CertificationTrack!.Name, cert.ReferenceNumber, cert.IssueDate, !cert.IsRevoked, completed));
    }

    /// <summary>Generates a certificate when a trainee has passed every required course.</summary>
    [Authorize(Roles = Roles.TrainingCoordinator)]
    [HttpPost("generate")]
    public async Task<IActionResult> Generate(string traineeId, int trackId)
    {
        var cert = await _service.GenerateIfEligibleAsync(traineeId, trackId);
        return cert == null ? BadRequest(new { message = "Trainee is not eligible yet." }) : Ok(cert);
    }
}
