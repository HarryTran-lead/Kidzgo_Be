using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Reports;

public class ReportShareLog : Entity
{
    public Guid Id { get; set; }
    public Guid StudentReportId { get; set; }
    public string RecipientName { get; set; } = null!;
    public string RecipientContact { get; set; } = null!;
    public ReportShareChannel Channel { get; set; }
    public ReportShareStatus Status { get; set; }
    public string? ProviderMessageId { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? ViewedAt { get; set; }
    public string? ErrorMessage { get; set; }

    public StudentReport StudentReport { get; set; } = null!;
}
