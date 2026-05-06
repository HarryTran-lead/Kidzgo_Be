using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionScheduleAvailability;

public sealed class GetProgramProgressionScheduleAvailabilityQueryHandler(
    IDbContext context)
    : IQueryHandler<GetProgramProgressionScheduleAvailabilityQuery, GetProgramProgressionScheduleAvailabilityResponse>
{
    public async Task<Result<GetProgramProgressionScheduleAvailabilityResponse>> Handle(
        GetProgramProgressionScheduleAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        if (query.SourceClassId == Guid.Empty)
        {
            return Result.Failure<GetProgramProgressionScheduleAvailabilityResponse>(
                Error.Validation("ProgramProgression.SourceClassIdRequired", "SourceClassId is required."));
        }

        if (query.ScheduledAt == default)
        {
            return Result.Failure<GetProgramProgressionScheduleAvailabilityResponse>(
                Error.Validation("ProgramProgression.ScheduledAtRequired", "ScheduledAt is required."));
        }

        var sourceClass = await context.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.SourceClassId, cancellationToken);

        if (sourceClass is null)
        {
            return Result.Failure<GetProgramProgressionScheduleAvailabilityResponse>(
                ProgramProgressionErrors.SourceClassNotFound(query.SourceClassId));
        }

        var duration = ProgramProgressionScheduleAvailability.NormalizeDuration(query.DurationMinutes);
        if (duration <= 0)
        {
            return Result.Failure<GetProgramProgressionScheduleAvailabilityResponse>(
                ProgramProgressionErrors.InvalidScheduleDuration);
        }

        var scheduledAtUtc = VietnamTime.NormalizeToUtc(query.ScheduledAt);

        var teacherCandidates = await ProgramProgressionScheduleAvailability.GetTeacherCandidatesAsync(
            context,
            sourceClass,
            scheduledAtUtc,
            duration,
            query.ExcludeScheduleId,
            cancellationToken);

        var roomCandidates = await ProgramProgressionScheduleAvailability.GetRoomCandidatesAsync(
            context,
            scheduledAtUtc,
            duration,
            sourceClass.BranchId,
            query.ExcludeScheduleId,
            cancellationToken);

        return Result.Success(new GetProgramProgressionScheduleAvailabilityResponse
        {
            SourceClassId = sourceClass.Id,
            ScheduledAt = scheduledAtUtc,
            EndAt = scheduledAtUtc.AddMinutes(duration),
            DurationMinutes = duration,
            Teachers = teacherCandidates
                .Where(candidate => query.IncludeUnavailable || candidate.IsAvailable)
                .Select(candidate => new AvailableProgressionTeacherDto
                {
                    UserId = candidate.UserId,
                    Name = candidate.Name,
                    Email = candidate.Email,
                    BranchId = candidate.BranchId,
                    RoleInClass = candidate.RoleInClass,
                    IsAvailable = candidate.IsAvailable,
                    Conflicts = query.IncludeUnavailable
                        ? candidate.Conflicts.Select(MapConflict).ToList()
                        : new List<ProgramProgressionScheduleConflictDto>()
                })
                .ToList(),
            Rooms = roomCandidates
                .Where(candidate => query.IncludeUnavailable || candidate.IsAvailable)
                .Select(candidate => new AvailableProgressionRoomDto
                {
                    RoomId = candidate.RoomId,
                    Name = candidate.Name,
                    BranchId = candidate.BranchId,
                    Capacity = candidate.Capacity,
                    IsAvailable = candidate.IsAvailable,
                    Conflicts = query.IncludeUnavailable
                        ? candidate.Conflicts.Select(MapConflict).ToList()
                        : new List<ProgramProgressionScheduleConflictDto>()
                })
                .ToList()
        });
    }

    private static ProgramProgressionScheduleConflictDto MapConflict(PlacementTests.PlacementTestScheduleConflict conflict)
    {
        return new ProgramProgressionScheduleConflictDto
        {
            Type = conflict.Type,
            ReferenceId = conflict.ReferenceId,
            Title = conflict.Title,
            StartAt = conflict.StartAt,
            EndAt = conflict.EndAt
        };
    }
}
