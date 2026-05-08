using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Events;
using Kidzgo.Domain.Reports.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.MonthlyReports.MonthlyReportPublishedEventHandler;

/// <summary>
/// UC-187: Gửi thông báo khi publish Monthly Report
/// </summary>
public sealed class MonthlyReportPublishedEventHandler(
    IDbContext context,
    ILogger<MonthlyReportPublishedEventHandler> logger
) : INotificationHandler<MonthlyReportPublishedEvent>
{
    public async Task Handle(MonthlyReportPublishedEvent notification, CancellationToken cancellationToken)
    {
        var report = await context.StudentMonthlyReports
            .Include(r => r.StudentProfile)
            .ThenInclude(sp => sp.StudentLinks)
            .FirstOrDefaultAsync(r => r.Id == notification.ReportId, cancellationToken);

        if (report is null)
        {
            logger.LogWarning("Monthly Report {ReportId} not found for notification", notification.ReportId);
            return;
        }

        // Get all parent profiles linked to this student
        var parentProfileIds = report.StudentProfile.StudentLinks
            .Select(psl => psl.ParentProfileId)
            .ToList();

        // Get parent users
        var parentUsers = await context.Profiles
            .Where(p => parentProfileIds.Contains(p.Id) && p.UserId != default(Guid))
            .Select(p => p.UserId)
            .ToListAsync(cancellationToken);

        // Also get student user if exists
        var studentUser = await context.Profiles
            .Where(p => p.Id == notification.StudentProfileId && p.UserId != default(Guid))
            .Select(p => p.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        var recipientUserIds = parentUsers.ToList();
        if (studentUser != default(Guid))
        {
            recipientUserIds.Add(studentUser);
        }

        if (!recipientUserIds.Any())
        {
            logger.LogWarning("No recipients found for Monthly Report {ReportId}", notification.ReportId);
            return;
        }

        // Create notifications for all recipients
        var now = VietnamTime.UtcNow();
        var notifications = recipientUserIds.Select(userId => new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = userId,
            Title = $"Báo cáo tháng {notification.Month}/{notification.Year}",
            Content = $"Báo cáo học tập tháng {notification.Month}/{notification.Year} của {report.StudentProfile.DisplayName} đã được phát hành.",
            Channel = NotificationChannel.Push, // Default to push, can be extended
            Status = NotificationStatus.Pending,
            Deeplink = $"/monthly-reports/{notification.ReportId}",
            CreatedAt = now
        }).ToList();

        context.Notifications.AddRange(notifications);

        // Raise domain events for each notification (will be handled by infrastructure)
        foreach (var notificationRecord in notifications)
        {
            notificationRecord.Raise(new NotificationCreatedDomainEvent(
                notificationRecord.Id,
                notificationRecord.Channel));
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created {Count} notifications for Monthly Report {ReportId}",
            notifications.Count, notification.ReportId);
    }
}

