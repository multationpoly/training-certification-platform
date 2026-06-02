using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrainingCertification.Reporting.Services;

namespace TrainingCertification.Reporting.Pages;

public class LogoutModel : PageModel
{
    private readonly ApiReportClient _client;

    public LogoutModel(ApiReportClient client) => _client = client;

    public IActionResult OnGet()
    {
        _client.Logout();
        return RedirectToPage("/Index");
    }
}
