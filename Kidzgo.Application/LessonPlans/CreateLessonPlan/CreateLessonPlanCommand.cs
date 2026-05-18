using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlans.CreateLessonPlan;

public sealed class CreateLessonPlanCommand : ICommand<CreateLessonPlanResponse>
{
    public Guid ClassId { get; init; }
    public Guid SessionId { get; init; }
    public Guid? TemplateId { get; init; }
    public string? PlannedContent { get; init; }
    public string? ActualContent { get; init; }
    public string? ActualHomework { get; init; }
    public string? TeacherNotes { get; init; }
    public decimal? CompletionPercent { get; init; }
    public string? CarryForwardContent { get; init; }
}
