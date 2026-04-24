using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Events;
using Kidzgo.Domain.Tickets;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Tickets.Notifications;

internal static class TicketNotificationHelper
{
    private const string AssignedInAppCode = "TICKET_ASSIGNED_INAPP";
    private const string AssignedPushCode = "TICKET_ASSIGNED_PUSH";
    private const string ReplyInAppCode = "TICKET_REPLY_INAPP";
    private const string ReplyEmailCode = "TICKET_REPLY_EMAIL";

    public static async Task NotifyAssignedSupportAsync(
        IDbContext context,
        ITemplateRenderer templateRenderer,
        Ticket ticket,
        Guid recipientUserId,
        string recipientRole,
        string senderRole,
        string senderName,
        CancellationToken cancellationToken)
    {
        if (recipientUserId == default || recipientUserId == ticket.OpenedByUserId)
        {
            return;
        }

        var details = await GetTicketDetailsAsync(context, ticket.Id, cancellationToken);
        if (details is null)
        {
            return;
        }

        var placeholders = BuildCommonPlaceholders(details);
        placeholders["sender_name"] = senderName;
        placeholders["sender_role"] = senderRole;

        var inAppContent = await RenderNotificationAsync(
            context,
            templateRenderer,
            AssignedInAppCode,
            NotificationChannel.InApp,
            placeholders,
            "Ticket hỗ trợ mới được giao cho bạn",
            $"Ticket \"{details.Subject}\" đã được gửi tới bạn bởi {details.OpenedByDisplayName}.")
            ;

        var pushContent = await RenderNotificationAsync(
            context,
            templateRenderer,
            AssignedPushCode,
            NotificationChannel.Push,
            placeholders,
            "Bạn có ticket hỗ trợ mới",
            $"Ticket \"{details.Subject}\" từ {details.OpenedByDisplayName} đang chờ bạn xử lý.");

        var now = VietnamTime.UtcNow();
        var notifications = new List<Notification>
        {
            CreateNotification(
                recipientUserId,
                null,
                NotificationChannel.InApp,
                inAppContent,
                $"/tickets/{ticket.Id}",
                recipientRole,
                "ticket_assigned",
                ticket.BranchId,
                ticket.ClassId,
                details.StudentProfileId,
                senderRole,
                senderName,
                now),
            CreateNotification(
                recipientUserId,
                null,
                NotificationChannel.Push,
                pushContent,
                $"/tickets/{ticket.Id}",
                recipientRole,
                "ticket_assigned",
                ticket.BranchId,
                ticket.ClassId,
                details.StudentProfileId,
                senderRole,
                senderName,
                now)
        };

        await PersistAndDispatchAsync(context, notifications, cancellationToken);
    }

    public static async Task NotifyRequesterReplyAsync(
        IDbContext context,
        ITemplateRenderer templateRenderer,
        Ticket ticket,
        string senderRole,
        string senderName,
        string replyMessage,
        CancellationToken cancellationToken)
    {
        if (ticket.OpenedByUserId == default)
        {
            return;
        }

        var details = await GetTicketDetailsAsync(context, ticket.Id, cancellationToken);
        if (details is null)
        {
            return;
        }

        var placeholders = BuildCommonPlaceholders(details);
        placeholders["sender_name"] = senderName;
        placeholders["sender_role"] = senderRole;
        placeholders["reply_message"] = replyMessage;

        var inAppContent = await RenderNotificationAsync(
            context,
            templateRenderer,
            ReplyInAppCode,
            NotificationChannel.InApp,
            placeholders,
            "Ticket hỗ trợ đã có phản hồi mới",
            $"{senderName} đã phản hồi ticket \"{details.Subject}\" của bạn.");

        var emailContent = await RenderNotificationAsync(
            context,
            templateRenderer,
            ReplyEmailCode,
            NotificationChannel.Email,
            placeholders,
            $"[KidzGo] Ticket \"{details.Subject}\" đã có phản hồi mới",
            $$"""
              <div style="font-family:Segoe UI,Arial,sans-serif;color:#1f2937;line-height:1.6;">
                <h2 style="margin:0 0 16px 0;">Ticket hỗ trợ đã có phản hồi</h2>
                <p>Xin chào {{details.OpenedByDisplayName}},</p>
                <p><strong>{{senderName}}</strong> đã phản hồi ticket <strong>{{details.Subject}}</strong>.</p>
                <div style="padding:12px 16px;border:1px solid #e5e7eb;border-radius:8px;background:#f9fafb;">
                  <p style="margin:0 0 8px 0;"><strong>Nội dung phản hồi:</strong></p>
                  <p style="margin:0;">{{System.Net.WebUtility.HtmlEncode(replyMessage)}}</p>
                </div>
                <p style="margin-top:16px;">Bạn có thể mở ứng dụng để xem chi tiết ticket.</p>
              </div>
              """);

        var now = VietnamTime.UtcNow();
        var notifications = new List<Notification>
        {
            CreateNotification(
                ticket.OpenedByUserId,
                ticket.OpenedByProfileId,
                NotificationChannel.InApp,
                inAppContent,
                $"/tickets/{ticket.Id}",
                details.OpenedByRole,
                "ticket_reply",
                ticket.BranchId,
                ticket.ClassId,
                details.StudentProfileId,
                senderRole,
                senderName,
                now),
            CreateNotification(
                ticket.OpenedByUserId,
                ticket.OpenedByProfileId,
                NotificationChannel.Email,
                emailContent,
                $"/tickets/{ticket.Id}",
                details.OpenedByRole,
                "ticket_reply",
                ticket.BranchId,
                ticket.ClassId,
                details.StudentProfileId,
                senderRole,
                senderName,
                now)
        };

        await PersistAndDispatchAsync(context, notifications, cancellationToken);
    }

    private static Notification CreateNotification(
        Guid recipientUserId,
        Guid? recipientProfileId,
        NotificationChannel channel,
        RenderedNotificationContent content,
        string deeplink,
        string? targetRole,
        string kind,
        Guid? scopeBranchId,
        Guid? scopeClassId,
        Guid? scopeStudentProfileId,
        string senderRole,
        string senderName,
        DateTime createdAt)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = recipientUserId,
            RecipientProfileId = recipientProfileId,
            Channel = channel,
            Title = content.Title,
            Content = content.Content,
            Deeplink = deeplink,
            NotificationTemplateId = content.TemplateId,
            Status = NotificationStatus.Pending,
            CreatedAt = createdAt,
            TargetRole = targetRole,
            Kind = kind,
            Priority = kind == "ticket_assigned" ? "high" : "normal",
            SenderRole = senderRole,
            SenderName = senderName,
            ScopeBranchId = scopeBranchId,
            ScopeClassId = scopeClassId,
            ScopeStudentProfileId = scopeStudentProfileId
        };
    }

    private static async Task PersistAndDispatchAsync(
        IDbContext context,
        List<Notification> notifications,
        CancellationToken cancellationToken)
    {
        if (notifications.Count == 0)
        {
            return;
        }

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var notification in notifications.Where(n => n.Channel != NotificationChannel.InApp))
        {
            notification.Raise(new NotificationCreatedDomainEvent(notification.Id, notification.Channel));
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task<RenderedNotificationContent> RenderNotificationAsync(
        IDbContext context,
        ITemplateRenderer templateRenderer,
        string templateCode,
        NotificationChannel channel,
        Dictionary<string, string> placeholders,
        string fallbackTitle,
        string fallbackContent)
    {
        var template = await context.NotificationTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.Code == templateCode && t.Channel == channel && t.IsActive && !t.IsDeleted,
                cancellationToken: CancellationToken.None);

        if (template is null)
        {
            return new RenderedNotificationContent(null, fallbackTitle, fallbackContent);
        }

        return new RenderedNotificationContent(
            template.Id,
            templateRenderer.Render(template.Title, placeholders),
            templateRenderer.Render(template.Content ?? string.Empty, placeholders));
    }

    private static async Task<TicketNotificationDetails?> GetTicketDetailsAsync(
        IDbContext context,
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        return await context.Tickets
            .AsNoTracking()
            .Where(t => t.Id == ticketId)
            .Select(t => new TicketNotificationDetails(
                t.Id,
                t.Subject,
                t.Category.ToString(),
                t.OpenedByProfileId,
                t.OpenedByProfile != null ? (t.OpenedByProfile.DisplayName ?? t.OpenedByUser.Name) : t.OpenedByUser.Name,
                t.OpenedByUser.Role.ToString(),
                t.Class != null ? t.Class.Code : string.Empty,
                t.Class != null ? t.Class.Title : string.Empty,
                t.OpenedByProfileId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static Dictionary<string, string> BuildCommonPlaceholders(TicketNotificationDetails details)
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ticket_id"] = details.TicketId.ToString(),
            ["ticket_subject"] = details.Subject,
            ["ticket_category"] = details.Category,
            ["opened_by_name"] = details.OpenedByDisplayName,
            ["opened_by_role"] = details.OpenedByRole,
            ["class_code"] = details.ClassCode,
            ["class_title"] = details.ClassTitle
        };
    }

    private sealed record TicketNotificationDetails(
        Guid TicketId,
        string Subject,
        string Category,
        Guid? OpenedByProfileId,
        string OpenedByDisplayName,
        string OpenedByRole,
        string ClassCode,
        string ClassTitle,
        Guid? StudentProfileId);

    private sealed record RenderedNotificationContent(
        Guid? TemplateId,
        string Title,
        string Content);
}
