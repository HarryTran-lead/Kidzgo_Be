namespace Kidzgo.Application.ReportsV3.GenerateReport;

public sealed class GenerateReportResponse
{
    public Guid ReportRunId { get; init; }
    public Guid StudentReportId { get; init; }
    public string Status { get; init; } = string.Empty;
}
