using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Programs;

public class TuitionPlanModuleSelection : Entity
{
    public Guid Id { get; set; }
    public Guid TuitionPlanId { get; set; }
    public Guid ModuleId { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public TuitionPlan TuitionPlan { get; set; } = null!;
    public Module Module { get; set; } = null!;
}
