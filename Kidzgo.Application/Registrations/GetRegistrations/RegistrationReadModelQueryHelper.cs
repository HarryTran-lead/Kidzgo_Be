using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Registrations;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.GetRegistrations;

internal static class RegistrationReadModelQueryHelper
{
    public static IQueryable<Registration> CreateBaseQuery(IDbContext context)
    {
        return context.Registrations
            .AsNoTracking()
            .Include(r => r.StudentProfile)
            .Include(r => r.Branch)
            .Include(r => r.Program)
            .Include(r => r.Level)
            .Include(r => r.SecondaryLevel)
            .Include(r => r.TuitionPlan)
            .Include(r => r.Class)
            .Include(r => r.SecondaryClass);
    }

    public static IQueryable<Registration> ApplyFilters(
        IQueryable<Registration> query,
        Guid? studentProfileId,
        Guid? branchId,
        Guid? programId,
        string? status,
        Guid? classId)
    {
        if (studentProfileId.HasValue)
        {
            query = query.Where(r => r.StudentProfileId == studentProfileId.Value);
        }

        if (branchId.HasValue)
        {
            query = query.Where(r => r.BranchId == branchId.Value);
        }

        if (programId.HasValue)
        {
            query = query.Where(r =>
                r.ProgramId == programId.Value ||
                r.SecondaryProgramId == programId.Value);
        }

        if (classId.HasValue)
        {
            query = query.Where(r =>
                r.ClassId == classId.Value ||
                r.SecondaryClassId == classId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<RegistrationStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(r => r.Status == parsedStatus);
        }

        return query;
    }

    public static IQueryable<RegistrationDto> SelectListDto(IQueryable<Registration> query)
    {
        return query.Select(r => new RegistrationDto
        {
            Id = r.Id,
            StudentProfileId = r.StudentProfileId,
            StudentName = r.StudentProfile.DisplayName,
            BranchId = r.BranchId,
            BranchName = r.Branch.Name,
            ProgramId = r.ProgramId,
            ProgramName = r.Program.Name,
            LevelId = r.LevelId,
            LevelName = r.Level.Name,
            SecondaryLevelId = r.SecondaryLevelId,
            SecondaryLevelName = r.SecondaryLevel != null ? r.SecondaryLevel.Name : null,
            SecondaryLevelSkillFocus = r.SecondaryProgramSkillFocus,
            TuitionPlanId = r.TuitionPlanId,
            TuitionPlanName = r.TuitionPlan.Name,
            RegistrationDate = r.RegistrationDate,
            ExpectedStartDate = r.ExpectedStartDate,
            ActualStartDate = r.ActualStartDate,
            PreferredSchedule = r.PreferredSchedule,
            Note = r.Note,
            Status = r.Status.ToString(),
            OperationType = r.OperationType != null ? r.OperationType.ToString() : null,
            ClassId = r.ClassId,
            ClassName = r.Class != null ? r.Class.Title : null,
            SecondaryClassId = r.SecondaryClassId,
            SecondaryClassName = r.SecondaryClass != null ? r.SecondaryClass.Title : null,
            TotalSessions = r.TotalSessions,
            UsedSessions = r.UsedSessions,
            RemainingSessions = r.RemainingSessions,
            DiscountCampaignId = r.DiscountCampaignId,
            DiscountCampaignName = r.DiscountCampaignName,
            DiscountType = r.DiscountType != null ? r.DiscountType.ToString() : null,
            DiscountValue = r.DiscountValue,
            OriginalTuitionAmount = r.OriginalTuitionAmount ?? r.TuitionPlan.TuitionAmount,
            DiscountAmount = r.DiscountAmount ?? 0m,
            CarryOverCreditAmount = r.CarryOverCreditAmount ?? 0m,
            FinalTuitionAmount = r.FinalTuitionAmount ?? ((r.OriginalTuitionAmount ?? r.TuitionPlan.TuitionAmount) - (r.DiscountAmount ?? 0m) - (r.CarryOverCreditAmount ?? 0m)),
            CreatedAt = r.CreatedAt
        });
    }
}
