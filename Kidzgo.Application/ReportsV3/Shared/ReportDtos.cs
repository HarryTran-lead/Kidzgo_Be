using System.Text.Json;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;

namespace Kidzgo.Application.ReportsV3.Shared;

public sealed class StudentReportListItemDto
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public Guid BranchId { get; init; }
    public Guid ReportPeriodId { get; init; }
    public string ReportType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsParentPublished { get; init; }
    public DateTime? ParentPublishedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class StudentReportDetailDto
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public Guid BranchId { get; init; }
    public Guid ReportPeriodId { get; init; }
    public string ReportPeriodName { get; init; } = string.Empty;
    public DateOnly ReportPeriodFrom { get; init; }
    public DateOnly ReportPeriodTo { get; init; }
    public string ReportType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsParentPublished { get; init; }
    public DateTime? ParentPublishedAt { get; init; }
    public JsonElement Snapshot { get; init; }
    public string? SummaryText { get; init; }
    public DateTime CreatedAt { get; init; }
    public IReadOnlyCollection<ReportInsightDto> Insights { get; init; } = Array.Empty<ReportInsightDto>();
    public IReadOnlyCollection<RiskAlertDto> Risks { get; init; } = Array.Empty<RiskAlertDto>();
    public IReadOnlyCollection<RecommendationDto> Recommendations { get; init; } = Array.Empty<RecommendationDto>();
    public IReadOnlyCollection<ReportShareLogDto> ShareLogs { get; init; } = Array.Empty<ReportShareLogDto>();
}

public sealed class ReportInsightDto
{
    public Guid Id { get; init; }
    public string InsightType { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public decimal? ConfidenceScore { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class RiskAlertDto
{
    public Guid Id { get; init; }
    public Guid? StudentId { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? BranchId { get; init; }
    public Guid ReportPeriodId { get; init; }
    public string RiskType { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string? Source { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
}

public sealed class RecommendationDto
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public Guid? ClassId { get; init; }
    public string RecommendationType { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public string AssignedRole { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime DueAt { get; init; }
    public bool IsOverdue { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public sealed class ReportShareLogDto
{
    public Guid Id { get; init; }
    public Guid StudentReportId { get; init; }
    public string RecipientName { get; init; } = string.Empty;
    public string RecipientContact { get; init; } = string.Empty;
    public ReportShareChannel Channel { get; init; }
    public ReportShareStatus Status { get; init; }
    public string? ProviderMessageId { get; init; }
    public DateTime SentAt { get; init; }
    public DateTime? ViewedAt { get; init; }
    public string? ErrorMessage { get; init; }
}
