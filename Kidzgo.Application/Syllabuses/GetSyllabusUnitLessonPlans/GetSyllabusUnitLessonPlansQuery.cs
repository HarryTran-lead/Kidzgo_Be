using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Syllabuses.GetSyllabusUnitLessonPlans;

public sealed class GetSyllabusUnitLessonPlansQuery : IQuery<GetSyllabusUnitLessonPlansResponse>
{
    public Guid SyllabusId { get; init; }
}

public sealed class GetSyllabusUnitLessonPlansResponse
{
    public Guid SyllabusId { get; init; }
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid LevelId { get; init; }
    public string LevelName { get; init; } = null!;
    public int TotalGroups { get; init; }
    public int TotalLessonPlans { get; init; }
    public IReadOnlyList<SyllabusUnitLessonPlanGroupDto> Groups { get; init; } = [];
}

public sealed class SyllabusUnitLessonPlanGroupDto
{
    public string GroupKey { get; init; } = null!;
    public string GroupType { get; init; } = null!;
    public int? UnitNumber { get; init; }
    public int? RevisionNumber { get; init; }
    public string DisplayName { get; init; } = null!;
    public Guid ModuleId { get; init; }
    public string ModuleCode { get; init; } = null!;
    public string ModuleName { get; init; } = null!;
    public int ModuleOrder { get; init; }
    public int LessonPlanCount { get; init; }
    public IReadOnlyList<SyllabusUnitLessonPlanDto> LessonPlans { get; init; } = [];
}

public sealed class SyllabusUnitLessonPlanDto
{
    public Guid LessonPlanTemplateId { get; init; }
    public Guid? SessionTemplateId { get; init; }
    public string? Title { get; init; }
    public int? LessonNumber { get; init; }
    public int SessionIndex { get; init; }
    public int SessionOrder { get; init; }
    public int? SessionIndexInModule { get; init; }
    public string? SessionTitle { get; init; }
    public string? SessionTopic { get; init; }
    public string? SourceFileName { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
