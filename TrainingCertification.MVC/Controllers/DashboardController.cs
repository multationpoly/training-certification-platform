using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCertification.API.Data;
using TrainingCertification.API.Models;
using TrainingCertification.MVC.ViewModels;

namespace TrainingCertification.MVC.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    public DashboardController(ApplicationDbContext db) => _db = db;
    public async Task<IActionResult> Index()
    {
        if (User.IsInRole(Roles.TrainingCoordinator)) return RedirectToAction("Dashboard", "Coordinator");
        if (User.IsInRole(Roles.Instructor)) return RedirectToAction("Dashboard", "Instructor");
        if (User.IsInRole(Roles.Trainee)) return RedirectToAction("Dashboard", "Trainee");
        return View(new DashboardViewModel
        {
            Courses = await _db.Courses.CountAsync(),
            Sessions = await _db.CourseSessions.CountAsync(),
            Enrollments = await _db.Enrollments.CountAsync(),
            Revenue = await _db.Payments.SumAsync(p => p.Amount),
            Certificates = await _db.Certificates.CountAsync()
        });
    }
}
