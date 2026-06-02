namespace TrainingCertification.Reporting.ViewModels;

public class InstructorWorkloadViewModel : ReportSectionState
{
    public List<InstructorWorkloadRowViewModel> Rows { get; set; } = new();
}

public class InstructorWorkloadRowViewModel
{
    public string InstructorName { get; set; } = string.Empty;
    public string ExpertiseAreas { get; set; } = "Not provided";
    public int TotalSessions { get; set; }
    public int UpcomingSessions { get; set; }
    public int TotalTraineesTaught { get; set; }
    public decimal CompletionRate { get; set; }
    public bool IsBusiest { get; set; }
    public bool HasNoUpcomingSessions => UpcomingSessions == 0;
}
