using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCertification.API.Data;
using TrainingCertification.API.DTOs;
using TrainingCertification.API.Models;

namespace TrainingCertification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.TrainingCoordinator)]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ReportsController(ApplicationDbContext db) => _db = db;

    /// <summary>Returns enrollment counts grouped by course and category.</summary>
    [HttpGet("enrollment-stats")]
    public async Task<ActionResult<ApiResponse<IEnumerable<EnrollmentReportDto>>>> EnrollmentStats(DateTime? from, DateTime? to)
    {
        var query = _db.Enrollments.Include(e => e.CourseSession)!.ThenInclude(s => s!.Course)!.ThenInclude(c => c!.CourseCategory).AsQueryable();
        if (from.HasValue) query = query.Where(e => e.EnrolledAt >= from.Value);
        if (to.HasValue) query = query.Where(e => e.EnrolledAt <= to.Value);

        var data = await query
            .Where(e => e.CourseSession != null && e.CourseSession.Course != null && e.CourseSession.Course.CourseCategory != null)
            .GroupBy(e => new { e.CourseSession!.Course!.Title, Category = e.CourseSession.Course.CourseCategory!.Name })
            .Select(g => new EnrollmentReportDto(g.Key.Title, g.Key.Category, g.Count()))
            .ToListAsync();
        return Ok(new ApiResponse<IEnumerable<EnrollmentReportDto>>(true, data, "Enrollment stats loaded."));
    }

    /// <summary>Returns sessions, trainees, and completion rate per instructor.</summary>
    [HttpGet("instructor-workload")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkloadReportDto>>>> InstructorWorkload()
    {
        var data = await _db.InstructorProfiles.Include(i => i.ApplicationUser).Include(i => i.Sessions).ThenInclude(s => s.Enrollments)
            .Select(i => new WorkloadReportDto(
                i.ApplicationUser!.FullName,
                i.Sessions.Count,
                i.Sessions.SelectMany(s => s.Enrollments).Count(),
                i.Sessions.SelectMany(s => s.Enrollments).Any() ? (decimal)i.Sessions.SelectMany(s => s.Enrollments).Count(e => e.Status == EnrollmentStatus.Completed) / i.Sessions.SelectMany(s => s.Enrollments).Count() * 100 : 0))
            .ToListAsync();
        return Ok(new ApiResponse<IEnumerable<WorkloadReportDto>>(true, data, "Instructor workload loaded."));
    }

    /// <summary>Returns certification completion counts per track.</summary>
    [HttpGet("certification-completion")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CertificationReportDto>>>> CertificationCompletion()
    {
        var data = await _db.CertificationTracks.Include(t => t.TraineeCertifications)
            .Select(t => new CertificationReportDto(
                t.Name,
                t.TraineeCertifications.Count(c => c.Status == TraineeCertificationStatus.Eligible),
                t.TraineeCertifications.Count(c => c.Status == TraineeCertificationStatus.Issued),
                t.TraineeCertifications.Count(c => c.Status == TraineeCertificationStatus.InProgress)))
            .ToListAsync();
        return Ok(new ApiResponse<IEnumerable<CertificationReportDto>>(true, data, "Certification report loaded."));
    }

    /// <summary>Returns revenue and outstanding balance per course session.</summary>
    [HttpGet("revenue")]
    public async Task<ActionResult<ApiResponse<IEnumerable<RevenueReportDto>>>> Revenue(DateTime? from, DateTime? to)
    {
        var query = _db.CourseSessions.Include(s => s.Course).Include(s => s.Payments).Include(s => s.Enrollments).AsQueryable();
        if (from.HasValue) query = query.Where(s => s.StartDateTime >= from.Value);
        if (to.HasValue) query = query.Where(s => s.StartDateTime <= to.Value);

        var data = await query
            .Where(s => s.Course != null)
            .Select(s => new RevenueReportDto(
                s.Id,
                s.Course!.Title,
                s.Course.EnrollmentFee,
                s.Payments.Sum(p => p.Amount),
                (s.Enrollments.Count(e => e.Status != EnrollmentStatus.Dropped) * s.Course.EnrollmentFee) - s.Payments.Sum(p => p.Amount),
                s.StartDateTime.Date < DateTime.UtcNow.Date && ((s.Enrollments.Count(e => e.Status != EnrollmentStatus.Dropped) * s.Course.EnrollmentFee) - s.Payments.Sum(p => p.Amount)) > 0))
            .ToListAsync();
        return Ok(new ApiResponse<IEnumerable<RevenueReportDto>>(true, data, "Revenue report loaded."));
    }
}
