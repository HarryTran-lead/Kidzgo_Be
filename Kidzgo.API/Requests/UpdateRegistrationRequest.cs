namespace Kidzgo.API.Requests;

public sealed class UpdateRegistrationRequest
{
    public DateTime? ExpectedStartDate { get; set; }
    public string? PreferredSchedule { get; set; }
    public string? Note { get; set; }
    public Guid? TuitionPlanId { get; set; }
    public Guid? SecondaryLevelId { get; set; }
    public string? SecondaryLevelSkillFocus { get; set; }
    public bool RemoveSecondaryLevel { get; set; }
}
