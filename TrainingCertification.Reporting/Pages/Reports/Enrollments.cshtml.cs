using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrainingCertification.Reporting.DTOs;
using TrainingCertification.Reporting.Services;
using TrainingCertification.Reporting.ViewModels;

namespace TrainingCertification.Reporting.Pages.Reports;

public class EnrollmentsModel : PageModel
{
    private readonly ApiReportClient _client;

    public EnrollmentsModel(ApiReportClient client) => _client = client;

    public EnrollmentReportViewModel Report { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(DateTime? from, DateTime? to)
    {
        var defaultFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        Report.From = from ?? defaultFrom;
        Report.To = to ?? DateTime.Today;

        var result = await _client.GetAsync<List<EnrollmentReportDto>>(
            $"api/reports/enrollment-stats?{ApiReportClient.DateRangeQuery(Report.From, Report.To)}");
        if (result.IsUnauthorized)
        {
            return RedirectToPage("/Index", new { message = ReportingMessages.SessionExpired });
        }

        if (!result.IsSuccess)
        {
            Report.ErrorMessage = result.ErrorMessage;
            return Page();
        }

        Report.Rows = (result.Data ?? new())
            .Select(row => new EnrollmentCourseRowViewModel
            {
                CourseTitle = row.CourseTitle,
                Category = row.Category,
                TotalEnrolled = row.TotalEnrollments
            })
            .OrderByDescending(row => row.TotalEnrolled)
            .ToList();

        return Page();
    }
}
