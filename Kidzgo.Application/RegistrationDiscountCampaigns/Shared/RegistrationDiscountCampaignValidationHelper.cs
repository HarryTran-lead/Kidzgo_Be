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
        Guid? tuitionPlanId,
        CancellationToken cancellationToken)
    {
        Branch? branch = null;
        Program? program = null;
        TuitionPlan? tuitionPlan = null;

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

            if (programId.HasValue && tuitionPlan.ProgramId != programId.Value)
            {
                return Result.Failure<RegistrationDiscountCampaignScopeValidationResult>(
                    RegistrationDiscountCampaignErrors.TuitionPlanProgramMismatch);
            }

            if (branchId.HasValue &&
                tuitionPlan.BranchId.HasValue &&
                tuitionPlan.BranchId.Value != branchId.Value)
            {
                return Result.Failure<RegistrationDiscountCampaignScopeValidationResult>(
                    RegistrationDiscountCampaignErrors.TuitionPlanBranchMismatch);
            }
        }

        return Result.Success(new RegistrationDiscountCampaignScopeValidationResult(branch, program, tuitionPlan));
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
    TuitionPlan? TuitionPlan);
