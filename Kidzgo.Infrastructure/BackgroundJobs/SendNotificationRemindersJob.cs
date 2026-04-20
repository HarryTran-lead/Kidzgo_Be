using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Services;
using Kidzgo.Application.Shared;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Media;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Events;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Kidzgo.Infrastructure.BackgroundJobs;

/// <summary>
/// UC-331-336: Tự động gửi notification reminders
/// Job quét và gửi thông báo nhắc cho:
/// - Session reminders (UC-331)
/// - Homework reminders (UC-332)
/// - Tuition reminders (UC-333)
/// - Makeup reminders (UC-334)
/// - Mission reminders (UC-335)
/// - Media reminders (UC-336)
/// </summary>
[DisallowConcurrentExecution]
public sealed class SendNotificationRemindersJob(
    IServiceScopeFactory scopeFactory,
    ILogger<SendNotificationRemindersJob> logger
) : IJob
{
    // Reminder windows (configurable via appsettings)
    private static readonly TimeSpan SessionReminderWindow = TimeSpan.FromHours(24); // 24h before session
    private static readonly TimeSpan HomeworkReminderWindow = TimeSpan.FromHours(24); // 24h before due date
    private static readonly TimeSpan TuitionReminderWindow = TimeSpan.FromDays(3); // 3 days before due date
    private static readonly TimeSpan MakeupReminderWindow = TimeSpan.FromHours(24); // 24h before makeup session
    private static readonly TimeSpan MissionReminderWindow = TimeSpan.FromHours(24); // 24h before due date
    private static readonly TimeSpan MediaReminderWindow = TimeSpan.FromHours(1); // 1h after media created

    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;
        var now = VietnamTime.UtcNow();

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDbContext>();
        var studentSessionAssignmentService = scope.ServiceProvider.GetRequiredService<StudentSessionAssignmentService>();

        logger.LogInformation("Starting notification reminders job at {Time}", now);

        try
        {
            // Send session reminders (UC-331)
            await SendSessionRemindersAsync(db, studentSessionAssignmentService, now, cancellationToken);

            // Send homework reminders (UC-332)
            await SendHomeworkRemindersAsync(db, now, cancellationToken);

            // Send tuition reminders (UC-333)
            await SendTuitionRemindersAsync(db, now, cancellationToken);

            // Send makeup reminders (UC-334)
            await SendMakeupRemindersAsync(db, now, cancellationToken);

            // Send mission reminders (UC-335)
            await SendMissionRemindersAsync(db, now, cancellationToken);

            // Send media reminders (UC-336)
            await SendMediaRemindersAsync(db, now, cancellationToken);

            logger.LogInformation("Completed notification reminders job");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing notification reminders job");
            throw; // Re-throw to let Quartz handle retry
        }
    }

    private async Task SendSessionRemindersAsync(
        IDbContext db,
        StudentSessionAssignmentService studentSessionAssignmentService,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var reminderTime = now.Add(SessionReminderWindow);
        var reminderTimeEnd = reminderTime.AddHours(1); // 1 hour window

        var sessions = await db.Sessions
            .Include(s => s.Class)
            .Include(s => s.PlannedRoom)
            .Where(s =>
                s.Status == SessionStatus.Scheduled &&
                s.PlannedDatetime >= reminderTime &&
                s.PlannedDatetime <= reminderTimeEnd)
            .ToListAsync(cancellationToken);

        // Get existing notifications to avoid duplicates
        var sessionIds = sessions.Select(s => s.Id).ToList();
        var existingNotifications = await db.Notifications
            .Where(n =>
                n.Channel == NotificationChannel.InApp &&
                n.Kind == "session_reminder" &&
                !string.IsNullOrEmpty(n.TemplateId) &&
                sessionIds.Any(sid => n.TemplateId == sid.ToString()))
            .Select(n => $"{n.TemplateId}_{n.RecipientUserId}")
            .ToHashSetAsync(cancellationToken);

        foreach (var session in sessions)
        {
            var regularParticipants = await studentSessionAssignmentService
                .GetRegularParticipantsAsync(session.Id, cancellationToken);

            foreach (var participant in regularParticipants)
            {
                var studentProfile = await db.Profiles
                    .Include(sp => sp.User)
                    .FirstOrDefaultAsync(sp => sp.Id == participant.StudentProfileId, cancellationToken);

                if (studentProfile?.User == null)
                    continue;

                // Check if notification already sent
                var key = $"{session.Id}_{studentProfile.UserId}";
                if (existingNotifications.Contains(key))
                    continue;

                session.Raise(new SessionReminderDomainEvent(
                    session.Id,
                    studentProfile.UserId,
                    studentProfile.Id,
                    session.Class.Title ?? "Lớp học",
                    session.PlannedDatetime,
                    session.Class.Title,
                    "Rex English Center",
                    studentProfile.DisplayName,
                    session.PlannedRoom?.Name
                ));
            }
        }

        if (sessions.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Raised {Count} session reminder events", sessions.Count);
        }
    }

    private async Task SendHomeworkRemindersAsync(IDbContext db, DateTime now, CancellationToken cancellationToken)
    {
        var reminderTime = now.Add(HomeworkReminderWindow);
        var reminderTimeEnd = reminderTime.AddHours(1);

        var homeworks = await db.HomeworkAssignments
            .Include(h => h.Session)
                .ThenInclude(s => s.Class)
            .Include(h => h.HomeworkStudents)
                .ThenInclude(hs => hs.StudentProfile)
                    .ThenInclude(sp => sp.User)
            .Where(h =>
                h.DueAt.HasValue &&
                h.DueAt.Value >= reminderTime &&
                h.DueAt.Value <= reminderTimeEnd)
            .ToListAsync(cancellationToken);

        foreach (var homework in homeworks)
        {
            var students = homework.HomeworkStudents
                .Where(hs => hs.Status == HomeworkStatus.Assigned && hs.SubmittedAt == null)
                .ToList();

            foreach (var student in students)
            {
                var studentProfile = student.StudentProfile;
                if (studentProfile?.User == null || string.IsNullOrWhiteSpace(studentProfile.User.Email))
                    continue;

                homework.Raise(new HomeworkReminderDomainEvent(
                    homework.Id,
                    studentProfile.UserId,
                    studentProfile.Id,
                    homework.Title,
                    homework.DueAt,
                    homework.Session?.Class?.Title,
                    studentProfile.DisplayName
                ));
            }
        }

        if (homeworks.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Raised {Count} homework reminder events", homeworks.Count);
        }
    }

    private async Task SendTuitionRemindersAsync(IDbContext db, DateTime now, CancellationToken cancellationToken)
    {
        var reminderTime = now.Add(TuitionReminderWindow);
        var reminderTimeEnd = reminderTime.AddDays(1);

        var invoices = await db.Invoices
            .Include(i => i.StudentProfile)
                .ThenInclude(sp => sp.User)
            .Include(i => i.InvoiceLines)
            .Where(i =>
                i.Status == InvoiceStatus.Pending &&
                i.DueDate.HasValue &&
                i.DueDate.Value >= DateOnly.FromDateTime(reminderTime) &&
                i.DueDate.Value <= DateOnly.FromDateTime(reminderTimeEnd))
            .ToListAsync(cancellationToken);

        foreach (var invoice in invoices)
        {
            var studentProfile = invoice.StudentProfile;
            if (studentProfile?.User == null || string.IsNullOrWhiteSpace(studentProfile.User.Email))
                continue;

            var totalAmount = invoice.InvoiceLines.Any() 
                ? invoice.InvoiceLines.Sum(il => il.Quantity * il.UnitPrice)
                : invoice.Amount;

            invoice.Raise(new TuitionReminderDomainEvent(
                invoice.Id,
                studentProfile.UserId,
                studentProfile.Id,
                totalAmount,
                invoice.DueDate.Value.ToDateTime(TimeOnly.MinValue),
                studentProfile.DisplayName
            ));
        }

        if (invoices.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Raised {Count} tuition reminder events", invoices.Count);
        }
    }

    private async Task SendMakeupRemindersAsync(IDbContext db, DateTime now, CancellationToken cancellationToken)
    {
        var reminderTime = now.Add(MakeupReminderWindow);
        var reminderTimeEnd = reminderTime.AddHours(1);

        // Get makeup allocations with upcoming sessions
        var makeupAllocations = await db.MakeupAllocations
            .Include(ma => ma.MakeupCredit)
                .ThenInclude(mc => mc.StudentProfile)
                    .ThenInclude(sp => sp.User)
            .Include(ma => ma.TargetSession)
                .ThenInclude(s => s.Class)
            .Include(ma => ma.TargetSession)
                .ThenInclude(s => s.PlannedRoom)
            .Where(ma =>
                ma.TargetSession.Status == SessionStatus.Scheduled &&
                ma.TargetSession.PlannedDatetime >= reminderTime &&
                ma.TargetSession.PlannedDatetime <= reminderTimeEnd)
            .ToListAsync(cancellationToken);

        foreach (var allocation in makeupAllocations)
        {
            var studentProfile = allocation.MakeupCredit.StudentProfile;
            if (studentProfile?.User == null || string.IsNullOrWhiteSpace(studentProfile.User.Email))
                continue;

            var session = allocation.TargetSession;
            session.Raise(new MakeupReminderDomainEvent(
                session.Id,
                studentProfile.UserId,
                studentProfile.Id,
                session.Class?.Title ?? "Buổi bù",
                session.PlannedDatetime,
                session.Class?.Title,
                "Rex English Center",
                studentProfile.DisplayName,
                session.PlannedRoom?.Name
            ));
        }

        if (makeupAllocations.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Raised {Count} makeup reminder events", makeupAllocations.Count);
        }
    }

    private async Task SendMissionRemindersAsync(IDbContext db, DateTime now, CancellationToken cancellationToken)
    {
        var reminderTime = now.Add(MissionReminderWindow);
        var reminderTimeEnd = reminderTime.AddHours(1);

        var missions = await db.Missions
            .Include(m => m.TargetClass)
                .ThenInclude(c => c.ClassEnrollments)
                    .ThenInclude(ce => ce.StudentProfile)
                        .ThenInclude(sp => sp.User)
            .Where(m =>
                m.EndAt.HasValue &&
                m.EndAt.Value >= reminderTime &&
                m.EndAt.Value <= reminderTimeEnd &&
                (m.StartAt == null || m.StartAt <= now))
            .ToListAsync(cancellationToken);

        foreach (var mission in missions)
        {
            if (mission.TargetClass == null)
                continue;

            var enrollments = mission.TargetClass.ClassEnrollments
                .Where(ce => ce.Status == EnrollmentStatus.Active)
                .ToList();

            foreach (var enrollment in enrollments)
            {
                var studentProfile = enrollment.StudentProfile;
                if (studentProfile?.User == null)
                    continue;

                mission.Raise(new MissionReminderDomainEvent(
                    mission.Id,
                    studentProfile.UserId,
                    studentProfile.Id,
                    mission.Title,
                    mission.EndAt,
                    mission.TargetClass.Title,
                    studentProfile.DisplayName
                ));
            }
        }

        if (missions.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Raised {Count} mission reminder events", missions.Count);
        }
    }

    private async Task SendMediaRemindersAsync(IDbContext db, DateTime now, CancellationToken cancellationToken)
    {
        var reminderTime = now.Subtract(MediaReminderWindow);

        var media = await db.MediaAssets
            .Include(m => m.Class)
                .ThenInclude(c => c.ClassEnrollments)
                    .ThenInclude(ce => ce.StudentProfile)
                        .ThenInclude(sp => sp.User)
            .Where(m =>
                m.CreatedAt >= reminderTime &&
                m.Visibility == Visibility.ClassOnly &&
                m.ApprovalStatus == ApprovalStatus.Approved &&
                !m.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var mediaItem in media)
        {
            if (mediaItem.Class == null)
                continue;

            var enrollments = mediaItem.Class.ClassEnrollments
                .Where(ce => ce.Status == EnrollmentStatus.Active)
                .ToList();

            foreach (var enrollment in enrollments)
            {
                var studentProfile = enrollment.StudentProfile;
                if (studentProfile?.User == null || string.IsNullOrWhiteSpace(studentProfile.User.Email))
                    continue;

                mediaItem.Raise(new MediaReminderDomainEvent(
                    mediaItem.Id,
                    studentProfile.UserId,
                    studentProfile.Id,
                    mediaItem.Caption ?? "Media mới",
                    VietnameseEnumText.ForMediaType(mediaItem.Type),
                    mediaItem.Class?.Title,
                    studentProfile.DisplayName
                ));
            }
        }

        if (media.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Raised {Count} media reminder events", media.Count);
        }
    }
}

