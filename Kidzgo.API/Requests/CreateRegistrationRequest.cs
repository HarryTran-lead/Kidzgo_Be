using Kidzgo.Application.Registrations.CreateRegistration;

namespace Kidzgo.API.Requests;

public sealed class CreateRegistrationRequest
{
    public Guid StudentProfileId { get; set; }
    public Guid BranchId { get; set; }
    public Guid ProgramId { get; set; }
    public Guid LevelId { get; set; }
    public Guid TuitionPlanId { get; set; }
    public Guid? SecondaryLevelId { get; set; }
    public string? SecondaryLevelSkillFocus { get; set; }
    public DateTime? ExpectedStartDate { get; set; }
    public string? PreferredSchedule { get; set; }
    public string? Note { get; set; }
}
