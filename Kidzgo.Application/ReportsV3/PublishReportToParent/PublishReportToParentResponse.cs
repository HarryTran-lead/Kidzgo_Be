namespace Kidzgo.Application.ReportsV3.PublishReportToParent;

public sealed class PublishReportToParentResponse
{
    public Guid ReportId { get; init; }
    public bool IsParentPublished { get; init; }
    public DateTime? ParentPublishedAt { get; init; }
    public int NotificationsCreated { get; init; }
}
