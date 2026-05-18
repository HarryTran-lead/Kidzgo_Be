namespace Kidzgo.Application.LessonPlans.CreateLessonPlan;

public sealed class CreateLessonPlanResponse
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public Guid SessionId { get; init; }
    public Guid? TemplateId { get; init; }
    public string? PlannedContent { get; init; }
    public string? ActualContent { get; init; }
    public string? ActualHomework { get; init; }
    public string? TeacherNotes { get; init; }
    public decimal? CompletionPercent { get; init; }
    public string? CarryForwardContent { get; init; }
    public Guid? SubmittedBy { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
