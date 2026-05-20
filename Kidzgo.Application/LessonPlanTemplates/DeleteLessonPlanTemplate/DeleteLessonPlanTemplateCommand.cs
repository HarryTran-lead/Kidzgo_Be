using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanTemplates.DeleteLessonPlanTemplate;

public sealed class DeleteLessonPlanTemplateCommand : ICommand<DeleteLessonPlanTemplateResponse>
{
    public Guid Id { get; init; }
}

public sealed class DeleteLessonPlanTemplateResponse
{
    public Guid Id { get; init; }
    public bool IsDeleted { get; init; }
    public DateTime UpdatedAt { get; init; }
}
