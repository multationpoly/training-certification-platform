using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrainingCertification.Reporting.Services;

namespace TrainingCertification.Reporting.Pages;

public class IndexModel : PageModel
{
    private readonly ApiReportClient _client;

    public IndexModel(ApiReportClient client) => _client = client;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? Error { get; set; }
    public string? Message { get; set; }

    public void OnGet(string? message)
    {
        Message = message;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _client.LoginAsync(Email, Password);
        if (result == LoginResult.Success)
        {
            return RedirectToPage("/Dashboard");
        }

        Error = result == LoginResult.ServiceUnavailable
            ? ReportingMessages.LoginUnavailable
            : ReportingMessages.InvalidLogin;
        return Page();
    }
}
