namespace TrainingCertification.Reporting.Helpers;

public static class ReportFormat
{
    public static string Money(decimal value) => $"BHD {value:N2}";

    public static string Percent(decimal value) =>
        value % 1 == 0 ? $"{value:0}%" : $"{value:0.0}%";
}
