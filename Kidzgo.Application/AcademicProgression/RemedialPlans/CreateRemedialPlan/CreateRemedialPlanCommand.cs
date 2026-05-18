using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;

namespace Kidzgo.Application.AcademicProgression.RemedialPlans.CreateRemedialPlan;

public sealed class CreateRemedialPlanCommand : ICommand<RemedialPlanDto>
{
    public Guid StudentProfileId { get; init; }
    public Guid ModuleId { get; init; }
    public string WeakSkills { get; init; } = null!;
    public int RecommendedSessionCount { get; init; }
    public string? Notes { get; init; }
}
