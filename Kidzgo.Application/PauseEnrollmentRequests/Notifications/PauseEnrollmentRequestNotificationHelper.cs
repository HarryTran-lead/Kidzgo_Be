using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Shared;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Events;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.Notifications;

internal static class PauseEnrollmentRequestNotificationHelper
{
    private const string ApprovedEmailCode = "PAUSE_ENROLLMENT_APPROVED_EMAIL";
    private const string RejectedEmailCode = "PAUSE_ENROLLMENT_REJECTED_EMAIL";
    private const string OutcomeEmailCode = "PAUSE_ENROLLMENT_OUTCOME_EMAIL";
    private const string ApprovedPushCode = "PAUSE_ENROLLMENT_APPROVED_PUSH";
    private const string RejectedPushCode = "PAUSE_ENROLLMENT_REJECTED_PUSH";
    private const string OutcomePushCode = "PAUSE_ENROLLMENT_OUTCOME_PUSH";
    private const string ApprovedZaloCode = "PAUSE_ENROLLMENT_APPROVED_ZALO";
    private const string RejectedZaloCode = "PAUSE_ENROLLMENT_REJECTED_ZALO";
    private const string OutcomeZaloCode = "PAUSE_ENROLLMENT_OUTCOME_ZALO";
    private const string StaffReassignInAppCode = "PAUSE_ENROLLMENT_STAFF_REASSIGN_INAPP";
    private const string StaffTutoringInAppCode = "PAUSE_ENROLLMENT_STAFF_TUTORING_INAPP";

    internal enum NotificationType
    {
        Approved,
        Rejected,
        OutcomeUpdated
    }

    internal enum StaffFollowUpType
    {
        ReassignEquivalentClass,
        ContinueWithTutoring
    }

    public static async Task NotifyAsync(
        IDbContext context,
        ITemplateRenderer templateRenderer,
        Guid studentProfileId,
        Guid requestId,
        NotificationType type,
        DateOnly pauseFrom,
        DateOnly pauseTo,
        string? outcomeText,
        string? outcomeNote,
        CancellationToken cancellationToken)
    {
        var studentName = await context.Profiles
            .Where(p => p.Id == studentProfileId)
            .Select(p => p.DisplayName)
            .FirstOrDefaultAsync(cancellationToken) ?? "Học sinh";

        var placeholders = new Dictionary<string, string>
        {
            ["student_name"] = studentName,
            ["pause_from"] = pauseFrom.ToString("dd/MM/yyyy"),
            ["pause_to"] = pauseTo.ToString("dd/MM/yyyy"),
            ["outcome"] = outcomeText ?? string.Empty,
            ["outcome_note"] = outcomeNote ?? string.Empty
        };

        var (inAppTitle, inAppContent, emailCode, pushCode, zaloCode) = type switch
        {
            NotificationType.Approved => (
                "Yêu cầu bảo lưu đã được duyệt",
                $"Yêu cầu bảo lưu từ {pauseFrom:dd/MM/yyyy} đến {pauseTo:dd/MM/yyyy} đã được duyệt.",
                ApprovedEmailCode,
                ApprovedPushCode,
                ApprovedZaloCode),
            NotificationType.Rejected => (
                "Yêu cầu bảo lưu bị từ chối",
                $"Yêu cầu bảo lưu từ {pauseFrom:dd/MM/yyyy} đến {pauseTo:dd/MM/yyyy} đã bị từ chối.",
                RejectedEmailCode,
                RejectedPushCode,
                RejectedZaloCode),
            _ => (
                "Kết quả bảo lưu đã được cập nhật",
                string.IsNullOrWhiteSpace(outcomeNote)
                    ? $"Kết quả bảo lưu: {outcomeText}."
                    : $"Kết quả bảo lưu: {outcomeText}. Ghi chú: {outcomeNote}",
                OutcomeEmailCode,
                OutcomePushCode,
                OutcomeZaloCode)
        };

        var parentUserIds = await context.ParentStudentLinks
            .Where(link => link.StudentProfileId == studentProfileId && link.ParentProfile.UserId != default(Guid))
            .Select(link => link.ParentProfile.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var studentUserId = await context.Profiles
            .Where(profile => profile.Id == studentProfileId)
            .Select(profile => profile.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        var recipientUserIds = new HashSet<Guid>(parentUserIds);
        if (studentUserId != default(Guid))
        {
            recipientUserIds.Add(studentUserId);
        }

        if (recipientUserIds.Count == 0)
        {
            return;
        }

        var now = VietnamTime.UtcNow();
        var notifications = new List<Notification>();

        var emailTemplate = await GetTemplateAsync(context, emailCode, NotificationChannel.Email, cancellationToken);
        var emailTitle = emailTemplate is null
            ? inAppTitle
            : templateRenderer.Render(emailTemplate.Title, placeholders);
        var emailContent = emailTemplate is null
            ? inAppContent
            : templateRenderer.Render(emailTemplate.Content ?? string.Empty, placeholders);

        var pushTemplate = await GetTemplateAsync(context, pushCode, NotificationChannel.Push, cancellationToken);
        var pushTitle = pushTemplate is null
            ? inAppTitle
            : templateRenderer.Render(pushTemplate.Title, placeholders);
        var pushContent = pushTemplate is null
            ? inAppContent
            : templateRenderer.Render(pushTemplate.Content ?? string.Empty, placeholders);

        var zaloTemplate = await GetTemplateAsync(context, zaloCode, NotificationChannel.ZaloOa, cancellationToken);
        var zaloTitle = zaloTemplate is null
            ? inAppTitle
            : templateRenderer.Render(zaloTemplate.Title, placeholders);
        var zaloContent = zaloTemplate is null
            ? inAppContent
            : templateRenderer.Render(zaloTemplate.Content ?? string.Empty, placeholders);

        foreach (var userId in recipientUserIds)
        {
            notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                RecipientUserId = userId,
                RecipientProfileId = studentProfileId,
                Channel = NotificationChannel.InApp,
                Title = inAppTitle,
                Content = inAppContent,
                Deeplink = $"/pause-enrollment-requests/{requestId}",
                Status = NotificationStatus.Pending,
                CreatedAt = now,
                SenderRole = "Staff",
                Kind = "pause_enrollment"
            });

            notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                RecipientUserId = userId,
                RecipientProfileId = studentProfileId,
                Channel = NotificationChannel.Email,
                Title = emailTitle,
                Content = emailContent,
                Deeplink = $"/pause-enrollment-requests/{requestId}",
                NotificationTemplateId = emailTemplate?.Id,
                Status = NotificationStatus.Pending,
                CreatedAt = now,
                SenderRole = "Staff",
                Kind = "pause_enrollment"
            });

            notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                RecipientUserId = userId,
                RecipientProfileId = studentProfileId,
                Channel = NotificationChannel.Push,
                Title = pushTitle,
                Content = pushContent,
                Deeplink = $"/pause-enrollment-requests/{requestId}",
                NotificationTemplateId = pushTemplate?.Id,
                Status = NotificationStatus.Pending,
                CreatedAt = now,
                SenderRole = "Staff",
                Kind = "pause_enrollment"
            });

            notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                RecipientUserId = userId,
                RecipientProfileId = studentProfileId,
                Channel = NotificationChannel.ZaloOa,
                Title = zaloTitle,
                Content = zaloContent,
                Deeplink = $"/pause-enrollment-requests/{requestId}",
                NotificationTemplateId = zaloTemplate?.Id,
                Status = NotificationStatus.Pending,
                CreatedAt = now,
                SenderRole = "Staff",
                Kind = "pause_enrollment"
            });
        }

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.Raise(new NotificationCreatedDomainEvent(notification.Id, notification.Channel));
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public static async Task NotifyStaffFollowUpAsync(
        IDbContext context,
        ITemplateRenderer templateRenderer,
        Guid studentProfileId,
        Guid requestId,
        DateOnly pauseFrom,
        DateOnly pauseTo,
        StaffFollowUpType type,
        string? outcomeNote,
        CancellationToken cancellationToken)
    {
        var studentName = await context.Profiles
            .Where(profile => profile.Id == studentProfileId)
            .Select(profile => profile.DisplayName)
            .FirstOrDefaultAsync(cancellationToken) ?? "Học sinh";

        var branchIds = await context.PauseEnrollmentRequestHistories
            .Where(history => history.PauseEnrollmentRequestId == requestId)
            .Select(history => history.Class.BranchId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var staffUsers = await context.Users
            .Where(user => user.IsActive && !user.IsDeleted &&
                           (user.Role == UserRole.Admin ||
                            (user.Role == UserRole.ManagementStaff &&
                             user.BranchId.HasValue &&
                             branchIds.Contains(user.BranchId.Value))))
            .Select(user => new { user.Id, user.BranchId, Role = user.Role.ToString() })
            .ToListAsync(cancellationToken);

        if (staffUsers.Count == 0)
        {
            return;
        }

        var placeholders = new Dictionary<string, string>
        {
            ["student_name"] = studentName,
            ["pause_from"] = pauseFrom.ToString("dd/MM/yyyy"),
            ["pause_to"] = pauseTo.ToString("dd/MM/yyyy"),
            ["outcome_note"] = outcomeNote ?? string.Empty
        };

        var reassignOutcomeText = VietnameseEnumText.ForPauseEnrollmentOutcome(PauseEnrollmentOutcome.ReassignEquivalentClass);
        var tutoringOutcomeText = VietnameseEnumText.ForPauseEnrollmentOutcome(PauseEnrollmentOutcome.ContinueWithTutoring);

        var (templateCode, fallbackTitle, fallbackContent, priority) = type switch
        {
            StaffFollowUpType.ReassignEquivalentClass => (
                StaffReassignInAppCode,
                "Cần xếp lớp mới cho học sinh bảo lưu",
                string.IsNullOrWhiteSpace(outcomeNote)
                    ? $"Học sinh {studentName} có outcome bảo lưu {reassignOutcomeText} cho giai đoạn {pauseFrom:dd/MM/yyyy} - {pauseTo:dd/MM/yyyy}. Vui lòng kiểm tra và xếp lớp khác thủ công."
                    : $"Học sinh {studentName} có outcome bảo lưu {reassignOutcomeText} cho giai đoạn {pauseFrom:dd/MM/yyyy} - {pauseTo:dd/MM/yyyy}. Ghi chú: {outcomeNote}. Vui lòng kiểm tra và xếp lớp khác thủ công.",
                "high"),
            _ => (
                StaffTutoringInAppCode,
                "Cần tư vấn gói học kèm sau bảo lưu",
                string.IsNullOrWhiteSpace(outcomeNote)
                    ? $"Học sinh {studentName} có outcome bảo lưu {tutoringOutcomeText} cho giai đoạn {pauseFrom:dd/MM/yyyy} - {pauseTo:dd/MM/yyyy}. Vui lòng tư vấn chọn gói và khóa học kèm thêm."
                    : $"Học sinh {studentName} có outcome bảo lưu {tutoringOutcomeText} cho giai đoạn {pauseFrom:dd/MM/yyyy} - {pauseTo:dd/MM/yyyy}. Ghi chú: {outcomeNote}. Vui lòng tư vấn chọn gói và khóa học kèm thêm.",
                "normal")
        };

        var template = await GetTemplateAsync(context, templateCode, NotificationChannel.InApp, cancellationToken);
        var title = template is null
            ? fallbackTitle
            : templateRenderer.Render(template.Title, placeholders);
        var content = template is null
            ? fallbackContent
            : templateRenderer.Render(template.Content ?? string.Empty, placeholders);

        var now = VietnamTime.UtcNow();
        var notifications = staffUsers
            .Select(staff => new Notification
            {
                Id = Guid.NewGuid(),
                RecipientUserId = staff.Id,
                RecipientProfileId = studentProfileId,
                Channel = NotificationChannel.InApp,
                Title = title,
                Content = content,
                Deeplink = $"/pause-enrollment-requests/{requestId}",
                NotificationTemplateId = template?.Id,
                Status = NotificationStatus.Pending,
                CreatedAt = now,
                TargetRole = staff.Role,
                Kind = "pause_enrollment_follow_up",
                Priority = priority,
                SenderRole = "System",
                SenderName = "System",
                ScopeBranchId = staff.BranchId,
                ScopeStudentProfileId = studentProfileId
            })
            .ToList();

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static Task<NotificationTemplate?> GetTemplateAsync(
        IDbContext context,
        string code,
        NotificationChannel channel,
        CancellationToken cancellationToken)
    {
        return context.NotificationTemplates
            .Where(template => template.Code == code && template.Channel == channel && template.IsActive && !template.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
