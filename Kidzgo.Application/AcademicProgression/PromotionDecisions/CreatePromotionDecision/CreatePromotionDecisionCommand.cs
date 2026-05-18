using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;

namespace Kidzgo.Application.AcademicProgression.PromotionDecisions.CreatePromotionDecision;

public sealed class CreatePromotionDecisionCommand : ICommand<PromotionDecisionDto>
{
    public Guid StudentProfileId { get; init; }
    public Guid ModuleId { get; init; }
    public string? Reason { get; init; }
    public DateTime? ApprovedAt { get; init; }
}
