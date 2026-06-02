namespace TrainingCertification.Reporting.DTOs;

public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Email, string FullName, IList<string> Roles);
public record EnrollmentReportDto(string CourseTitle, string Category, int TotalEnrollments);
public record WorkloadReportDto(string InstructorName, int SessionsAssigned, int TotalTrainees, decimal CompletionRate);
public record CertificationReportDto(string TrackName, int TotalEligible, int TotalIssued, int InProgress);
public record RevenueReportDto(int SessionId, string CourseTitle, decimal Fee, decimal TotalCollected, decimal Outstanding, bool IsOverdue);
