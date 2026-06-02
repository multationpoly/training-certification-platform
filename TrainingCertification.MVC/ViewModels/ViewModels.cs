using System.ComponentModel.DataAnnotations;
using TrainingCertification.API.Models;

namespace TrainingCertification.MVC.ViewModels;

public class LoginViewModel { [Required, EmailAddress] public string Email { get; set; } = string.Empty; [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty; [Display(Name = "Remember me")] public bool RememberMe { get; set; } public string? ReturnUrl { get; set; } }
public class RegisterViewModel { [Required] public string FullName { get; set; } = string.Empty; [Required, EmailAddress] public string Email { get; set; } = string.Empty; [Required, DataType(DataType.Password)] [StringLength(100, MinimumLength = 8)] public string Password { get; set; } = string.Empty; [Required, DataType(DataType.Password), Compare(nameof(Password))] public string ConfirmPassword { get; set; } = string.Empty; }
public class DashboardViewModel { public int Courses { get; set; } public int Sessions { get; set; } public int Enrollments { get; set; } public decimal Revenue { get; set; } public int Certificates { get; set; } }
public class CourseListViewModel { public IEnumerable<Course> Courses { get; set; } = new List<Course>(); public string? Search { get; set; } public int Page { get; set; } = 1; public int TotalPages { get; set; } = 1; }
public class SessionFormViewModel { public CourseSession Session { get; set; } = new(); public IEnumerable<Course> Courses { get; set; } = new List<Course>(); public IEnumerable<InstructorProfile> Instructors { get; set; } = new List<InstructorProfile>(); public IEnumerable<Classroom> Classrooms { get; set; } = new List<Classroom>(); }
public class PublicLookupViewModel { [Display(Name="Trainee ID"), Required] public string TraineeId { get; set; } = string.Empty; [Display(Name="Certificate Reference Number"), Required] public string ReferenceNumber { get; set; } = string.Empty; public PublicCertificationLookupResultViewModel? Result { get; set; } public string? Error { get; set; } public bool SearchCompleted { get; set; } }
public class PublicCertificationLookupResultViewModel { public string TraineeName { get; set; } = string.Empty; public string CertificationName { get; set; } = string.Empty; public string Status { get; set; } = string.Empty; public DateTime? IssuedDate { get; set; } public IEnumerable<CompletedCourseViewModel> CompletedCourses { get; set; } = new List<CompletedCourseViewModel>(); }
public class CompletedCourseViewModel { public string Title { get; set; } = string.Empty; public string Result { get; set; } = string.Empty; public DateTime CompletionDate { get; set; } }
public class ApiResponseViewModel<T> { public bool Success { get; set; } public T? Data { get; set; } public string Message { get; set; } = string.Empty; }

public class CourseFormViewModel
{
    public Course Course { get; set; } = new();
    [Display(Name = "Prerequisite Course")] public int? PrerequisiteCourseId { get; set; }
    public IEnumerable<CourseCategory> Categories { get; set; } = new List<CourseCategory>();
    public IEnumerable<Course> AvailablePrerequisites { get; set; } = new List<Course>();
}

public class ClassroomFormViewModel
{
    public Classroom Classroom { get; set; } = new();
    [Display(Name = "Equipment, comma separated")] public string EquipmentCsv { get; set; } = string.Empty;
}

public class InstructorManagementViewModel
{
    public InstructorProfile Instructor { get; set; } = new();
    public IEnumerable<CourseCategory> Categories { get; set; } = new List<CourseCategory>();
    public int[] SelectedCategoryIds { get; set; } = Array.Empty<int>();
    public List<InstructorAvailabilityInput> Availability { get; set; } = new();
}

public class InstructorAvailabilityInput
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; } = new(9, 0, 0);
    public TimeSpan EndTime { get; set; } = new(17, 0, 0);
    public bool IsAvailable { get; set; }
}

public class CourseCatalogItemViewModel
{
    public Course Course { get; set; } = new();
    public string? PrerequisiteTitle { get; set; }
    public bool PrerequisiteMet { get; set; } = true;
    public bool AlreadyPassed { get; set; }
}

public class CertificationProgressViewModel
{
    public CertificationTrack Track { get; set; } = new();
    public int CompletedRequiredCourses { get; set; }
    public int TotalRequiredCourses { get; set; }
    public TraineeCertificationStatus Status { get; set; } = TraineeCertificationStatus.InProgress;
    public string? CertificateReferenceNumber { get; set; }
    public IEnumerable<string> RemainingCourses { get; set; } = new List<string>();
}

public class PaymentSummaryViewModel
{
    public Enrollment Enrollment { get; set; } = new();
    public decimal Fee { get; set; }
    public decimal Paid { get; set; }
    public decimal Outstanding => Math.Max(0, Fee - Paid);
    public bool IsOverdue { get; set; }
    public PaymentStatus Status => Outstanding <= 0 ? PaymentStatus.Paid : IsOverdue ? PaymentStatus.Overdue : PaymentStatus.Partial;
}

public class TraineeDashboardViewModel
{
    public IEnumerable<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public IEnumerable<CertificationProgressViewModel> CertificationProgress { get; set; } = new List<CertificationProgressViewModel>();
    public IEnumerable<PaymentSummaryViewModel> PaymentSummaries { get; set; } = new List<PaymentSummaryViewModel>();
}
