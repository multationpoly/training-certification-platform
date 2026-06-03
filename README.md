# Training & Certification Platform

University-level ASP.NET Core solution for managing courses, instructors, classrooms, enrollments, assessments, certifications, payments, notifications, and reporting.

## Solution Structure

- `TrainingCertification.API` - ASP.NET Core Web API, EF Core models, Identity, JWT, SignalR, seed data, business services, REST endpoints.
- `TrainingCertification.MVC` - Main MVC application used by trainees, instructors, and training coordinators. It uses the shared API project data layer directly. The public certificate lookup uses `HttpClient` only.
- `TrainingCertification.Reporting` - Razor Pages reporting portal. It has no EF Core DbContext and no API project reference. It logs in with JWT and calls API endpoints only.

## Demo Credentials

| Role | Email | Password |
| --- | --- | --- |
| TrainingCoordinator | tcp.coordinator@gmail.com | Admin@12345 |
| Instructor | tcp.instructor@outlook.com | Admin@12345 |
| Trainee | tcp.trainee@gmail.com | Admin@12345 |

## Main API Endpoints

| Method | Endpoint | Purpose | Auth |
| --- | --- | --- | --- |
| POST | `/api/auth/login` | JWT login for API clients and Reporting | Public |
| POST | `/api/auth/register` | Public trainee account registration | Public |
| GET | `/api/courses` | Active course catalog | JWT |
| GET | `/api/courses/{id}` | Course details with sessions | JWT |
| GET | `/api/courses/{id}/sessions` | Available upcoming course sessions | JWT |
| GET | `/api/sessions` | Secured session list | JWT |
| POST | `/api/sessions` | Schedule a validated course session | TrainingCoordinator JWT |
| POST | `/api/sessions/{id}/cancel` | Cancel a course session and notify users | TrainingCoordinator JWT |
| POST | `/api/enrollments` | Create enrollment after capacity/prerequisite checks | JWT |
| GET | `/api/enrollments/my` | Authenticated trainee enrollment list | Trainee JWT |
| GET | `/api/enrollments/session/{sessionId}` | Session trainee list | TrainingCoordinator/Instructor JWT |
| PUT | `/api/enrollments/{id}/status` | Enforce enrollment lifecycle transition | TrainingCoordinator/Instructor JWT |
| POST | `/api/enrollments/{id}/drop` | Drop enrollment | JWT |
| POST | `/api/enrollments/assessment-results` | Record pass/fail assessment result | TrainingCoordinator/Instructor JWT |
| GET | `/api/enrollments/sessions/{sessionId}/remaining-seats` | Live/public remaining seat count | Public |
| GET | `/api/public/certification` | Public certification lookup used by MVC `HttpClient` | Public |
| GET | `/api/certificates/verify` | Public certificate verification | Public |
| POST | `/api/certificates/generate` | Generate eligible certificate | TrainingCoordinator JWT |
| GET | `/api/reports/enrollment-stats` | Enrollment statistics | TrainingCoordinator JWT |
| GET | `/api/reports/instructor-workload` | Instructor workload | TrainingCoordinator JWT |
| GET | `/api/reports/certification-completion` | Certification completion | TrainingCoordinator JWT |
| GET | `/api/reports/revenue` | Revenue and outstanding balances | TrainingCoordinator JWT |

Swagger is enabled in development at `/swagger`.

## Setup Instructions

1. Open `TrainingCertificationPlatform.sln` in Visual Studio.
2. Confirm SQL Server LocalDB is installed, or change `DefaultConnection` in API and MVC `appsettings.json`.
3. Restore NuGet packages.
4. In Package Manager Console, select `TrainingCertification.API` and run:
   ```powershell
   Add-Migration InitialCreate -Project TrainingCertification.API -StartupProject TrainingCertification.API
   Update-Database -Project TrainingCertification.API -StartupProject TrainingCertification.API
   ```
5. Run the API once to seed roles, demo users, courses, classrooms, sessions, payments, and certification tracks.
6. Configure multiple startup projects:
   - `TrainingCertification.API`
   - `TrainingCertification.MVC`
   - `TrainingCertification.Reporting`
7. Run all three projects. Login to MVC with the demo credentials. Login to Reporting with the coordinator account.

## ERD Explanation

Courses belong to categories and may require prerequisite courses through `CoursePrerequisite`. Instructors are Identity users with `InstructorProfile` and weekly `InstructorAvailability`. `CourseSession` joins a course, instructor, classroom, and time range. `Enrollment` joins trainees to sessions and records status history, payments, and assessment result. Certification tracks contain required courses; when a trainee passes all required courses, the system generates a `Certificate` with a unique reference number. Notifications belong to users and are created for enrollment, payment, schedule, and certification events.

## Azure Deployment

1. Create Azure SQL Database and copy its connection string.
2. Create three Azure App Services, one for each project.
3. In API and MVC App Service configuration, set `ConnectionStrings__DefaultConnection` to the Azure SQL connection string.
4. In MVC and Reporting App Service configuration, set `ApiSettings__BaseUrl` to the deployed API URL.
5. In API App Service configuration, set `JwtSettings__Key` to a strong secret value and keep the same issuer/audience in Reporting.
6. Publish the API first, run migrations against Azure SQL, then publish MVC and Reporting.

## Database Scripts

- `Database/schema.sql` creates the SQL Server database schema, including ASP.NET Identity tables, constraints, relationships, indexes, and EF migration history rows.
- `Database/seed.sql` seeds lookup/demo data and Identity roles/users for each role. All seeded demo accounts use `Admin@12345`.
- Identity and application data share one SQL Server database through `ApplicationDbContext`, so there is no separate Identity database script.

## Live Deployment

The platform is deployed on Microsoft Azure across three separate App Services.

| Project | URL |
|---------|-----|
| API | https://training-cert-api-hrbpeaach4h8b5e6.westeurope-01.azurewebsites.net |
| MVC | https://training-cert-mvc-hffreseeasa6cqbh.westeurope-01.azurewebsites.net |
| Reporting | https://training-cert-reporting-cnejd8haereqcsht.westeurope-01.azurewebsites.net |

All three services are hosted in the **West Europe** region under a shared resource group.
The database is an **Azure SQL Database** with migrations applied against the live instance.
Environment variables for connection strings, JWT settings, and API base URLs are configured
directly in each App Service — no secrets are stored in the repository.

Use the demo credentials above to log in.
