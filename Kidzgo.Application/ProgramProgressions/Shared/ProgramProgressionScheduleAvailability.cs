using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.PlacementTests;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions;
using Kidzgo.Domain.ProgramProgressions.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.Shared;

internal static class ProgramProgressionScheduleAvailability
{
    public const int DefaultDurationMinutes = PlacementTestScheduleAvailability.DefaultDurationMinutes;

    public static int NormalizeDuration(int? durationMinutes)
        => durationMinutes.GetValueOrDefault(DefaultDurationMinutes);

    public static async Task<Result> EnsureTeacherAssignableAsync(
        IDbContext context,
        Class sourceClass,
        Guid teacherUserId,
        CancellationToken cancellationToken)
    {
        var teacher = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == teacherUserId && user.IsActive && !user.IsDeleted)
            .Select(user => new
            {
                user.Id,
                user.Role
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (teacher is null || teacher.Role != UserRole.Teacher)
        {
            return Result.Failure(ProgramProgressionErrors.AssignedTeacherMustTeachClass(teacherUserId, sourceClass.Id));
        }

        return sourceClass.MainTeacherId == teacherUserId || sourceClass.AssistantTeacherId == teacherUserId
            ? Result.Success()
            : Result.Failure(ProgramProgressionErrors.AssignedTeacherMustTeachClass(teacherUserId, sourceClass.Id));
    }

    public static async Task<Result> EnsureRoomAssignableAsync(
        IDbContext context,
        Guid? roomId,
        Guid branchId,
        CancellationToken cancellationToken)
    {
        if (!roomId.HasValue)
        {
            return Result.Success();
        }

        var room = await context.Classrooms
            .AsNoTracking()
            .Where(classroom => classroom.Id == roomId.Value && classroom.IsActive)
            .Select(classroom => new
            {
                classroom.Id,
                classroom.BranchId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (room is null)
        {
            return Result.Failure(Error.NotFound("ProgramProgression.RoomNotFound", $"Room '{roomId.Value}' was not found."));
        }

        return room.BranchId == branchId
            ? Result.Success()
            : Result.Failure(ProgramProgressionErrors.RoomBranchMismatch(roomId.Value, branchId));
    }

    public static async Task<Result> EnsureScheduleAvailableAsync(
        IDbContext context,
        Class sourceClass,
        Guid teacherUserId,
        Guid? roomId,
        DateTime scheduledAt,
        int durationMinutes,
        Guid? excludeScheduleId,
        CancellationToken cancellationToken)
    {
        if (durationMinutes <= 0)
        {
            return Result.Failure(ProgramProgressionErrors.InvalidScheduleDuration);
        }

        var teacherAssignable = await EnsureTeacherAssignableAsync(context, sourceClass, teacherUserId, cancellationToken);
        if (teacherAssignable.IsFailure)
        {
            return teacherAssignable;
        }

        var teacherAvailable = await EnsureTeacherAvailableAsync(
            context,
            teacherUserId,
            scheduledAt,
            durationMinutes,
            excludeScheduleId,
            cancellationToken);
        if (teacherAvailable.IsFailure)
        {
            return teacherAvailable;
        }

        return await EnsureRoomAvailableAsync(
            context,
            roomId,
            sourceClass.BranchId,
            scheduledAt,
            durationMinutes,
            excludeScheduleId,
            cancellationToken);
    }

    public static async Task<List<ProgramProgressionTeacherAvailabilityCandidate>> GetTeacherCandidatesAsync(
        IDbContext context,
        Class sourceClass,
        DateTime scheduledAt,
        int durationMinutes,
        Guid? excludeScheduleId,
        CancellationToken cancellationToken)
    {
        var teacherIds = new[] { sourceClass.MainTeacherId, sourceClass.AssistantTeacherId }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (teacherIds.Count == 0)
        {
            return [];
        }

        var teachers = await context.Users
            .AsNoTracking()
            .Where(user => teacherIds.Contains(user.Id) && user.IsActive && !user.IsDeleted && user.Role == UserRole.Teacher)
            .Select(user => new
            {
                user.Id,
                user.Name,
                user.Email,
                user.BranchId
            })
            .ToListAsync(cancellationToken);

        var candidates = new List<ProgramProgressionTeacherAvailabilityCandidate>();

        foreach (var teacher in teachers)
        {
            var conflicts = await GetTeacherConflictsAsync(
                context,
                teacher.Id,
                scheduledAt,
                durationMinutes,
                excludeScheduleId,
                cancellationToken);

            candidates.Add(new ProgramProgressionTeacherAvailabilityCandidate
            {
                UserId = teacher.Id,
                Name = teacher.Name,
                Email = teacher.Email,
                BranchId = teacher.BranchId,
                RoleInClass = teacher.Id == sourceClass.MainTeacherId
                    ? "MainTeacher"
                    : "AssistantTeacher",
                IsAvailable = conflicts.Count == 0,
                Conflicts = conflicts
            });
        }

        return candidates
            .OrderBy(candidate => candidate.RoleInClass)
            .ThenBy(candidate => candidate.Name)
            .ToList();
    }

    public static async Task<List<ProgramProgressionRoomAvailabilityCandidate>> GetRoomCandidatesAsync(
        IDbContext context,
        DateTime scheduledAt,
        int durationMinutes,
        Guid branchId,
        Guid? excludeScheduleId,
        CancellationToken cancellationToken)
    {
        var rooms = await context.Classrooms
            .AsNoTracking()
            .Where(room => room.IsActive && room.BranchId == branchId)
            .OrderBy(room => room.Name)
            .Select(room => new
            {
                room.Id,
                room.Name,
                room.BranchId,
                room.Capacity
            })
            .ToListAsync(cancellationToken);

        var candidates = new List<ProgramProgressionRoomAvailabilityCandidate>();

        foreach (var room in rooms)
        {
            var conflicts = await GetRoomConflictsAsync(
                context,
                room.Id,
                scheduledAt,
                durationMinutes,
                excludeScheduleId,
                cancellationToken);

            candidates.Add(new ProgramProgressionRoomAvailabilityCandidate
            {
                RoomId = room.Id,
                Name = room.Name,
                BranchId = room.BranchId,
                Capacity = room.Capacity,
                IsAvailable = conflicts.Count == 0,
                Conflicts = conflicts
            });
        }

        return candidates;
    }

    private static async Task<Result> EnsureTeacherAvailableAsync(
        IDbContext context,
        Guid teacherUserId,
        DateTime scheduledAt,
        int durationMinutes,
        Guid? excludeScheduleId,
        CancellationToken cancellationToken)
    {
        var placementAndSessionConflicts = await PlacementTestScheduleAvailability.GetInvigilatorConflictsAsync(
            context,
            teacherUserId,
            scheduledAt,
            durationMinutes,
            excludePlacementTestId: null,
            cancellationToken);

        var progressionConflicts = await GetTeacherProgressionConflictsAsync(
            context,
            teacherUserId,
            scheduledAt,
            durationMinutes,
            excludeScheduleId,
            cancellationToken);

        if (placementAndSessionConflicts.Count > 0 || progressionConflicts.Count > 0)
        {
            return Result.Failure(ProgramProgressionErrors.AssignedTeacherUnavailable(teacherUserId));
        }

        return Result.Success();
    }

    private static async Task<Result> EnsureRoomAvailableAsync(
        IDbContext context,
        Guid? roomId,
        Guid branchId,
        DateTime scheduledAt,
        int durationMinutes,
        Guid? excludeScheduleId,
        CancellationToken cancellationToken)
    {
        var roomAssignable = await EnsureRoomAssignableAsync(context, roomId, branchId, cancellationToken);
        if (roomAssignable.IsFailure)
        {
            return roomAssignable;
        }

        if (!roomId.HasValue)
        {
            return Result.Success();
        }

        var placementAndSessionConflicts = await PlacementTestScheduleAvailability.GetRoomConflictsAsync(
            context,
            roomId.Value,
            scheduledAt,
            durationMinutes,
            excludePlacementTestId: null,
            cancellationToken);

        var progressionConflicts = await GetRoomProgressionConflictsAsync(
            context,
            roomId.Value,
            scheduledAt,
            durationMinutes,
            excludeScheduleId,
            cancellationToken);

        if (placementAndSessionConflicts.Count > 0 || progressionConflicts.Count > 0)
        {
            return Result.Failure(ProgramProgressionErrors.RoomUnavailable(roomId.Value));
        }

        return Result.Success();
    }

    private static async Task<List<PlacementTestScheduleConflict>> GetTeacherConflictsAsync(
        IDbContext context,
        Guid teacherUserId,
        DateTime scheduledAt,
        int durationMinutes,
        Guid? excludeScheduleId,
        CancellationToken cancellationToken)
    {
        var placementAndSessionConflicts = await PlacementTestScheduleAvailability.GetInvigilatorConflictsAsync(
            context,
            teacherUserId,
            scheduledAt,
            durationMinutes,
            excludePlacementTestId: null,
            cancellationToken);

        var progressionConflicts = await GetTeacherProgressionConflictsAsync(
            context,
            teacherUserId,
            scheduledAt,
            durationMinutes,
            excludeScheduleId,
            cancellationToken);

        return placementAndSessionConflicts
            .Concat(progressionConflicts)
            .OrderBy(conflict => conflict.StartAt)
            .ToList();
    }

    private static async Task<List<PlacementTestScheduleConflict>> GetRoomConflictsAsync(
        IDbContext context,
        Guid roomId,
        DateTime scheduledAt,
        int durationMinutes,
        Guid? excludeScheduleId,
        CancellationToken cancellationToken)
    {
        var placementAndSessionConflicts = await PlacementTestScheduleAvailability.GetRoomConflictsAsync(
            context,
            roomId,
            scheduledAt,
            durationMinutes,
            excludePlacementTestId: null,
            cancellationToken);

        var progressionConflicts = await GetRoomProgressionConflictsAsync(
            context,
            roomId,
            scheduledAt,
            durationMinutes,
            excludeScheduleId,
            cancellationToken);

        return placementAndSessionConflicts
            .Concat(progressionConflicts)
            .OrderBy(conflict => conflict.StartAt)
            .ToList();
    }

    private static async Task<List<PlacementTestScheduleConflict>> GetTeacherProgressionConflictsAsync(
        IDbContext context,
        Guid teacherUserId,
        DateTime scheduledAt,
        int durationMinutes,
        Guid? excludeScheduleId,
        CancellationToken cancellationToken)
    {
        var start = VietnamTime.NormalizeToUtc(scheduledAt);
        var end = start.AddMinutes(durationMinutes);

        return await context.ProgramProgressionSchedules
            .AsNoTracking()
            .Where(schedule =>
                schedule.Id != excludeScheduleId &&
                schedule.AssignedTeacherUserId == teacherUserId &&
                schedule.Status == ProgramProgressionScheduleStatus.Scheduled &&
                schedule.ScheduledAt < end &&
                schedule.ScheduledAt.AddMinutes(schedule.DurationMinutes) > start)
            .Select(schedule => new PlacementTestScheduleConflict
            {
                Type = "ProgramProgressionSchedule",
                ReferenceId = schedule.Id,
                Title = schedule.SourceClass.Code + " - " + schedule.SourceClass.Title,
                StartAt = schedule.ScheduledAt,
                EndAt = schedule.ScheduledAt.AddMinutes(schedule.DurationMinutes)
            })
            .ToListAsync(cancellationToken);
    }

    private static async Task<List<PlacementTestScheduleConflict>> GetRoomProgressionConflictsAsync(
        IDbContext context,
        Guid roomId,
        DateTime scheduledAt,
        int durationMinutes,
        Guid? excludeScheduleId,
        CancellationToken cancellationToken)
    {
        var start = VietnamTime.NormalizeToUtc(scheduledAt);
        var end = start.AddMinutes(durationMinutes);

        return await context.ProgramProgressionSchedules
            .AsNoTracking()
            .Where(schedule =>
                schedule.Id != excludeScheduleId &&
                schedule.RoomId == roomId &&
                schedule.Status == ProgramProgressionScheduleStatus.Scheduled &&
                schedule.ScheduledAt < end &&
                schedule.ScheduledAt.AddMinutes(schedule.DurationMinutes) > start)
            .Select(schedule => new PlacementTestScheduleConflict
            {
                Type = "ProgramProgressionSchedule",
                ReferenceId = schedule.Id,
                Title = schedule.SourceClass.Code + " - " + schedule.SourceClass.Title,
                StartAt = schedule.ScheduledAt,
                EndAt = schedule.ScheduledAt.AddMinutes(schedule.DurationMinutes)
            })
            .ToListAsync(cancellationToken);
    }
}

internal sealed class ProgramProgressionTeacherAvailabilityCandidate
{
    public Guid UserId { get; init; }
    public string? Name { get; init; }
    public string Email { get; init; } = null!;
    public Guid? BranchId { get; init; }
    public string RoleInClass { get; init; } = null!;
    public bool IsAvailable { get; init; }
    public List<PlacementTestScheduleConflict> Conflicts { get; init; } = new();
}

internal sealed class ProgramProgressionRoomAvailabilityCandidate
{
    public Guid RoomId { get; init; }
    public string Name { get; init; } = null!;
    public Guid BranchId { get; init; }
    public int Capacity { get; init; }
    public bool IsAvailable { get; init; }
    public List<PlacementTestScheduleConflict> Conflicts { get; init; } = new();
}
