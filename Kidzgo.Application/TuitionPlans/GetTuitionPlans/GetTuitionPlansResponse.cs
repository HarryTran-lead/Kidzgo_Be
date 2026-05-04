using Kidzgo.Domain.Common;

namespace Kidzgo.Application.TuitionPlans.GetTuitionPlans;

public sealed class GetTuitionPlansResponse
{
    public Page<TuitionPlanDto> TuitionPlans { get; init; } = null!;
}

public sealed class TuitionPlanDto
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
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

