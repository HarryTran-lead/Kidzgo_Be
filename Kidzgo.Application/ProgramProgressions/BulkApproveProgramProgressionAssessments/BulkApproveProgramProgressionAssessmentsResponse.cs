namespace Kidzgo.Application.ProgramProgressions.BulkApproveProgramProgressionAssessments;

public sealed class BulkApproveProgramProgressionAssessmentsResponse
{
    public int ApprovedCount { get; init; }
    public int SkippedCount { get; init; }
    public List<BulkApproveProgramProgressionAssessmentResult> Results { get; init; } = new();
    public List<BulkApproveProgramProgressionAssessmentError> Errors { get; init; } = new();
}

public sealed class BulkApproveProgramProgressionAssessmentResult
{
    public Guid AssessmentId { get; init; }
    public Guid? GeneratedRegistrationId { get; init; }
}

public sealed class BulkApproveProgramProgressionAssessmentError
{
    public Guid AssessmentId { get; init; }
    public string ErrorCode { get; init; } = null!;
    public string ErrorDescription { get; init; } = null!;
}
