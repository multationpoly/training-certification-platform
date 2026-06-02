namespace TrainingCertification.Reporting.ViewModels;

public class ReportSectionState
{
    public string? ErrorMessage { get; set; }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
}
