using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TrainingCertification.API.Models;

public class ApplicationUser : IdentityUser
{
    [Required, StringLength(120)] public string FullName { get; set; } = string.Empty;
    [StringLength(30)] public string TraineeNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public InstructorProfile? InstructorProfile { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<TraineeCertification> TraineeCertifications { get; set; } = new List<TraineeCertification>();
}

public static class Roles
{
    public const string Trainee = "Trainee";
    public const string Instructor = "Instructor";
    public const string TrainingCoordinator = "TrainingCoordinator";
}

public enum EnrollmentStatus { Enrolled, Confirmed, Attending, Completed, Dropped }
public enum PaymentStatus { Partial, Paid, Overdue }
public enum AssessmentStatus { Pending, Pass, Fail }
public enum CourseSessionStatus { Scheduled, Ongoing, Completed, Cancelled }
public enum TraineeCertificationStatus { InProgress, Eligible, Issued }
public enum NotificationType { EnrollmentConfirmation, EnrollmentStatusChanged, ScheduleChange, AssessmentRecorded, PaymentReminder, CertificationCompletion, CertificationEligible, NewSessionScheduled }

public class CourseCategory
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<InstructorExpertise> InstructorExpertises { get; set; } = new List<InstructorExpertise>();
}

public class Category : CourseCategory { }

public class Course
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Title { get; set; } = string.Empty;
    [Required, StringLength(1000)] public string Description { get; set; } = string.Empty;
    [Range(1, 3650)] public int DurationDays { get; set; }
    [NotMapped] public int Duration { get => DurationDays; set => DurationDays = value; }
    [Range(1, 500)] public int Capacity { get; set; }
    [Range(0, 999999)] public decimal EnrollmentFee { get; set; }
    [StringLength(300)] public string RequiredEquipment { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int CourseCategoryId { get; set; }
    [NotMapped] public int CategoryId { get => CourseCategoryId; set => CourseCategoryId = value; }
    public CourseCategory? CourseCategory { get; set; }
    [NotMapped] public CourseCategory? Category { get => CourseCategory; set => CourseCategory = value; }
    [NotMapped] public int? PrerequisiteCourseId { get; set; }
    public ICollection<CoursePrerequisite> Prerequisites { get; set; } = new List<CoursePrerequisite>();
    public ICollection<CoursePrerequisite> RequiredForCourses { get; set; } = new List<CoursePrerequisite>();
    public ICollection<CourseSession> Sessions { get; set; } = new List<CourseSession>();
    public ICollection<CertificationTrackCourse> CertificationTrackCourses { get; set; } = new List<CertificationTrackCourse>();
}

public class CoursePrerequisite
{
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public int PrerequisiteCourseId { get; set; }
    public Course? PrerequisiteCourse { get; set; }
}

public class InstructorProfile
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser? ApplicationUser { get; set; }
    [Required, StringLength(120)] public string FullName { get; set; } = string.Empty;
    [Required, StringLength(250)] public string ExpertiseAreas { get; set; } = string.Empty;
    [StringLength(1000)] public string Bio { get; set; } = string.Empty;
    public ICollection<InstructorExpertise> Expertises { get; set; } = new List<InstructorExpertise>();
    public ICollection<InstructorAvailability> Availability { get; set; } = new List<InstructorAvailability>();
    public ICollection<CourseSession> Sessions { get; set; } = new List<CourseSession>();
}

public class Instructor : InstructorProfile { }

public class InstructorExpertise
{
    public int Id { get; set; }
    public int InstructorProfileId { get; set; }
    [NotMapped] public int InstructorId { get => InstructorProfileId; set => InstructorProfileId = value; }
    public InstructorProfile? InstructorProfile { get; set; }
    [NotMapped] public InstructorProfile? Instructor { get => InstructorProfile; set => InstructorProfile = value; }
    public int CourseCategoryId { get; set; }
    [NotMapped] public int CategoryId { get => CourseCategoryId; set => CourseCategoryId = value; }
    public CourseCategory? CourseCategory { get; set; }
    [NotMapped] public CourseCategory? Category { get => CourseCategory; set => CourseCategory = value; }
}

public class InstructorAvailability
{
    public int Id { get; set; }
    public int InstructorProfileId { get; set; }
    [NotMapped] public int InstructorId { get => InstructorProfileId; set => InstructorProfileId = value; }
    public InstructorProfile? InstructorProfile { get; set; }
    [NotMapped] public InstructorProfile? Instructor { get => InstructorProfile; set => InstructorProfile = value; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}

public class Classroom
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string RoomName { get; set; } = string.Empty;
    [NotMapped] public string Name { get => RoomName; set => RoomName = value; }
    [Range(1, 500)] public int Capacity { get; set; }
    [StringLength(200)] public string Location { get; set; } = string.Empty;
    public ICollection<ClassroomEquipment> Equipment { get; set; } = new List<ClassroomEquipment>();
    public ICollection<CourseSession> Sessions { get; set; } = new List<CourseSession>();
}

public class ClassroomEquipment
{
    public int Id { get; set; }
    public int ClassroomId { get; set; }
    public Classroom? Classroom { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    [NotMapped] public string EquipmentType { get => Name; set => Name = value; }
}

public class CourseSession
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public int InstructorProfileId { get; set; }
    [NotMapped] public int InstructorId { get => InstructorProfileId; set => InstructorProfileId = value; }
    public InstructorProfile? InstructorProfile { get; set; }
    [NotMapped] public InstructorProfile? Instructor { get => InstructorProfile; set => InstructorProfile = value; }
    public int ClassroomId { get; set; }
    [NotMapped] public int RoomId { get => ClassroomId; set => ClassroomId = value; }
    public Classroom? Classroom { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public CourseSessionStatus Status { get; set; } = CourseSessionStatus.Scheduled;
    [NotMapped] public DateTime SessionDate { get => StartDateTime.Date; set => StartDateTime = value.Date.Add(StartTime); }
    [NotMapped] public TimeSpan StartTime { get => StartDateTime.TimeOfDay; set => StartDateTime = SessionDate.Date.Add(value); }
    [NotMapped] public TimeSpan EndTime { get => EndDateTime.TimeOfDay; set => EndDateTime = SessionDate.Date.Add(value); }
    [Range(1, 500)] public int Capacity { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class Enrollment
{
    public int Id { get; set; }
    public string TraineeId { get; set; } = string.Empty;
    public ApplicationUser? Trainee { get; set; }
    public int CourseSessionId { get; set; }
    public CourseSession? CourseSession { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Enrolled;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public AssessmentResult? AssessmentResult { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<EnrollmentStatusHistory> StatusHistory { get; set; } = new List<EnrollmentStatusHistory>();
}

public class AssessmentResult
{
    public int Id { get; set; }
    public int EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }
    public AssessmentStatus Result { get; set; } = AssessmentStatus.Pending;
    [Range(0, 100)] public decimal Score { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    [StringLength(500)] public string Remarks { get; set; } = string.Empty;
    [NotMapped] public string InstructorNotes { get => Remarks; set => Remarks = value; }
}

public class CertificationTrack
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string Name { get; set; } = string.Empty;
    [StringLength(1000)] public string Description { get; set; } = string.Empty;
    public ICollection<CertificationTrackCourse> RequiredCourses { get; set; } = new List<CertificationTrackCourse>();
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
    public ICollection<TraineeCertification> TraineeCertifications { get; set; } = new List<TraineeCertification>();
}

public class CertificationTrackCourse
{
    public int Id { get; set; }
    public int CertificationTrackId { get; set; }
    public CertificationTrack? CertificationTrack { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
}

public class Certificate
{
    public int Id { get; set; }
    public string TraineeId { get; set; } = string.Empty;
    public ApplicationUser? Trainee { get; set; }
    public int CertificationTrackId { get; set; }
    public CertificationTrack? CertificationTrack { get; set; }
    [Required, StringLength(40)] public string ReferenceNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; }
}

public class TraineeCertification
{
    public int Id { get; set; }
    public string TraineeId { get; set; } = string.Empty;
    public ApplicationUser? Trainee { get; set; }
    public int CertificationTrackId { get; set; }
    public CertificationTrack? CertificationTrack { get; set; }
    public TraineeCertificationStatus Status { get; set; } = TraineeCertificationStatus.InProgress;
    public DateTime? IssuedAt { get; set; }
    [StringLength(40)] public string? CertificateReferenceNumber { get; set; }
}

public class Payment
{
    public int Id { get; set; }
    public int EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }
    public string TraineeId { get; set; } = string.Empty;
    public ApplicationUser? Trainee { get; set; }
    public int CourseSessionId { get; set; }
    public CourseSession? CourseSession { get; set; }
    [Range(0, 999999)] public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    [NotMapped] public DateTime PaidAt { get => PaymentDate; set => PaymentDate = value; }
    [StringLength(40)] public string Method { get; set; } = "Cash";
    [NotMapped] public string PaymentMethod { get => Method; set => Method = value; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Partial;
    [StringLength(500)] public string Notes { get; set; } = string.Empty;
}

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    [Required, StringLength(120)] public string Title { get; set; } = string.Empty;
    [Required, StringLength(600)] public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class EnrollmentStatusHistory
{
    public int Id { get; set; }
    public int EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }
    public EnrollmentStatus OldStatus { get; set; }
    public EnrollmentStatus NewStatus { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    [StringLength(300)] public string Notes { get; set; } = string.Empty;
}
