using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrainingCertification.Reporting.DTOs;
using TrainingCertification.Reporting.ViewModels;
using TrainingCertification.Reporting.Services;

namespace TrainingCertification.Reporting.Pages;

public class DashboardModel : PageModel
{
    private readonly ApiReportClient _client;

    public DashboardModel(ApiReportClient client) => _client = client;

    public DashboardViewModel Dashboard { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        Dashboard.CoordinatorName = _client.CoordinatorName;

        var firstDay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var enrollmentResult = await _client.GetAsync<List<EnrollmentReportDto>>(
            $"api/reports/enrollment-stats?{ApiReportClient.DateRangeQuery(firstDay, DateTime.Today)}");
        var certificationResult = await _client.GetAsync<List<CertificationReportDto>>("api/reports/certification-completion");
        var revenueResult = await _client.GetAsync<List<RevenueReportDto>>("api/reports/revenue");

        if (enrollmentResult.IsUnauthorized || certificationResult.IsUnauthorized || revenueResult.IsUnauthorized)
        {
            return RedirectToPage("/Index", new { message = ReportingMessages.SessionExpired });
        }

        if (enrollmentResult.IsSuccess && enrollmentResult.Data is not null)
        {
            Dashboard.TotalActiveCourses = enrollmentResult.Data.Select(row => row.CourseTitle).Distinct().Count();
            Dashboard.TotalEnrollmentsThisMonth = enrollmentResult.Data.Sum(row => row.TotalEnrollments);
        }

        if (certificationResult.IsSuccess && certificationResult.Data is not null)
        {
            Dashboard.TotalCertificationsIssued = certificationResult.Data.Sum(row => row.TotalIssued);
        }

        if (revenueResult.IsSuccess && revenueResult.Data is not null)
        {
            Dashboard.TotalOutstandingPaymentBalance = revenueResult.Data.Sum(row => row.Outstanding);
        }

        Dashboard.ErrorMessage = enrollmentResult.ErrorMessage ?? certificationResult.ErrorMessage ?? revenueResult.ErrorMessage;
        return Page();
    }
}
