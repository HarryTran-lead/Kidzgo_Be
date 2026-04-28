using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Programs.Shared;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.CreateRegistration;

public sealed class CreateRegistrationCommandHandler(
    IDbContext context
) : ICommandHandler<CreateRegistrationCommand, CreateRegistrationResponse>
{
    public async Task<Result<CreateRegistrationResponse>> Handle(
        CreateRegistrationCommand command,
        CancellationToken cancellationToken)
    {
        var student = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId && p.ProfileType == Kidzgo.Domain.Users.ProfileType.Student, cancellationToken);

        if (student == null)
        {
            return Result.Failure<CreateRegistrationResponse>(RegistrationErrors.StudentNotFound(command.StudentProfileId));
        }

        var branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<CreateRegistrationResponse>(RegistrationErrors.BranchNotFound(command.BranchId));
        }

        var program = await context.Programs
            .FirstOrDefaultAsync(p => p.Id == command.ProgramId && p.IsActive && !p.IsDeleted, cancellationToken);

        if (program == null)
        {
            return Result.Failure<CreateRegistrationResponse>(RegistrationErrors.ProgramNotFound(command.ProgramId));
        }

        var programAssignedToBranch = await BranchProgramAccessHelper.IsProgramAssignedToBranchAsync(
            context,
            command.BranchId,
            command.ProgramId,
            cancellationToken);

        if (!programAssignedToBranch)
        {
            return Result.Failure<CreateRegistrationResponse>(
                RegistrationErrors.ProgramNotAvailableInBranch(command.ProgramId, command.BranchId));
        }

        var tuitionPlan = await context.TuitionPlans
            .FirstOrDefaultAsync(
                tp => tp.Id == command.TuitionPlanId &&
                      tp.ProgramId == command.ProgramId &&
                      tp.IsActive &&
                      !tp.IsDeleted &&
                      (!tp.BranchId.HasValue || tp.BranchId == command.BranchId),
                cancellationToken);

        if (tuitionPlan == null)
        {
            return Result.Failure<CreateRegistrationResponse>(RegistrationErrors.TuitionPlanNotFound(command.TuitionPlanId));
        }

        if (command.SecondaryProgramId == command.ProgramId)
        {
            return Result.Failure<CreateRegistrationResponse>(
                Error.Validation("Registration.SecondaryProgramDuplicated", "Secondary program must be different from primary program"));
        }

        Domain.Programs.Program? secondaryProgram = null;
        if (command.SecondaryProgramId.HasValue)
        {
            secondaryProgram = await context.Programs
                .FirstOrDefaultAsync(p => p.Id == command.SecondaryProgramId.Value && p.IsActive && !p.IsDeleted, cancellationToken);

            if (secondaryProgram is null)
            {
                return Result.Failure<CreateRegistrationResponse>(
                    RegistrationErrors.ProgramNotFound(command.SecondaryProgramId.Value));
            }

            var secondaryProgramAssignedToBranch = await BranchProgramAccessHelper.IsProgramAssignedToBranchAsync(
                context,
                command.BranchId,
                command.SecondaryProgramId.Value,
                cancellationToken);

            if (!secondaryProgramAssignedToBranch)
            {
                return Result.Failure<CreateRegistrationResponse>(
                    RegistrationErrors.ProgramNotAvailableInBranch(command.SecondaryProgramId.Value, command.BranchId));
            }

            if (secondaryProgram.IsSupplementary)
            {
                return Result.Failure<CreateRegistrationResponse>(
                    RegistrationErrors.SecondarySupplementaryRequiresSeparateRegistration(command.SecondaryProgramId.Value));
            }
        }

        var activeRegistrations = context.Registrations
            .Where(r => r.StudentProfileId == command.StudentProfileId
                && r.Status != RegistrationStatus.Completed
                && r.Status != RegistrationStatus.Cancelled);

        var hasPrimaryConflict = await activeRegistrations.AnyAsync(
            r => r.ProgramId == command.ProgramId || r.SecondaryProgramId == command.ProgramId,
            cancellationToken);

        if (hasPrimaryConflict)
        {
            return Result.Failure<CreateRegistrationResponse>(
                RegistrationErrors.AlreadyExists(command.StudentProfileId, command.ProgramId));
        }

        if (command.SecondaryProgramId.HasValue)
        {
            var secondaryProgramId = command.SecondaryProgramId.Value;
            var hasSecondaryConflict = await activeRegistrations.AnyAsync(
                r => r.ProgramId == secondaryProgramId || r.SecondaryProgramId == secondaryProgramId,
                cancellationToken);

            if (hasSecondaryConflict)
            {
                return Result.Failure<CreateRegistrationResponse>(
                    RegistrationErrors.AlreadyExists(command.StudentProfileId, secondaryProgramId));
            }
        }

        var now = VietnamTime.UtcNow();
        var operationType = await RegistrationDiscountPricingHelper.ResolveInitialOrRenewalOperationTypeAsync(
            context,
            command.StudentProfileId,
            excludeRegistrationId: null,
            cancellationToken);
        var pricing = await RegistrationDiscountPricingHelper.ResolveAsync(
            context,
            command.BranchId,
            command.ProgramId,
            tuitionPlan.Id,
            operationType,
            now,
            tuitionPlan.TuitionAmount,
            carryOverCreditAmount: 0m,
            cancellationToken);
        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            StudentProfileId = command.StudentProfileId,
            BranchId = command.BranchId,
            ProgramId = command.ProgramId,
            TuitionPlanId = command.TuitionPlanId,
            SecondaryProgramId = command.SecondaryProgramId,
            RegistrationDate = now,
            ExpectedStartDate = command.ExpectedStartDate,
            PreferredSchedule = command.PreferredSchedule,
            Note = command.Note,
            SecondaryProgramSkillFocus = string.IsNullOrWhiteSpace(command.SecondaryProgramSkillFocus)
                ? null
                : command.SecondaryProgramSkillFocus.Trim(),
            Status = RegistrationStatus.New,
            TotalSessions = tuitionPlan.TotalSessions,
            UsedSessions = 0,
            RemainingSessions = tuitionPlan.TotalSessions,
            CreatedAt = now,
            UpdatedAt = now
        };
        RegistrationDiscountPricingHelper.ApplyToRegistration(registration, pricing);

        context.Registrations.Add(registration);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateRegistrationResponse
        {
            Id = registration.Id,
            StudentProfileId = registration.StudentProfileId,
            BranchId = registration.BranchId,
            ProgramId = registration.ProgramId,
            ProgramName = program.Name,
            SecondaryProgramId = registration.SecondaryProgramId,
            SecondaryProgramName = secondaryProgram?.Name,
            SecondaryProgramSkillFocus = registration.SecondaryProgramSkillFocus,
            TuitionPlanId = registration.TuitionPlanId,
            TuitionPlanName = tuitionPlan.Name,
            RegistrationDate = registration.RegistrationDate,
            ExpectedStartDate = registration.ExpectedStartDate,
            PreferredSchedule = registration.PreferredSchedule,
            Note = registration.Note,
            Status = registration.Status.ToString(),
            OperationType = registration.OperationType?.ToString(),
            ClassId = null,
            ClassName = null,
            SecondaryClassId = null,
            SecondaryClassName = null,
            DiscountCampaignId = registration.DiscountCampaignId,
            DiscountCampaignName = registration.DiscountCampaignName,
            DiscountType = registration.DiscountType?.ToString(),
            DiscountValue = registration.DiscountValue,
            OriginalTuitionAmount = registration.OriginalTuitionAmount ?? tuitionPlan.TuitionAmount,
            DiscountAmount = registration.DiscountAmount ?? 0m,
            CarryOverCreditAmount = registration.CarryOverCreditAmount ?? 0m,
            FinalTuitionAmount = registration.FinalTuitionAmount ?? tuitionPlan.TuitionAmount,
            CreatedAt = registration.CreatedAt
        };
    }
}
