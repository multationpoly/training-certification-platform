/*
    Training & Certification Platform seed data.
    Run after schema.sql.

    Demo account password for all seeded users: Admin@12345
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE [TrainingCertificationPlatformDb];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @PasswordHash nvarchar(max) = N'AQAAAAIAAYagAAAAEAARIjNEVWZ3iJmqu8zd7v+kMK6k7aweKuH8iwj01sUtLp9Ilcb+In1q6Ffz3FLxNg==';

IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Id] = N'role-trainee')
    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (N'role-trainee', N'Trainee', N'TRAINEE', N'role-trainee-stamp');

IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Id] = N'role-instructor')
    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (N'role-instructor', N'Instructor', N'INSTRUCTOR', N'role-instructor-stamp');

IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Id] = N'role-training-coordinator')
    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (N'role-training-coordinator', N'TrainingCoordinator', N'TRAININGCOORDINATOR', N'role-training-coordinator-stamp');

IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = N'user-coordinator')
    INSERT INTO [AspNetUsers] (
        [Id], [FullName], [TraineeNumber], [CreatedAt], [UserName], [NormalizedUserName],
        [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp],
        [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled],
        [LockoutEnd], [LockoutEnabled], [AccessFailedCount])
    VALUES (
        N'user-coordinator', N'Fatima Al Khalifa', N'TC-000', SYSUTCDATETIME(), N'tcp.coordinator@gmail.com', N'TCP.COORDINATOR@GMAIL.COM',
        N'tcp.coordinator@gmail.com', N'TCP.COORDINATOR@GMAIL.COM', 1, @PasswordHash, N'security-coordinator',
        N'concurrency-coordinator', NULL, 0, 0, NULL, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = N'user-instructor-1')
    INSERT INTO [AspNetUsers] (
        [Id], [FullName], [TraineeNumber], [CreatedAt], [UserName], [NormalizedUserName],
        [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp],
        [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled],
        [LockoutEnd], [LockoutEnabled], [AccessFailedCount])
    VALUES (
        N'user-instructor-1', N'Noor Al Zayani', N'', SYSUTCDATETIME(), N'tcp.instructor@outlook.com', N'TCP.INSTRUCTOR@OUTLOOK.COM',
        N'tcp.instructor@outlook.com', N'TCP.INSTRUCTOR@OUTLOOK.COM', 1, @PasswordHash, N'security-instructor-1',
        N'concurrency-instructor-1', NULL, 0, 0, NULL, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = N'user-instructor-2')
    INSERT INTO [AspNetUsers] (
        [Id], [FullName], [TraineeNumber], [CreatedAt], [UserName], [NormalizedUserName],
        [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp],
        [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled],
        [LockoutEnd], [LockoutEnabled], [AccessFailedCount])
    VALUES (
        N'user-instructor-2', N'Salman Al Mahmood', N'', SYSUTCDATETIME(), N'tcp.instructor2@outlook.com', N'TCP.INSTRUCTOR2@OUTLOOK.COM',
        N'tcp.instructor2@outlook.com', N'TCP.INSTRUCTOR2@OUTLOOK.COM', 1, @PasswordHash, N'security-instructor-2',
        N'concurrency-instructor-2', NULL, 0, 0, NULL, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = N'user-trainee-1')
    INSERT INTO [AspNetUsers] (
        [Id], [FullName], [TraineeNumber], [CreatedAt], [UserName], [NormalizedUserName],
        [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp],
        [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled],
        [LockoutEnd], [LockoutEnabled], [AccessFailedCount])
    VALUES (
        N'user-trainee-1', N'Ali Al Haddad', N'TR-1001', SYSUTCDATETIME(), N'tcp.trainee@gmail.com', N'TCP.TRAINEE@GMAIL.COM',
        N'tcp.trainee@gmail.com', N'TCP.TRAINEE@GMAIL.COM', 1, @PasswordHash, N'security-trainee-1',
        N'concurrency-trainee-1', NULL, 0, 0, NULL, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = N'user-trainee-2')
    INSERT INTO [AspNetUsers] (
        [Id], [FullName], [TraineeNumber], [CreatedAt], [UserName], [NormalizedUserName],
        [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp],
        [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled],
        [LockoutEnd], [LockoutEnabled], [AccessFailedCount])
    VALUES (
        N'user-trainee-2', N'Mariam Al Jalahma', N'TR-1002', SYSUTCDATETIME(), N'tcp.trainee2@gmail.com', N'TCP.TRAINEE2@GMAIL.COM',
        N'tcp.trainee2@gmail.com', N'TCP.TRAINEE2@GMAIL.COM', 1, @PasswordHash, N'security-trainee-2',
        N'concurrency-trainee-2', NULL, 0, 0, NULL, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = N'user-coordinator' AND [RoleId] = N'role-training-coordinator')
    INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'user-coordinator', N'role-training-coordinator');
IF NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = N'user-instructor-1' AND [RoleId] = N'role-instructor')
    INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'user-instructor-1', N'role-instructor');
IF NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = N'user-instructor-2' AND [RoleId] = N'role-instructor')
    INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'user-instructor-2', N'role-instructor');
IF NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = N'user-trainee-1' AND [RoleId] = N'role-trainee')
    INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'user-trainee-1', N'role-trainee');
IF NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = N'user-trainee-2' AND [RoleId] = N'role-trainee')
    INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'user-trainee-2', N'role-trainee');
GO

SET IDENTITY_INSERT [CourseCategories] ON;
IF NOT EXISTS (SELECT 1 FROM [CourseCategories] WHERE [Id] = 1) INSERT INTO [CourseCategories] ([Id], [Name]) VALUES (1, N'Software Development');
IF NOT EXISTS (SELECT 1 FROM [CourseCategories] WHERE [Id] = 2) INSERT INTO [CourseCategories] ([Id], [Name]) VALUES (2, N'Cloud Computing');
IF NOT EXISTS (SELECT 1 FROM [CourseCategories] WHERE [Id] = 3) INSERT INTO [CourseCategories] ([Id], [Name]) VALUES (3, N'Project Management');
SET IDENTITY_INSERT [CourseCategories] OFF;

SET IDENTITY_INSERT [Courses] ON;
IF NOT EXISTS (SELECT 1 FROM [Courses] WHERE [Id] = 1) INSERT INTO [Courses] ([Id], [Title], [Description], [DurationDays], [Capacity], [EnrollmentFee], [RequiredEquipment], [IsActive], [CourseCategoryId]) VALUES (1, N'C# Fundamentals', N'Programming basics using C# and .NET.', 5, 25, 95.00, N'', 1, 1);
IF NOT EXISTS (SELECT 1 FROM [Courses] WHERE [Id] = 2) INSERT INTO [Courses] ([Id], [Title], [Description], [DurationDays], [Capacity], [EnrollmentFee], [RequiredEquipment], [IsActive], [CourseCategoryId]) VALUES (2, N'ASP.NET Core MVC', N'Build web applications using MVC and Razor Views.', 7, 25, 135.00, N'Projector, Workstations', 1, 1);
IF NOT EXISTS (SELECT 1 FROM [Courses] WHERE [Id] = 3) INSERT INTO [Courses] ([Id], [Title], [Description], [DurationDays], [Capacity], [EnrollmentFee], [RequiredEquipment], [IsActive], [CourseCategoryId]) VALUES (3, N'Azure Essentials', N'Deploy applications and databases to Azure.', 4, 20, 120.00, N'Projector', 1, 2);
IF NOT EXISTS (SELECT 1 FROM [Courses] WHERE [Id] = 4) INSERT INTO [Courses] ([Id], [Title], [Description], [DurationDays], [Capacity], [EnrollmentFee], [RequiredEquipment], [IsActive], [CourseCategoryId]) VALUES (4, N'Agile Project Management', N'Scrum, planning, tracking, and team delivery.', 3, 30, 80.00, N'', 1, 3);
SET IDENTITY_INSERT [Courses] OFF;

IF NOT EXISTS (SELECT 1 FROM [CoursePrerequisites] WHERE [CourseId] = 2 AND [PrerequisiteCourseId] = 1)
    INSERT INTO [CoursePrerequisites] ([CourseId], [PrerequisiteCourseId]) VALUES (2, 1);

SET IDENTITY_INSERT [Classrooms] ON;
IF NOT EXISTS (SELECT 1 FROM [Classrooms] WHERE [Id] = 1) INSERT INTO [Classrooms] ([Id], [RoomName], [Capacity], [Location]) VALUES (1, N'Lab A', 25, N'Main Campus');
IF NOT EXISTS (SELECT 1 FROM [Classrooms] WHERE [Id] = 2) INSERT INTO [Classrooms] ([Id], [RoomName], [Capacity], [Location]) VALUES (2, N'Room 204', 30, N'Training Wing');
SET IDENTITY_INSERT [Classrooms] OFF;

SET IDENTITY_INSERT [ClassroomEquipment] ON;
IF NOT EXISTS (SELECT 1 FROM [ClassroomEquipment] WHERE [Id] = 1) INSERT INTO [ClassroomEquipment] ([Id], [ClassroomId], [Name]) VALUES (1, 1, N'Projector');
IF NOT EXISTS (SELECT 1 FROM [ClassroomEquipment] WHERE [Id] = 2) INSERT INTO [ClassroomEquipment] ([Id], [ClassroomId], [Name]) VALUES (2, 1, N'25 Workstations');
IF NOT EXISTS (SELECT 1 FROM [ClassroomEquipment] WHERE [Id] = 3) INSERT INTO [ClassroomEquipment] ([Id], [ClassroomId], [Name]) VALUES (3, 2, N'Whiteboard');
SET IDENTITY_INSERT [ClassroomEquipment] OFF;

SET IDENTITY_INSERT [CertificationTracks] ON;
IF NOT EXISTS (SELECT 1 FROM [CertificationTracks] WHERE [Id] = 1) INSERT INTO [CertificationTracks] ([Id], [Name], [Description]) VALUES (1, N'Full Stack .NET Developer', N'C#, MVC, and Azure certification path.');
IF NOT EXISTS (SELECT 1 FROM [CertificationTracks] WHERE [Id] = 2) INSERT INTO [CertificationTracks] ([Id], [Name], [Description]) VALUES (2, N'Agile Practitioner', N'Project delivery and agile practices.');
SET IDENTITY_INSERT [CertificationTracks] OFF;

SET IDENTITY_INSERT [CertificationTrackCourses] ON;
IF NOT EXISTS (SELECT 1 FROM [CertificationTrackCourses] WHERE [Id] = 1) INSERT INTO [CertificationTrackCourses] ([Id], [CertificationTrackId], [CourseId]) VALUES (1, 1, 1);
IF NOT EXISTS (SELECT 1 FROM [CertificationTrackCourses] WHERE [Id] = 2) INSERT INTO [CertificationTrackCourses] ([Id], [CertificationTrackId], [CourseId]) VALUES (2, 1, 2);
IF NOT EXISTS (SELECT 1 FROM [CertificationTrackCourses] WHERE [Id] = 3) INSERT INTO [CertificationTrackCourses] ([Id], [CertificationTrackId], [CourseId]) VALUES (3, 1, 3);
IF NOT EXISTS (SELECT 1 FROM [CertificationTrackCourses] WHERE [Id] = 4) INSERT INTO [CertificationTrackCourses] ([Id], [CertificationTrackId], [CourseId]) VALUES (4, 2, 4);
SET IDENTITY_INSERT [CertificationTrackCourses] OFF;
GO

SET IDENTITY_INSERT [InstructorProfiles] ON;
IF NOT EXISTS (SELECT 1 FROM [InstructorProfiles] WHERE [Id] = 1)
    INSERT INTO [InstructorProfiles] ([Id], [ApplicationUserId], [FullName], [ExpertiseAreas], [Bio])
    VALUES (1, N'user-instructor-1', N'Noor Al Zayani', N'C#, ASP.NET Core, Azure', N'Senior trainer with practical web development experience.');
IF NOT EXISTS (SELECT 1 FROM [InstructorProfiles] WHERE [Id] = 2)
    INSERT INTO [InstructorProfiles] ([Id], [ApplicationUserId], [FullName], [ExpertiseAreas], [Bio])
    VALUES (2, N'user-instructor-2', N'Salman Al Mahmood', N'Agile, project delivery', N'Agile trainer with software team coaching experience.');
SET IDENTITY_INSERT [InstructorProfiles] OFF;

SET IDENTITY_INSERT [InstructorExpertises] ON;
IF NOT EXISTS (SELECT 1 FROM [InstructorExpertises] WHERE [Id] = 1) INSERT INTO [InstructorExpertises] ([Id], [InstructorProfileId], [CourseCategoryId]) VALUES (1, 1, 1);
IF NOT EXISTS (SELECT 1 FROM [InstructorExpertises] WHERE [Id] = 2) INSERT INTO [InstructorExpertises] ([Id], [InstructorProfileId], [CourseCategoryId]) VALUES (2, 1, 2);
IF NOT EXISTS (SELECT 1 FROM [InstructorExpertises] WHERE [Id] = 3) INSERT INTO [InstructorExpertises] ([Id], [InstructorProfileId], [CourseCategoryId]) VALUES (3, 2, 3);
SET IDENTITY_INSERT [InstructorExpertises] OFF;

SET IDENTITY_INSERT [InstructorAvailabilities] ON;
IF NOT EXISTS (SELECT 1 FROM [InstructorAvailabilities] WHERE [Id] = 1) INSERT INTO [InstructorAvailabilities] ([Id], [InstructorProfileId], [DayOfWeek], [StartTime], [EndTime]) VALUES (1, 1, 1, '09:00:00', '17:00:00');
IF NOT EXISTS (SELECT 1 FROM [InstructorAvailabilities] WHERE [Id] = 2) INSERT INTO [InstructorAvailabilities] ([Id], [InstructorProfileId], [DayOfWeek], [StartTime], [EndTime]) VALUES (2, 1, 3, '09:00:00', '17:00:00');
IF NOT EXISTS (SELECT 1 FROM [InstructorAvailabilities] WHERE [Id] = 3) INSERT INTO [InstructorAvailabilities] ([Id], [InstructorProfileId], [DayOfWeek], [StartTime], [EndTime]) VALUES (3, 2, 2, '09:00:00', '17:00:00');
SET IDENTITY_INSERT [InstructorAvailabilities] OFF;

SET IDENTITY_INSERT [CourseSessions] ON;
IF NOT EXISTS (SELECT 1 FROM [CourseSessions] WHERE [Id] = 1)
    INSERT INTO [CourseSessions] ([Id], [CourseId], [InstructorProfileId], [ClassroomId], [StartDateTime], [EndDateTime], [Status], [Capacity])
    VALUES (1, 1, 1, 1, '2026-06-01T09:00:00', '2026-06-05T13:00:00', N'Scheduled', 25);
IF NOT EXISTS (SELECT 1 FROM [CourseSessions] WHERE [Id] = 2)
    INSERT INTO [CourseSessions] ([Id], [CourseId], [InstructorProfileId], [ClassroomId], [StartDateTime], [EndDateTime], [Status], [Capacity])
    VALUES (2, 2, 1, 1, '2026-06-08T09:00:00', '2026-06-14T13:00:00', N'Scheduled', 25);
IF NOT EXISTS (SELECT 1 FROM [CourseSessions] WHERE [Id] = 3)
    INSERT INTO [CourseSessions] ([Id], [CourseId], [InstructorProfileId], [ClassroomId], [StartDateTime], [EndDateTime], [Status], [Capacity])
    VALUES (3, 4, 2, 2, '2026-06-02T09:00:00', '2026-06-04T13:00:00', N'Scheduled', 30);
SET IDENTITY_INSERT [CourseSessions] OFF;
GO

SET IDENTITY_INSERT [Enrollments] ON;
IF NOT EXISTS (SELECT 1 FROM [Enrollments] WHERE [Id] = 1)
    INSERT INTO [Enrollments] ([Id], [TraineeId], [CourseSessionId], [Status], [EnrolledAt])
    VALUES (1, N'user-trainee-1', 1, N'Completed', '2026-05-18T09:00:00');
IF NOT EXISTS (SELECT 1 FROM [Enrollments] WHERE [Id] = 2)
    INSERT INTO [Enrollments] ([Id], [TraineeId], [CourseSessionId], [Status], [EnrolledAt])
    VALUES (2, N'user-trainee-2', 2, N'Confirmed', '2026-05-18T09:15:00');
SET IDENTITY_INSERT [Enrollments] OFF;

SET IDENTITY_INSERT [AssessmentResults] ON;
IF NOT EXISTS (SELECT 1 FROM [AssessmentResults] WHERE [Id] = 1)
    INSERT INTO [AssessmentResults] ([Id], [EnrollmentId], [Result], [Score], [RecordedAt], [Remarks])
    VALUES (1, 1, N'Pass', 86.00, '2026-05-18T10:00:00', N'Good practical work.');
SET IDENTITY_INSERT [AssessmentResults] OFF;

SET IDENTITY_INSERT [Payments] ON;
IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [Id] = 1)
    INSERT INTO [Payments] ([Id], [EnrollmentId], [TraineeId], [CourseSessionId], [Amount], [PaymentDate], [Method], [Status], [Notes])
    VALUES (1, 1, N'user-trainee-1', 1, 95.00, '2026-05-18T10:05:00', N'Bank Transfer', N'Paid', N'Demo full payment.');
IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [Id] = 2)
    INSERT INTO [Payments] ([Id], [EnrollmentId], [TraineeId], [CourseSessionId], [Amount], [PaymentDate], [Method], [Status], [Notes])
    VALUES (2, 2, N'user-trainee-2', 2, 65.00, '2026-05-18T10:10:00', N'Cash', N'Partial', N'Demo partial payment.');
SET IDENTITY_INSERT [Payments] OFF;

SET IDENTITY_INSERT [TraineeCertifications] ON;
IF NOT EXISTS (SELECT 1 FROM [TraineeCertifications] WHERE [Id] = 1)
    INSERT INTO [TraineeCertifications] ([Id], [TraineeId], [CertificationTrackId], [Status], [IssuedAt], [CertificateReferenceNumber])
    VALUES (1, N'user-trainee-1', 1, N'InProgress', NULL, NULL);
SET IDENTITY_INSERT [TraineeCertifications] OFF;

SET IDENTITY_INSERT [Notifications] ON;
IF NOT EXISTS (SELECT 1 FROM [Notifications] WHERE [Id] = 1)
    INSERT INTO [Notifications] ([Id], [UserId], [Title], [Message], [Type], [IsRead], [CreatedAt])
    VALUES (1, N'user-trainee-1', N'Enrollment confirmed', N'Your enrollment in C# Fundamentals is confirmed.', N'EnrollmentConfirmation', 0, '2026-05-18T10:15:00');
IF NOT EXISTS (SELECT 1 FROM [Notifications] WHERE [Id] = 2)
    INSERT INTO [Notifications] ([Id], [UserId], [Title], [Message], [Type], [IsRead], [CreatedAt])
    VALUES (2, N'user-trainee-2', N'Payment reminder', N'Your ASP.NET Core MVC enrollment has an outstanding balance.', N'PaymentReminder', 0, '2026-05-18T10:20:00');
SET IDENTITY_INSERT [Notifications] OFF;
GO
