using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrainingCertification.API.Models;

namespace TrainingCertification.API.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<CourseCategory> CourseCategories => Set<CourseCategory>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CoursePrerequisite> CoursePrerequisites => Set<CoursePrerequisite>();
    public DbSet<InstructorProfile> InstructorProfiles => Set<InstructorProfile>();
    public DbSet<InstructorExpertise> InstructorExpertises => Set<InstructorExpertise>();
    public DbSet<InstructorAvailability> InstructorAvailabilities => Set<InstructorAvailability>();
    public DbSet<Classroom> Classrooms => Set<Classroom>();
    public DbSet<ClassroomEquipment> ClassroomEquipment => Set<ClassroomEquipment>();
    public DbSet<CourseSession> CourseSessions => Set<CourseSession>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<AssessmentResult> AssessmentResults => Set<AssessmentResult>();
    public DbSet<CertificationTrack> CertificationTracks => Set<CertificationTrack>();
    public DbSet<CertificationTrackCourse> CertificationTrackCourses => Set<CertificationTrackCourse>();
    public DbSet<TraineeCertification> TraineeCertifications => Set<TraineeCertification>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<EnrollmentStatusHistory> EnrollmentStatusHistories => Set<EnrollmentStatusHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Ignore<Category>();
        builder.Ignore<Instructor>();

        builder.Entity<Course>().Property(x => x.EnrollmentFee).HasColumnType("decimal(18,2)");
        builder.Entity<Payment>().Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.Entity<AssessmentResult>().Property(x => x.Score).HasColumnType("decimal(5,2)");
        builder.Entity<Enrollment>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<Payment>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<AssessmentResult>().Property(x => x.Result).HasConversion<string>();
        builder.Entity<CourseSession>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<TraineeCertification>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<Notification>().Property(x => x.Type).HasConversion<string>();

        builder.Entity<CoursePrerequisite>().HasKey(x => new { x.CourseId, x.PrerequisiteCourseId });
        builder.Entity<CoursePrerequisite>()
            .HasOne(x => x.Course).WithMany(x => x.Prerequisites).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<CoursePrerequisite>()
            .HasOne(x => x.PrerequisiteCourse).WithMany(x => x.RequiredForCourses).HasForeignKey(x => x.PrerequisiteCourseId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<InstructorExpertise>()
            .HasOne(x => x.InstructorProfile)
            .WithMany(x => x.Expertises)
            .HasForeignKey(x => x.InstructorProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<InstructorExpertise>()
            .HasOne(x => x.CourseCategory)
            .WithMany(x => x.InstructorExpertises)
            .HasForeignKey(x => x.CourseCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<InstructorExpertise>().HasIndex(x => new { x.InstructorProfileId, x.CourseCategoryId }).IsUnique();

        builder.Entity<CertificationTrackCourse>().HasKey(x => x.Id);
        builder.Entity<CertificationTrackCourse>().HasIndex(x => new { x.CertificationTrackId, x.CourseId }).IsUnique();
        builder.Entity<CertificationTrackCourse>()
            .HasOne(x => x.CertificationTrack)
            .WithMany(x => x.RequiredCourses)
            .HasForeignKey(x => x.CertificationTrackId);
        builder.Entity<CertificationTrackCourse>()
            .HasOne(x => x.Course)
            .WithMany(x => x.CertificationTrackCourses)
            .HasForeignKey(x => x.CourseId);

        builder.Entity<Enrollment>()
            .HasOne(x => x.CourseSession)
            .WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.CourseSessionId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Enrollment>()
            .HasOne(x => x.Trainee)
            .WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.TraineeId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Enrollment>().HasIndex(x => new { x.TraineeId, x.CourseSessionId }).IsUnique();

        builder.Entity<Payment>()
            .HasOne(x => x.Enrollment)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.EnrollmentId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Payment>()
            .HasOne(x => x.Trainee)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.TraineeId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Payment>()
            .HasOne(x => x.CourseSession)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.CourseSessionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<TraineeCertification>()
            .HasOne(x => x.Trainee)
            .WithMany(x => x.TraineeCertifications)
            .HasForeignKey(x => x.TraineeId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<TraineeCertification>()
            .HasOne(x => x.CertificationTrack)
            .WithMany(x => x.TraineeCertifications)
            .HasForeignKey(x => x.CertificationTrackId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<TraineeCertification>().HasIndex(x => new { x.TraineeId, x.CertificationTrackId }).IsUnique();
        builder.Entity<TraineeCertification>().HasIndex(x => x.CertificateReferenceNumber).IsUnique().HasFilter("[CertificateReferenceNumber] IS NOT NULL");

        builder.Entity<Certificate>().HasIndex(x => x.ReferenceNumber).IsUnique();
        builder.Entity<CourseSession>().HasIndex(x => new { x.InstructorProfileId, x.StartDateTime, x.EndDateTime });
        builder.Entity<CourseSession>().HasIndex(x => new { x.ClassroomId, x.StartDateTime, x.EndDateTime });
        builder.Entity<CourseSession>().HasIndex(x => x.CourseId);
        builder.Entity<Payment>().HasIndex(x => new { x.TraineeId, x.CourseSessionId });
        builder.Entity<Notification>().HasIndex(x => x.UserId);
        builder.Entity<ApplicationUser>().HasIndex(x => x.TraineeNumber).IsUnique().HasFilter("[TraineeNumber] <> ''");

        builder.Entity<CourseCategory>().HasData(
            new CourseCategory { Id = 1, Name = "Software Development" },
            new CourseCategory { Id = 2, Name = "Cloud Computing" },
            new CourseCategory { Id = 3, Name = "Project Management" });

        builder.Entity<Course>().HasData(
            new Course { Id = 1, CourseCategoryId = 1, Title = "C# Fundamentals", Description = "Programming basics using C# and .NET.", DurationDays = 5, Capacity = 25, EnrollmentFee = 95.00m },
            new Course { Id = 2, CourseCategoryId = 1, Title = "ASP.NET Core MVC", Description = "Build web applications using MVC and Razor Views.", DurationDays = 7, Capacity = 25, EnrollmentFee = 135.00m, RequiredEquipment = "Projector, Workstations" },
            new Course { Id = 3, CourseCategoryId = 2, Title = "Azure Essentials", Description = "Deploy applications and databases to Azure.", DurationDays = 4, Capacity = 20, EnrollmentFee = 120.00m, RequiredEquipment = "Projector" },
            new Course { Id = 4, CourseCategoryId = 3, Title = "Agile Project Management", Description = "Scrum, planning, tracking, and team delivery.", DurationDays = 3, Capacity = 30, EnrollmentFee = 80.00m });

        builder.Entity<CoursePrerequisite>().HasData(new CoursePrerequisite { CourseId = 2, PrerequisiteCourseId = 1 });

        builder.Entity<Classroom>().HasData(
            new Classroom { Id = 1, RoomName = "Lab A", Capacity = 25, Location = "Main Campus" },
            new Classroom { Id = 2, RoomName = "Room 204", Capacity = 30, Location = "Training Wing" });

        builder.Entity<ClassroomEquipment>().HasData(
            new ClassroomEquipment { Id = 1, ClassroomId = 1, Name = "Projector" },
            new ClassroomEquipment { Id = 2, ClassroomId = 1, Name = "25 Workstations" },
            new ClassroomEquipment { Id = 3, ClassroomId = 2, Name = "Whiteboard" });

        builder.Entity<CertificationTrack>().HasData(
            new CertificationTrack { Id = 1, Name = "Full Stack .NET Developer", Description = "C#, MVC, and Azure certification path." },
            new CertificationTrack { Id = 2, Name = "Agile Practitioner", Description = "Project delivery and agile practices." });

        builder.Entity<CertificationTrackCourse>().HasData(
            new CertificationTrackCourse { Id = 1, CertificationTrackId = 1, CourseId = 1 },
            new CertificationTrackCourse { Id = 2, CertificationTrackId = 1, CourseId = 2 },
            new CertificationTrackCourse { Id = 3, CertificationTrackId = 1, CourseId = 3 },
            new CertificationTrackCourse { Id = 4, CertificationTrackId = 2, CourseId = 4 });
    }
}
