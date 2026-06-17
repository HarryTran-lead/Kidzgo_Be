using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Registrations;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.Notifications;

internal static class WaitingListThresholdNotificationHelper
{
    private const int Threshold = 7;
    private const string NotificationKind = "registration_waitlist_threshold";
    private const string SenderRole = "System";
    private const string SenderName = "Rex Centre";

    public static async Task NotifyAsync(
        IDbContext context,
        Registration registration,
        CancellationToken cancellationToken)
    {
        if (registration.BranchId == Guid.Empty || registration.ProgramId == Guid.Empty)
        {
            return;
        }

        if (registration.ClassId is null)
        {
            await NotifyGroupAsync(
                context,
                registration.BranchId,
                registration.ProgramId,
                registration.LevelId,
                RegistrationTrackHelper.PrimaryTrack,
                cancellationToken);
        }

        if (registration.SecondaryLevelId.HasValue && registration.SecondaryClassId is null)
        {
            await NotifyGroupAsync(
                context,
                registration.BranchId,
                registration.ProgramId,
                registration.SecondaryLevelId.Value,
                RegistrationTrackHelper.SecondaryTrack,
                cancellationToken);
        }
    }

    private static async Task NotifyGroupAsync(
        IDbContext context,
        Guid branchId,
        Guid programId,
        Guid levelId,
        string track,
        CancellationToken cancellationToken)
    {
        var waitingCount = await CountWaitingAsync(
            context,
            branchId,
            programId,
            levelId,
            track,
            cancellationToken);

        if (waitingCount < Threshold)
        {
            return;
        }

        var milestone = waitingCount / Threshold;
        if (milestone == 0)
        {
            return;
        }

        var branchName = await context.Branches
            .AsNoTracking()
            .Where(branch => branch.Id == branchId)
            .Select(branch => branch.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? "chi nhanh";

        var programName = await context.Programs
            .AsNoTracking()
            .Where(program => program.Id == programId)
            .Select(program => program.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? "chuong trinh";

        var levelName = await context.Levels
            .AsNoTracking()
            .Where(level => level.Id == levelId)
            .Select(level => level.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? "level";

        var recipients = await context.Users
            .AsNoTracking()
            .Where(user => user.IsActive && !user.IsDeleted &&
                           (user.Role == UserRole.Admin ||
                            (user.Role == UserRole.ManagementStaff &&
                             user.BranchId == branchId)))
            .Select(user => new
            {
                user.Id,
                Role = user.Role.ToString()
            })
            .ToListAsync(cancellationToken);

        if (recipients.Count == 0)
        {
            return;
        }

        var candidateKeys = recipients
            .Select(recipient => BuildDedupKey(branchId, programId, levelId, track, milestone, recipient.Id))
            .ToHashSet(StringComparer.Ordinal);

        var existingKeys = await context.Notifications
            .AsNoTracking()
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

        var createdAt = VietnamTime.UtcNow();
        var title = $"Du {Threshold} hoc vien cho lop moi";
        var trackLabel = string.Equals(track, RegistrationTrackHelper.SecondaryTrack, StringComparison.OrdinalIgnoreCase)
            ? "track phu"
            : "track chinh";
        var content =
            $"{branchName} - {programName} - {levelName} ({trackLabel}) da co {waitingCount} hoc vien dang cho xep lop. Co the mo lop moi.";
        var deeplink =
            $"/registrations/waiting-list?branchId={branchId}&programId={programId}&levelId={levelId}&track={track}";

        var notifications = new List<Notification>();

        foreach (var recipient in recipients)
        {
            var dedupKey = BuildDedupKey(branchId, programId, levelId, track, milestone, recipient.Id);
            if (existingKeys.Contains(dedupKey))
            {
                continue;
            }

            notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                RecipientUserId = recipient.Id,
                RecipientProfileId = null,
                Channel = NotificationChannel.InApp,
                Title = title,
                Content = content,
                Deeplink = deeplink,
                Status = NotificationStatus.Pending,
                CreatedAt = createdAt,
                TargetRole = recipient.Role,
                Kind = NotificationKind,
                Priority = "high",
                SenderRole = SenderRole,
                SenderName = SenderName,
                ScopeBranchId = branchId,
                ScopeClassId = null,
                ScopeStudentProfileId = null,
                TemplateId = dedupKey
            });
        }

        if (notifications.Count == 0)
        {
            return;
        }

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task<int> CountWaitingAsync(
        IDbContext context,
        Guid branchId,
        Guid programId,
        Guid levelId,
        string track,
        CancellationToken cancellationToken)
    {
        var baseQuery = context.Registrations
            .AsNoTracking()
            .Where(registration =>
                registration.BranchId == branchId &&
                registration.ProgramId == programId &&
                registration.Status != RegistrationStatus.Completed &&
                registration.Status != RegistrationStatus.Cancelled);

        if (string.Equals(track, RegistrationTrackHelper.SecondaryTrack, StringComparison.OrdinalIgnoreCase))
        {
            return await baseQuery
                .CountAsync(registration =>
                    registration.SecondaryLevelId == levelId &&
                    registration.SecondaryClassId == null,
                    cancellationToken);
        }

        return await baseQuery
            .CountAsync(registration =>
                registration.LevelId == levelId &&
                registration.ClassId == null,
                cancellationToken);
    }

    private static string BuildDedupKey(
        Guid branchId,
        Guid programId,
        Guid levelId,
        string track,
        int milestone,
        Guid recipientUserId)
    {
        return $"registration-waitlist:{branchId:N}:{programId:N}:{levelId:N}:{track.ToLowerInvariant()}:{Threshold}:{milestone}:{recipientUserId:N}";
    }
}
