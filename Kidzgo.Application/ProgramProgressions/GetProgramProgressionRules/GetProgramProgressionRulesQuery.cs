using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionRules;

public sealed class GetProgramProgressionRulesQuery : IQuery<GetProgramProgressionRulesResponse>
{
    public Guid? SourceProgramId { get; init; }
    public bool? IsActive { get; init; }
}
