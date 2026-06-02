using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrainingCertification.Reporting.DTOs;
using TrainingCertification.Reporting.Services;
using TrainingCertification.Reporting.ViewModels;

namespace TrainingCertification.Reporting.Pages.Reports;

public class RevenueModel : PageModel
{
    private readonly ApiReportClient _client;

    public RevenueModel(ApiReportClient client) => _client = client;

    public RevenueReportViewModel Report { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(DateTime? from, DateTime? to)
    {
        var defaultFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        Report.From = from ?? defaultFrom;
        Report.To = to ?? DateTime.Today;

        var result = await _client.GetAsync<List<RevenueReportDto>>(
            $"api/reports/revenue?{ApiReportClient.DateRangeQuery(Report.From, Report.To)}");
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
            .Select(row => new RevenueSessionRowViewModel
            {
                CourseTitle = row.CourseTitle,
                Collected = row.TotalCollected,
                Outstanding = row.Outstanding,
                Status = row.Outstanding <= 0
                    ? RevenueStatus.FullyPaid
                    : row.IsOverdue ? RevenueStatus.Overdue : RevenueStatus.PartiallyPaid
            })
            .OrderByDescending(row => row.Status == RevenueStatus.Overdue)
            .ThenBy(row => row.CourseTitle)
            .ToList();

        return Page();
    }
}
