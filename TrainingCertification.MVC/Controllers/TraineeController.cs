using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrainingCertification.API.Data;
using TrainingCertification.API.Models;
using TrainingCertification.MVC.ViewModels;

namespace TrainingCertification.MVC.Controllers;

[Authorize(Roles = Roles.Trainee)]
public class TraineeController : Controller
{
    private readonly ApplicationDbContext _db;
    public TraineeController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Dashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var enrollments = await _db.Enrollments
            .Include(e => e.Payments)
            .Include(e => e.AssessmentResult)
            .Include(e => e.CourseSession)!.ThenInclude(s => s!.Course)
            .Include(e => e.CourseSession)!.ThenInclude(s => s!.Classroom)
            .Where(e => e.TraineeId == userId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        var passedCourseIds = enrollments
            .Where(e => e.Status == EnrollmentStatus.Completed && e.AssessmentResult?.Result == AssessmentStatus.Pass)
            .Select(e => e.CourseSession!.CourseId)
            .Distinct()
            .ToList();

        var tracked = await _db.TraineeCertifications.Where(t => t.TraineeId == userId).ToListAsync();
        var tracks = await _db.CertificationTracks.Include(t => t.RequiredCourses).ThenInclude(rc => rc.Course).OrderBy(t => t.Name).ToListAsync();
        var progress = tracks.Select(track =>
        {
            var required = track.RequiredCourses.Select(rc => rc.CourseId).ToList();
            var existing = tracked.FirstOrDefault(t => t.CertificationTrackId == track.Id);
            return new CertificationProgressViewModel
            {
                Track = track,
                TotalRequiredCourses = required.Count,
                CompletedRequiredCourses = required.Count(passedCourseIds.Contains),
                Status = existing?.Status ?? TraineeCertificationStatus.InProgress,
                CertificateReferenceNumber = existing?.CertificateReferenceNumber,
                RemainingCourses = track.RequiredCourses.Where(rc => !passedCourseIds.Contains(rc.CourseId)).Select(rc => rc.Course?.Title ?? "Required course").ToList()
            };
        }).ToList();

        var payments = enrollments.Select(e => new PaymentSummaryViewModel
        {
            Enrollment = e,
            Fee = e.CourseSession?.Course?.EnrollmentFee ?? 0,
            Paid = e.Payments.Sum(p => p.Amount),
            IsOverdue = e.CourseSession?.StartDateTime.Date < DateTime.Today && e.Payments.Sum(p => p.Amount) < (e.CourseSession?.Course?.EnrollmentFee ?? 0)
        }).ToList();

        return View(new TraineeDashboardViewModel { Enrollments = enrollments, CertificationProgress = progress, PaymentSummaries = payments });
    }

    public IActionResult Catalog() => RedirectToAction("Index", "Courses");
    public IActionResult Enrollments() => RedirectToAction("Index", "Enrollments");
    public IActionResult Certifications() => RedirectToAction("Index", "Certificates");
    public IActionResult Payments() => RedirectToAction("Index", "Payments");
    public IActionResult Notifications() => RedirectToAction("Index", "Notifications");
}
