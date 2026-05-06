using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.ProgramProgressions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.Shared;

public sealed class ProgramProgressionScheduleNotificationService(IDbContext context)
{
    public Task NotifyCreatedAsync(ProgramProgressionSchedule schedule, CancellationToken cancellationToken)
        => NotifyAsync(schedule, "created", cancellationToken);

    public Task NotifyUpdatedAsync(ProgramProgressionSchedule schedule, CancellationToken cancellationToken)
        => NotifyAsync(schedule, "updated", cancellationToken);

    public Task NotifyCancelledAsync(ProgramProgressionSchedule schedule, CancellationToken cancellationToken)
        => NotifyAsync(schedule, "cancelled", cancellationToken);

    private async Task NotifyAsync(
        ProgramProgressionSchedule schedule,
        string action,
        CancellationToken cancellationToken)
    {
        var participants = schedule.Participants
            .OrderBy(participant => participant.StudentProfile.DisplayName)
            .ToList();

        if (participants.Count == 0)
        {
            return;
        }

        var studentProfileIds = participants
            .Select(participant => participant.StudentProfileId)
            .Distinct()
            .ToList();

        var parentRecipients = await context.ParentStudentLinks
            .AsNoTracking()
            .Where(link => studentProfileIds.Contains(link.StudentProfileId))
            .Select(link => new
            {
                link.StudentProfileId,
                ParentUserId = link.ParentProfile.UserId
            })
            .Where(link => link.ParentUserId != default)
            .Distinct()
            .ToListAsync(cancellationToken);

        var now = VietnamTime.UtcNow();
        var scheduleAtText = VietnamTime.ToVietnamDateTime(schedule.ScheduledAt).ToString("dd/MM/yyyy HH:mm");
        var roomText = string.IsNullOrWhiteSpace(schedule.Room?.Name)
            ? "chua cap nhat phong"
            : $"phong {schedule.Room.Name}";
        var deeplink = $"/program-progressions/assessment-schedules/{schedule.Id}";

        var (studentTitle, studentContentPrefix, teacherTitle, teacherContentPrefix) = action switch
        {
            "updated" => (
                "Lich kiem tra len chuong trinh da duoc cap nhat",
                "Lich kiem tra len chuong trinh cua hoc sinh",
                "Lich kiem tra len chuong trinh da duoc cap nhat",
                "Lich kiem tra len chuong trinh duoc phan cong cho ban da duoc cap nhat"),
            "cancelled" => (
                "Lich kiem tra len chuong trinh da bi huy",
                "Lich kiem tra len chuong trinh cua hoc sinh",
                "Lich kiem tra len chuong trinh da bi huy",
                "Lich kiem tra len chuong trinh duoc phan cong cho ban da bi huy"),
            _ => (
                "Lich kiem tra len chuong trinh da duoc tao",
                "Hoc sinh co lich kiem tra len chuong trinh",
                "Ban duoc phan cong lich kiem tra len chuong trinh",
                "Ban duoc phan cong coi va nhap ket qua kiem tra len chuong trinh")
        };

        var notifications = new List<Notification>();

        foreach (var participant in participants)
        {
            var studentContent =
                $"{studentContentPrefix} {participant.StudentProfile.DisplayName} vao luc {scheduleAtText} tai lop {schedule.SourceClass.Code} - {schedule.SourceClass.Title}, {roomText}.";

            if (participant.StudentProfile.UserId != Guid.Empty)
            {
                notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    RecipientUserId = participant.StudentProfile.UserId,
                    RecipientProfileId = participant.StudentProfileId,
                    Channel = NotificationChannel.InApp,
                    Title = studentTitle,
                    Content = studentContent,
                    Deeplink = deeplink,
                    Status = NotificationStatus.Pending,
                    CreatedAt = now,
                    TargetRole = "Student",
                    Kind = "program_progression_schedule",
                    Priority = "normal",
                    SenderRole = "System",
                    SenderName = "System",
                    ScopeBranchId = schedule.BranchId,
                    ScopeClassId = schedule.SourceClassId,
                    ScopeStudentProfileId = participant.StudentProfileId
                });
            }

            foreach (var parentRecipient in parentRecipients.Where(recipient => recipient.StudentProfileId == participant.StudentProfileId))
            {
                notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    RecipientUserId = parentRecipient.ParentUserId,
                    RecipientProfileId = participant.StudentProfileId,
                    Channel = NotificationChannel.InApp,
                    Title = studentTitle,
                    Content = studentContent,
                    Deeplink = deeplink,
                    Status = NotificationStatus.Pending,
                    CreatedAt = now,
                    TargetRole = "Parent",
                    Kind = "program_progression_schedule",
                    Priority = "normal",
                    SenderRole = "System",
                    SenderName = "System",
                    ScopeBranchId = schedule.BranchId,
                    ScopeClassId = schedule.SourceClassId,
                    ScopeStudentProfileId = participant.StudentProfileId
                });
            }
        }

        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = schedule.AssignedTeacherUserId,
            RecipientProfileId = null,
            Channel = NotificationChannel.InApp,
            Title = teacherTitle,
            Content = $"{teacherContentPrefix} vao luc {scheduleAtText} cho lop {schedule.SourceClass.Code} - {schedule.SourceClass.Title} voi {participants.Count} hoc sinh, {roomText}.",
            Deeplink = deeplink,
            Status = NotificationStatus.Pending,
            CreatedAt = now,
            TargetRole = "Teacher",
            Kind = "program_progression_schedule",
            Priority = "normal",
            SenderRole = "System",
            SenderName = "System",
            ScopeBranchId = schedule.BranchId,
            ScopeClassId = schedule.SourceClassId
        });

        if (notifications.Count == 0)
        {
            return;
        }

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync(cancellationToken);
    }
}
