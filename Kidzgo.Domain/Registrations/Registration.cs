using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Programs;

namespace Kidzgo.Domain.Registrations;

public class Registration : Entity
{
    public Guid Id { get; set; }
    
    // Thong tin hoc vien
    public Guid StudentProfileId { get; set; }
    
    // Thong tin dang ky
    public Guid BranchId { get; set; }
    public Guid ProgramId { get; set; }
    public Guid LevelId { get; set; }
    public Guid? SecondaryLevelId { get; set; }
    public Guid TuitionPlanId { get; set; }
    public Guid? SecondaryProgramId { get; set; }
    
    // Ngay dang ky va ngay bat dau
    public DateTime RegistrationDate { get; set; }
    public DateTime? ExpectedStartDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    
    // Nhu cau cua hoc vien
    public string? PreferredSchedule { get; set; }
    public string? Note { get; set; }
    
    // Trang thai dang ky
    public RegistrationStatus Status { get; set; }
    
    // Thong tin xep lop
    public Guid? ClassId { get; set; }
    public DateTime? ClassAssignedDate { get; set; }
    public EntryType? EntryType { get; set; }
    public Guid? SecondaryClassId { get; set; }
    public DateTime? SecondaryClassAssignedDate { get; set; }
    public EntryType? SecondaryEntryType { get; set; }
    public string? SecondaryProgramSkillFocus { get; set; }
    
    // Nghiem vu phat sinh - lien ket voi registration goc
    public Guid? OriginalRegistrationId { get; set; }
    public OperationType? OperationType { get; set; }

    // Pricing snapshot
    public Guid? DiscountCampaignId { get; set; }
    public string? DiscountCampaignName { get; set; }
    public RegistrationDiscountType? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal? OriginalTuitionAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? CarryOverCreditAmount { get; set; }
    public decimal? FinalTuitionAmount { get; set; }
    public DateTime? PricingAppliedAt { get; set; }
    
    // Thong tin hoc vu
    public int TotalSessions { get; set; }
    public int UsedSessions { get; set; }
    public int RemainingSessions { get; set; }
    public DateTime? ExpiryDate { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Profile StudentProfile { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public Program Program { get; set; } = null!;
    public Level Level { get; set; } = null!;
    public Level? SecondaryLevel { get; set; }
    public TuitionPlan TuitionPlan { get; set; } = null!;
    public Classes.Class? Class { get; set; }
    public Program? SecondaryProgram { get; set; }
    public Classes.Class? SecondaryClass { get; set; }
    public Registration? OriginalRegistration { get; set; }
    public RegistrationDiscountCampaign? DiscountCampaign { get; set; }
}
