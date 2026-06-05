using Kidzgo.Application.TuitionPlans.Shared;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Registrations.GetRegistrationById;

public sealed class GetRegistrationByIdResponse
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public Guid? StudentHomeBranchId { get; init; }
    public string? StudentHomeBranchName { get; init; }
    public Guid? StudentActiveBranchId { get; init; }
    public string? StudentActiveBranchName { get; init; }
    public bool IsCrossBranchRegistration { get; init; }
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid LevelId { get; init; }
    public string LevelName { get; init; } = null!;
    public Guid? SyllabusId { get; init; }
    public string? SyllabusCode { get; init; }
    public int? SyllabusVersion { get; init; }
    public string? SyllabusTitle { get; init; }
    public IReadOnlyList<Guid> ModuleIds { get; init; } = Array.Empty<Guid>();
    public IReadOnlyList<TuitionPlanModuleDto> Modules { get; init; } = Array.Empty<TuitionPlanModuleDto>();
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
    public Guid? ClassId { get; init; }
    public string? ClassName { get; init; }
    public string? EntryType { get; init; }
    public Guid? SecondaryClassId { get; init; }
    public string? SecondaryClassName { get; init; }
    public string? SecondaryEntryType { get; init; }
    public int TotalSessions { get; init; }
    public int UsedSessions { get; init; }
    public int RemainingSessions { get; init; }
    public Guid? OriginalRegistrationId { get; init; }
    public string? OperationType { get; init; }
    public Guid? DiscountCampaignId { get; init; }
    public string? DiscountCampaignName { get; init; }
    public string? DiscountType { get; init; }
    public decimal? DiscountValue { get; init; }
    public decimal OriginalTuitionAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal CarryOverCreditAmount { get; init; }
    public decimal FinalTuitionAmount { get; init; }
    public RegistrationFirstStudySessionDto? FirstStudySession { get; init; }
    public List<RegistrationActualStudyScheduleDto> ActualStudySchedules { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class RegistrationFirstStudySessionDto
{
    public Guid SessionId { get; init; }
    public Guid ClassEnrollmentId { get; init; }
    public string Track { get; init; } = null!;
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = null!;
    public DateTime PlannedDatetime { get; init; }
    public DateOnly StudyDate { get; init; }
}
