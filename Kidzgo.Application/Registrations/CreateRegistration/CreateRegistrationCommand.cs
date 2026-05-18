using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.CreateRegistration;

public sealed class CreateRegistrationCommand : ICommand<CreateRegistrationResponse>
{
    public Guid StudentProfileId { get; init; }
    public Guid BranchId { get; init; }
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
    public Guid TuitionPlanId { get; init; }
    public Guid? SecondaryLevelId { get; init; }
    public string? SecondaryLevelSkillFocus { get; init; }
    
    public DateTime? ExpectedStartDate { get; init; }
    public string? PreferredSchedule { get; init; }
    public string? Note { get; init; }
}
