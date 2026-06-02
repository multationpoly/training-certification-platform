namespace TrainingCertification.Reporting.ViewModels;

public class CertificationReportViewModel : ReportSectionState
{
    public int TotalInProgress => Rows.Sum(row => row.InProgress);
    public int TotalEligible => Rows.Sum(row => row.Eligible);
    public int TotalIssued => Rows.Sum(row => row.Issued);
    public List<CertificationTrackRowViewModel> Rows { get; set; } = new();
}

public class CertificationTrackRowViewModel
{
    public string TrackName { get; set; } = string.Empty;
    public string RequiredCourses { get; set; } = "Not provided";
    public int InProgress { get; set; }
    public int Eligible { get; set; }
    public int Issued { get; set; }
    public decimal CompletionRate
    {
        get
        {
            var total = InProgress + Eligible + Issued;
            return total == 0 ? 0 : (decimal)(Eligible + Issued) / total * 100;
        }
    }
}
