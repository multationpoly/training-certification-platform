using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrainingCertification.Reporting.DTOs;
using TrainingCertification.Reporting.Services;
using TrainingCertification.Reporting.ViewModels;

namespace TrainingCertification.Reporting.Pages.Reports;

public class InstructorWorkloadModel : PageModel
{
    private readonly ApiReportClient _client;

    public InstructorWorkloadModel(ApiReportClient client) => _client = client;

    public InstructorWorkloadViewModel Report { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _client.GetAsync<List<WorkloadReportDto>>("api/reports/instructor-workload");
        if (result.IsUnauthorized)
        {
            return RedirectToPage("/Index", new { message = ReportingMessages.SessionExpired });
        }

        if (!result.IsSuccess)
        {
            Report.ErrorMessage = result.ErrorMessage;
            return Page();
        }

        var rows = (result.Data ?? new())
            .Select(row => new InstructorWorkloadRowViewModel
            {
                InstructorName = row.InstructorName,
                TotalSessions = row.SessionsAssigned,
                TotalTraineesTaught = row.TotalTrainees,
                CompletionRate = row.CompletionRate
            })
            .OrderByDescending(row => row.TotalSessions)
            .ToList();

        foreach (var row in rows.Take(3))
        {
            row.IsBusiest = true;
        }

        Report.Rows = rows;
        return Page();
    }
}
