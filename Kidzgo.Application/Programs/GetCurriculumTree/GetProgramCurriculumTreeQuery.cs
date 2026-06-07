using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Programs.GetCurriculumTree;

public sealed class GetProgramCurriculumTreeQuery : IQuery<GetProgramCurriculumTreeResponse>
{
    public Guid ProgramId { get; init; }
}

public sealed class GetProgramCurriculumTreeResponse
{
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public string ProgramCode { get; init; } = null!;
    public bool IsActive { get; init; }
    public IReadOnlyList<ProgramCurriculumTreeLevelDto> Levels { get; init; } = [];
}

public sealed class ProgramCurriculumTreeLevelDto
{
    public Guid LevelId { get; init; }
    public string LevelCode { get; init; } = null!;
    public string LevelName { get; init; } = null!;
    public int LevelOrderIndex { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<ProgramCurriculumTreeModuleDto> Modules { get; init; } = [];
}

public sealed class ProgramCurriculumTreeModuleDto
{
    public Guid ModuleId { get; init; }
    public string ModuleCode { get; init; } = null!;
    public string ModuleName { get; init; } = null!;
    public int ModuleOrderIndex { get; init; }
    public string ModuleType { get; init; } = null!;
    public bool IsActive { get; init; }
    public IReadOnlyList<ProgramCurriculumTreeUnitDto> Units { get; init; } = [];
}

public sealed class ProgramCurriculumTreeUnitDto
{
    public Guid? UnitId { get; init; }
    public string UnitKey { get; init; } = null!;
    public string UnitName { get; init; } = null!;
    public int? UnitNumber { get; init; }
    public string? UnitTitle { get; init; }
    public int UnitOrderIndex { get; init; }
    public bool IsSynthetic { get; init; }
    public IReadOnlyList<ProgramCurriculumTreeSyllabusDto> Syllabuses { get; init; } = [];
}

public sealed class ProgramCurriculumTreeSyllabusDto
{
    public Guid SyllabusId { get; init; }
    public string SyllabusCode { get; init; } = null!;
    public int Version { get; init; }
    public string SyllabusTitle { get; init; } = null!;
    public bool IsActive { get; init; }
    public IReadOnlyList<ProgramCurriculumTreeLessonTemplateDto> LessonTemplates { get; init; } = [];
}

public sealed class ProgramCurriculumTreeLessonTemplateDto
{
    public Guid LessonTemplateId { get; init; }
    public Guid? SessionTemplateId { get; init; }
    public string? Title { get; init; }
    public string LessonType { get; init; } = null!;
    public int SessionIndex { get; init; }
    public int SessionOrder { get; init; }
    public int? SessionIndexInModule { get; init; }
    public int OrderIndex { get; init; }
    public bool IsActive { get; init; }
}
