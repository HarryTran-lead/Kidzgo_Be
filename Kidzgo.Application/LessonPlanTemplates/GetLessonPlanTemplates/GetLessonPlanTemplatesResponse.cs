using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.LessonPlanTemplates.GetLessonPlanTemplates;

public sealed class GetLessonPlanTemplatesResponse
{
    public Page<LessonPlanTemplateDto> Templates { get; init; } = null!;
}

public sealed class LessonPlanTemplateDto
{
    public Guid Id { get; init; }
    public Guid ModuleId { get; init; }
    public string ModuleCode { get; init; } = null!;
    public string ModuleName { get; init; } = null!;
    public Guid? LessonPlanUnitId { get; init; }
    public string? LessonPlanUnitName { get; init; }
    public int OrderIndexInUnit { get; init; }
    public Guid LevelId { get; init; }
    public string LevelName { get; init; } = null!;
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public string? Title { get; init; }
    public int SessionIndex { get; init; }
    public int SessionOrder { get; init; }
    public string? SyllabusMetadata { get; init; }
    public string? SyllabusContent { get; init; }
    public string? Objectives { get; init; }
    public string? LanguageContent { get; init; }
    public string? Vocabulary { get; init; }
    public string? Grammar { get; init; }
    public string? TeachingMethodology { get; init; }
    public string? TeacherMaterials { get; init; }
    public string? StudentMaterials { get; init; }
    public string? Procedure { get; init; }
    public string? Evaluation { get; init; }
    public string? SourceFileName { get; init; }
    public string? Attachment { get; init; }
    public bool IsActive { get; init; }
    public Guid? CreatedBy { get; init; }
    public string? CreatedByName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int UsedCount { get; init; }
}
