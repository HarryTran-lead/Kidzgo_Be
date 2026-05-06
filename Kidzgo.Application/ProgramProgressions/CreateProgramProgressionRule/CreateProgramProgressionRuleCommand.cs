using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.ProgramProgressions;

namespace Kidzgo.Application.ProgramProgressions.CreateProgramProgressionRule;

public sealed class CreateProgramProgressionRuleCommand : ICommand<ProgramProgressionRuleDto>
{
    public Guid SourceProgramId { get; init; }
    public Guid? TargetProgramId { get; init; }
    public ProgramProgressionMethod Method { get; init; }
    public int? MinimumShieldCount { get; init; }
    public int? MinimumSkillShieldCount { get; init; }
    public decimal? MinimumOverallScore { get; init; }
    public bool CarryOverRemainingSessions { get; init; } = true;
    public bool StopCurrentEnrollmentOnApproval { get; init; } = true;
    public bool IsActive { get; init; } = true;
    public string? Notes { get; init; }
    public IReadOnlyCollection<ProgramProgressionShieldRange> ShieldMappings { get; init; } = Array.Empty<ProgramProgressionShieldRange>();
    public IReadOnlyCollection<ProgramProgressionClassificationBand> ClassificationBands { get; init; } = Array.Empty<ProgramProgressionClassificationBand>();
}
