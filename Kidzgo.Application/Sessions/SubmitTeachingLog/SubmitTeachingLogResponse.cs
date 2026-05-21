namespace Kidzgo.Application.Sessions.SubmitTeachingLog;

public sealed class SubmitTeachingLogResponse
{
    public Guid TeachingLogId { get; init; }
    public Guid SessionId { get; init; }
    public Guid? PlannedLessonPlanTemplateId { get; init; }
    public Guid? ActualLessonPlanTemplateId { get; init; }
    public string ActualTeachingType { get; init; } = null!;
    public string ProgressStatus { get; init; } = null!;
    public Guid ClassId { get; init; }
    public Guid? CurrentModuleId { get; init; }
    public int CurrentSessionIndex { get; init; }
    public Guid? CurrentLessonPlanTemplateId { get; init; }
    public int UpdatedFutureSessionCount { get; init; }
}
