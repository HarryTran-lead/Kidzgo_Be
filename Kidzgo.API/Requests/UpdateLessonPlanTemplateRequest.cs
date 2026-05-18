namespace Kidzgo.API.Requests;

public sealed class UpdateLessonPlanTemplateRequest
{
    public Guid? ModuleId { get; init; }
    public string? Level { get; init; }
    public string? Title { get; init; }
    public int? SessionIndex { get; init; }
    public int? SessionOrder { get; init; }
    public string? SyllabusMetadata { get; init; }
    public string? SyllabusContent { get; init; }
    public string? SourceFileName { get; init; }
    public string? Attachment { get; init; }
    public bool? IsActive { get; init; }
}
