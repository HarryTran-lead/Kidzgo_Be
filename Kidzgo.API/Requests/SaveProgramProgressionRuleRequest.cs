using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.ProgramProgressions;

namespace Kidzgo.API.Requests;

public sealed class SaveProgramProgressionRuleRequest
{
    public Guid SourceProgramId { get; set; }
    public Guid? TargetProgramId { get; set; }
    public ProgramProgressionMethod Method { get; set; }
    public int? MinimumShieldCount { get; set; }
    public int? MinimumSkillShieldCount { get; set; }
    public decimal? MinimumOverallScore { get; set; }
    public bool CarryOverRemainingSessions { get; set; } = true;
    public bool StopCurrentEnrollmentOnApproval { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public List<ProgramProgressionShieldRange> ShieldMappings { get; set; } = new();
    public List<ProgramProgressionClassificationBand> ClassificationBands { get; set; } = new();
    public List<PracticeTestScoreMapping> PracticeTestScoreMappings { get; set; } = new();
}
