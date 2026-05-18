using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Registrations.GetRegistrations;

public sealed class RegistrationDto
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid LevelId { get; init; }
    public string LevelName { get; init; } = null!;
    public Guid? SecondaryLevelId { get; init; }
    public string? SecondaryLevelName { get; init; }
    public string? SecondaryLevelSkillFocus { get; init; }
    public Guid TuitionPlanId { get; init; }
    public string TuitionPlanName { get; init; } = null!;
    public DateTime RegistrationDate { get; init; }
    public DateTime? ExpectedStartDate { get; init; }
    public DateTime? ActualStartDate { get; init; }
    public string? PreferredSchedule { get; init; }
    public string? Note { get; init; }
    public string Status { get; init; } = null!;
    public string? OperationType { get; init; }
    public Guid? ClassId { get; init; }
    public string? ClassName { get; init; }
    public Guid? SecondaryClassId { get; init; }
    public string? SecondaryClassName { get; init; }
    public int TotalSessions { get; init; }
    public int UsedSessions { get; init; }
    public int RemainingSessions { get; init; }
    public Guid? DiscountCampaignId { get; init; }
    public string? DiscountCampaignName { get; init; }
    public string? DiscountType { get; init; }
    public decimal? DiscountValue { get; init; }
    public decimal OriginalTuitionAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal CarryOverCreditAmount { get; init; }
    public decimal FinalTuitionAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class GetRegistrationsResponse
{
    public Page<RegistrationDto> Page { get; init; } = null!;
}
