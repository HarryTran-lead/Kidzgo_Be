using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Kidzgo.Domain.Schools;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.RegistrationDiscountCampaigns.Shared;

internal static class RegistrationDiscountCampaignValidationHelper
{
    internal static async Task<Result<RegistrationDiscountCampaignScopeValidationResult>> ValidateScopeAsync(
        IDbContext context,
        Guid? branchId,
        Guid? programId,
        Guid? levelId,
        Guid? tuitionPlanId,
        CancellationToken cancellationToken)
    {
        Branch? branch = null;
        Program? program = null;
        Level? level = null;
        TuitionPlan? tuitionPlan = null;
        var resolvedProgramId = programId;

        if (branchId.HasValue)
        {
            branch = await context.Branches
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == branchId.Value, cancellationToken);

            if (branch is null)
            {
                return Result.Failure<RegistrationDiscountCampaignScopeValidationResult>(
                    RegistrationDiscountCampaignErrors.BranchNotFound(branchId.Value));
            }
        }

        if (programId.HasValue)
        {
            program = await context.Programs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == programId.Value && !x.IsDeleted, cancellationToken);

            if (program is null)
            {
                return Result.Failure<RegistrationDiscountCampaignScopeValidationResult>(
                    RegistrationDiscountCampaignErrors.ProgramNotFound(programId.Value));
            }
        }

        if (levelId.HasValue)
        {
            level = await context.Levels
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == levelId.Value && x.IsActive, cancellationToken);

            if (level is null)
            {
                return Result.Failure<RegistrationDiscountCampaignScopeValidationResult>(
                    RegistrationDiscountCampaignErrors.LevelNotFound(levelId.Value));
            }

            if (resolvedProgramId.HasValue && level.ProgramId != resolvedProgramId.Value)
            {
                return Result.Failure<RegistrationDiscountCampaignScopeValidationResult>(
                    RegistrationDiscountCampaignErrors.LevelProgramMismatch);
            }

            if (!resolvedProgramId.HasValue)
            {
                program = await context.Programs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == level.ProgramId && !x.IsDeleted, cancellationToken);

                if (program is null)
                {
                    return Result.Failure<RegistrationDiscountCampaignScopeValidationResult>(
                        RegistrationDiscountCampaignErrors.ProgramNotFound(level.ProgramId));
                }

                resolvedProgramId = program.Id;
            }
        }

        if (tuitionPlanId.HasValue)
        {
            tuitionPlan = await context.TuitionPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == tuitionPlanId.Value && !x.IsDeleted, cancellationToken);

            if (tuitionPlan is null)
            {
                return Result.Failure<RegistrationDiscountCampaignScopeValidationResult>(
                    RegistrationDiscountCampaignErrors.TuitionPlanNotFound(tuitionPlanId.Value));
            }

            if (resolvedProgramId.HasValue && tuitionPlan.ProgramId != resolvedProgramId.Value)
            {
                return Result.Failure<RegistrationDiscountCampaignScopeValidationResult>(
                    RegistrationDiscountCampaignErrors.TuitionPlanProgramMismatch);
            }

            if (levelId.HasValue && tuitionPlan.LevelId != levelId.Value)
            {
                return Result.Failure<RegistrationDiscountCampaignScopeValidationResult>(
                    RegistrationDiscountCampaignErrors.TuitionPlanLevelMismatch);
            }

        }

        return Result.Success(new RegistrationDiscountCampaignScopeValidationResult(branch, program, level, tuitionPlan));
    }

    internal static Result ValidateFixedAmountAgainstTuitionPlan(
        RegistrationDiscountType discountType,
        decimal discountValue,
        TuitionPlan? tuitionPlan)
    {
        if (discountType != RegistrationDiscountType.FixedAmount || tuitionPlan is null)
        {
            return Result.Success();
        }

        return discountValue <= tuitionPlan.TuitionAmount
            ? Result.Success()
            : Result.Failure(RegistrationDiscountCampaignErrors.FixedAmountExceedsTuitionPlanAmount(
                discountValue,
                tuitionPlan.TuitionAmount));
    }
}

internal sealed record RegistrationDiscountCampaignScopeValidationResult(
    Branch? Branch,
    Program? Program,
    Level? Level,
    TuitionPlan? TuitionPlan);
