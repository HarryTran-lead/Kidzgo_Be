using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.UpdateRegistration.Handler;

public sealed class UpdateRegistrationCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<UpdateRegistrationCommand, UpdateRegistrationResponse>
{
    public async Task<Result<UpdateRegistrationResponse>> Handle(
        UpdateRegistrationCommand command,
        CancellationToken cancellationToken)
    {
        var now = VietnamTime.UtcNow();

        var registration = await context.Registrations
            .Include(r => r.TuitionPlan)
            .Include(r => r.SecondaryLevel)
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

        if (registration == null)
        {
            return Result.Failure<UpdateRegistrationResponse>(RegistrationErrors.NotFound(command.Id));
        }

        // Can only update if not completed or cancelled
        if (registration.Status == RegistrationStatus.Completed || 
            registration.Status == RegistrationStatus.Cancelled)
        {
            return Result.Failure<UpdateRegistrationResponse>(
                RegistrationErrors.InvalidStatus(registration.Status.ToString(), "update"));
        }

        var beforeSnapshot = RegistrationAuditLogHelper.CreateSnapshot(registration);

        if (command.ExpectedStartDate.HasValue)
        {
            registration.ExpectedStartDate = command.ExpectedStartDate;
        }

        if (command.PreferredSchedule != null)
        {
            registration.PreferredSchedule = command.PreferredSchedule;
        }

        if (command.Note != null)
        {
            registration.Note = command.Note;
        }

        if (command.RemoveSecondaryLevel)
        {
            if (registration.SecondaryClassId.HasValue)
            {
                return Result.Failure<UpdateRegistrationResponse>(
                    Error.Validation("Registration.SecondaryClassAssigned", "Cannot remove the secondary level while a secondary class is assigned"));
            }

            registration.SecondaryLevelId = null;
            registration.SecondaryProgramId = null;
            registration.SecondaryLevel = null;
            registration.SecondaryProgramSkillFocus = null;
            registration.SecondaryClassAssignedDate = null;
            registration.SecondaryEntryType = null;
        }

        if (command.SecondaryLevelId.HasValue)
        {
            if (command.SecondaryLevelId.Value == registration.LevelId)
            {
                return Result.Failure<UpdateRegistrationResponse>(
                    Error.Validation("Registration.SecondaryLevelDuplicated", "Secondary level must be different from primary level"));
            }

            var secondaryLevel = await context.Levels
                .FirstOrDefaultAsync(
                    l => l.Id == command.SecondaryLevelId.Value &&
                         l.ProgramId == registration.ProgramId &&
                         l.IsActive,
                    cancellationToken);

            if (secondaryLevel is null)
            {
                return Result.Failure<UpdateRegistrationResponse>(
                    Error.Validation(
                        "Registration.SecondaryLevelNotFoundInProgram",
                        $"Secondary level '{command.SecondaryLevelId.Value}' was not found, inactive, or does not belong to the registration program."));
            }

            if (registration.SecondaryClassId.HasValue &&
                registration.SecondaryLevelId != command.SecondaryLevelId.Value)
            {
                return Result.Failure<UpdateRegistrationResponse>(
                    Error.Validation("Registration.SecondaryClassAssigned", "Cannot change the secondary level while a secondary class is assigned"));
            }

            registration.SecondaryLevelId = secondaryLevel.Id;
            registration.SecondaryLevel = secondaryLevel;
            registration.SecondaryProgramId = null;
            registration.SecondaryProgramSkillFocus = string.IsNullOrWhiteSpace(command.SecondaryLevelSkillFocus)
                ? null
                : command.SecondaryLevelSkillFocus.Trim();
            registration.SecondaryEntryType = null;
            registration.SecondaryClassAssignedDate = null;
        }
        else if (command.SecondaryLevelSkillFocus is not null && registration.SecondaryLevelId is not null)
        {
            registration.SecondaryProgramSkillFocus = string.IsNullOrWhiteSpace(command.SecondaryLevelSkillFocus)
                ? null
                : command.SecondaryLevelSkillFocus.Trim();
        }
        else if (command.SecondaryLevelSkillFocus is not null && registration.SecondaryLevelId is null)
        {
            return Result.Failure<UpdateRegistrationResponse>(
                Error.Validation(
                    "Registration.SecondaryLevelMissing",
                    "Secondary level skill focus can only be set when secondary level exists."));
        }

        if (command.TuitionPlanId.HasValue && (registration.ClassId != null || registration.SecondaryClassId != null))
        {
            return Result.Failure<UpdateRegistrationResponse>(
                Error.Validation("Registration.ClassAlreadyAssigned", "Tuition plan cannot be changed after any class has been assigned"));
        }

        if (command.TuitionPlanId.HasValue)
        {
            var tuitionPlan = await context.TuitionPlans.FindAsync(
                new object[] { command.TuitionPlanId.Value }, 
                cancellationToken);

            if (tuitionPlan == null)
            {
                return Result.Failure<UpdateRegistrationResponse>(
                    RegistrationErrors.TuitionPlanNotFound(command.TuitionPlanId.Value));
            }

            // Validate same program
            if (tuitionPlan.ProgramId != registration.ProgramId)
            {
                return Result.Failure<UpdateRegistrationResponse>(
                    Error.Validation("DifferentProgram", "Tuition plan must belong to the same program"));
            }

            if (tuitionPlan.LevelId != registration.LevelId)
            {
                return Result.Failure<UpdateRegistrationResponse>(
                    Error.Validation("DifferentLevel", "Tuition plan must match registration level"));
            }

            registration.TuitionPlanId = command.TuitionPlanId.Value;
            registration.TuitionPlan = tuitionPlan;
            registration.TotalSessions = tuitionPlan.TotalSessions;
            registration.RemainingSessions = tuitionPlan.TotalSessions - registration.UsedSessions;

            var operationType = registration.OperationType is OperationType.Initial or OperationType.Renewal
                ? registration.OperationType.Value
                : await RegistrationDiscountPricingHelper.ResolveInitialOrRenewalOperationTypeAsync(
                    context,
                    registration.StudentProfileId,
                    registration.Id,
                    cancellationToken);

            var pricing = await RegistrationDiscountPricingHelper.ResolveAsync(
                context,
                registration.BranchId,
                registration.ProgramId,
                registration.LevelId,
                tuitionPlan.Id,
                operationType,
                registration.RegistrationDate,
                tuitionPlan.TuitionAmount,
                carryOverCreditAmount: 0m,
                cancellationToken);

            RegistrationDiscountPricingHelper.ApplyToRegistration(registration, pricing);
        }

        registration.UpdatedAt = now;
        RegistrationAuditLogHelper.AddAuditLog(
            context,
            userContext,
            RegistrationAuditActions.UpdateRegistration,
            registration,
            dataBefore: beforeSnapshot,
            dataAfter: new
            {
                registration = RegistrationAuditLogHelper.CreateSnapshot(registration)
            },
            timestamp: now);

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateRegistrationResponse
        {
            Id = registration.Id,
            ExpectedStartDate = registration.ExpectedStartDate,
            PreferredSchedule = registration.PreferredSchedule,
            Note = registration.Note,
            TuitionPlanId = registration.TuitionPlanId,
            TuitionPlanName = registration.TuitionPlan?.Name,
            SecondaryLevelId = registration.SecondaryLevelId,
            SecondaryLevelName = registration.SecondaryLevel?.Name,
            SecondaryLevelSkillFocus = registration.SecondaryProgramSkillFocus,
            OperationType = registration.OperationType?.ToString(),
            DiscountCampaignId = registration.DiscountCampaignId,
            DiscountCampaignName = registration.DiscountCampaignName,
            DiscountType = registration.DiscountType?.ToString(),
            DiscountValue = registration.DiscountValue,
            OriginalTuitionAmount = registration.OriginalTuitionAmount ?? registration.TuitionPlan?.TuitionAmount,
            DiscountAmount = registration.DiscountAmount ?? 0m,
            CarryOverCreditAmount = registration.CarryOverCreditAmount ?? 0m,
            FinalTuitionAmount = registration.FinalTuitionAmount ?? registration.TuitionPlan?.TuitionAmount,
            UpdatedAt = now
        };
    }
}
