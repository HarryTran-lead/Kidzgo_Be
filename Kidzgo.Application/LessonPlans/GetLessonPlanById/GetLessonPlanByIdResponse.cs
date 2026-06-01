using Kidzgo.Application.Sessions.Shared;

namespace Kidzgo.Application.LessonPlans.GetLessonPlanById;

public sealed class GetLessonPlanByIdResponse
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public string? ClassCode { get; init; }
    public Guid SessionId { get; init; }
    public string? SessionTitle { get; init; }
    public DateTime? SessionDate { get; init; }
    public Guid? TemplateId { get; init; }
    public string? TemplateLevel { get; init; }
    public int? TemplateSessionIndex { get; init; }
    public string? PlannedContent { get; init; }
    public string? ActualContent { get; init; }
    public string? ActualHomework { get; init; }
    public string? TeacherNotes { get; init; }
    public string? TeacherNote => TeacherNotes;
    public decimal? CompletionPercent { get; init; }
    public string? CarryForwardContent { get; init; }
    public Guid? SubmittedBy { get; init; }
    public string? SubmittedByName { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public TeachingLogSnapshotDto? TeachingLog { get; init; }
}
