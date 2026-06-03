using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrainingCertification.Reporting.DTOs;
using TrainingCertification.Reporting.Services;
using TrainingCertification.Reporting.ViewModels;

namespace TrainingCertification.Reporting.Pages.Reports;

public class CertificationsModel : PageModel
{
    private readonly ApiReportClient _client;

    public CertificationsModel(ApiReportClient client) => _client = client;

    public CertificationReportViewModel Report { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _client.GetAsync<List<CertificationReportDto>>("api/reports/certification-completion");
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
            .Select(row => new CertificationTrackRowViewModel
            {
                TrackName = row.TrackName,
                InProgress = row.InProgress,
                Eligible = row.TotalEligible,
                Issued = row.TotalIssued
            })
            .OrderBy(row => row.TrackName)
            .ToList();

        return Page();
    }
}
