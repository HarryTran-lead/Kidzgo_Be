using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Notifications.SendMissionReminderNotification;

public sealed class MissionReminderDomainEventHandler(
    IDbContext context
) : INotificationHandler<MissionReminderDomainEvent>
{
    public async Task Handle(MissionReminderDomainEvent notification, CancellationToken cancellationToken)
    {
        var userExists = await context.Users
            .AnyAsync(u => u.Id == notification.RecipientUserId, cancellationToken);

        if (!userExists)
        {
            return;
        }

        var dueDateText = notification.DueDate?.ToString("dd/MM/yyyy HH:mm") ?? "sớm";
        var missionTitle = string.IsNullOrWhiteSpace(notification.MissionTitle) ? "nhiệm vụ" : notification.MissionTitle;
        var className = string.IsNullOrWhiteSpace(notification.ClassName) ? "lớp học" : notification.ClassName;
        var studentName = string.IsNullOrWhiteSpace(notification.StudentName) ? "học sinh" : notification.StudentName;
        var title = $"Nhắc nhở: Nhiệm vụ {missionTitle} sắp kết thúc";
        var content =
            $"Học sinh {studentName} có nhiệm vụ {missionTitle} của {className} cần hoàn thành trước {dueDateText}.";
        var deeplink = $"/missions/{notification.MissionId}";
        var now = VietnamTime.UtcNow();

        var notifications = new List<Notification>
        {
            CreateNotification(NotificationChannel.InApp),
            CreateNotification(NotificationChannel.Push)
        };

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var notificationRecord in notifications.Where(n => n.Channel != NotificationChannel.InApp))
        {
            notificationRecord.Raise(new NotificationCreatedDomainEvent(notificationRecord.Id, notificationRecord.Channel));
        }

        await context.SaveChangesAsync(cancellationToken);

        Notification CreateNotification(NotificationChannel channel)
        {
            return new Notification
            {
                Id = Guid.NewGuid(),
                RecipientUserId = notification.RecipientUserId,
                RecipientProfileId = notification.RecipientProfileId,
                Channel = channel,
                Title = title,
                Content = content,
                Deeplink = deeplink,
                Status = NotificationStatus.Pending,
                TemplateId = notification.MissionId.ToString(),
                CreatedAt = now,
                TargetRole = "Student",
                Kind = "mission_reminder",
                Priority = "normal",
                SenderRole = "System",
                SenderName = "KidzGo Centre",
                ScopeStudentProfileId = notification.RecipientProfileId
            };
        }
    }
}

