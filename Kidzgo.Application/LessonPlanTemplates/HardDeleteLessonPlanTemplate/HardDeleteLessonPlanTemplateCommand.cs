using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanTemplates.HardDeleteLessonPlanTemplate;

public sealed class HardDeleteLessonPlanTemplateCommand : ICommand<HardDeleteLessonPlanTemplateResponse>
{
    public Guid Id { get; init; }
}

public sealed class HardDeleteLessonPlanTemplateResponse
{
    public Guid Id { get; init; }
    public int DeletedLessonPlanCount { get; init; }
    public int DeletedLessonPlanUnitCount { get; init; }
}
