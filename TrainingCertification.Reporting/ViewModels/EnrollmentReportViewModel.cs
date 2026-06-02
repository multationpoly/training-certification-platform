namespace TrainingCertification.Reporting.ViewModels;

public class EnrollmentReportViewModel : ReportSectionState
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int TotalEnrollments => Rows.Sum(row => row.TotalEnrolled);
    public int Confirmed => Rows.Sum(row => row.Confirmed);
    public int Completed => Rows.Sum(row => row.Completed);
    public int Dropped => Rows.Sum(row => row.Dropped);
    public List<EnrollmentCourseRowViewModel> Rows { get; set; } = new();
}

public class EnrollmentCourseRowViewModel
{
    public string CourseTitle { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int TotalEnrolled { get; set; }
    public int Confirmed { get; set; }
    public int Attending { get; set; }
    public int Completed { get; set; }
    public int Dropped { get; set; }
    public decimal? PassRate { get; set; }
}
