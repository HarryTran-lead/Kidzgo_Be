using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Audit;
using Kidzgo.Domain.Registrations;

namespace Kidzgo.Application.Registrations.Shared;

internal static class RegistrationAuditLogHelper
{
    internal const string EntityType = "Registration";

    internal static RegistrationAuditSnapshot CreateSnapshot(Registration registration)
    {
        return new RegistrationAuditSnapshot
        {
            Id = registration.Id,
            StudentProfileId = registration.StudentProfileId,
            BranchId = registration.BranchId,
            ProgramId = registration.ProgramId,
            LevelId = registration.LevelId,
            SecondaryLevelId = registration.SecondaryLevelId,
            TuitionPlanId = registration.TuitionPlanId,
            SecondaryProgramId = registration.SecondaryProgramId,
            RegistrationDate = registration.RegistrationDate,
            ExpectedStartDate = registration.ExpectedStartDate,
            ActualStartDate = registration.ActualStartDate,
            PreferredSchedule = registration.PreferredSchedule,
            Note = registration.Note,
            Status = registration.Status.ToString(),
            ClassId = registration.ClassId,
            ClassAssignedDate = registration.ClassAssignedDate,
            EntryType = registration.EntryType?.ToString(),
            SecondaryClassId = registration.SecondaryClassId,
            SecondaryClassAssignedDate = registration.SecondaryClassAssignedDate,
            SecondaryEntryType = registration.SecondaryEntryType?.ToString(),
            SecondaryProgramSkillFocus = registration.SecondaryProgramSkillFocus,
            OriginalRegistrationId = registration.OriginalRegistrationId,
            OperationType = registration.OperationType?.ToString(),
            DiscountCampaignId = registration.DiscountCampaignId,
            DiscountCampaignName = registration.DiscountCampaignName,
            DiscountType = registration.DiscountType?.ToString(),
            DiscountValue = registration.DiscountValue,
            OriginalTuitionAmount = registration.OriginalTuitionAmount,
            DiscountAmount = registration.DiscountAmount,
            CarryOverCreditAmount = registration.CarryOverCreditAmount,
            FinalTuitionAmount = registration.FinalTuitionAmount,
            PricingAppliedAt = registration.PricingAppliedAt,
            TotalSessions = registration.TotalSessions,
            UsedSessions = registration.UsedSessions,
            RemainingSessions = registration.RemainingSessions,
            ExpiryDate = registration.ExpiryDate,
            CreatedAt = registration.CreatedAt,
            UpdatedAt = registration.UpdatedAt
        };
    }

    internal static void AddAuditLog(
        IDbContext context,
        IUserContext userContext,
        string action,
        Registration registration,
        object? dataBefore,
        object? dataAfter,
        DateTime timestamp)
    {
        context.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = userContext.UserId,
            ActorProfileId = userContext.StudentId ?? userContext.ParentId,
            Action = action,
            EntityType = EntityType,
            EntityId = registration.Id,
            DataBefore = Serialize(dataBefore),
            DataAfter = Serialize(dataAfter),
            IpAddress = userContext.IpAddress,
            CreatedAt = timestamp
        });
    }

    private static string? Serialize(object? value)
    {
        return value is null ? null : JsonSerializer.Serialize(value);
    }
}

internal static class RegistrationAuditActions
{
    internal const string CreateRegistration = "CreateRegistration";
    internal const string ImportActiveRegistration = "ImportActiveRegistration";
    internal const string UpdateRegistration = "UpdateRegistration";
    internal const string CancelRegistration = "CancelRegistration";
    internal const string AssignRegistrationClass = "AssignRegistrationClass";
    internal const string TransferRegistrationClass = "TransferRegistrationClass";
    internal const string TransferRegistrationBranch = "TransferRegistrationBranch";
    internal const string UpgradeRegistrationTuitionPlan = "UpgradeRegistrationTuitionPlan";
}

internal sealed class RegistrationAuditSnapshot
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public Guid BranchId { get; init; }
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
    public Guid? SecondaryLevelId { get; init; }
    public Guid TuitionPlanId { get; init; }
    public Guid? SecondaryProgramId { get; init; }
    public DateTime RegistrationDate { get; init; }
    public DateTime? ExpectedStartDate { get; init; }
    public DateTime? ActualStartDate { get; init; }
    public string? PreferredSchedule { get; init; }
    public string? Note { get; init; }
    public string Status { get; init; } = null!;
    public Guid? ClassId { get; init; }
    public DateTime? ClassAssignedDate { get; init; }
    public string? EntryType { get; init; }
    public Guid? SecondaryClassId { get; init; }
    public DateTime? SecondaryClassAssignedDate { get; init; }
    public string? SecondaryEntryType { get; init; }
    public string? SecondaryProgramSkillFocus { get; init; }
    public Guid? OriginalRegistrationId { get; init; }
    public string? OperationType { get; init; }
    public Guid? DiscountCampaignId { get; init; }
    public string? DiscountCampaignName { get; init; }
    public string? DiscountType { get; init; }
    public decimal? DiscountValue { get; init; }
    public decimal? OriginalTuitionAmount { get; init; }
    public decimal? DiscountAmount { get; init; }
    public decimal? CarryOverCreditAmount { get; init; }
    public decimal? FinalTuitionAmount { get; init; }
    public DateTime? PricingAppliedAt { get; init; }
    public int TotalSessions { get; init; }
    public int UsedSessions { get; init; }
    public int RemainingSessions { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
