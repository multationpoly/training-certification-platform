using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrainingCertification.API.Data;
using TrainingCertification.API.Models;
using TrainingCertification.API.Services;
using TrainingCertification.MVC.Helpers;
using TrainingCertification.MVC.ViewModels;

namespace TrainingCertification.MVC.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IPaymentReminderService _paymentReminders;
    public PaymentsController(ApplicationDbContext db, IPaymentReminderService paymentReminders)
    {
        _db = db;
        _paymentReminders = paymentReminders;
    }
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = _db.Enrollments
            .Include(e => e.Trainee)
            .Include(e => e.Payments)
            .Include(e => e.CourseSession)!.ThenInclude(s => s!.Course)
            .Include(e => e.CourseSession)!.ThenInclude(s => s!.InstructorProfile)
            .Where(e => e.Status != EnrollmentStatus.Dropped)
            .AsQueryable();
        if (User.IsInRole(Roles.Trainee)) query = query.Where(e => e.TraineeId == userId);
        if (User.IsInRole(Roles.Instructor)) query = query.Where(e => e.CourseSession!.InstructorProfile!.ApplicationUserId == userId);

        var summaries = (await query.OrderByDescending(e => e.EnrolledAt).ToListAsync()).Select(e => new PaymentSummaryViewModel
        {
            Enrollment = e,
            Fee = e.CourseSession?.Course?.EnrollmentFee ?? 0,
            Paid = e.Payments.Sum(p => p.Amount),
            IsOverdue = e.CourseSession?.StartDateTime.Date < DateTime.Today && e.Payments.Sum(p => p.Amount) < (e.CourseSession?.Course?.EnrollmentFee ?? 0)
        }).ToList();

        if (User.IsInRole(Roles.TrainingCoordinator)) await _paymentReminders.CreateOverduePaymentRemindersAsync();

        return View(summaries);
    }
    [Authorize(Roles = Roles.TrainingCoordinator)]
    public IActionResult Create(int enrollmentId) => View(new Payment { EnrollmentId = enrollmentId, PaymentDate = DateTime.Today });
    [HttpPost, Authorize(Roles = Roles.TrainingCoordinator), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Payment payment)
    {
        var enrollment = await _db.Enrollments.Include(e => e.CourseSession)!.ThenInclude(s => s!.Course).FirstOrDefaultAsync(e => e.Id == payment.EnrollmentId);
        if (enrollment == null) return NotFound();
        if (payment.Amount <= 0) ModelState.AddModelError(nameof(payment.Amount), "Payment amount must be greater than zero.");
        if (!ModelState.IsValid) return View(payment);
        payment.TraineeId = enrollment.TraineeId;
        payment.CourseSessionId = enrollment.CourseSessionId;
        payment.PaymentDate = DateTime.UtcNow;
        _db.Payments.Add(payment);
        var fee = enrollment.CourseSession?.Course?.EnrollmentFee ?? await _db.CourseSessions.Where(s => s.Id == enrollment.CourseSessionId).Select(s => s.Course!.EnrollmentFee).FirstOrDefaultAsync();
        var paidBefore = await _db.Payments.Where(p => p.EnrollmentId == enrollment.Id).SumAsync(p => p.Amount);
        var totalPaid = paidBefore + payment.Amount;
        _db.Notifications.Add(new Notification
        {
            UserId = enrollment.TraineeId,
            Title = totalPaid >= fee ? "Payment completed" : "Payment received",
            Message = totalPaid >= fee ? "Your course fee is fully paid." : $"Payment recorded. Outstanding balance: {CurrencyFormat.Money(fee - totalPaid)}.",
            Type = totalPaid >= fee ? NotificationType.EnrollmentStatusChanged : NotificationType.PaymentReminder
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Payment recorded.";
        return RedirectToAction(nameof(Index));
    }
}

[Authorize]
public class CertificatesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICertificationService _service;
    public CertificatesController(ApplicationDbContext db, ICertificationService service) { _db = db; _service = service; }
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = _db.Certificates.Include(c => c.Trainee).Include(c => c.CertificationTrack).AsQueryable();
        if (User.IsInRole(Roles.Trainee)) query = query.Where(c => c.TraineeId == userId);
        if (User.IsInRole(Roles.TrainingCoordinator))
        {
            ViewBag.EligibleCertifications = await _db.TraineeCertifications
                .Include(x => x.Trainee)
                .Include(x => x.CertificationTrack)
                .Where(x => x.Status == TraineeCertificationStatus.Eligible)
                .OrderBy(x => x.Trainee!.FullName)
                .ToListAsync();
        }
        return View(await query.OrderByDescending(c => c.IssueDate).ToListAsync());
    }
    [HttpPost, Authorize(Roles = Roles.TrainingCoordinator), ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(string traineeId, int trackId)
    {
        var cert = await _service.GenerateIfEligibleAsync(traineeId, trackId);
        TempData[cert == null ? "Error" : "Success"] = cert == null ? "Trainee is not eligible yet." : $"Certificate {cert.ReferenceNumber} generated.";
        return RedirectToAction(nameof(Index));
    }
}

[Authorize]
public class NotificationsController : Controller
{
    private readonly ApplicationDbContext _db;
    public NotificationsController(ApplicationDbContext db) => _db = db;
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = _db.Notifications.Include(n => n.User).AsQueryable();
        if (!User.IsInRole(Roles.TrainingCoordinator)) query = query.Where(n => n.UserId == userId);
        return View(await query.OrderByDescending(n => n.CreatedAt).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var notifications = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var notification in notifications) notification.IsRead = true;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Notifications marked as read.";
        return RedirectToAction(nameof(Index));
    }
}
