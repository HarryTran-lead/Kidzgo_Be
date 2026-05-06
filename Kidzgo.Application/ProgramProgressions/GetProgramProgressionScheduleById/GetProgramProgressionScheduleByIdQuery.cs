using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;

namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionScheduleById;

public sealed class GetProgramProgressionScheduleByIdQuery : IQuery<ProgramProgressionScheduleDto>
{
    public Guid Id { get; init; }
}
