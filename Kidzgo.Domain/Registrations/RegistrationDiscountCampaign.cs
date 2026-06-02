using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Schools;

namespace Kidzgo.Domain.Registrations;

public class RegistrationDiscountCampaign : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? ProgramId { get; set; }
    public Guid? LevelId { get; set; }
    public Guid? TuitionPlanId { get; set; }
    public RegistrationDiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public int Priority { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool ApplyForInitialRegistration { get; set; }
    public bool ApplyForRenewal { get; set; }
    public bool ApplyForUpgrade { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Branch? Branch { get; set; }
    public Program? Program { get; set; }
    public Level? Level { get; set; }
    public TuitionPlan? TuitionPlan { get; set; }
}
