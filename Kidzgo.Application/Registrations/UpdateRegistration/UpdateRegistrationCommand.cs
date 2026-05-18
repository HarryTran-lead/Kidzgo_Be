using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.UpdateRegistration;

public sealed class UpdateRegistrationCommand : ICommand<UpdateRegistrationResponse>
{
    public Guid Id { get; init; }
    public DateTime? ExpectedStartDate { get; init; }
    public string? PreferredSchedule { get; init; }
    public string? Note { get; init; }
    public Guid? TuitionPlanId { get; init; }
    public Guid? SecondaryLevelId { get; init; }
    public string? SecondaryLevelSkillFocus { get; init; }
    public bool RemoveSecondaryLevel { get; init; }
}
