using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrainingCertification.API.Data;
using TrainingCertification.API.Models;

namespace TrainingCertification.MVC.Controllers;

[Authorize(Roles = Roles.Instructor)]
public class InstructorController : Controller
{
    private readonly ApplicationDbContext _db;
    public InstructorController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Dashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessions = await _db.CourseSessions.Include(s => s.Course).Include(s => s.Classroom).Include(s => s.Enrollments).Where(s => s.InstructorProfile!.ApplicationUserId == userId).OrderBy(s => s.StartDateTime).ToListAsync();
        return View(sessions);
    }

    public IActionResult Sessions() => RedirectToAction("Index", "Sessions");
    public IActionResult Assessments() => RedirectToAction("Index", "Enrollments");
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var profile = await _db.InstructorProfiles.Include(i => i.Expertises).ThenInclude(e => e.CourseCategory).Include(i => i.Availability).FirstOrDefaultAsync(i => i.ApplicationUserId == userId);
        return profile == null ? NotFound() : View(profile);
    }
}
