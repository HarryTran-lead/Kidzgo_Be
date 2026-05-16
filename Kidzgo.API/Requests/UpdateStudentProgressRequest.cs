namespace Kidzgo.API.Requests;

public sealed class UpdateStudentProgressRequest
{
    public Guid StudentProfileId { get; init; }
    public Guid ModuleId { get; init; }
    public Guid? CurrentLessonPlanTemplateId { get; init; }
    public decimal? CompletionPercent { get; init; }
}
