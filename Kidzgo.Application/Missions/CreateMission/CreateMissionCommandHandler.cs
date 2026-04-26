using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Missions.Shared;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Kidzgo.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Missions.CreateMission;

public sealed class CreateMissionCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateMissionCommand, CreateMissionResponse>
{
    public async Task<Result<CreateMissionResponse>> Handle(
        CreateMissionCommand command,
        CancellationToken cancellationToken)
    {
        // Validate scope requirements
        if (command.Scope == MissionScope.Class && !command.TargetClassId.HasValue)
        {
            return Result.Failure<CreateMissionResponse>(
                MissionErrors.InvalidScope);
        }

        if (command.Scope == MissionScope.Student && !command.TargetStudentId.HasValue)
        {
            return Result.Failure<CreateMissionResponse>(
                MissionErrors.InvalidScope);
        }

        if (command.Scope == MissionScope.Group &&
            (command.TargetGroup == null || command.TargetGroup.Count == 0))
        {
            return Result.Failure<CreateMissionResponse>(
                MissionErrors.InvalidScope);
        }

        // Validate date range
        if (command.StartAt.HasValue && command.EndAt.HasValue && command.EndAt.Value <= command.StartAt.Value)
        {
            return Result.Failure<CreateMissionResponse>(
                MissionErrors.InvalidDateRange);
        }

        // Verify class exists if scope is Class
        if (command.Scope == MissionScope.Class && command.TargetClassId.HasValue)
        {
            var classExists = await context.Classes
                .AnyAsync(c => c.Id == command.TargetClassId.Value, cancellationToken);

            if (!classExists)
            {
                return Result.Failure<CreateMissionResponse>(
                    MissionErrors.ClassNotFound);
            }
        }

        // Verify student profile exists if scope is Student
        if (command.Scope == MissionScope.Student && command.TargetStudentId.HasValue)
        {
            var studentExists = await context.Profiles
                .AnyAsync(p => p.Id == command.TargetStudentId.Value, cancellationToken);

            if (!studentExists)
            {
                return Result.Failure<CreateMissionResponse>(
                    MissionErrors.StudentNotFound);
            }
        }

        // Verify student profiles exist if scope is Group
        if (command.Scope == MissionScope.Group && command.TargetGroup != null && command.TargetGroup.Count > 0)
        {
            var existingStudentIds = await context.Profiles
                .Where(p => command.TargetGroup.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            var missingIds = command.TargetGroup.Except(existingStudentIds).ToList();
            if (missingIds.Count > 0)
            {
                return Result.Failure<CreateMissionResponse>(
                    MissionErrors.SomeStudentsNotFound(missingIds.Count));
            }
        }

        var now = VietnamTime.UtcNow();
        var userId = userContext.UserId;

        var teacherScopeValidation = await TeacherMissionTargetGuard.EnsureActorCanManageTargetsAsync(
            context,
            userId,
            command.Scope,
            command.TargetClassId,
            command.TargetStudentId,
            command.TargetGroup,
            cancellationToken);

        if (teacherScopeValidation.IsFailure)
        {
            return Result.Failure<CreateMissionResponse>(teacherScopeValidation.Error);
        }

        var startAtUtc = VietnamTime.NormalizeToUtc(command.StartAt);
        var endAtUtc = VietnamTime.NormalizeToUtc(command.EndAt);

        var rewardRuleResult = await MissionRewardRuleResolver.ResolveActiveAsync(
            context,
            command.MissionType,
            command.ProgressMode,
            command.TotalRequired,
            cancellationToken);

        if (rewardRuleResult.IsFailure)
        {
            return Result.Failure<CreateMissionResponse>(rewardRuleResult.Error);
        }

        var rewardRule = rewardRuleResult.Value;

        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Title = command.Title,
            Description = command.Description,
            Scope = command.Scope,
            TargetClassId = command.TargetClassId,
            TargetStudentId = command.TargetStudentId,
            TargetGroup = command.TargetGroup,
            MissionType = command.MissionType,
            ProgressMode = command.ProgressMode,
            StartAt = startAtUtc,
            EndAt = endAtUtc,
            RewardStars = rewardRule.RewardStars,
            RewardExp = rewardRule.RewardExp,
            TotalRequired = rewardRule.TotalRequired,
            CreatedBy = userId,
            CreatedAt = now
        };

        context.Missions.Add(mission);

        // ============================================================
        // GIAI DOAN 1: Auto-create MissionProgress for all target students
        // ============================================================
        List<Guid> targetStudentIds = [];

        switch (command.Scope)
        {
            case MissionScope.Class when command.TargetClassId.HasValue:
                // Lay danh sach student profiles dang ACTIVE trong lop
                var missionStartDate = VietnamTime.ToVietnamDateOnly(startAtUtc ?? now);
                var missionEndDate = VietnamTime.ToVietnamDateOnly(endAtUtc ?? startAtUtc ?? now);

                targetStudentIds = await context.ClassEnrollments
                    .AsNoTracking()
                    .Where(e =>
                        e.ClassId == command.TargetClassId.Value &&
                        e.Status == EnrollmentStatus.Active &&
                        !context.PauseEnrollmentRequestHistories.Any(h =>
                            h.StudentProfileId == e.StudentProfileId &&
                            h.ClassId == e.ClassId &&
                            h.NewStatus == EnrollmentStatus.Paused &&
                            h.PauseEnrollmentRequest.Status == PauseEnrollmentRequestStatus.Approved &&
                            h.PauseFrom <= missionEndDate &&
                            h.PauseTo >= missionStartDate))
                    .Select(e => e.StudentProfileId)
                    .Distinct()
                    .ToListAsync(cancellationToken);
                break;

            case MissionScope.Student when command.TargetStudentId.HasValue:
                targetStudentIds = [command.TargetStudentId.Value];
                break;

            case MissionScope.Group when command.TargetGroup != null && command.TargetGroup.Count > 0:
                targetStudentIds = command.TargetGroup;
                break;
        }

        // Tao MissionProgress cho moi hoc sinh
        foreach (var studentProfileId in targetStudentIds)
        {
            var missionProgress = new MissionProgress
            {
                Id = Guid.NewGuid(),
                MissionId = mission.Id,
                StudentProfileId = studentProfileId,
                Status = MissionProgressStatus.Assigned,
                ProgressValue = 0,
                CompletedAt = null,
                VerifiedBy = null
            };

            context.MissionProgresses.Add(missionProgress);
        }

        // Tao in-app notifications de hoc sinh nhan duoc thong bao web ngay khi mission duoc tao.
        if (targetStudentIds.Count > 0)
        {
            var distinctStudentIds = targetStudentIds.Distinct().ToList();

            string? classTitle = null;
            if (mission.TargetClassId.HasValue)
            {
                classTitle = await context.Classes
                    .Where(c => c.Id == mission.TargetClassId.Value)
                    .Select(c => c.Title)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var recipients = await context.Profiles
                .Where(p => distinctStudentIds.Contains(p.Id) && p.IsActive && !p.IsDeleted)
                .Select(p => new
                {
                    p.Id,
                    p.UserId
                })
                .ToListAsync(cancellationToken);

            var notifications = recipients
                .Select(p => new Notification
                {
                    Id = Guid.NewGuid(),
                    RecipientUserId = p.UserId,
                    RecipientProfileId = p.Id,
                    Channel = NotificationChannel.InApp,
                    Title = $"Nhiệm vụ mới: {mission.Title}",
                    Content = BuildMissionNotificationContent(mission, classTitle),
                    Deeplink = "/missions",
                    Status = NotificationStatus.Pending,
                    CreatedAt = now,
                    TemplateId = mission.Id.ToString(),
                    TargetRole = "Student",
                    Kind = "system",
                    Priority = "normal",
                    SenderRole = "System",
                    SenderName = "Rex Centre",
                    ScopeClassId = mission.TargetClassId,
                    ScopeStudentProfileId = p.Id
                })
                .ToList();

            if (notifications.Count > 0)
            {
                context.Notifications.AddRange(notifications);
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return new CreateMissionResponse
        {
            Id = mission.Id,
            Title = mission.Title,
            Description = mission.Description,
            Scope = mission.Scope.ToString(),
            TargetClassId = mission.TargetClassId,
            TargetStudentId = mission.TargetStudentId,
            TargetGroup = mission.TargetGroup,
            MissionType = mission.MissionType.ToString(),
            ProgressMode = mission.ProgressMode.ToString(),
            StartAt = mission.StartAt,
            EndAt = mission.EndAt,
            RewardStars = mission.RewardStars,
            RewardExp = mission.RewardExp,
            TotalRequired = mission.TotalRequired,
            CreatedBy = mission.CreatedBy,
            CreatedAt = mission.CreatedAt
        };
    }

    private static string BuildMissionNotificationContent(Mission mission, string? classTitle)
    {
        var details = new List<string>();

        if (!string.IsNullOrWhiteSpace(mission.Description))
        {
            details.Add(mission.Description.Trim());
        }

        details.Add($"Loại nhiệm vụ: {mission.MissionType}.");
        details.Add($"Cách tính: {mission.ProgressMode}.");

        if (!string.IsNullOrWhiteSpace(classTitle))
        {
            details.Add($"Lớp áp dụng: {classTitle}.");
        }

        if (mission.TotalRequired.HasValue)
        {
            details.Add($"Mục tiêu: {mission.TotalRequired.Value}.");
        }

        if (mission.RewardStars.HasValue || mission.RewardExp.HasValue)
        {
            details.Add($"Thưởng: {mission.RewardStars ?? 0} sao, {mission.RewardExp ?? 0} XP.");
        }

        if (mission.EndAt.HasValue)
        {
            details.Add($"Kết thúc: {mission.EndAt.Value:dd/MM/yyyy HH:mm} UTC.");
        }

        details.Add("Hãy vào mục Nhiệm vụ để xem chi tiết và bắt đầu thực hiện.");

        return string.Join(" ", details);
    }
}
