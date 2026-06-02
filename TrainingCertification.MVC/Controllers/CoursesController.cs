using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrainingCertification.API.Data;
using TrainingCertification.API.Models;
using TrainingCertification.MVC.ViewModels;

namespace TrainingCertification.MVC.Controllers;

public class CoursesController : Controller
{
    private readonly ApplicationDbContext _db;
    public CoursesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Courses
            .Include(c => c.CourseCategory)
            .Include(c => c.Prerequisites).ThenInclude(p => p.PrerequisiteCourse)
            .AsQueryable();

        if (!User.IsInRole(Roles.TrainingCoordinator)) query = query.Where(c => c.IsActive);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(c => c.Title.Contains(search) || c.CourseCategory!.Name.Contains(search));

        var courses = await query.OrderBy(c => c.Title).ToListAsync();
        ViewBag.CatalogItems = await BuildCatalogItems(courses);
        return View(new CourseListViewModel { Courses = courses, Search = search });
    }

    public async Task<IActionResult> Details(int id)
    {
        var course = await _db.Courses.Include(c => c.CourseCategory).Include(c => c.Prerequisites).ThenInclude(p => p.PrerequisiteCourse).Include(c => c.Sessions).ThenInclude(s => s.Enrollments).FirstOrDefaultAsync(c => c.Id == id);
        if (course != null) ViewBag.CatalogItems = await BuildCatalogItems(new[] { course });
        return course == null ? NotFound() : View(course);
    }

    [Authorize(Roles = Roles.TrainingCoordinator)]
    public async Task<IActionResult> Create() => View("Form", await BuildForm(new Course { IsActive = true, Capacity = 20, DurationDays = 1 }));

    [HttpPost, Authorize(Roles = Roles.TrainingCoordinator), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseFormViewModel model)
    {
        await ValidatePrerequisite(model.Course.Id, model.PrerequisiteCourseId);
        if (!ModelState.IsValid) return View("Form", await BuildForm(model.Course, model.PrerequisiteCourseId));
        _db.Courses.Add(model.Course);
        await _db.SaveChangesAsync();
        await SavePrerequisite(model.Course.Id, model.PrerequisiteCourseId);
        TempData["Success"] = "Course created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.TrainingCoordinator)]
    public async Task<IActionResult> Edit(int id)
    {
        var course = await _db.Courses.Include(c => c.Prerequisites).FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return NotFound();
        return View("Form", await BuildForm(course, course.Prerequisites.Select(p => (int?)p.PrerequisiteCourseId).FirstOrDefault()));
    }

    [HttpPost, Authorize(Roles = Roles.TrainingCoordinator), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CourseFormViewModel model)
    {
        var course = await _db.Courses.Include(c => c.Prerequisites).FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return NotFound();
        await ValidatePrerequisite(id, model.PrerequisiteCourseId);
        if (!ModelState.IsValid) return View("Form", await BuildForm(model.Course, model.PrerequisiteCourseId));

        course.Title = model.Course.Title;
        course.Description = model.Course.Description;
        course.DurationDays = model.Course.DurationDays;
        course.Capacity = model.Course.Capacity;
        course.EnrollmentFee = model.Course.EnrollmentFee;
        course.RequiredEquipment = model.Course.RequiredEquipment ?? string.Empty;
        course.CourseCategoryId = model.Course.CourseCategoryId;
        course.IsActive = model.Course.IsActive;
        await _db.SaveChangesAsync();
        await SavePrerequisite(course.Id, model.PrerequisiteCourseId);
        TempData["Success"] = "Course updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Roles = Roles.TrainingCoordinator), ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();
        course.IsActive = !course.IsActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = course.IsActive ? "Course activated." : "Course deactivated.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<CourseFormViewModel> BuildForm(Course course, int? prerequisiteCourseId = null) => new()
    {
        Course = course,
        PrerequisiteCourseId = prerequisiteCourseId,
        Categories = await _db.CourseCategories.OrderBy(c => c.Name).ToListAsync(),
        AvailablePrerequisites = await _db.Courses.Where(c => c.Id != course.Id).OrderBy(c => c.Title).ToListAsync()
    };

    private async Task SavePrerequisite(int courseId, int? prerequisiteCourseId)
    {
        var existing = await _db.CoursePrerequisites.Where(p => p.CourseId == courseId).ToListAsync();
        _db.CoursePrerequisites.RemoveRange(existing);
        if (prerequisiteCourseId.HasValue && prerequisiteCourseId.Value != courseId)
        {
            _db.CoursePrerequisites.Add(new CoursePrerequisite { CourseId = courseId, PrerequisiteCourseId = prerequisiteCourseId.Value });
        }

        await _db.SaveChangesAsync();
    }

    private async Task ValidatePrerequisite(int courseId, int? prerequisiteCourseId)
    {
        if (!prerequisiteCourseId.HasValue) return;
        if (prerequisiteCourseId.Value == courseId)
        {
            ModelState.AddModelError(nameof(CourseFormViewModel.PrerequisiteCourseId), "A course cannot require itself.");
            return;
        }

        var prerequisite = await _db.Courses.Include(c => c.Prerequisites).FirstOrDefaultAsync(c => c.Id == prerequisiteCourseId.Value);
        if (prerequisite == null || !prerequisite.IsActive)
        {
            ModelState.AddModelError(nameof(CourseFormViewModel.PrerequisiteCourseId), "Prerequisite must be an active course.");
            return;
        }

        var visited = new HashSet<int> { courseId };
        var current = prerequisite;
        while (current.Prerequisites.Any())
        {
            var nextId = current.Prerequisites.Select(p => p.PrerequisiteCourseId).First();
            if (!visited.Add(nextId))
            {
                ModelState.AddModelError(nameof(CourseFormViewModel.PrerequisiteCourseId), "Prerequisite selection would create a circular dependency.");
                return;
            }

            current = await _db.Courses.Include(c => c.Prerequisites).FirstOrDefaultAsync(c => c.Id == nextId);
            if (current == null) return;
        }
    }

    private async Task<List<CourseCatalogItemViewModel>> BuildCatalogItems(IEnumerable<Course> courses)
    {
        if (User.Identity?.IsAuthenticated != true || !User.IsInRole(Roles.Trainee))
        {
            return courses.Select(c => new CourseCatalogItemViewModel { Course = c, PrerequisiteTitle = c.Prerequisites.Select(p => p.PrerequisiteCourse?.Title).FirstOrDefault() }).ToList();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var passedCourseIds = await _db.Enrollments
            .Where(e => e.TraineeId == userId && e.Status == EnrollmentStatus.Completed && e.AssessmentResult != null && e.AssessmentResult.Result == AssessmentStatus.Pass)
            .Select(e => e.CourseSession!.CourseId)
            .Distinct()
            .ToListAsync();

        return courses.Select(c =>
        {
            var prerequisite = c.Prerequisites.FirstOrDefault();
            return new CourseCatalogItemViewModel
            {
                Course = c,
                PrerequisiteTitle = prerequisite?.PrerequisiteCourse?.Title,
                PrerequisiteMet = prerequisite == null || passedCourseIds.Contains(prerequisite.PrerequisiteCourseId),
                AlreadyPassed = passedCourseIds.Contains(c.Id)
            };
        }).ToList();
    }
}
