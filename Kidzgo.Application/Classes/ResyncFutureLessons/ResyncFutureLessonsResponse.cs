namespace Kidzgo.Application.Classes.ResyncFutureLessons;

public sealed class ResyncFutureLessonsResponse
{
    public Guid ClassId { get; init; }
    public int UpdatedSessionCount { get; init; }
    public Guid CurrentModuleId { get; init; }
    public int CurrentSessionIndex { get; init; }
    public Guid? CurrentLessonPlanTemplateId { get; init; }
}
