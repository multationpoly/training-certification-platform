using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TrainingCertification.Reporting.DTOs;

namespace TrainingCertification.Reporting.Services;

public class ApiReportClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _context;
    private readonly ILogger<ApiReportClient> _logger;

    public ApiReportClient(HttpClient http, IHttpContextAccessor context, ILogger<ApiReportClient> logger)
    {
        _http = http;
        _context = context;
        _logger = logger;
    }

    public string CoordinatorName =>
        _context.HttpContext?.Session.GetString(SessionKeys.CoordinatorName) ?? "Fatima Al Khalifa";

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new LoginRequest(email, password));
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return LoginResult.InvalidCredentials;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Login API returned status {StatusCode}", response.StatusCode);
                return LoginResult.InvalidCredentials;
            }

            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (auth is null || string.IsNullOrWhiteSpace(auth.Token) || !auth.Roles.Contains(ReportingConstants.CoordinatorRole))
            {
                return LoginResult.InvalidCredentials;
            }

            var expiresUtc = GetTokenExpiry(auth.Token);
            if (expiresUtc is null || expiresUtc <= DateTimeOffset.UtcNow)
            {
                return LoginResult.InvalidCredentials;
            }

            var session = _context.HttpContext!.Session;
            session.SetString(SessionKeys.JwtToken, auth.Token);
            session.SetString(SessionKeys.JwtExpiresUtc, expiresUtc.Value.ToString("O"));
            session.SetString(SessionKeys.CoordinatorName, auth.FullName);
            return LoginResult.Success;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Login API is unreachable.");
            return LoginResult.ServiceUnavailable;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Login API timed out.");
            return LoginResult.ServiceUnavailable;
        }
    }

    public async Task<ApiResult<T>> GetAsync<T>(string url)
    {
        if (!TryAttachToken())
        {
            return ApiResult<T>.SessionExpired();
        }

        try
        {
            using var response = await _http.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ClearSession();
                return ApiResult<T>.Unauthorized();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return ApiResult<T>.Forbidden();
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Report API returned status {StatusCode} for {Url}", response.StatusCode, url);
                return ApiResult<T>.Failure(ReportingMessages.DataServiceUnavailable);
            }

            var wrapped = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
            if (wrapped is { Success: true })
            {
                return ApiResult<T>.Success(wrapped.Data);
            }

            return ApiResult<T>.Failure(ReportingMessages.DataServiceUnavailable);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Report API is unreachable for {Url}", url);
            return ApiResult<T>.Failure(ReportingMessages.DataServiceUnavailable);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Report API timed out for {Url}", url);
            return ApiResult<T>.Failure(ReportingMessages.DataServiceUnavailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report API response could not be processed for {Url}", url);
            return ApiResult<T>.Failure(ReportingMessages.DataServiceUnavailable);
        }
    }

    public void Logout() => ClearSession();

    public static string DateRangeQuery(DateTime from, DateTime to)
    {
        var inclusiveTo = to.Date.AddDays(1).AddTicks(-1);
        return $"from={Uri.EscapeDataString(from.Date.ToString("O"))}&to={Uri.EscapeDataString(inclusiveTo.ToString("O"))}";
    }

    private bool TryAttachToken()
    {
        var session = _context.HttpContext!.Session;
        var token = session.GetString(SessionKeys.JwtToken);
        var expiresValue = session.GetString(SessionKeys.JwtExpiresUtc);
        if (string.IsNullOrWhiteSpace(token)
            || !DateTimeOffset.TryParse(expiresValue, out var expiresUtc)
            || expiresUtc <= DateTimeOffset.UtcNow)
        {
            ClearSession();
            return false;
        }

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return true;
    }

    private void ClearSession() => _context.HttpContext?.Session.Clear();

    private static DateTimeOffset? GetTokenExpiry(string token)
    {
        var parts = token.Split('.');
        if (parts.Length < 2)
        {
            return null;
        }

        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty("exp", out var exp)
            ? DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64())
            : null;
    }
}

public enum LoginResult
{
    Success,
    InvalidCredentials,
    ServiceUnavailable
}

public record ApiResponse<T>(bool Success, T? Data, string Message);

public class ApiResult<T>
{
    private ApiResult(T? data, bool isSuccess, bool isUnauthorized, string? errorMessage)
    {
        Data = data;
        IsSuccess = isSuccess;
        IsUnauthorized = isUnauthorized;
        ErrorMessage = errorMessage;
    }

    public T? Data { get; }
    public bool IsSuccess { get; }
    public bool IsUnauthorized { get; }
    public string? ErrorMessage { get; }

    public static ApiResult<T> Success(T? data) => new(data, true, false, null);
    public static ApiResult<T> Failure(string message) => new(default, false, false, message);
    public static ApiResult<T> Forbidden() => new(default, false, false, ReportingMessages.PermissionDenied);
    public static ApiResult<T> Unauthorized() => new(default, false, true, ReportingMessages.SessionExpired);
    public static ApiResult<T> SessionExpired() => new(default, false, true, ReportingMessages.SessionExpired);
}
