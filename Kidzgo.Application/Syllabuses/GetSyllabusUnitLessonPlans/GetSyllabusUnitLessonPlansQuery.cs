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
    public int TotalModules { get; init; }
    public int TotalUnits { get; init; }
    public int TotalGroups { get; init; }
    public int TotalLessonPlans { get; init; }
    public IReadOnlyList<SyllabusModuleUnitLessonPlanGroupDto> Groups { get; init; } = [];
    public IReadOnlyList<SyllabusUnitLessonPlanDto> OrphanLessons { get; init; } = [];
}

public sealed class SyllabusModuleUnitLessonPlanGroupDto
{
    public Guid ModuleId { get; init; }
    public string ModuleCode { get; init; } = null!;
    public string ModuleName { get; init; } = null!;
    public int ModuleOrder { get; init; }
    public int ModuleOrderIndex { get; init; }
    public int UnitCount { get; init; }
    public int LessonPlanCount { get; init; }
    public IReadOnlyList<SyllabusLessonPlanUnitDto> Units { get; init; } = [];
}

public sealed class SyllabusLessonPlanUnitDto
{
    public Guid UnitId { get; init; }
    public string UnitName { get; init; } = null!;
    public int OrderIndex { get; init; }
    public int UnitOrderIndex { get; init; }
    public int? UnitNumber { get; init; }
    public string? UnitTitle { get; init; }
    public int LessonPlanCount { get; init; }
    public IReadOnlyList<SyllabusUnitLessonPlanDto> Lessons { get; init; } = [];
}

public sealed class SyllabusUnitLessonPlanDto
{
    public Guid LessonPlanTemplateId { get; init; }
    public Guid ModuleId { get; init; }
    public int ModuleOrderIndex { get; init; }
    public Guid? LessonPlanUnitId { get; init; }
    public Guid? UnitId { get; init; }
    public int? UnitOrderIndex { get; init; }
    public int? UnitNumber { get; init; }
    public string? UnitTitle { get; init; }
    public Guid? SessionTemplateId { get; init; }
    public string? Title { get; init; }
    public int? LessonNumber { get; init; }
    public int SessionIndex { get; init; }
    public int SessionOrder { get; init; }
    public int? SessionIndexInModule { get; init; }
    public string? SessionTitle { get; init; }
    public string? SessionTopic { get; init; }
    public string? SourceFileName { get; init; }
    public int OrderIndexInUnit { get; init; }
    public int LessonOrderIndexInUnit { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
