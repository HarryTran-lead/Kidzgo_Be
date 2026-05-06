using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;

namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionRuleById;

public sealed class GetProgramProgressionRuleByIdQuery : IQuery<ProgramProgressionRuleDto>
{
    public Guid Id { get; init; }
}
