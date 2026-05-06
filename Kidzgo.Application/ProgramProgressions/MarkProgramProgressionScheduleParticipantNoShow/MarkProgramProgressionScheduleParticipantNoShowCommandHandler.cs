using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions;
using Kidzgo.Domain.ProgramProgressions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.MarkProgramProgressionScheduleParticipantNoShow;

public sealed class MarkProgramProgressionScheduleParticipantNoShowCommandHandler(
    IDbContext context)
    : ICommandHandler<MarkProgramProgressionScheduleParticipantNoShowCommand, ProgramProgressionScheduleDto>
{
    public async Task<Result<ProgramProgressionScheduleDto>> Handle(
        MarkProgramProgressionScheduleParticipantNoShowCommand command,
        CancellationToken cancellationToken)
    {
        var participant = await context.ProgramProgressionScheduleParticipants
            .Include(p => p.Schedule)
                .ThenInclude(schedule => schedule.Participants)
            .FirstOrDefaultAsync(p => p.Id == command.ParticipantId, cancellationToken);

        if (participant is null)
        {
            return Result.Failure<ProgramProgressionScheduleDto>(
                ProgramProgressionErrors.ScheduleParticipantNotFound(command.ParticipantId));
        }

        if (participant.Schedule.Status != ProgramProgressionScheduleStatus.Scheduled ||
            participant.Status != ProgramProgressionScheduleParticipantStatus.Scheduled)
        {
            return Result.Failure<ProgramProgressionScheduleDto>(
                ProgramProgressionErrors.ScheduleParticipantCannotBeMarkedNoShow(
                    participant.Id,
                    participant.Status.ToString()));
        }

        var now = VietnamTime.UtcNow();
        participant.Status = ProgramProgressionScheduleParticipantStatus.NoShow;
        participant.UpdatedAt = now;

        if (participant.Schedule.Participants.All(p =>
                p.Id == participant.Id ||
                p.Status != ProgramProgressionScheduleParticipantStatus.Scheduled))
        {
            participant.Schedule.Status = ProgramProgressionScheduleStatus.Completed;
            participant.Schedule.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        var schedule = await ProgramProgressionScheduleReadQuery.Build(context)
            .FirstAsync(s => s.Id == participant.ScheduleId, cancellationToken);

        return Result.Success(schedule.ToDto());
    }
}
