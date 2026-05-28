namespace Kidzgo.API.Requests;

public sealed class GeneratePhase3ReportRequest
{
    public string ReportType { get; set; } = "parent";
    public Guid? StudentId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid PeriodId { get; set; }
}
