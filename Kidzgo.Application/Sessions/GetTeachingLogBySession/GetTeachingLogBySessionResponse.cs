namespace Kidzgo.Application.Sessions.GetTeachingLogBySession;

public sealed class GetTeachingLogBySessionResponse
{
    public Guid TeachingLogId { get; init; }
    public Guid SessionId { get; init; }
    public Guid? PlannedLessonPlanTemplateId { get; init; }
    public string? PlannedLessonTitle { get; init; }
    public Guid? ActualLessonPlanTemplateId { get; init; }
    public string? ActualLessonTitle { get; init; }
    public string TeachingLogStatus { get; init; } = null!;
    public string? ProgressStatus { get; init; }
    public string ActualTeachingType { get; init; } = null!;
    public string? ActualContent { get; init; }
    public string? ActualHomework { get; init; }
    public string? TeacherNote { get; init; }
    public Guid? SubmittedBy { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
