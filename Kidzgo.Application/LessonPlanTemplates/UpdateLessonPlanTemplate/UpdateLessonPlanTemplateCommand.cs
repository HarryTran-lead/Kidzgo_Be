using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanTemplates.UpdateLessonPlanTemplate;

public sealed class UpdateLessonPlanTemplateCommand : ICommand<UpdateLessonPlanTemplateResponse>
{
    public Guid Id { get; init; }
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
