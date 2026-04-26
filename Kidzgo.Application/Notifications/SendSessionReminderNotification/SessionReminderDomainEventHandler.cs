using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Notifications.SendSessionReminderNotification;

public sealed class SessionReminderDomainEventHandler(
    IDbContext context
) : INotificationHandler<SessionReminderDomainEvent>
{
    public async Task Handle(SessionReminderDomainEvent notification, CancellationToken cancellationToken)
    {
        var userExists = await context.Users
            .AnyAsync(u => u.Id == notification.RecipientUserId, cancellationToken);

        if (!userExists)
        {
            return;
        }

        var startTimeText = notification.SessionStartTime.ToString("dd/MM/yyyy HH:mm");
        var sessionTitle = string.IsNullOrWhiteSpace(notification.SessionTitle) ? "buổi học" : notification.SessionTitle;
        var className = string.IsNullOrWhiteSpace(notification.ClassName) ? "Lớp học" : notification.ClassName;
        var studentName = string.IsNullOrWhiteSpace(notification.StudentName) ? "học sinh" : notification.StudentName;
        var classroomName = string.IsNullOrWhiteSpace(notification.ClassroomName) ? "chưa cập nhật" : notification.ClassroomName;
        var location = string.IsNullOrWhiteSpace(notification.Location) ? "trung tâm" : notification.Location;

        var title = $"Nhắc nhở: Buổi học {sessionTitle} sắp bắt đầu";
        var content =
            $"Học sinh {studentName} có buổi học {className} lúc {startTimeText} tại {location}, phòng {classroomName}.";
        var deeplink = $"/sessions/{notification.SessionId}";
        var now = VietnamTime.UtcNow();

        var notifications = new List<Notification>
        {
            CreateNotification(NotificationChannel.InApp),
            CreateNotification(NotificationChannel.Push),
            CreateNotification(NotificationChannel.ZaloOa)
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
                TemplateId = notification.SessionId.ToString(),
                CreatedAt = now,
                TargetRole = "Student",
                Kind = "session_reminder",
                Priority = "normal",
                SenderRole = "System",
                SenderName = "Rex Centre",
                ScopeStudentProfileId = notification.RecipientProfileId
            };
        }
    }
}
