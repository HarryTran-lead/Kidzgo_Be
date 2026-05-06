using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.GetSessionAvailability;

public sealed class GetSessionAvailabilityQueryHandler(
    IDbContext context
) : IQueryHandler<GetSessionAvailabilityQuery, GetSessionAvailabilityResponse>
{
    private const int DefaultDurationMinutes = 60;

    public async Task<Result<GetSessionAvailabilityResponse>> Handle(
        GetSessionAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        if (query.ScheduledAt == default)
        {
            return Result.Failure<GetSessionAvailabilityResponse>(
                Error.Validation("Session.ScheduledAtRequired", "ScheduledAt is required"));
        }

        var duration = query.DurationMinutes.GetValueOrDefault(DefaultDurationMinutes);
        if (duration <= 0)
        {
            return Result.Failure<GetSessionAvailabilityResponse>(
                Error.Validation("Session.InvalidDuration", "DurationMinutes must be greater than 0"));
        }

        var scheduledAtUtc = VietnamTime.NormalizeToUtc(query.ScheduledAt);

        var teacherCandidates = await context.Users
            .AsNoTracking()
            .Where(u =>
                u.IsActive &&
                !u.IsDeleted &&
                u.Role == UserRole.Teacher &&
                (!query.BranchId.HasValue || u.BranchId == query.BranchId.Value))
            .OrderBy(u => u.Name)
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.Role,
                u.BranchId
            })
            .ToListAsync(cancellationToken);

        var teachers = new List<SessionAvailableTeacherDto>();
        foreach (var candidate in teacherCandidates)
        {
            var conflicts = await GetTeacherConflictsAsync(
                context,
                candidate.Id,
                scheduledAtUtc,
                duration,
                query.ExcludeSessionId,
                cancellationToken);

            var isAvailable = conflicts.Count == 0;
            if (!query.IncludeUnavailable && !isAvailable) continue;

            teachers.Add(new SessionAvailableTeacherDto
            {
                UserId = candidate.Id,
                Name = candidate.Name,
                Email = candidate.Email,
                Role = candidate.Role.ToString(),
                BranchId = candidate.BranchId,
                IsAvailable = isAvailable,
                Conflicts = query.IncludeUnavailable ? conflicts : []
            });
        }

        var roomCandidates = await context.Classrooms
            .AsNoTracking()
            .Where(r =>
                r.IsActive &&
                (!query.BranchId.HasValue || r.BranchId == query.BranchId.Value))
            .OrderBy(r => r.Name)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.BranchId,
                r.Capacity
            })
            .ToListAsync(cancellationToken);

        var rooms = new List<SessionAvailableRoomDto>();
        foreach (var candidate in roomCandidates)
        {
            var conflicts = await GetRoomConflictsAsync(
                context,
                candidate.Id,
                scheduledAtUtc,
                duration,
                query.ExcludeSessionId,
                cancellationToken);

            var isAvailable = conflicts.Count == 0;
            if (!query.IncludeUnavailable && !isAvailable) continue;

            rooms.Add(new SessionAvailableRoomDto
            {
                RoomId = candidate.Id,
                Name = candidate.Name,
                BranchId = candidate.BranchId,
                Capacity = candidate.Capacity,
                IsAvailable = isAvailable,
                Conflicts = query.IncludeUnavailable ? conflicts : []
            });
        }

        return new GetSessionAvailabilityResponse
        {
            ScheduledAt = scheduledAtUtc,
            EndAt = scheduledAtUtc.AddMinutes(duration),
            DurationMinutes = duration,
            Teachers = teachers,
            Rooms = rooms
        };
    }

    private static async Task<List<SessionScheduleConflict>> GetTeacherConflictsAsync(
        IDbContext context,
        Guid teacherId,
        DateTime start,
        int durationMinutes,
        Guid? excludeSessionId,
        CancellationToken cancellationToken)
    {
        var end = start.AddMinutes(durationMinutes);

        var sessionConflicts = await context.Sessions
            .AsNoTracking()
            .Where(s =>
                s.Status != SessionStatus.Cancelled &&
                (excludeSessionId == null || s.Id != excludeSessionId.Value) &&
                (s.PlannedTeacherId == teacherId ||
                 s.ActualTeacherId == teacherId ||
                 s.PlannedAssistantId == teacherId ||
                 s.ActualAssistantId == teacherId) &&
                (s.ActualDatetime ?? s.PlannedDatetime) < end &&
                (s.ActualDatetime ?? s.PlannedDatetime).AddMinutes(s.DurationMinutes) > start)
            .Select(s => new SessionScheduleConflict
            {
                Type = "ClassSession",
                ReferenceId = s.Id,
                Title = s.Class.Code + " - " + s.Class.Title,
                StartAt = s.ActualDatetime ?? s.PlannedDatetime,
                EndAt = (s.ActualDatetime ?? s.PlannedDatetime).AddMinutes(s.DurationMinutes)
            })
            .ToListAsync(cancellationToken);

        var placementTestConflicts = await context.PlacementTests
            .AsNoTracking()
            .Where(pt =>
                pt.InvigilatorUserId == teacherId &&
                pt.ScheduledAt.HasValue &&
                pt.Status == PlacementTestStatus.Scheduled &&
                pt.ScheduledAt.Value < end &&
                pt.ScheduledAt.Value.AddMinutes(pt.DurationMinutes) > start)
            .Select(pt => new SessionScheduleConflict
            {
                Type = "PlacementTest",
                ReferenceId = pt.Id,
                Title = "Placement test",
                StartAt = pt.ScheduledAt!.Value,
                EndAt = pt.ScheduledAt.Value.AddMinutes(pt.DurationMinutes)
            })
            .ToListAsync(cancellationToken);

        return sessionConflicts
            .Concat(placementTestConflicts)
            .OrderBy(c => c.StartAt)
            .ToList();
    }

    private static async Task<List<SessionScheduleConflict>> GetRoomConflictsAsync(
        IDbContext context,
        Guid roomId,
        DateTime start,
        int durationMinutes,
        Guid? excludeSessionId,
        CancellationToken cancellationToken)
    {
        var end = start.AddMinutes(durationMinutes);

        var sessionConflicts = await context.Sessions
            .AsNoTracking()
            .Where(s =>
                s.Status != SessionStatus.Cancelled &&
                (excludeSessionId == null || s.Id != excludeSessionId.Value) &&
                (s.PlannedRoomId == roomId || s.ActualRoomId == roomId) &&
                (s.ActualDatetime ?? s.PlannedDatetime) < end &&
                (s.ActualDatetime ?? s.PlannedDatetime).AddMinutes(s.DurationMinutes) > start)
            .Select(s => new SessionScheduleConflict
            {
                Type = "ClassSession",
                ReferenceId = s.Id,
                Title = s.Class.Code + " - " + s.Class.Title,
                StartAt = s.ActualDatetime ?? s.PlannedDatetime,
                EndAt = (s.ActualDatetime ?? s.PlannedDatetime).AddMinutes(s.DurationMinutes)
            })
            .ToListAsync(cancellationToken);

        var roomName = await context.Classrooms
            .AsNoTracking()
            .Where(r => r.Id == roomId)
            .Select(r => r.Name)
            .FirstOrDefaultAsync(cancellationToken);

        var normalizedRoomName = roomName?.Trim().ToLower();

        var placementTestConflicts = await context.PlacementTests
            .AsNoTracking()
            .Where(pt =>
                pt.ScheduledAt.HasValue &&
                pt.Status == PlacementTestStatus.Scheduled &&
                pt.ScheduledAt.Value < end &&
                pt.ScheduledAt.Value.AddMinutes(pt.DurationMinutes) > start &&
                (pt.RoomId == roomId ||
                 (pt.RoomId == null &&
                  normalizedRoomName != null &&
                  pt.Room != null &&
                  pt.Room.Trim().ToLower() == normalizedRoomName)))
            .Select(pt => new SessionScheduleConflict
            {
                Type = "PlacementTest",
                ReferenceId = pt.Id,
                Title = "Placement test",
                StartAt = pt.ScheduledAt!.Value,
                EndAt = pt.ScheduledAt.Value.AddMinutes(pt.DurationMinutes)
            })
            .ToListAsync(cancellationToken);

        return sessionConflicts
            .Concat(placementTestConflicts)
            .OrderBy(c => c.StartAt)
            .ToList();
    }
}
