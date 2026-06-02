namespace TrainingCertification.Reporting.Services;

public static class ReportingConstants
{
    public const string CoordinatorRole = "TrainingCoordinator";
    public const string CurrencyPrefix = "BD";
    public const string DateFormat = "dd MMM yyyy";
}

public static class SessionKeys
{
    public const string JwtToken = "JwtToken";
    public const string JwtExpiresUtc = "JwtExpiresUtc";
    public const string CoordinatorName = "CoordinatorName";
}

public static class ReportingMessages
{
    public const string InvalidLogin = "Invalid email or password";
    public const string LoginUnavailable = "Reporting service is currently unavailable. Please try again later.";
    public const string SessionExpired = "Your session has expired, please log in again";
    public const string PermissionDenied = "You do not have permission to view this report";
    public const string DataServiceUnavailable = "Unable to connect to the data service. Please refresh the page.";
}
