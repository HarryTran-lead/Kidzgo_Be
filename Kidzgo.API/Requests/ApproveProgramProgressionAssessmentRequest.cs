namespace Kidzgo.API.Requests;

public sealed class ApproveProgramProgressionAssessmentRequest
{
    public Guid? TuitionPlanId { get; set; }
    public string? ApprovalNote { get; set; }
}
