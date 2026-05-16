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
    public Guid ProgramId { get; init; }
    public Guid? ModuleId { get; init; }
    public string? ModuleCode { get; init; }
    public string? ModuleName { get; init; }
    public string? ProgramName { get; init; }
    public string? Title { get; init; }
    public string? Level { get; init; }
    public int SessionIndex { get; init; }
    public int SessionOrder { get; init; }
    public string? SyllabusMetadata { get; init; }
    public string? SyllabusContent { get; init; }
    public string? SourceFileName { get; init; }
    public string? Attachment { get; init; }
    public bool IsActive { get; init; }
    public Guid? CreatedBy { get; init; }
    public string? CreatedByName { get; init; }
    public DateTime CreatedAt { get; init; }
    public int UsedCount { get; init; }
}
