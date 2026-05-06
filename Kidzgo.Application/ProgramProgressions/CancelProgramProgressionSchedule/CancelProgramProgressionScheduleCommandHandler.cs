using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions;
using Kidzgo.Domain.ProgramProgressions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.CancelProgramProgressionSchedule;

public sealed class CancelProgramProgressionScheduleCommandHandler(
    IDbContext context,
    ProgramProgressionScheduleNotificationService notificationService)
    : ICommandHandler<CancelProgramProgressionScheduleCommand, ProgramProgressionScheduleDto>
{
    public async Task<Result<ProgramProgressionScheduleDto>> Handle(
        CancelProgramProgressionScheduleCommand command,
        CancellationToken cancellationToken)
    {
        var schedule = await context.ProgramProgressionSchedules
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

        var now = VietnamTime.UtcNow();
        schedule.Status = ProgramProgressionScheduleStatus.Cancelled;
        schedule.UpdatedAt = now;

        foreach (var participant in schedule.Participants)
        {
            participant.Status = ProgramProgressionScheduleParticipantStatus.Cancelled;
            participant.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        var cancelledSchedule = await ProgramProgressionScheduleReadQuery.Build(context)
            .FirstAsync(s => s.Id == schedule.Id, cancellationToken);

        await notificationService.NotifyCancelledAsync(cancelledSchedule, cancellationToken);

        return Result.Success(cancelledSchedule.ToDto());
    }
}
