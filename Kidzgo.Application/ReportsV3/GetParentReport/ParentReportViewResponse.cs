namespace Kidzgo.Application.ReportsV3.GetParentReport;

public sealed class ParentReportViewResponse
{
    public Guid ReportId { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string ClassName { get; init; } = string.Empty;
    public DateOnly PeriodFrom { get; init; }
    public DateOnly PeriodTo { get; init; }
    public decimal AttendanceRate { get; init; }
    public decimal CompletionPercent { get; init; }
    public string TeacherComment { get; init; } = string.Empty;
    public string ParentMessage { get; init; } = string.Empty;
    public int RemainingTickets { get; init; }
    public IReadOnlyCollection<string> Strengths { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Recommendations { get; init; } = Array.Empty<string>();
    public string? RemedialStatus { get; init; }
}
