using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.SessionReports;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.SessionReports.PublishSessionReport;

public sealed class PublishSessionReportCommandHandler(
    IDbContext context
) : ICommandHandler<PublishSessionReportCommand, PublishSessionReportResponse>
{
    public async Task<Result<PublishSessionReportResponse>> Handle(
        PublishSessionReportCommand command,
        CancellationToken cancellationToken)
    {
        var sessionReport = await context.SessionReports
            .Include(sr => sr.StudentProfile)
            .Include(sr => sr.Session)
                .ThenInclude(s => s.Class)
            .FirstOrDefaultAsync(sr => sr.Id == command.SessionReportId, cancellationToken);

        if (sessionReport is null)
        {
            return Result.Failure<PublishSessionReportResponse>(
                SessionReportErrors.NotFound(command.SessionReportId));
        }

        // Only APPROVED status can be published
        if (sessionReport.Status != ReportStatus.Approved)
        {
            return Result.Failure<PublishSessionReportResponse>(
                SessionReportErrors.InvalidStatusForOperation(
                    sessionReport.Status,
                    "publish"));
        }

        var now = VietnamTime.UtcNow();
        var sessionEndedCheck = SessionReportScheduleGuard.EnsureSessionHasEnded(sessionReport.Session, now);
        if (sessionEndedCheck.IsFailure)
        {
            return Result.Failure<PublishSessionReportResponse>(sessionEndedCheck.Error);
        }

        // Update status to Published
        sessionReport.Status = ReportStatus.Published;
        sessionReport.PublishedAt = now;
        sessionReport.UpdatedAt = now;

        await AddPublishedReportNotificationsAsync(sessionReport, now, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new PublishSessionReportResponse
        {
            Id = sessionReport.Id,
            SessionId = sessionReport.SessionId,
            StudentProfileId = sessionReport.StudentProfileId,
            StudentName = sessionReport.StudentProfile.DisplayName,
            Status = sessionReport.Status,
            PublishedAt = sessionReport.PublishedAt,
            UpdatedAt = sessionReport.UpdatedAt
        };
    }

    private async Task AddPublishedReportNotificationsAsync(
        SessionReport sessionReport,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var parentRecipients = await context.ParentStudentLinks
            .AsNoTracking()
            .Where(link => link.StudentProfileId == sessionReport.StudentProfileId &&
                           link.ParentProfile.UserId != default(Guid))
            .Select(link => new SessionReportNotificationRecipient(
                link.ParentProfile.UserId,
                "Parent"))
            .ToListAsync(cancellationToken);

        var recipientsByUserId = new Dictionary<Guid, string>();

        foreach (var recipient in parentRecipients)
        {
            recipientsByUserId.TryAdd(recipient.UserId, recipient.TargetRole);
        }

        if (sessionReport.StudentProfile.UserId != default(Guid))
        {
            recipientsByUserId.TryAdd(sessionReport.StudentProfile.UserId, "Student");
        }

        if (recipientsByUserId.Count == 0)
        {
            return;
        }

        var notifications = recipientsByUserId
            .Select(recipient => new Notification
            {
                Id = Guid.NewGuid(),
                RecipientUserId = recipient.Key,
                RecipientProfileId = sessionReport.StudentProfileId,
                Channel = NotificationChannel.InApp,
                Title = "Báo cáo buổi học mới",
                Content = BuildNotificationContent(sessionReport),
                Deeplink = $"/session-reports/{sessionReport.Id}",
                Status = NotificationStatus.Pending,
                TemplateId = sessionReport.Id.ToString(),
                CreatedAt = now,
                TargetRole = recipient.Value,
                Kind = "session_report",
                Priority = "normal",
                SenderRole = "System",
                SenderName = "Rex Centre",
                ScopeClassId = sessionReport.Session.ClassId,
                ScopeStudentProfileId = sessionReport.StudentProfileId
            })
            .ToList();

        context.Notifications.AddRange(notifications);
    }

    private static string BuildNotificationContent(SessionReport sessionReport)
    {
        var studentName = string.IsNullOrWhiteSpace(sessionReport.StudentProfile.DisplayName)
            ? "học sinh"
            : sessionReport.StudentProfile.DisplayName;
        var classTitle = string.IsNullOrWhiteSpace(sessionReport.Session.Class.Title)
            ? sessionReport.Session.Class.Code
            : sessionReport.Session.Class.Title;
        var classSegment = string.IsNullOrWhiteSpace(classTitle)
            ? string.Empty
            : $" trong lớp {classTitle}";

        return $"Báo cáo buổi học ngày {sessionReport.ReportDate:dd/MM/yyyy} của {studentName}{classSegment} đã được phát hành.";
    }

    private sealed record SessionReportNotificationRecipient(Guid UserId, string TargetRole);
}
