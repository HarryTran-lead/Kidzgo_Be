using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions;
using Kidzgo.Domain.ProgramProgressions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.UpdateProgramProgressionSchedule;

public sealed class UpdateProgramProgressionScheduleCommandHandler(
    IDbContext context,
    ProgramProgressionScheduleNotificationService notificationService)
    : ICommandHandler<UpdateProgramProgressionScheduleCommand, ProgramProgressionScheduleDto>
{
    public async Task<Result<ProgramProgressionScheduleDto>> Handle(
        UpdateProgramProgressionScheduleCommand command,
        CancellationToken cancellationToken)
    {
        var schedule = await context.ProgramProgressionSchedules
            .Include(s => s.SourceClass)
            .Include(s => s.Participants)
            .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);

        if (schedule is null)
        {
            return Result.Failure<ProgramProgressionScheduleDto>(
                ProgramProgressionErrors.ScheduleNotFound(command.Id));
        }

        if (schedule.Status != ProgramProgressionScheduleStatus.Scheduled ||
            schedule.Participants.Any(participant => participant.Status != ProgramProgressionScheduleParticipantStatus.Scheduled))
        {
            return Result.Failure<ProgramProgressionScheduleDto>(
                ProgramProgressionErrors.ScheduleAlreadyProcessing(schedule.Id));
        }

        var effectiveDuration = command.DurationMinutes ?? schedule.DurationMinutes;
        if (effectiveDuration <= 0)
        {
            return Result.Failure<ProgramProgressionScheduleDto>(ProgramProgressionErrors.InvalidScheduleDuration);
        }

        var effectiveScheduledAt = command.ScheduledAt.HasValue
            ? VietnamTime.NormalizeToUtc(command.ScheduledAt.Value)
            : schedule.ScheduledAt;
        var effectiveTeacherUserId = command.AssignedTeacherUserId ?? schedule.AssignedTeacherUserId;
        var effectiveRoomId = command.RoomId ?? schedule.RoomId;

        var availability = await ProgramProgressionScheduleAvailability.EnsureScheduleAvailableAsync(
            context,
            schedule.SourceClass,
            effectiveTeacherUserId,
            effectiveRoomId,
            effectiveScheduledAt,
            effectiveDuration,
            excludeScheduleId: schedule.Id,
            cancellationToken);

        if (availability.IsFailure)
        {
            return Result.Failure<ProgramProgressionScheduleDto>(availability.Error);
        }

        schedule.ScheduledAt = effectiveScheduledAt;
        schedule.DurationMinutes = effectiveDuration;
        schedule.AssignedTeacherUserId = effectiveTeacherUserId;
        schedule.RoomId = effectiveRoomId;
        schedule.Notes = string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim();
        schedule.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        var updatedSchedule = await ProgramProgressionScheduleReadQuery.Build(context)
            .FirstAsync(s => s.Id == schedule.Id, cancellationToken);

        await notificationService.NotifyUpdatedAsync(updatedSchedule, cancellationToken);

        return Result.Success(updatedSchedule.ToDto());
    }
}
