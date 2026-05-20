namespace Kidzgo.API.Requests;

public sealed class ReorderLessonPlanTemplateSessionOrderRequest
{
    public Guid Id { get; init; }
    public int SessionOrder { get; init; }
}
