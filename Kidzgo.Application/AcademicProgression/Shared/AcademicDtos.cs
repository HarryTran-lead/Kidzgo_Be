namespace Kidzgo.Application.AcademicProgression.Shared;

public sealed class LevelDto
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public int Order { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}

public sealed class ModuleDto
{
    public Guid Id { get; init; }
    public Guid LevelId { get; init; }
    public string LevelCode { get; init; } = null!;
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public int Order { get; init; }
    public string? Description { get; init; }
    public int PlannedSessionCount { get; init; }
    public int LessonPlanCount { get; init; }
    public bool IsActive { get; init; }
}

public sealed class StudentProgressDto
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public Guid ModuleId { get; init; }
    public string ModuleCode { get; init; } = null!;
    public string ModuleName { get; init; } = null!;
    public string LevelCode { get; init; } = null!;
    public string Status { get; init; } = null!;
    public decimal CompletionPercent { get; init; }
    public string AssessmentStatus { get; init; } = null!;
    public string PromotionStatus { get; init; } = null!;
    public Guid? LastAssessmentId { get; init; }
    public Guid? CurrentLessonPlanTemplateId { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public sealed class AssessmentDto
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public Guid ModuleId { get; init; }
    public string ModuleCode { get; init; } = null!;
    public string Type { get; init; } = null!;
    public decimal Score { get; init; }
    public string Result { get; init; } = null!;
    public string? TeacherComment { get; init; }
    public Guid AssessedBy { get; init; }
    public DateTime AssessedAt { get; init; }
}

public sealed class TeacherEvaluationDto
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public Guid ModuleId { get; init; }
    public string ModuleCode { get; init; } = null!;
    public int Speaking { get; init; }
    public int Listening { get; init; }
    public int Reading { get; init; }
    public int Writing { get; init; }
    public int Participation { get; init; }
    public int Confidence { get; init; }
    public int Behavior { get; init; }
    public string? Notes { get; init; }
    public Guid EvaluatedBy { get; init; }
    public DateTime EvaluatedAt { get; init; }
}

public sealed class PromotionDecisionDto
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public Guid ModuleId { get; init; }
    public string ModuleCode { get; init; } = null!;
    public string Decision { get; init; } = null!;
    public string? Reason { get; init; }
    public Guid ApprovedBy { get; init; }
    public DateTime ApprovedAt { get; init; }
}

public sealed class RemedialPlanDto
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public Guid ModuleId { get; init; }
    public string ModuleCode { get; init; } = null!;
    public string WeakSkills { get; init; } = null!;
    public int RecommendedSessionCount { get; init; }
    public string? Notes { get; init; }
    public Guid CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; }
}
