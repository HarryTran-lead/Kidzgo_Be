using Kidzgo.Application.TuitionPlans.Shared;

namespace Kidzgo.Application.TuitionPlans.UpdateTuitionPlan;

public sealed class UpdateTuitionPlanResponse
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
    public string LevelName { get; init; } = null!;
    public Guid? SyllabusId { get; init; }
    public string? SyllabusCode { get; init; }
    public int? SyllabusVersion { get; init; }
    public string? SyllabusTitle { get; init; }
    public Guid? ModuleId { get; init; }
    public string? ModuleName { get; init; }
    public IReadOnlyList<Guid> ModuleIds { get; init; } = Array.Empty<Guid>();
    public IReadOnlyList<TuitionPlanModuleDto> Modules { get; init; } = Array.Empty<TuitionPlanModuleDto>();
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

