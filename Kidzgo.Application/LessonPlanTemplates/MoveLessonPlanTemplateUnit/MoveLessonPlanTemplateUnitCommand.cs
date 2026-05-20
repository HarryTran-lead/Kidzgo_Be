using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanTemplates.MoveLessonPlanTemplateUnit;

public sealed class MoveLessonPlanTemplateUnitCommand : ICommand<MoveLessonPlanTemplateUnitResponse>
{
    public Guid Id { get; init; }
    public Guid? LessonPlanUnitId { get; init; }
    public int? OrderIndexInUnit { get; init; }
}

public sealed class MoveLessonPlanTemplateUnitResponse
{
    public Guid Id { get; init; }
    public Guid ModuleId { get; init; }
    public Guid? LessonPlanUnitId { get; init; }
    public int OrderIndexInUnit { get; init; }
    public DateTime UpdatedAt { get; init; }
}
