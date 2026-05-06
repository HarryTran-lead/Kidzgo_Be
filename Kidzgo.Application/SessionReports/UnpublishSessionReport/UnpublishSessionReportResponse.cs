using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.SessionReports.UnpublishSessionReport;

public sealed class UnpublishSessionReportResponse
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public ReportStatus Status { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
