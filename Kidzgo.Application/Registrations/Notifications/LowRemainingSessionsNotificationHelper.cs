using System.Net;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Events;
using Kidzgo.Domain.Registrations;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.Notifications;

internal static class LowRemainingSessionsNotificationHelper
{
    private const int InAppThreshold = 3;
    private const int UrgentThreshold = 1;
    private const string NotificationKind = "registration_low_sessions";
    private const string SenderRole = "System";
    private const string SenderName = "Rex Centre";

    public static async Task QueueAsync(
        IDbContext context,
        Registration registration,
        CancellationToken cancellationToken)
    {
        if (registration.StudentProfileId == Guid.Empty || registration.RemainingSessions > InAppThreshold)
        {
            return;
        }

        var student = await context.Profiles
            .Where(profile => profile.Id == registration.StudentProfileId)
            .Select(profile => new
            {
                profile.DisplayName,
                profile.UserId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (student is null)
        {
            return;
        }

        var recipientRoles = new Dictionary<Guid, string>();

        var parentUserIds = await context.ParentStudentLinks
            .Where(link => link.StudentProfileId == registration.StudentProfileId &&
                           link.ParentProfile.UserId != default(Guid))
            .Select(link => link.ParentProfile.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var parentUserId in parentUserIds)
        {
            recipientRoles.TryAdd(parentUserId, "Parent");
        }

        if (student.UserId != default(Guid))
        {
            recipientRoles.TryAdd(student.UserId, "Student");
        }

        if (recipientRoles.Count == 0)
        {
            return;
        }

        var cycleKey = $"{registration.TuitionPlanId:N}:{registration.TotalSessions}";
        var candidateKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var recipient in recipientRoles)
        {
            candidateKeys.Add(BuildDedupKey(registration.Id, cycleKey, recipient.Key, InAppThreshold, NotificationChannel.InApp));

            if (registration.RemainingSessions <= UrgentThreshold)
            {
                candidateKeys.Add(BuildDedupKey(registration.Id, cycleKey, recipient.Key, UrgentThreshold, NotificationChannel.Email));
                candidateKeys.Add(BuildDedupKey(registration.Id, cycleKey, recipient.Key, UrgentThreshold, NotificationChannel.Push));
            }
        }

        var existingKeys = await context.Notifications
            .Where(notification => notification.TemplateId != null &&
                                   candidateKeys.Contains(notification.TemplateId))
            .Select(notification => notification.TemplateId!)
            .ToHashSetAsync(cancellationToken);

        foreach (var notification in context.Notifications.Local)
        {
            if (!string.IsNullOrWhiteSpace(notification.TemplateId) &&
                candidateKeys.Contains(notification.TemplateId))
            {
                existingKeys.Add(notification.TemplateId);
            }
        }

        var studentName = string.IsNullOrWhiteSpace(student.DisplayName) ? "Hoc sinh" : student.DisplayName;
        var encodedStudentName = WebUtility.HtmlEncode(studentName);
        var remainingSessionsText = registration.RemainingSessions == 1
            ? "1 buoi hoc"
            : $"{registration.RemainingSessions} buoi hoc";
        var encodedRemainingSessionsText = WebUtility.HtmlEncode(remainingSessionsText);
        var inAppTitle = "Sap het goi hoc";
        var inAppContent =
            $"Hoc sinh {studentName} chi con {remainingSessionsText} trong goi hoc hien tai. Vui long lien he trung tam de duoc ho tro gia han hoac nang goi.";
        var urgentTitle = $"Canh bao: chi con {remainingSessionsText}";
        var urgentPushContent =
            $"Hoc sinh {studentName} chi con {remainingSessionsText}. Vui long lien he trung tam de tranh gian doan lich hoc.";
        var urgentEmailContent =
            $"""
             <div style="font-family: Arial, sans-serif; color:#222; line-height:1.6;">
               <h2 style="color:#c05621;">Hoc sinh sap het goi hoc</h2>
               <p>Hoc sinh <strong>{encodedStudentName}</strong> chi con <strong>{encodedRemainingSessionsText}</strong> trong goi hoc hien tai.</p>
               <p>Vui long lien he trung tam de duoc ho tro gia han hoac nang goi, tranh gian doan lich hoc.</p>
               <p>Tran trong,<br/>Rex Centre</p>
             </div>
             """;
        var now = VietnamTime.UtcNow();
        var notifications = new List<Notification>();

        foreach (var recipient in recipientRoles)
        {
            var inAppKey = BuildDedupKey(registration.Id, cycleKey, recipient.Key, InAppThreshold, NotificationChannel.InApp);
            if (existingKeys.Add(inAppKey))
            {
                notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    RecipientUserId = recipient.Key,
                    RecipientProfileId = registration.StudentProfileId,
                    Channel = NotificationChannel.InApp,
                    Title = inAppTitle,
                    Content = inAppContent,
                    Status = NotificationStatus.Pending,
                    TemplateId = inAppKey,
                    CreatedAt = now,
                    TargetRole = recipient.Value,
                    Kind = NotificationKind,
                    Priority = "normal",
                    SenderRole = SenderRole,
                    SenderName = SenderName,
                    ScopeBranchId = registration.BranchId,
                    ScopeStudentProfileId = registration.StudentProfileId
                });
            }

            if (registration.RemainingSessions > UrgentThreshold)
            {
                continue;
            }

            var emailKey = BuildDedupKey(registration.Id, cycleKey, recipient.Key, UrgentThreshold, NotificationChannel.Email);
            if (existingKeys.Add(emailKey))
            {
                notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    RecipientUserId = recipient.Key,
                    RecipientProfileId = registration.StudentProfileId,
                    Channel = NotificationChannel.Email,
                    Title = urgentTitle,
                    Content = urgentEmailContent,
                    Status = NotificationStatus.Pending,
                    TemplateId = emailKey,
                    CreatedAt = now,
                    TargetRole = recipient.Value,
                    Kind = NotificationKind,
                    Priority = "high",
                    SenderRole = SenderRole,
                    SenderName = SenderName,
                    ScopeBranchId = registration.BranchId,
                    ScopeStudentProfileId = registration.StudentProfileId
                });
            }

            var pushKey = BuildDedupKey(registration.Id, cycleKey, recipient.Key, UrgentThreshold, NotificationChannel.Push);
            if (existingKeys.Add(pushKey))
            {
                notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    RecipientUserId = recipient.Key,
                    RecipientProfileId = registration.StudentProfileId,
                    Channel = NotificationChannel.Push,
                    Title = urgentTitle,
                    Content = urgentPushContent,
                    Status = NotificationStatus.Pending,
                    TemplateId = pushKey,
                    CreatedAt = now,
                    TargetRole = recipient.Value,
                    Kind = NotificationKind,
                    Priority = "high",
                    SenderRole = SenderRole,
                    SenderName = SenderName,
                    ScopeBranchId = registration.BranchId,
                    ScopeStudentProfileId = registration.StudentProfileId
                });
            }
        }

        if (notifications.Count == 0)
        {
            return;
        }

        context.Notifications.AddRange(notifications);

        foreach (var notification in notifications.Where(notification => notification.Channel != NotificationChannel.InApp))
        {
            notification.Raise(new NotificationCreatedDomainEvent(notification.Id, notification.Channel));
        }
    }

    private static string BuildDedupKey(
        Guid registrationId,
        string cycleKey,
        Guid recipientUserId,
        int threshold,
        NotificationChannel channel)
    {
        return $"registration-low-sessions:{registrationId:N}:{cycleKey}:{recipientUserId:N}:{threshold}:{channel}";
    }
}
