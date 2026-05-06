using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.ProgramProgressions.BulkApproveProgramProgressionAssessments;

public sealed class BulkApproveProgramProgressionAssessmentsCommand
    : ICommand<BulkApproveProgramProgressionAssessmentsResponse>
{
    public IReadOnlyCollection<BulkApproveProgramProgressionAssessmentItem> Items { get; init; } = Array.Empty<BulkApproveProgramProgressionAssessmentItem>();
}

public sealed class BulkApproveProgramProgressionAssessmentItem
{
    public Guid AssessmentId { get; init; }
    public Guid? TuitionPlanId { get; init; }
    public string? ApprovalNote { get; init; }
}
