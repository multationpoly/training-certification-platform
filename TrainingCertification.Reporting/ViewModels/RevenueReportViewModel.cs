namespace TrainingCertification.Reporting.ViewModels;

public class RevenueReportViewModel : ReportSectionState
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal TotalCollected => Rows.Sum(row => row.Collected);
    public decimal TotalOutstanding => Rows.Sum(row => row.Outstanding);
    public decimal TotalOverdue => Rows.Where(row => row.Status == RevenueStatus.Overdue).Sum(row => row.Outstanding);
    public decimal CollectionRate
    {
        get
        {
            var total = TotalCollected + TotalOutstanding;
            return total == 0 ? 0 : TotalCollected / total * 100;
        }
    }

    public List<RevenueSessionRowViewModel> Rows { get; set; } = new();
}

public class RevenueSessionRowViewModel
{
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime? SessionDate { get; set; }
    public string Instructor { get; set; } = "Not provided";
    public int? EnrolledCount { get; set; }
    public decimal ExpectedRevenue => Collected + Outstanding;
    public decimal Collected { get; set; }
    public decimal Outstanding { get; set; }
    public RevenueStatus Status { get; set; }
}

public enum RevenueStatus
{
    FullyPaid,
    PartiallyPaid,
    Overdue
}
