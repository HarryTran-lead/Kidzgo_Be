namespace Kidzgo.Application.TuitionPlans.CreateTuitionPlan;

public sealed class CreateTuitionPlanResponse
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
    public Guid? LevelId { get; init; }
    public string? LevelName { get; init; }
    public Guid? LearningTicketTypeId { get; init; }
    public string? LearningTicketTypeCode { get; init; }
    public string ProgramName { get; init; } = null!;
    public string Name { get; init; } = null!;
    public int TotalSessions { get; init; }
    public decimal TuitionAmount { get; init; }
    public decimal UnitPriceSession { get; init; }
    public string Currency { get; init; } = null!;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

