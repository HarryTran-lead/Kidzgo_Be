using Kidzgo.Application.ProgramProgressions.Shared;

namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionRules;

public sealed class GetProgramProgressionRulesResponse
{
    public List<ProgramProgressionRuleDto> Rules { get; init; } = new();
    public int TotalCount { get; init; }
}
