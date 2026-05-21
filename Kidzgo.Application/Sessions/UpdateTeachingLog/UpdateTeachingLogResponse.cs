namespace Kidzgo.Application.Sessions.UpdateTeachingLog;

public sealed class UpdateTeachingLogResponse
{
    public Guid TeachingLogId { get; init; }
    public Guid SessionId { get; init; }
    public Guid ClassId { get; init; }
    public Guid? PlannedLessonPlanTemplateId { get; init; }
    public Guid? ActualLessonPlanTemplateId { get; init; }
    public string ActualTeachingType { get; init; } = null!;
    public string ProgressStatus { get; init; } = null!;
    public Guid? CurrentModuleId { get; init; }
    public int CurrentSessionIndex { get; init; }
    public Guid? CurrentLessonPlanTemplateId { get; init; }
    public int UpdatedFutureSessionCount { get; init; }
}
