using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TuitionPlans.CreateTuitionPlan;

public sealed class CreateTuitionPlanCommand : ICommand<CreateTuitionPlanResponse>
{
    public Guid ProgramId { get; init; }
    public Guid? LearningTicketTypeId { get; init; }
    public string Name { get; init; } = null!;
    public int TotalSessions { get; init; }
    public decimal TuitionAmount { get; init; }
    public string Currency { get; init; } = null!;
}
