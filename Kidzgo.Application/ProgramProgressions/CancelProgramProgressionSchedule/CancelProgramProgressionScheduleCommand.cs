using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;

namespace Kidzgo.Application.ProgramProgressions.CancelProgramProgressionSchedule;

public sealed class CancelProgramProgressionScheduleCommand : ICommand<ProgramProgressionScheduleDto>
{
    public Guid Id { get; init; }
}
