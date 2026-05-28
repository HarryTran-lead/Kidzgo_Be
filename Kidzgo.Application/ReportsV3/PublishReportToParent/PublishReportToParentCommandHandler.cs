using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.PublishReportToParent;

public sealed class PublishReportToParentCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<PublishReportToParentCommand, PublishReportToParentResponse>
{
    private const string NotificationKind = "student_report_progress";
    private const string SenderRole = "System";
    private const string SenderName = "Rex Centre";

    public async Task<Result<PublishReportToParentResponse>> Handle(
        PublishReportToParentCommand command,
        CancellationToken cancellationToken)
    {
        var accessGuard = new ReportAccessGuard(context, userContext);
        var userResult = await accessGuard.GetCurrentUserAsync(cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<PublishReportToParentResponse>(userResult.Error);
        }

        var currentUser = userResult.Value;
        if (currentUser.Role is not (UserRole.Admin or UserRole.ManagementStaff))
        {
            return Result.Failure<PublishReportToParentResponse>(
                Error.Validation("Report.PublishParentDenied", "Only admin/management can publish reports to parent."));
        }

        var report = await context.StudentReports
            .Include(r => r.Student)
            .Include(r => r.Class)
            .Include(r => r.ReportPeriod)
            .FirstOrDefaultAsync(r => r.Id == command.ReportId, cancellationToken);

        if (report is null)
        {
            return Result.Failure<PublishReportToParentResponse>(
                Error.NotFound("Report.NotFound", "Report was not found."));
        }

        if (report.ReportType != StudentReportType.Parent)
        {
            return Result.Failure<PublishReportToParentResponse>(
                Error.Validation("Report.ParentTypeRequired", "Only parent report type can be published to parent."));
        }

        if (report.Status != StudentReportStatus.Completed)
        {
            return Result.Failure<PublishReportToParentResponse>(
                Error.Validation("Report.InvalidStatus", "Only completed reports can be published to parent."));
        }

        if (report.IsParentPublished)
        {
            return Result.Success(new PublishReportToParentResponse
            {
                ReportId = report.Id,
                IsParentPublished = true,
                ParentPublishedAt = report.ParentPublishedAt,
                NotificationsCreated = 0
            });
        }

        var parentUserIds = await context.ParentStudentLinks
            .AsNoTracking()
            .Where(link =>
                link.StudentProfileId == report.StudentId &&
                link.ParentProfile.UserId != default(Guid))
            .Select(link => link.ParentProfile.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var dedupKeys = parentUserIds
            .Select(parentUserId => BuildDedupKey(report.Id, parentUserId))
            .ToHashSet(StringComparer.Ordinal);

        var existingDedupKeys = dedupKeys.Count == 0
            ? new HashSet<string>(StringComparer.Ordinal)
            : await context.Notifications
                .Where(notification =>
                    notification.TemplateId != null &&
                    dedupKeys.Contains(notification.TemplateId))
                .Select(notification => notification.TemplateId!)
                .ToHashSetAsync(cancellationToken);

        foreach (var notification in context.Notifications.Local)
        {
            if (!string.IsNullOrWhiteSpace(notification.TemplateId) &&
                dedupKeys.Contains(notification.TemplateId))
            {
                existingDedupKeys.Add(notification.TemplateId);
            }
        }

        var now = VietnamTime.UtcNow();
        var notifications = new List<Notification>();

        foreach (var parentUserId in parentUserIds)
        {
            var dedupKey = BuildDedupKey(report.Id, parentUserId);
            if (!existingDedupKeys.Add(dedupKey))
            {
                continue;
            }

            notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                RecipientUserId = parentUserId,
                RecipientProfileId = report.StudentId,
                Channel = NotificationChannel.InApp,
                Title = "Bao cao tien trinh moi",
                Content = BuildNotificationContent(report),
                Deeplink = $"/reports/{report.Id}",
                Status = NotificationStatus.Pending,
                TemplateId = dedupKey,
                CreatedAt = now,
                TargetRole = "Parent",
                Kind = NotificationKind,
                Priority = "normal",
                SenderRole = SenderRole,
                SenderName = SenderName,
                ScopeBranchId = report.BranchId,
                ScopeClassId = report.ClassId,
                ScopeStudentProfileId = report.StudentId
            });
        }

        report.IsParentPublished = true;
        report.ParentPublishedAt = now;
        report.ParentPublishedBy = currentUser.Id;
        report.UpdatedAt = now;

        if (notifications.Count > 0)
        {
            context.Notifications.AddRange(notifications);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new PublishReportToParentResponse
        {
            ReportId = report.Id,
            IsParentPublished = report.IsParentPublished,
            ParentPublishedAt = report.ParentPublishedAt,
            NotificationsCreated = notifications.Count
        });
    }

    private static string BuildDedupKey(Guid reportId, Guid parentUserId)
    {
        return $"report-v3-parent:{reportId:N}:{parentUserId:N}:inapp";
    }

    private static string BuildNotificationContent(StudentReport report)
    {
        var studentName = string.IsNullOrWhiteSpace(report.Student.DisplayName)
            ? "hoc sinh"
            : report.Student.DisplayName;
        var periodName = string.IsNullOrWhiteSpace(report.ReportPeriod.Name)
            ? "ky hien tai"
            : report.ReportPeriod.Name;

        return $"Bao cao tien trinh {periodName} cua {studentName} da san sang. Vui long mo ung dung de xem chi tiet.";
    }
}
