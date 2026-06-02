using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TrainingCertification.Reporting.Services;

public class ReportingSessionPageFilter : IAsyncPageFilter
{
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        var token = httpContext.Session.GetString(SessionKeys.JwtToken);
        var expiresValue = httpContext.Session.GetString(SessionKeys.JwtExpiresUtc);

        if (string.IsNullOrWhiteSpace(token)
            || !DateTimeOffset.TryParse(expiresValue, out var expiresUtc)
            || expiresUtc <= DateTimeOffset.UtcNow)
        {
            httpContext.Session.Clear();
            context.Result = new RedirectToPageResult("/Index", new { message = ReportingMessages.SessionExpired });
            return;
        }

        await next();
    }
}
