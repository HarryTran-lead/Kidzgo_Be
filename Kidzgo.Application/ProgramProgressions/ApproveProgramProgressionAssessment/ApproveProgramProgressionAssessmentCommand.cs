using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;

namespace Kidzgo.Application.ProgramProgressions.ApproveProgramProgressionAssessment;

public sealed class ApproveProgramProgressionAssessmentCommand : ICommand<ProgramProgressionAssessmentDto>
{
    public Guid AssessmentId { get; init; }
    public Guid? TuitionPlanId { get; init; }
    public string? ApprovalNote { get; init; }
}
