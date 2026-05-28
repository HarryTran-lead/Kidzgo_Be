using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Reports;

public class AIInsight : Entity
{
    public Guid Id { get; set; }
    public Guid StudentReportId { get; set; }
    public AIInsightType InsightType { get; set; }
    public string Content { get; set; } = null!;
    public decimal? ConfidenceScore { get; set; }
    public string? SourceDataJson { get; set; }
    public DateTime CreatedAt { get; set; }

    public StudentReport StudentReport { get; set; } = null!;
}
