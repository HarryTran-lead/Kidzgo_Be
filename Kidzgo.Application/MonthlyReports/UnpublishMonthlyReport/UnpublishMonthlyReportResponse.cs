namespace Kidzgo.Application.MonthlyReports.UnpublishMonthlyReport;

public sealed class UnpublishMonthlyReportResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = null!;
    public DateTime? PublishedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
