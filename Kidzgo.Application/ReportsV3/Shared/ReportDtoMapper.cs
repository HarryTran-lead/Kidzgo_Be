using System.Text.Json;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportsV3.Shared;

internal static class ReportDtoMapper
{
    public static StudentReportListItemDto ToListItem(StudentReport report)
    {
        return new StudentReportListItemDto
        {
            Id = report.Id,
            StudentId = report.StudentId,
            StudentName = report.Student.DisplayName,
            ClassId = report.ClassId,
            ClassName = report.Class.Title,
            BranchId = report.BranchId,
            ReportPeriodId = report.ReportPeriodId,
            ReportType = report.ReportType.ToString(),
            Status = report.Status.ToString(),
            IsParentPublished = report.IsParentPublished,
            ParentPublishedAt = report.ParentPublishedAt,
            CreatedAt = report.CreatedAt
        };
    }

    public static StudentReportDetailDto ToDetail(
        StudentReport report,
        IReadOnlyCollection<ReportInsightDto> insights,
        IReadOnlyCollection<RiskAlertDto> risks,
        IReadOnlyCollection<RecommendationDto> recommendations,
        IReadOnlyCollection<ReportShareLogDto> shareLogs)
    {
        return new StudentReportDetailDto
        {
            Id = report.Id,
            StudentId = report.StudentId,
            StudentName = report.Student.DisplayName,
            ClassId = report.ClassId,
            ClassName = report.Class.Title,
            BranchId = report.BranchId,
            ReportPeriodId = report.ReportPeriodId,
            ReportPeriodName = report.ReportPeriod.Name,
            ReportPeriodFrom = report.ReportPeriod.StartDate,
            ReportPeriodTo = report.ReportPeriod.EndDate,
            ReportType = report.ReportType.ToString(),
            Status = report.Status.ToString(),
            IsParentPublished = report.IsParentPublished,
            ParentPublishedAt = report.ParentPublishedAt,
            Snapshot = ParseSnapshot(report.SnapshotJson),
            SummaryText = report.SummaryText,
            CreatedAt = report.CreatedAt,
            Insights = insights,
            Risks = risks,
            Recommendations = recommendations,
            ShareLogs = shareLogs
        };
    }

    public static ReportInsightDto ToInsightDto(AIInsight insight)
    {
        return new ReportInsightDto
        {
            Id = insight.Id,
            InsightType = insight.InsightType.ToString(),
            Content = insight.Content,
            ConfidenceScore = insight.ConfidenceScore,
            CreatedAt = insight.CreatedAt
        };
    }

    public static RiskAlertDto ToRiskDto(RiskAlert risk)
    {
        return new RiskAlertDto
        {
            Id = risk.Id,
            StudentId = risk.StudentId,
            ClassId = risk.ClassId,
            BranchId = risk.BranchId,
            ReportPeriodId = risk.ReportPeriodId,
            RiskType = risk.RiskType.ToString(),
            Severity = risk.Severity.ToString(),
            Reason = risk.Reason,
            Source = risk.Source,
            Status = risk.Status.ToString(),
            CreatedAt = risk.CreatedAt,
            ResolvedAt = risk.ResolvedAt
        };
    }

    public static RecommendationDto ToRecommendationDto(Recommendation recommendation)
    {
        return new RecommendationDto
        {
            Id = recommendation.Id,
            StudentId = recommendation.StudentId,
            ClassId = recommendation.ClassId,
            RecommendationType = recommendation.RecommendationType.ToString(),
            Content = recommendation.Content,
            Priority = recommendation.Priority.ToString(),
            AssignedRole = recommendation.AssignedRole,
            Status = recommendation.Status.ToString(),
            DueAt = recommendation.DueAt,
            IsOverdue = recommendation.Status != RecommendationStatus.Done &&
                        recommendation.Status != RecommendationStatus.Rejected &&
                        recommendation.DueAt < VietnamTime.UtcNow(),
            CreatedAt = recommendation.CreatedAt,
            CompletedAt = recommendation.CompletedAt
        };
    }

    public static ReportShareLogDto ToShareDto(ReportShareLog shareLog)
    {
        return new ReportShareLogDto
        {
            Id = shareLog.Id,
            StudentReportId = shareLog.StudentReportId,
            RecipientName = shareLog.RecipientName,
            RecipientContact = shareLog.RecipientContact,
            Channel = shareLog.Channel,
            Status = shareLog.Status,
            ProviderMessageId = shareLog.ProviderMessageId,
            SentAt = shareLog.SentAt,
            ViewedAt = shareLog.ViewedAt,
            ErrorMessage = shareLog.ErrorMessage
        };
    }

    private static JsonElement ParseSnapshot(string snapshotJson)
    {
        if (string.IsNullOrWhiteSpace(snapshotJson))
        {
            return new JsonElement();
        }

        using var document = JsonDocument.Parse(snapshotJson);
        return document.RootElement.Clone();
    }
}
