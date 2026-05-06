using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;

namespace Kidzgo.Application.ProgramProgressions.MarkProgramProgressionScheduleParticipantNoShow;

public sealed class MarkProgramProgressionScheduleParticipantNoShowCommand : ICommand<ProgramProgressionScheduleDto>
{
    public Guid ParticipantId { get; init; }
}
