namespace TrainingCertification.Reporting.ViewModels;

public class DashboardViewModel : ReportSectionState
{
    public string CoordinatorName { get; set; } = "Fatima Al Khalifa";
    public int? TotalActiveCourses { get; set; }
    public int? TotalEnrollmentsThisMonth { get; set; }
    public int? TotalCertificationsIssued { get; set; }
    public decimal? TotalOutstandingPaymentBalance { get; set; }
}
