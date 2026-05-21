namespace Kidzgo.API.Requests;

public sealed class SubmitTeachingLogRequest
{
    public Guid? ActualLessonPlanTemplateId { get; set; }
    public string? ActualTeachingType { get; set; }
    public string ProgressStatus { get; set; } = null!;
    public string? ActualContent { get; set; }
    public string? ActualHomework { get; set; }
    public string? TeacherNote { get; set; }
}
