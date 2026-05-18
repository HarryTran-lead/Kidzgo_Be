using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using DomainProgram = Kidzgo.Domain.Programs.Program;

namespace Kidzgo.Domain.ProgramProgressions;

public class ProgramProgressionRule : Entity
{
    public Guid Id { get; set; }
    public Guid SourceLevelId { get; set; }
    public Guid? TargetLevelId { get; set; }
    public Guid SourceProgramId { get; set; }
    public Guid? TargetProgramId { get; set; }
    public ProgramProgressionMethod Method { get; set; }
    public int? MinimumShieldCount { get; set; }
    public int? MinimumSkillShieldCount { get; set; }
    public decimal? MinimumOverallScore { get; set; }
    public bool CarryOverRemainingSessions { get; set; } = true;
    public bool StopCurrentEnrollmentOnApproval { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public string? ShieldMappingJson { get; set; }
    public string? ClassificationBandsJson { get; set; }
    public string? PracticeTestScoreMappingsJson { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public DomainProgram SourceProgram { get; set; } = null!;
    public DomainProgram? TargetProgram { get; set; }
    public Level SourceLevel { get; set; } = null!;
    public Level? TargetLevel { get; set; }
}
