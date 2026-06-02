using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCertification.API.Data;
using TrainingCertification.API.DTOs;
using TrainingCertification.API.Models;

namespace TrainingCertification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public CoursesController(ApplicationDbContext db) => _db = db;

    /// <summary>Returns all active courses with category and prerequisite information.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CourseDto>>>> GetCourses()
    {
        var courses = await _db.Courses.Include(c => c.CourseCategory).Include(c => c.Prerequisites).ThenInclude(p => p.PrerequisiteCourse).Where(c => c.IsActive)
            .Select(c => new CourseDto(c.Id, c.Title, c.CourseCategory!.Name, c.DurationDays, c.Capacity, c.EnrollmentFee, c.Description, c.Prerequisites.Select(p => p.PrerequisiteCourse!.Title).FirstOrDefault(), c.RequiredEquipment))
            .ToListAsync();
        return Ok(new ApiResponse<IEnumerable<CourseDto>>(true, courses, "Courses loaded."));
    }

    /// <summary>Returns one course with upcoming sessions and enrollment counts.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CourseDetailDto>>> GetCourse(int id)
    {
        var course = await _db.Courses.Include(c => c.CourseCategory).Include(c => c.Prerequisites).ThenInclude(p => p.PrerequisiteCourse).FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return NotFound(new { message = "Course was not found." });

        var sessions = await QuerySessions().Where(s => s.CourseId == id).ToListAsync();
        var data = new CourseDetailDto(course.Id, course.Title, course.Description, course.CourseCategory!.Name, course.DurationDays, course.Capacity, course.EnrollmentFee, course.Prerequisites.Select(p => p.PrerequisiteCourse!.Title).FirstOrDefault(), course.RequiredEquipment, sessions);
        return Ok(new ApiResponse<CourseDetailDto>(true, data, "Course loaded."));
    }

    /// <summary>Returns available upcoming sessions for a course.</summary>
    [HttpGet("{id:int}/sessions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SessionDto>>>> GetSessions(int id)
    {
        var sessions = await QuerySessions()
            .Where(s => s.CourseId == id && s.StartDateTime >= DateTime.UtcNow && s.RemainingSeats > 0)
            .ToListAsync();
        return Ok(new ApiResponse<IEnumerable<SessionDto>>(true, sessions, "Sessions loaded."));
    }

    private IQueryable<SessionDto> QuerySessions()
    {
        return _db.CourseSessions
            .Include(s => s.Course)
            .Include(s => s.Classroom)
            .Include(s => s.InstructorProfile)!.ThenInclude(i => i!.ApplicationUser)
            .Include(s => s.Enrollments)
            .Where(s => s.Status != CourseSessionStatus.Cancelled
                && s.Course != null
                && s.InstructorProfile != null
                && s.InstructorProfile.ApplicationUser != null
                && s.Classroom != null)
            .Select(s => new SessionDto(
                s.Id,
                s.CourseId,
                s.Course!.Title,
                s.InstructorProfile!.ApplicationUser!.FullName,
                s.Classroom!.RoomName,
                s.StartDateTime,
                s.EndDateTime,
                s.Capacity,
                s.Enrollments.Count(e => e.Status != EnrollmentStatus.Dropped),
                s.Capacity - s.Enrollments.Count(e => e.Status != EnrollmentStatus.Dropped)));
    }
}
