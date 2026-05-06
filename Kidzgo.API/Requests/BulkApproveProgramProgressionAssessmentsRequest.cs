namespace Kidzgo.API.Requests;

public sealed class BulkApproveProgramProgressionAssessmentsRequest
{
    public List<BulkApproveProgramProgressionAssessmentRequestItem> Items { get; set; } = new();
}

public sealed class BulkApproveProgramProgressionAssessmentRequestItem
{
    public Guid AssessmentId { get; set; }
    public Guid? TuitionPlanId { get; set; }
    public string? ApprovalNote { get; set; }
}
