/*
    Training & Certification Platform database schema.
    Run this script first, then run seed.sql.
    Target DBMS: Microsoft SQL Server / SQL Server LocalDB.
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF DB_ID(N'TrainingCertificationPlatformDb') IS NULL
BEGIN
    CREATE DATABASE [TrainingCertificationPlatformDb];
END;
GO

USE [TrainingCertificationPlatformDb];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]', N'U') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

IF OBJECT_ID(N'[AspNetRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FullName] nvarchar(120) NOT NULL,
        [TraineeNumber] nvarchar(30) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[CourseCategories]', N'U') IS NULL
BEGIN
    CREATE TABLE [CourseCategories] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_CourseCategories] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[CertificationTracks]', N'U') IS NULL
BEGIN
    CREATE TABLE [CertificationTracks] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(120) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        CONSTRAINT [PK_CertificationTracks] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[Classrooms]', N'U') IS NULL
BEGIN
    CREATE TABLE [Classrooms] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [RoomName] nvarchar(100) NOT NULL,
        [Capacity] int NOT NULL,
        [Location] nvarchar(200) NOT NULL,
        CONSTRAINT [PK_Classrooms] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[AspNetRoleClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[AspNetUserClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[AspNetUserLogins]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[AspNetUserRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[AspNetUserTokens]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[Courses]', N'U') IS NULL
BEGIN
    CREATE TABLE [Courses] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Title] nvarchar(100) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [DurationDays] int NOT NULL,
        [Capacity] int NOT NULL,
        [EnrollmentFee] decimal(18,2) NOT NULL,
        [RequiredEquipment] nvarchar(300) NOT NULL,
        [IsActive] bit NOT NULL,
        [CourseCategoryId] int NOT NULL,
        CONSTRAINT [PK_Courses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Courses_CourseCategories_CourseCategoryId] FOREIGN KEY ([CourseCategoryId]) REFERENCES [CourseCategories] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[ClassroomEquipment]', N'U') IS NULL
BEGIN
    CREATE TABLE [ClassroomEquipment] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [ClassroomId] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_ClassroomEquipment] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClassroomEquipment_Classrooms_ClassroomId] FOREIGN KEY ([ClassroomId]) REFERENCES [Classrooms] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[InstructorProfiles]', N'U') IS NULL
BEGIN
    CREATE TABLE [InstructorProfiles] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [ApplicationUserId] nvarchar(450) NOT NULL,
        [FullName] nvarchar(120) NOT NULL,
        [ExpertiseAreas] nvarchar(250) NOT NULL,
        [Bio] nvarchar(1000) NOT NULL,
        CONSTRAINT [PK_InstructorProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InstructorProfiles_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[InstructorAvailabilities]', N'U') IS NULL
BEGIN
    CREATE TABLE [InstructorAvailabilities] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [InstructorProfileId] int NOT NULL,
        [DayOfWeek] int NOT NULL,
        [StartTime] time NOT NULL,
        [EndTime] time NOT NULL,
        CONSTRAINT [PK_InstructorAvailabilities] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InstructorAvailabilities_InstructorProfiles_InstructorProfileId] FOREIGN KEY ([InstructorProfileId]) REFERENCES [InstructorProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[InstructorExpertises]', N'U') IS NULL
BEGIN
    CREATE TABLE [InstructorExpertises] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [InstructorProfileId] int NOT NULL,
        [CourseCategoryId] int NOT NULL,
        CONSTRAINT [PK_InstructorExpertises] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InstructorExpertises_CourseCategories_CourseCategoryId] FOREIGN KEY ([CourseCategoryId]) REFERENCES [CourseCategories] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_InstructorExpertises_InstructorProfiles_InstructorProfileId] FOREIGN KEY ([InstructorProfileId]) REFERENCES [InstructorProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[CoursePrerequisites]', N'U') IS NULL
BEGIN
    CREATE TABLE [CoursePrerequisites] (
        [CourseId] int NOT NULL,
        [PrerequisiteCourseId] int NOT NULL,
        CONSTRAINT [PK_CoursePrerequisites] PRIMARY KEY ([CourseId], [PrerequisiteCourseId]),
        CONSTRAINT [FK_CoursePrerequisites_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoursePrerequisites_Courses_PrerequisiteCourseId] FOREIGN KEY ([PrerequisiteCourseId]) REFERENCES [Courses] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'[CertificationTrackCourses]', N'U') IS NULL
BEGIN
    CREATE TABLE [CertificationTrackCourses] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [CertificationTrackId] int NOT NULL,
        [CourseId] int NOT NULL,
        CONSTRAINT [PK_CertificationTrackCourses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CertificationTrackCourses_CertificationTracks_CertificationTrackId] FOREIGN KEY ([CertificationTrackId]) REFERENCES [CertificationTracks] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CertificationTrackCourses_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[CourseSessions]', N'U') IS NULL
BEGIN
    CREATE TABLE [CourseSessions] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [CourseId] int NOT NULL,
        [InstructorProfileId] int NOT NULL,
        [ClassroomId] int NOT NULL,
        [StartDateTime] datetime2 NOT NULL,
        [EndDateTime] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [Capacity] int NOT NULL,
        CONSTRAINT [PK_CourseSessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CourseSessions_Classrooms_ClassroomId] FOREIGN KEY ([ClassroomId]) REFERENCES [Classrooms] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CourseSessions_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CourseSessions_InstructorProfiles_InstructorProfileId] FOREIGN KEY ([InstructorProfileId]) REFERENCES [InstructorProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[Enrollments]', N'U') IS NULL
BEGIN
    CREATE TABLE [Enrollments] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [TraineeId] nvarchar(450) NOT NULL,
        [CourseSessionId] int NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [EnrolledAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Enrollments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Enrollments_AspNetUsers_TraineeId] FOREIGN KEY ([TraineeId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Enrollments_CourseSessions_CourseSessionId] FOREIGN KEY ([CourseSessionId]) REFERENCES [CourseSessions] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'[AssessmentResults]', N'U') IS NULL
BEGIN
    CREATE TABLE [AssessmentResults] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [EnrollmentId] int NOT NULL,
        [Result] nvarchar(max) NOT NULL,
        [Score] decimal(5,2) NOT NULL,
        [RecordedAt] datetime2 NOT NULL,
        [Remarks] nvarchar(500) NOT NULL,
        CONSTRAINT [PK_AssessmentResults] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AssessmentResults_Enrollments_EnrollmentId] FOREIGN KEY ([EnrollmentId]) REFERENCES [Enrollments] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[EnrollmentStatusHistories]', N'U') IS NULL
BEGIN
    CREATE TABLE [EnrollmentStatusHistories] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [EnrollmentId] int NOT NULL,
        [OldStatus] int NOT NULL,
        [NewStatus] int NOT NULL,
        [ChangedAt] datetime2 NOT NULL,
        [Notes] nvarchar(300) NOT NULL,
        CONSTRAINT [PK_EnrollmentStatusHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EnrollmentStatusHistories_Enrollments_EnrollmentId] FOREIGN KEY ([EnrollmentId]) REFERENCES [Enrollments] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[Payments]', N'U') IS NULL
BEGIN
    CREATE TABLE [Payments] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [EnrollmentId] int NOT NULL,
        [TraineeId] nvarchar(450) NOT NULL,
        [CourseSessionId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentDate] datetime2 NOT NULL,
        [Method] nvarchar(40) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [Notes] nvarchar(500) NOT NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payments_AspNetUsers_TraineeId] FOREIGN KEY ([TraineeId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Payments_CourseSessions_CourseSessionId] FOREIGN KEY ([CourseSessionId]) REFERENCES [CourseSessions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Payments_Enrollments_EnrollmentId] FOREIGN KEY ([EnrollmentId]) REFERENCES [Enrollments] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'[Certificates]', N'U') IS NULL
BEGIN
    CREATE TABLE [Certificates] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [TraineeId] nvarchar(450) NOT NULL,
        [CertificationTrackId] int NOT NULL,
        [ReferenceNumber] nvarchar(40) NOT NULL,
        [IssueDate] datetime2 NOT NULL,
        [IsRevoked] bit NOT NULL,
        CONSTRAINT [PK_Certificates] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Certificates_AspNetUsers_TraineeId] FOREIGN KEY ([TraineeId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Certificates_CertificationTracks_CertificationTrackId] FOREIGN KEY ([CertificationTrackId]) REFERENCES [CertificationTracks] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[TraineeCertifications]', N'U') IS NULL
BEGIN
    CREATE TABLE [TraineeCertifications] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [TraineeId] nvarchar(450) NOT NULL,
        [CertificationTrackId] int NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [IssuedAt] datetime2 NULL,
        [CertificateReferenceNumber] nvarchar(40) NULL,
        CONSTRAINT [PK_TraineeCertifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TraineeCertifications_AspNetUsers_TraineeId] FOREIGN KEY ([TraineeId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TraineeCertifications_CertificationTracks_CertificationTrackId] FOREIGN KEY ([CertificationTrackId]) REFERENCES [CertificationTracks] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[Notifications]', N'U') IS NULL
BEGIN
    CREATE TABLE [Notifications] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Title] nvarchar(120) NOT NULL,
        [Message] nvarchar(600) NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetRoleClaims_RoleId' AND object_id = OBJECT_ID(N'[AspNetRoleClaims]')) CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'RoleNameIndex' AND object_id = OBJECT_ID(N'[AspNetRoles]')) CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserClaims_UserId' AND object_id = OBJECT_ID(N'[AspNetUserClaims]')) CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserLogins_UserId' AND object_id = OBJECT_ID(N'[AspNetUserLogins]')) CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserRoles_RoleId' AND object_id = OBJECT_ID(N'[AspNetUserRoles]')) CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'EmailIndex' AND object_id = OBJECT_ID(N'[AspNetUsers]')) CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUsers_TraineeNumber' AND object_id = OBJECT_ID(N'[AspNetUsers]')) CREATE UNIQUE INDEX [IX_AspNetUsers_TraineeNumber] ON [AspNetUsers] ([TraineeNumber]) WHERE [TraineeNumber] <> '';
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UserNameIndex' AND object_id = OBJECT_ID(N'[AspNetUsers]')) CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AssessmentResults_EnrollmentId' AND object_id = OBJECT_ID(N'[AssessmentResults]')) CREATE UNIQUE INDEX [IX_AssessmentResults_EnrollmentId] ON [AssessmentResults] ([EnrollmentId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Certificates_CertificationTrackId' AND object_id = OBJECT_ID(N'[Certificates]')) CREATE INDEX [IX_Certificates_CertificationTrackId] ON [Certificates] ([CertificationTrackId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Certificates_ReferenceNumber' AND object_id = OBJECT_ID(N'[Certificates]')) CREATE UNIQUE INDEX [IX_Certificates_ReferenceNumber] ON [Certificates] ([ReferenceNumber]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Certificates_TraineeId' AND object_id = OBJECT_ID(N'[Certificates]')) CREATE INDEX [IX_Certificates_TraineeId] ON [Certificates] ([TraineeId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CertificationTrackCourses_CourseId' AND object_id = OBJECT_ID(N'[CertificationTrackCourses]')) CREATE INDEX [IX_CertificationTrackCourses_CourseId] ON [CertificationTrackCourses] ([CourseId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CertificationTrackCourses_CertificationTrackId_CourseId' AND object_id = OBJECT_ID(N'[CertificationTrackCourses]')) CREATE UNIQUE INDEX [IX_CertificationTrackCourses_CertificationTrackId_CourseId] ON [CertificationTrackCourses] ([CertificationTrackId], [CourseId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ClassroomEquipment_ClassroomId' AND object_id = OBJECT_ID(N'[ClassroomEquipment]')) CREATE INDEX [IX_ClassroomEquipment_ClassroomId] ON [ClassroomEquipment] ([ClassroomId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CoursePrerequisites_PrerequisiteCourseId' AND object_id = OBJECT_ID(N'[CoursePrerequisites]')) CREATE INDEX [IX_CoursePrerequisites_PrerequisiteCourseId] ON [CoursePrerequisites] ([PrerequisiteCourseId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Courses_CourseCategoryId' AND object_id = OBJECT_ID(N'[Courses]')) CREATE INDEX [IX_Courses_CourseCategoryId] ON [Courses] ([CourseCategoryId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CourseSessions_ClassroomId_StartDateTime_EndDateTime' AND object_id = OBJECT_ID(N'[CourseSessions]')) CREATE INDEX [IX_CourseSessions_ClassroomId_StartDateTime_EndDateTime] ON [CourseSessions] ([ClassroomId], [StartDateTime], [EndDateTime]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CourseSessions_CourseId' AND object_id = OBJECT_ID(N'[CourseSessions]')) CREATE INDEX [IX_CourseSessions_CourseId] ON [CourseSessions] ([CourseId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CourseSessions_InstructorProfileId_StartDateTime_EndDateTime' AND object_id = OBJECT_ID(N'[CourseSessions]')) CREATE INDEX [IX_CourseSessions_InstructorProfileId_StartDateTime_EndDateTime] ON [CourseSessions] ([InstructorProfileId], [StartDateTime], [EndDateTime]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Enrollments_CourseSessionId' AND object_id = OBJECT_ID(N'[Enrollments]')) CREATE INDEX [IX_Enrollments_CourseSessionId] ON [Enrollments] ([CourseSessionId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Enrollments_TraineeId_CourseSessionId' AND object_id = OBJECT_ID(N'[Enrollments]')) CREATE UNIQUE INDEX [IX_Enrollments_TraineeId_CourseSessionId] ON [Enrollments] ([TraineeId], [CourseSessionId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EnrollmentStatusHistories_EnrollmentId' AND object_id = OBJECT_ID(N'[EnrollmentStatusHistories]')) CREATE INDEX [IX_EnrollmentStatusHistories_EnrollmentId] ON [EnrollmentStatusHistories] ([EnrollmentId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_InstructorAvailabilities_InstructorProfileId' AND object_id = OBJECT_ID(N'[InstructorAvailabilities]')) CREATE INDEX [IX_InstructorAvailabilities_InstructorProfileId] ON [InstructorAvailabilities] ([InstructorProfileId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_InstructorExpertises_CourseCategoryId' AND object_id = OBJECT_ID(N'[InstructorExpertises]')) CREATE INDEX [IX_InstructorExpertises_CourseCategoryId] ON [InstructorExpertises] ([CourseCategoryId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_InstructorExpertises_InstructorProfileId_CourseCategoryId' AND object_id = OBJECT_ID(N'[InstructorExpertises]')) CREATE UNIQUE INDEX [IX_InstructorExpertises_InstructorProfileId_CourseCategoryId] ON [InstructorExpertises] ([InstructorProfileId], [CourseCategoryId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_InstructorProfiles_ApplicationUserId' AND object_id = OBJECT_ID(N'[InstructorProfiles]')) CREATE UNIQUE INDEX [IX_InstructorProfiles_ApplicationUserId] ON [InstructorProfiles] ([ApplicationUserId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Notifications_UserId' AND object_id = OBJECT_ID(N'[Notifications]')) CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Payments_CourseSessionId' AND object_id = OBJECT_ID(N'[Payments]')) CREATE INDEX [IX_Payments_CourseSessionId] ON [Payments] ([CourseSessionId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Payments_EnrollmentId' AND object_id = OBJECT_ID(N'[Payments]')) CREATE INDEX [IX_Payments_EnrollmentId] ON [Payments] ([EnrollmentId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Payments_TraineeId_CourseSessionId' AND object_id = OBJECT_ID(N'[Payments]')) CREATE INDEX [IX_Payments_TraineeId_CourseSessionId] ON [Payments] ([TraineeId], [CourseSessionId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TraineeCertifications_CertificateReferenceNumber' AND object_id = OBJECT_ID(N'[TraineeCertifications]')) CREATE UNIQUE INDEX [IX_TraineeCertifications_CertificateReferenceNumber] ON [TraineeCertifications] ([CertificateReferenceNumber]) WHERE [CertificateReferenceNumber] IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TraineeCertifications_CertificationTrackId' AND object_id = OBJECT_ID(N'[TraineeCertifications]')) CREATE INDEX [IX_TraineeCertifications_CertificationTrackId] ON [TraineeCertifications] ([CertificationTrackId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TraineeCertifications_TraineeId_CertificationTrackId' AND object_id = OBJECT_ID(N'[TraineeCertifications]')) CREATE UNIQUE INDEX [IX_TraineeCertifications_TraineeId_CertificationTrackId] ON [TraineeCertifications] ([TraineeId], [CertificationTrackId]);
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260517100928_InitialCreate')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260517100928_InitialCreate', N'8.0.11');
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260517193431_RevalidateBriefCRequirements')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260517193431_RevalidateBriefCRequirements', N'8.0.11');
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260518120000_CoverRequirementGaps')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260518120000_CoverRequirementGaps', N'8.0.11');
GO
