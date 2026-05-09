using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kidzgo.Application.MonthlyReports.UpdateMonthlyReportDraft;

/// <summary>
/// UC-180: Teacher chỉnh sửa draft Monthly Report
/// </summary>
public sealed class UpdateMonthlyReportDraftCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<UpdateMonthlyReportDraftCommand, UpdateMonthlyReportDraftResponse>
{
    public async Task<Result<UpdateMonthlyReportDraftResponse>> Handle(
        UpdateMonthlyReportDraftCommand command,
        CancellationToken cancellationToken)
    {
        var report = await context.StudentMonthlyReports
            .Include(r => r.Class)
            .FirstOrDefaultAsync(r => r.Id == command.ReportId, cancellationToken);

        if (report is null)
        {
            return Result.Failure<UpdateMonthlyReportDraftResponse>(
                MonthlyReportErrors.NotFound(command.ReportId));
        }

        // Validate: Can edit when status is Draft, Review, or Rejected
        // Rejected reports can be edited to allow teacher to fix and resubmit
        if (report.Status != ReportStatus.Draft && 
            report.Status != ReportStatus.Review && 
            report.Status != ReportStatus.Rejected)
        {
            return Result.Failure<UpdateMonthlyReportDraftResponse>(
                MonthlyReportErrors.InvalidStatus(report.Status, "edit"));
        }

        // Authorization: Teacher can only edit reports of their classes
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null || currentUser.Role != Domain.Users.UserRole.Teacher)
        {
            return Result.Failure<UpdateMonthlyReportDraftResponse>(
                Error.Validation("MonthlyReport.Unauthorized", "Only teachers can edit reports"));
        }

        var isTeacherOfClass = await context.Classes
            .AnyAsync(c => c.Id == report.ClassId &&
                         (c.MainTeacherId == currentUser.Id || c.AssistantTeacherId == currentUser.Id),
                cancellationToken);

        if (!isTeacherOfClass)
        {
            return Result.Failure<UpdateMonthlyReportDraftResponse>(
                Error.Validation("MonthlyReport.Unauthorized", "You can only edit reports of your classes"));
        }

        string? validJsonContent = null;
        if (!string.IsNullOrWhiteSpace(command.DraftContent))
        {
            validJsonContent = NormalizeDraftContent(command.DraftContent, report.DraftContent);
        }

        report.DraftContent = validJsonContent;
        report.UpdatedAt = VietnamTime.UtcNow();

        // If report was Rejected, change status back to Draft so teacher can resubmit
        if (report.Status == ReportStatus.Rejected)
        {
            report.Status = ReportStatus.Draft;
            // Clear review information since it's being edited again
            report.ReviewedBy = null;
            report.ReviewedAt = null;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateMonthlyReportDraftResponse
        {
            Id = report.Id,
            DraftContent = report.DraftContent,
            UpdatedAt = report.UpdatedAt
        };
    }

    private static string NormalizeDraftContent(string draftContent, string? existingContent)
    {
        try
        {
            using var draftDoc = JsonDocument.Parse(draftContent);

            if (draftDoc.RootElement.ValueKind == JsonValueKind.Object)
            {
                return draftContent;
            }

            var normalizedText = ReadFlexibleString(draftDoc.RootElement) ?? draftContent;
            return BuildStructuredDraftContent(normalizedText, existingContent);
        }
        catch (JsonException)
        {
            return BuildStructuredDraftContent(draftContent, existingContent);
        }
    }

    private static string BuildStructuredDraftContent(string draftText, string? existingContent)
    {
        var aiUsed = false;
        JsonElement? sections = null;

        if (!string.IsNullOrWhiteSpace(existingContent))
        {
            try
            {
                using var existingDoc = JsonDocument.Parse(existingContent);
                var root = existingDoc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (TryGetProperty(root, "ai_used", "aiUsed", out var aiUsedElement) &&
                        (aiUsedElement.ValueKind == JsonValueKind.True ||
                         aiUsedElement.ValueKind == JsonValueKind.False))
                    {
                        aiUsed = aiUsedElement.GetBoolean();
                    }

                    if (TryGetProperty(root, "sections", "Sections", out var sectionsElement) &&
                        sectionsElement.ValueKind == JsonValueKind.Object)
                    {
                        sections = sectionsElement.Clone();
                    }
                }
            }
            catch (JsonException)
            {
                // Ignore legacy malformed content and keep the plain draft text.
            }
        }

        if (sections.HasValue)
        {
            return JsonSerializer.Serialize(new
            {
                ai_used = aiUsed,
                draft_text = draftText,
                sections = sections.Value
            });
        }

        return JsonSerializer.Serialize(new
        {
            ai_used = aiUsed,
            draft_text = draftText
        });
    }

    private static bool TryGetProperty(
        JsonElement element,
        string primaryProperty,
        string fallbackProperty,
        out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            (element.TryGetProperty(primaryProperty, out value) ||
             element.TryGetProperty(fallbackProperty, out value)))
        {
            return true;
        }

        value = default;
        return false;
    }

    private static string? ReadFlexibleString(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            JsonValueKind.Object => element.GetRawText(),
            JsonValueKind.Array => element.GetRawText(),
            _ => element.ToString()
        };
    }
}

