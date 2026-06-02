using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCertification.API.Data;
using TrainingCertification.API.Models;
using TrainingCertification.MVC.ViewModels;

namespace TrainingCertification.MVC.Controllers;

[Authorize(Roles = Roles.TrainingCoordinator)]
public class CoordinatorController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    public CoordinatorController(ApplicationDbContext db, UserManager<ApplicationUser> users) { _db = db; _users = users; }

    public async Task<IActionResult> Dashboard() => View("Dashboard", new DashboardViewModel
    {
        Courses = await _db.Courses.CountAsync(),
        Sessions = await _db.CourseSessions.CountAsync(),
        Enrollments = await _db.Enrollments.CountAsync(),
        Revenue = await _db.Payments.SumAsync(p => p.Amount),
        Certificates = await _db.Certificates.CountAsync()
    });

    public async Task<IActionResult> Users()
    {
        var users = await _users.Users.OrderBy(u => u.FullName).ToListAsync();
        var model = new List<(ApplicationUser User, IList<string> Roles)>();
        foreach (var user in users) model.Add((user, await _users.GetRolesAsync(user)));
        return View(model);
    }

    public IActionResult CreateInstructor() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInstructor(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
        var result = await _users.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(model);
        }
        await _users.AddToRoleAsync(user, Roles.Instructor);
        _db.InstructorProfiles.Add(new InstructorProfile { ApplicationUserId = user.Id, FullName = user.FullName, ExpertiseAreas = string.Empty });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Instructor account created.";
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> Categories() => View(await _db.CourseCategories.Include(c => c.Courses).OrderBy(c => c.Name).ToListAsync());
    public async Task<IActionResult> Classrooms() => View(await _db.Classrooms.Include(c => c.Equipment).OrderBy(c => c.RoomName).ToListAsync());
    public async Task<IActionResult> Instructors() => View(await _db.InstructorProfiles.Include(i => i.ApplicationUser).Include(i => i.Expertises).ThenInclude(e => e.CourseCategory).Include(i => i.Sessions).OrderBy(i => i.FullName).ToListAsync());
    public IActionResult Assessments() => RedirectToAction("Index", "Enrollments");
    public IActionResult Certifications() => RedirectToAction("Index", "Certificates");
    public IActionResult Payments() => RedirectToAction("Index", "Payments");

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory(CourseCategory category)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Category name is required.";
            return RedirectToAction(nameof(Categories));
        }

        if (category.Id == 0) _db.CourseCategories.Add(category);
        else
        {
            var existing = await _db.CourseCategories.FindAsync(category.Id);
            if (existing == null) return NotFound();
            existing.Name = category.Name;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Category saved.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _db.CourseCategories.Include(c => c.Courses).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return NotFound();
        if (category.Courses.Any())
        {
            TempData["Error"] = "Category cannot be deleted while courses use it.";
            return RedirectToAction(nameof(Categories));
        }

        _db.CourseCategories.Remove(category);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Category deleted.";
        return RedirectToAction(nameof(Categories));
    }

    public IActionResult CreateClassroom() => View("ClassroomForm", new ClassroomFormViewModel());

    public async Task<IActionResult> EditClassroom(int id)
    {
        var room = await _db.Classrooms.Include(c => c.Equipment).FirstOrDefaultAsync(c => c.Id == id);
        if (room == null) return NotFound();
        return View("ClassroomForm", new ClassroomFormViewModel { Classroom = room, EquipmentCsv = string.Join(", ", room.Equipment.Select(e => e.Name)) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveClassroom(ClassroomFormViewModel model)
    {
        if (!ModelState.IsValid) return View("ClassroomForm", model);
        var room = model.Classroom.Id == 0
            ? model.Classroom
            : await _db.Classrooms.Include(c => c.Equipment).FirstOrDefaultAsync(c => c.Id == model.Classroom.Id);
        if (room == null) return NotFound();

        room.RoomName = model.Classroom.RoomName;
        room.Location = model.Classroom.Location;
        room.Capacity = model.Classroom.Capacity;
        if (model.Classroom.Id == 0) _db.Classrooms.Add(room);
        await _db.SaveChangesAsync();

        var existing = await _db.ClassroomEquipment.Where(e => e.ClassroomId == room.Id).ToListAsync();
        _db.ClassroomEquipment.RemoveRange(existing);
        foreach (var equipment in model.EquipmentCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _db.ClassroomEquipment.Add(new ClassroomEquipment { ClassroomId = room.Id, Name = equipment });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Classroom saved.";
        return RedirectToAction(nameof(Classrooms));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteClassroom(int id)
    {
        var room = await _db.Classrooms.Include(c => c.Sessions).FirstOrDefaultAsync(c => c.Id == id);
        if (room == null) return NotFound();
        if (room.Sessions.Any())
        {
            TempData["Error"] = "Classroom cannot be deleted while sessions use it.";
            return RedirectToAction(nameof(Classrooms));
        }

        _db.Classrooms.Remove(room);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Classroom deleted.";
        return RedirectToAction(nameof(Classrooms));
    }

    public async Task<IActionResult> EditInstructor(int id)
    {
        var instructor = await _db.InstructorProfiles.Include(i => i.Expertises).Include(i => i.Availability).FirstOrDefaultAsync(i => i.Id == id);
        if (instructor == null) return NotFound();
        return View("InstructorForm", await BuildInstructorForm(instructor));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveInstructor(InstructorManagementViewModel model)
    {
        var instructor = await _db.InstructorProfiles.Include(i => i.Expertises).Include(i => i.Availability).FirstOrDefaultAsync(i => i.Id == model.Instructor.Id);
        if (instructor == null) return NotFound();

        instructor.FullName = model.Instructor.FullName;
        instructor.ExpertiseAreas = model.Instructor.ExpertiseAreas ?? string.Empty;
        instructor.Bio = model.Instructor.Bio ?? string.Empty;

        _db.InstructorExpertises.RemoveRange(instructor.Expertises);
        foreach (var categoryId in model.SelectedCategoryIds.Distinct())
        {
            _db.InstructorExpertises.Add(new InstructorExpertise { InstructorProfileId = instructor.Id, CourseCategoryId = categoryId });
        }

        _db.InstructorAvailabilities.RemoveRange(instructor.Availability);
        foreach (var slot in model.Availability.Where(a => a.IsAvailable && a.EndTime > a.StartTime))
        {
            _db.InstructorAvailabilities.Add(new InstructorAvailability { InstructorProfileId = instructor.Id, DayOfWeek = slot.DayOfWeek, StartTime = slot.StartTime, EndTime = slot.EndTime });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Instructor profile saved.";
        return RedirectToAction(nameof(Instructors));
    }

    private async Task<InstructorManagementViewModel> BuildInstructorForm(InstructorProfile instructor)
    {
        var availability = Enum.GetValues<DayOfWeek>().Select(day =>
        {
            var existing = instructor.Availability.FirstOrDefault(a => a.DayOfWeek == day);
            return new InstructorAvailabilityInput
            {
                DayOfWeek = day,
                IsAvailable = existing != null,
                StartTime = existing?.StartTime ?? new TimeSpan(9, 0, 0),
                EndTime = existing?.EndTime ?? new TimeSpan(17, 0, 0)
            };
        }).ToList();

        return new InstructorManagementViewModel
        {
            Instructor = instructor,
            Categories = await _db.CourseCategories.OrderBy(c => c.Name).ToListAsync(),
            SelectedCategoryIds = instructor.Expertises.Select(e => e.CourseCategoryId).ToArray(),
            Availability = availability
        };
    }
}
