using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Programs.Shared;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Registrations.Notifications;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Application.Students.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LearningTickets;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.CreateRegistration;

public sealed class CreateRegistrationCommandHandler(
    IDbContext context,
    TicketGrantService ticketGrantService,
    IUserContext userContext
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

        var branchAccessResult = await StudentBranchAccessHelper.ValidateBranchAccessAsync(
            context,
            command.StudentProfileId,
            command.BranchId,
            allowCrossBranchEnrollment: false,
            cancellationToken);
        if (branchAccessResult.IsFailure)
        {
            return Result.Failure<CreateRegistrationResponse>(branchAccessResult.Error);
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

        var level = await context.Levels
            .FirstOrDefaultAsync(
                l => l.Id == command.LevelId &&
                     l.ProgramId == command.ProgramId &&
                     l.IsActive,
                cancellationToken);

        if (level is null)
        {
            return Result.Failure<CreateRegistrationResponse>(
                Error.Validation(
                    "Registration.LevelNotFoundInProgram",
                    $"Level '{command.LevelId}' was not found, inactive, or does not belong to the selected program."));
        }

        Kidzgo.Domain.Programs.Level? secondaryLevel = null;
        if (command.SecondaryLevelId.HasValue)
        {
            if (command.SecondaryLevelId.Value == command.LevelId)
            {
                return Result.Failure<CreateRegistrationResponse>(
                    Error.Validation(
                        "Registration.SecondaryLevelDuplicated",
                        "Secondary level must be different from primary level."));
            }

            secondaryLevel = await context.Levels
                .FirstOrDefaultAsync(
                    l => l.Id == command.SecondaryLevelId.Value &&
                         l.ProgramId == command.ProgramId &&
                         l.IsActive,
                    cancellationToken);

            if (secondaryLevel is null)
            {
                return Result.Failure<CreateRegistrationResponse>(
                    Error.Validation(
                        "Registration.SecondaryLevelNotFoundInProgram",
                        $"Secondary level '{command.SecondaryLevelId.Value}' was not found, inactive, or does not belong to the selected program."));
            }
        }
        else if (command.SecondaryLevelSkillFocus is not null)
        {
            return Result.Failure<CreateRegistrationResponse>(
                Error.Validation(
                    "Registration.SecondaryLevelMissing",
                    "Secondary level skill focus can only be set when secondary level exists."));
        }

        var tuitionPlan = await context.TuitionPlans
            .FirstOrDefaultAsync(
                tp => tp.Id == command.TuitionPlanId &&
                      tp.ProgramId == command.ProgramId &&
                      tp.LevelId == command.LevelId &&
                      tp.IsActive &&
                      !tp.IsDeleted,
                cancellationToken);

        if (tuitionPlan == null)
        {
            return Result.Failure<CreateRegistrationResponse>(RegistrationErrors.TuitionPlanNotFound(command.TuitionPlanId));
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
            command.LevelId,
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
            LevelId = command.LevelId,
            SecondaryLevelId = command.SecondaryLevelId,
            TuitionPlanId = command.TuitionPlanId,
            SecondaryProgramId = null,
            RegistrationDate = now,
            ExpectedStartDate = command.ExpectedStartDate,
            PreferredSchedule = command.PreferredSchedule,
            Note = command.Note,
            SecondaryProgramSkillFocus = string.IsNullOrWhiteSpace(command.SecondaryLevelSkillFocus)
                ? null
                : command.SecondaryLevelSkillFocus.Trim(),
            Status = RegistrationStatus.New,
            TotalSessions = tuitionPlan.TotalSessions,
            UsedSessions = 0,
            RemainingSessions = tuitionPlan.TotalSessions,
            CreatedAt = now,
            UpdatedAt = now
        };
        RegistrationDiscountPricingHelper.ApplyToRegistration(registration, pricing);

        context.Registrations.Add(registration);
        await ticketGrantService.GrantTicketsAsync(
            registration.StudentProfileId,
            registration.Id,
            tuitionPlan.TotalSessions,
            $"Purchase {tuitionPlan.Name}",
            LearningTicketSource.Purchase,
            createdByUserId: null,
            cancellationToken);
        var rolloverCredits = await ticketGrantService.GrantRolloverMakeupCreditsAsync(
            registration.StudentProfileId,
            registration.Id,
            createdByUserId: null,
            cancellationToken);

        registration.TotalSessions += rolloverCredits;
        registration.RemainingSessions += rolloverCredits;

        RegistrationAuditLogHelper.AddAuditLog(
            context,
            userContext,
            RegistrationAuditActions.CreateRegistration,
            registration,
            dataBefore: null,
            dataAfter: new
            {
                registration = RegistrationAuditLogHelper.CreateSnapshot(registration),
                source = "manual",
                RolloverMakeupCredits = rolloverCredits
            },
            timestamp: now);
        await context.SaveChangesAsync(cancellationToken);
        await WaitingListThresholdNotificationHelper.NotifyAsync(context, registration, cancellationToken);

        return new CreateRegistrationResponse
        {
            Id = registration.Id,
            StudentProfileId = registration.StudentProfileId,
            BranchId = registration.BranchId,
            ProgramId = registration.ProgramId,
            ProgramName = program.Name,
            LevelId = registration.LevelId,
            LevelName = level.Name,
            SecondaryLevelId = registration.SecondaryLevelId,
            SecondaryLevelName = secondaryLevel?.Name,
            SecondaryLevelSkillFocus = registration.SecondaryProgramSkillFocus,
            TuitionPlanId = registration.TuitionPlanId,
            TuitionPlanName = tuitionPlan.Name,
            RegistrationDate = registration.RegistrationDate,
            ExpectedStartDate = registration.ExpectedStartDate,
            PreferredSchedule = registration.PreferredSchedule,
            Note = registration.Note,
            Status = registration.Status.ToString(),
            StudentHomeBranchId = branchAccessResult.Value.State.HomeBranchId,
            StudentActiveBranchId = branchAccessResult.Value.State.ActiveBranchId,
            IsCrossBranchRegistration = branchAccessResult.Value.IsCrossBranch,
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
            RolledOverMakeupCredits = rolloverCredits,
            CreatedAt = registration.CreatedAt
        };
    }
}
