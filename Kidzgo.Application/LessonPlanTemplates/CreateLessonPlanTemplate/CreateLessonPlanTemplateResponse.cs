namespace Kidzgo.Application.LessonPlanTemplates.CreateLessonPlanTemplate;

public sealed class CreateLessonPlanTemplateResponse
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
    public Guid? ModuleId { get; init; }
    public string? Level { get; init; }
    public string? Title { get; init; }
    public int SessionIndex { get; init; }
    public int SessionOrder { get; init; }
    public string? SyllabusMetadata { get; init; }
    public string? SyllabusContent { get; init; }
    public string? SourceFileName { get; init; }
    public string? Attachment { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
