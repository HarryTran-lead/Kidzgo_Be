using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TuitionPlans.UpdateTuitionPlan;

public sealed class UpdateTuitionPlanCommand : ICommand<UpdateTuitionPlanResponse>
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
    public Guid? SyllabusId { get; init; }
    public IReadOnlyList<Guid>? ModuleIds { get; init; }
    public Guid? LearningTicketTypeId { get; init; }
    public string Name { get; init; } = null!;
    public int TotalSessions { get; init; }
    public decimal TuitionAmount { get; init; }
    public string Currency { get; init; } = null!;
}
