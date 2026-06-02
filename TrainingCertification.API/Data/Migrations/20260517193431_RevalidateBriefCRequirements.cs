using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrainingCertification.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RevalidateBriefCRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_AspNetUsers_TraineeId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Enrollments_EnrollmentId",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CertificationTrackCourses",
                table: "CertificationTrackCourses");

            migrationBuilder.DeleteData(
                table: "CertificationTrackCourses",
                keyColumns: new[] { "CertificationTrackId", "CourseId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "CertificationTrackCourses",
                keyColumns: new[] { "CertificationTrackId", "CourseId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "CertificationTrackCourses",
                keyColumns: new[] { "CertificationTrackId", "CourseId" },
                keyValues: new object[] { 1, 3 });

            migrationBuilder.DeleteData(
                table: "CertificationTrackCourses",
                keyColumns: new[] { "CertificationTrackId", "CourseId" },
                keyValues: new object[] { 2, 4 });

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AddColumn<int>(
                name: "CourseSessionId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TraineeId",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "InstructorProfiles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "InstructorProfiles",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Enrollments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CourseSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Scheduled");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Courses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<decimal>(
                name: "EnrollmentFee",
                table: "Courses",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Courses",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1200)",
                oldMaxLength: 1200);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CourseCategories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);

            migrationBuilder.AlterColumn<string>(
                name: "RoomName",
                table: "Classrooms",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Classrooms",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CertificationTracks",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "CertificationTrackCourses",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "AssessmentResults",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql(
                """
                UPDATE [Payments]
                SET [Status] = CASE [Status]
                    WHEN '0' THEN 'Partial'
                    WHEN '1' THEN 'Paid'
                    WHEN '2' THEN 'Overdue'
                    ELSE [Status]
                END;

                UPDATE [Notifications]
                SET [Type] = CASE [Type]
                    WHEN '0' THEN 'EnrollmentConfirmation'
                    WHEN '1' THEN 'EnrollmentStatusChanged'
                    WHEN '2' THEN 'ScheduleChange'
                    WHEN '3' THEN 'AssessmentRecorded'
                    WHEN '4' THEN 'PaymentReminder'
                    WHEN '5' THEN 'CertificationCompletion'
                    WHEN '6' THEN 'CertificationEligible'
                    WHEN '7' THEN 'NewSessionScheduled'
                    ELSE [Type]
                END;

                UPDATE [Enrollments]
                SET [Status] = CASE [Status]
                    WHEN '0' THEN 'Enrolled'
                    WHEN '1' THEN 'Confirmed'
                    WHEN '2' THEN 'Attending'
                    WHEN '3' THEN 'Completed'
                    WHEN '4' THEN 'Dropped'
                    ELSE [Status]
                END;

                UPDATE [AssessmentResults]
                SET [Result] = CASE [Result]
                    WHEN '0' THEN 'Pending'
                    WHEN '1' THEN 'Pass'
                    WHEN '2' THEN 'Fail'
                    ELSE [Result]
                END;

                UPDATE [CourseSessions]
                SET [Status] = 'Scheduled'
                WHERE [Status] IS NULL OR [Status] = '';

                UPDATE p
                SET p.[TraineeId] = e.[TraineeId],
                    p.[CourseSessionId] = e.[CourseSessionId]
                FROM [Payments] p
                INNER JOIN [Enrollments] e ON e.[Id] = p.[EnrollmentId]
                WHERE p.[TraineeId] = '' OR p.[CourseSessionId] = 0;
                """);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CertificationTrackCourses",
                table: "CertificationTrackCourses",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "InstructorExpertises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstructorProfileId = table.Column<int>(type: "int", nullable: false),
                    CourseCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructorExpertises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstructorExpertises_CourseCategories_CourseCategoryId",
                        column: x => x.CourseCategoryId,
                        principalTable: "CourseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstructorExpertises_InstructorProfiles_InstructorProfileId",
                        column: x => x.InstructorProfileId,
                        principalTable: "InstructorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TraineeCertifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TraineeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CertificationTrackId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CertificateReferenceNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraineeCertifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TraineeCertifications_AspNetUsers_TraineeId",
                        column: x => x.TraineeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TraineeCertifications_CertificationTracks_CertificationTrackId",
                        column: x => x.CertificationTrackId,
                        principalTable: "CertificationTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CertificationTrackCourses",
                columns: new[] { "Id", "CertificationTrackId", "CourseId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 1, 2 },
                    { 3, 1, 3 },
                    { 4, 2, 4 }
                });

            migrationBuilder.UpdateData(
                table: "Classrooms",
                keyColumn: "Id",
                keyValue: 1,
                column: "Location",
                value: "Main Campus");

            migrationBuilder.UpdateData(
                table: "Classrooms",
                keyColumn: "Id",
                keyValue: 2,
                column: "Location",
                value: "Training Wing");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CourseSessionId",
                table: "Payments",
                column: "CourseSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TraineeId_CourseSessionId",
                table: "Payments",
                columns: new[] { "TraineeId", "CourseSessionId" });

            migrationBuilder.CreateIndex(
                name: "IX_CertificationTrackCourses_CertificationTrackId_CourseId",
                table: "CertificationTrackCourses",
                columns: new[] { "CertificationTrackId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstructorExpertises_CourseCategoryId",
                table: "InstructorExpertises",
                column: "CourseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorExpertises_InstructorProfileId_CourseCategoryId",
                table: "InstructorExpertises",
                columns: new[] { "InstructorProfileId", "CourseCategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TraineeCertifications_CertificateReferenceNumber",
                table: "TraineeCertifications",
                column: "CertificateReferenceNumber",
                unique: true,
                filter: "[CertificateReferenceNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeCertifications_CertificationTrackId",
                table: "TraineeCertifications",
                column: "CertificationTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeCertifications_TraineeId_CertificationTrackId",
                table: "TraineeCertifications",
                columns: new[] { "TraineeId", "CertificationTrackId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_AspNetUsers_TraineeId",
                table: "Enrollments",
                column: "TraineeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_TraineeId",
                table: "Payments",
                column: "TraineeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_CourseSessions_CourseSessionId",
                table: "Payments",
                column: "CourseSessionId",
                principalTable: "CourseSessions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Enrollments_EnrollmentId",
                table: "Payments",
                column: "EnrollmentId",
                principalTable: "Enrollments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_AspNetUsers_TraineeId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_TraineeId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_CourseSessions_CourseSessionId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Enrollments_EnrollmentId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "InstructorExpertises");

            migrationBuilder.DropTable(
                name: "TraineeCertifications");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CourseSessionId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_TraineeId_CourseSessionId",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CertificationTrackCourses",
                table: "CertificationTrackCourses");

            migrationBuilder.DropIndex(
                name: "IX_CertificationTrackCourses_CertificationTrackId_CourseId",
                table: "CertificationTrackCourses");

            migrationBuilder.DeleteData(
                table: "CertificationTrackCourses",
                keyColumn: "Id",
                keyColumnType: "int",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CertificationTrackCourses",
                keyColumn: "Id",
                keyColumnType: "int",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CertificationTrackCourses",
                keyColumn: "Id",
                keyColumnType: "int",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "CertificationTrackCourses",
                keyColumn: "Id",
                keyColumnType: "int",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "CourseSessionId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TraineeId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "InstructorProfiles");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CourseSessions");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Classrooms");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CertificationTrackCourses");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Payments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payments",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Notifications",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "InstructorProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Enrollments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Courses",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<decimal>(
                name: "EnrollmentFee",
                table: "Courses",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Courses",
                type: "nvarchar(1200)",
                maxLength: 1200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CourseCategories",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "RoomName",
                table: "Classrooms",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CertificationTracks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<int>(
                name: "Result",
                table: "AssessmentResults",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CertificationTrackCourses",
                table: "CertificationTrackCourses",
                columns: new[] { "CertificationTrackId", "CourseId" });

            migrationBuilder.InsertData(
                table: "CertificationTrackCourses",
                columns: new[] { "CertificationTrackId", "CourseId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 1, 3 },
                    { 2, 4 }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_AspNetUsers_TraineeId",
                table: "Enrollments",
                column: "TraineeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Enrollments_EnrollmentId",
                table: "Payments",
                column: "EnrollmentId",
                principalTable: "Enrollments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
