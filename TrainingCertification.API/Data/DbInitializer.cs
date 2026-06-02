using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TrainingCertification.API.Models;

namespace TrainingCertification.API.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await db.Database.EnsureCreatedAsync();
        await db.Database.OpenConnectionAsync();

        var seedLockAcquired = false;
        try
        {
            await AcquireSeedLock(db);
            seedLockAcquired = true;

            foreach (var role in new[] { Roles.Trainee, Roles.Instructor, Roles.TrainingCoordinator })
            {
                await EnsureRole(roleManager, role);
            }

            var coordinator = await EnsureUser(userManager, "tcp.coordinator@gmail.com", "Fatima Al Khalifa", "TC-000", "Admin@12345", Roles.TrainingCoordinator);
            var instructor = await EnsureUser(userManager, "tcp.instructor@outlook.com", "Noor Al Zayani", "", "Admin@12345", Roles.Instructor);
            var instructorTwo = await EnsureUser(userManager, "tcp.instructor2@outlook.com", "Salman Al Mahmood", "", "Admin@12345", Roles.Instructor);
            var trainee = await EnsureUser(userManager, "tcp.trainee@gmail.com", "Ali Al Haddad", "TR-1001", "Admin@12345", Roles.Trainee);
            await EnsureUser(userManager, "tcp.trainee2@gmail.com", "Mariam Al Jalahma", "TR-1002", "Admin@12345", Roles.Trainee);

            await UpdateSeededCourseFeesAsync(db);

            var instructorProfile = await db.InstructorProfiles
                .Include(x => x.Availability)
                .FirstOrDefaultAsync(x => x.ApplicationUserId == instructor.Id);

            if (instructorProfile == null)
            {
                instructorProfile = new InstructorProfile
                {
                    ApplicationUserId = instructor.Id,
                    FullName = instructor.FullName,
                    ExpertiseAreas = "C#, ASP.NET Core, Azure",
                    Bio = "Senior trainer with practical web development experience.",
                    Availability = new List<InstructorAvailability>
                    {
                        new() { DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9,0,0), EndTime = new TimeSpan(17,0,0) },
                        new() { DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(9,0,0), EndTime = new TimeSpan(17,0,0) }
                    }
                };

                db.InstructorProfiles.Add(instructorProfile);
                await db.SaveChangesAsync();
            }
            else if (string.IsNullOrWhiteSpace(instructorProfile.FullName))
            {
                instructorProfile.FullName = instructor.FullName;
            }

            if (!await db.InstructorProfiles.AnyAsync(x => x.ApplicationUserId == instructorTwo.Id))
            {
                db.InstructorProfiles.Add(new InstructorProfile
                {
                    ApplicationUserId = instructorTwo.Id,
                    FullName = instructorTwo.FullName,
                    ExpertiseAreas = "Agile, project delivery",
                    Bio = "Agile trainer with software team coaching experience.",
                    Availability = new List<InstructorAvailability>
                    {
                        new() { DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(9,0,0), EndTime = new TimeSpan(17,0,0) }
                    }
                });
                await db.SaveChangesAsync();
            }

            var firstSession = await db.CourseSessions
                .FirstOrDefaultAsync(x => x.CourseId == 1 && x.InstructorProfileId == instructorProfile.Id);

            if (firstSession == null)
            {
                firstSession = new CourseSession { CourseId = 1, InstructorProfileId = instructorProfile.Id, ClassroomId = 1, StartDateTime = DateTime.Today.AddDays(7).AddHours(9), EndDateTime = DateTime.Today.AddDays(11).AddHours(13), Capacity = 25, Status = CourseSessionStatus.Scheduled };
                db.CourseSessions.Add(firstSession);
            }

            if (!await db.CourseSessions.AnyAsync(x => x.CourseId == 2 && x.InstructorProfileId == instructorProfile.Id))
            {
                db.CourseSessions.Add(new CourseSession { CourseId = 2, InstructorProfileId = instructorProfile.Id, ClassroomId = 1, StartDateTime = DateTime.Today.AddDays(14).AddHours(9), EndDateTime = DateTime.Today.AddDays(20).AddHours(13), Capacity = 25, Status = CourseSessionStatus.Scheduled });
            }

            if (db.ChangeTracker.HasChanges())
            {
                await db.SaveChangesAsync();
            }

            var enrollment = await db.Enrollments
                .Include(x => x.AssessmentResult)
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.TraineeId == trainee.Id && x.CourseSessionId == firstSession.Id);

            if (enrollment == null)
            {
                enrollment = new Enrollment { TraineeId = trainee.Id, CourseSessionId = firstSession.Id, Status = EnrollmentStatus.Completed };
                db.Enrollments.Add(enrollment);
                await db.SaveChangesAsync();
            }

            if (enrollment.AssessmentResult == null)
            {
                db.AssessmentResults.Add(new AssessmentResult { EnrollmentId = enrollment.Id, Result = AssessmentStatus.Pass, Score = 86, Remarks = "Good practical work." });
            }

            if (!enrollment.Payments.Any())
            {
                db.Payments.Add(new Payment { EnrollmentId = enrollment.Id, TraineeId = trainee.Id, CourseSessionId = firstSession.Id, Amount = 95.00m, Method = "Bank Transfer", Status = PaymentStatus.Paid });
            }
            else
            {
                foreach (var payment in enrollment.Payments.Where(payment => payment.Amount == 25000m))
                {
                    payment.Amount = 95.00m;
                }
            }

            if (!await db.Notifications.AnyAsync(x => x.UserId == trainee.Id && x.Title == "Enrollment confirmed"))
            {
                db.Notifications.Add(new Notification { UserId = trainee.Id, Title = "Enrollment confirmed", Message = "Your enrollment in C# Fundamentals is confirmed.", Type = NotificationType.EnrollmentConfirmation });
            }

            if (db.ChangeTracker.HasChanges())
            {
                await db.SaveChangesAsync();
            }
        }
        finally
        {
            if (seedLockAcquired)
            {
                await ReleaseSeedLock(db);
            }

            await db.Database.CloseConnectionAsync();
        }
    }

    private static async Task<ApplicationUser> EnsureUser(UserManager<ApplicationUser> userManager, string email, string fullName, string traineeNo, string password, string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = fullName, TraineeNumber = traineeNo };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    ThrowIdentityFailure($"creating seed user '{email}'", result);
                }
            }
        }

        if (user == null)
        {
            throw new InvalidOperationException($"Identity seed failed while finding seed user '{email}'.");
        }

        if (user.Email != email || user.UserName != email || user.FullName != fullName || user.TraineeNumber != traineeNo)
        {
            user.Email = email;
            user.NormalizedEmail = userManager.NormalizeEmail(email);
            user.UserName = email;
            user.NormalizedUserName = userManager.NormalizeName(email);
            user.FullName = fullName;
            user.TraineeNumber = traineeNo;
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ThrowIdentityFailure($"updating seed user '{email}'", result);
            }
        }

        await EnsureUserRole(userManager, user, role);
        return user;
    }

    private static async Task UpdateSeededCourseFeesAsync(ApplicationDbContext db)
    {
        var seededFees = new Dictionary<int, (decimal LegacyFee, decimal BahrainFee)>
        {
            [1] = (25000m, 95.00m),
            [2] = (35000m, 135.00m),
            [3] = (30000m, 120.00m),
            [4] = (22000m, 80.00m)
        };
        var seededCourseIds = seededFees.Keys.ToArray();

        var seededCourses = await db.Courses
            .Where(course => seededCourseIds.Contains(course.Id))
            .ToListAsync();

        foreach (var course in seededCourses)
        {
            var fees = seededFees[course.Id];
            if (course.EnrollmentFee == fees.LegacyFee)
            {
                course.EnrollmentFee = fees.BahrainFee;
            }
        }
    }

    private static async Task AcquireSeedLock(ApplicationDbContext db)
    {
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText =
            """
            DECLARE @result int;
            EXEC @result = sp_getapplock
                @Resource = 'TrainingCertificationPlatform:DbInitializer',
                @LockMode = 'Exclusive',
                @LockOwner = 'Session',
                @LockTimeout = 60000;
            SELECT @result;
            """;

        var lockResult = Convert.ToInt32(await command.ExecuteScalarAsync());

        if (lockResult < 0)
        {
            throw new TimeoutException("Timed out waiting for the database seed lock.");
        }
    }

    private static async Task ReleaseSeedLock(ApplicationDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            EXEC sp_releaseapplock
                @Resource = 'TrainingCertificationPlatform:DbInitializer',
                @LockOwner = 'Session';
            """);
    }

    private static async Task EnsureRole(RoleManager<IdentityRole> roleManager, string role)
    {
        if (await roleManager.RoleExistsAsync(role)) return;

        var result = await roleManager.CreateAsync(new IdentityRole(role));
        if (!result.Succeeded && !await roleManager.RoleExistsAsync(role))
        {
            ThrowIdentityFailure($"creating seed role '{role}'", result);
        }
    }

    private static async Task EnsureUserRole(UserManager<ApplicationUser> userManager, ApplicationUser user, string role)
    {
        if (await userManager.IsInRoleAsync(user, role)) return;

        try
        {
            var result = await userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded && !await userManager.IsInRoleAsync(user, role))
            {
                ThrowIdentityFailure($"adding seed user '{user.Email}' to role '{role}'", result);
            }
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
        {
            if (await userManager.IsInRoleAsync(user, role)) return;

            throw;
        }
    }

    private static bool IsDuplicateKeyException(DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException sqlException
            && sqlException.Errors.Cast<SqlError>().Any(error => error.Number is 2601 or 2627);
    }

    private static void ThrowIdentityFailure(string action, IdentityResult result)
    {
        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Identity seed failed while {action}: {errors}");
    }
}
