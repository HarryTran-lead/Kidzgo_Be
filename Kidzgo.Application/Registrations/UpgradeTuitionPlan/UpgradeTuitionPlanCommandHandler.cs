using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LearningTickets;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.UpgradeTuitionPlan.Handler;

public sealed class UpgradeTuitionPlanCommandHandler(
    IDbContext context,
    TicketGrantService ticketGrantService,
    IUserContext userContext
) : ICommandHandler<UpgradeTuitionPlanCommand, UpgradeTuitionPlanResponse>
{
    public async Task<Result<UpgradeTuitionPlanResponse>> Handle(
        UpgradeTuitionPlanCommand command,
        CancellationToken cancellationToken)
    {
        var now = VietnamTime.UtcNow();

        // 1. Get current registration
        var registration = await context.Registrations
            .Include(r => r.TuitionPlan)
            .FirstOrDefaultAsync(r => r.Id == command.RegistrationId, cancellationToken);

        if (registration == null)
        {
            return Result.Failure<UpgradeTuitionPlanResponse>(RegistrationErrors.NotFound(command.RegistrationId));
        }

        // 2. Validate registration is active (studying or waiting for class)
        if (registration.Status != RegistrationStatus.Studying && 
            registration.Status != RegistrationStatus.ClassAssigned &&
            registration.Status != RegistrationStatus.WaitingForClass)
        {
            return Result.Failure<UpgradeTuitionPlanResponse>(
                RegistrationErrors.NoActiveRegistrationForUpgrade(registration.StudentProfileId));
        }

        var beforeSnapshot = RegistrationAuditLogHelper.CreateSnapshot(registration);

        // 3. Get new tuition plan
        var newTuitionPlan = await context.TuitionPlans.FindAsync(
            new object[] { command.NewTuitionPlanId }, 
            cancellationToken);

        if (newTuitionPlan == null)
        {
            return Result.Failure<UpgradeTuitionPlanResponse>(
                RegistrationErrors.TuitionPlanNotFound(command.NewTuitionPlanId));
        }

        // 4. Validate new tuition plan belongs to same program
        if (newTuitionPlan.ProgramId != registration.ProgramId)
        {
            return Result.Failure<UpgradeTuitionPlanResponse>(
                Error.Validation("DifferentProgram", "New tuition plan must belong to the same program"));
        }

        if (newTuitionPlan.LevelId != registration.LevelId)
        {
            return Result.Failure<UpgradeTuitionPlanResponse>(
                Error.Validation("DifferentLevel", "New tuition plan must match the registration level"));
        }

        // 5. Extend the current registration in place
        var oldTotalSessions = registration.TotalSessions;
        var carriedForwardSessions = Math.Max(registration.RemainingSessions, 0);
        var oldTuitionPlanName = registration.TuitionPlan.Name;
        var carryOverCreditAmount = Math.Round(
            Math.Max(registration.RemainingSessions, 0) * registration.TuitionPlan.UnitPriceSession,
            2,
            MidpointRounding.AwayFromZero);
        var pricing = await RegistrationDiscountPricingHelper.ResolveAsync(
            context,
            registration.BranchId,
            registration.ProgramId,
            registration.LevelId,
            newTuitionPlan.Id,
            OperationType.Upgrade,
            now,
            newTuitionPlan.TuitionAmount,
            carryOverCreditAmount,
            cancellationToken);
        var rolloverCredits = await ticketGrantService.GrantRolloverMakeupCreditsAsync(
            registration.StudentProfileId,
            registration.Id,
            createdByUserId: null,
            cancellationToken);
        var upgradedRemainingSessions = carriedForwardSessions + newTuitionPlan.TotalSessions + rolloverCredits;
        var upgradedTotalSessions = registration.UsedSessions + upgradedRemainingSessions;

        registration.TuitionPlanId = newTuitionPlan.Id;
        registration.TuitionPlan = newTuitionPlan;
        registration.TotalSessions = upgradedTotalSessions;
        registration.RemainingSessions = upgradedRemainingSessions;
        registration.Status = RegistrationTrackHelper.ResolveStatus(registration);
        registration.UpdatedAt = now;
        RegistrationDiscountPricingHelper.ApplyToRegistration(registration, pricing);

        /*
        // 7. Create new registration with upgraded tuition plan
        var newRegistration = new Registration
        {
            Id = Guid.NewGuid(),
            StudentProfileId = registration.StudentProfileId,
            BranchId = registration.BranchId,
            ProgramId = registration.ProgramId,
            TuitionPlanId = newTuitionPlan.Id,
            SecondaryProgramId = registration.SecondaryProgramId,
            RegistrationDate = now,
            ExpectedStartDate = now, // Start immediately after upgrade
            ActualStartDate = registration.ActualStartDate,
            PreferredSchedule = registration.PreferredSchedule,
            Note = $"Nâng cấp từ gói {oldTuitionPlanName}",
            Status = registration.Status,
            ClassId = registration.ClassId,
            ClassAssignedDate = registration.ClassAssignedDate,
            EntryType = registration.EntryType,
            SecondaryClassId = registration.SecondaryClassId,
            SecondaryClassAssignedDate = registration.SecondaryClassAssignedDate,
            SecondaryEntryType = registration.SecondaryEntryType,
            SecondaryProgramSkillFocus = registration.SecondaryProgramSkillFocus,
            OriginalRegistrationId = registration.Id,
            OperationType = OperationType.Upgrade,
            TotalSessions = upgradedTotalSessions,
            UsedSessions = 0,
            RemainingSessions = upgradedTotalSessions,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Registrations.Add(newRegistration);
        newRegistration.Status = RegistrationTrackHelper.ResolveStatus(newRegistration);
        */

        var enrollments = await context.ClassEnrollments
            .Where(ce => ce.StudentProfileId == registration.StudentProfileId
                && ce.Status == Domain.Classes.EnrollmentStatus.Active
                && (ce.RegistrationId == registration.Id ||
                    (!ce.RegistrationId.HasValue &&
                     (ce.ClassId == registration.ClassId || ce.ClassId == registration.SecondaryClassId))))
            .ToListAsync(cancellationToken);

        foreach (var enrollment in enrollments)
        {
            enrollment.TuitionPlanId = newTuitionPlan.Id;
            enrollment.UpdatedAt = now;
        }

        await ticketGrantService.GrantTicketsAsync(
            registration.StudentProfileId,
            registration.Id,
            newTuitionPlan.TotalSessions,
            $"Upgrade to {newTuitionPlan.Name}",
            LearningTicketSource.Purchase,
            createdByUserId: null,
            cancellationToken);
        RegistrationAuditLogHelper.AddAuditLog(
            context,
            userContext,
            RegistrationAuditActions.UpgradeRegistrationTuitionPlan,
            registration,
            dataBefore: beforeSnapshot,
            dataAfter: new
            {
                registration = RegistrationAuditLogHelper.CreateSnapshot(registration),
                oldTuitionPlanName,
                NewTuitionPlanId = newTuitionPlan.Id,
                NewTuitionPlanName = newTuitionPlan.Name,
                oldTotalSessions,
                NewTotalSessions = upgradedTotalSessions,
                AddedSessions = newTuitionPlan.TotalSessions,
                RolloverMakeupCredits = rolloverCredits,
                CarryOverCreditAmount = registration.CarryOverCreditAmount ?? carryOverCreditAmount
            },
            timestamp: now);

        await context.SaveChangesAsync(cancellationToken);

        return new UpgradeTuitionPlanResponse
        {
            OriginalRegistrationId = registration.Id,
            NewRegistrationId = registration.Id,
            OldTuitionPlanName = oldTuitionPlanName,
            NewTuitionPlanName = newTuitionPlan.Name,
            OldTotalSessions = oldTotalSessions,
            NewTotalSessions = upgradedTotalSessions,
            AddedSessions = newTuitionPlan.TotalSessions,
            DiscountCampaignId = registration.DiscountCampaignId,
            DiscountCampaignName = registration.DiscountCampaignName,
            DiscountType = registration.DiscountType?.ToString(),
            DiscountValue = registration.DiscountValue,
            OriginalTuitionAmount = registration.OriginalTuitionAmount ?? newTuitionPlan.TuitionAmount,
            DiscountAmount = registration.DiscountAmount ?? 0m,
            CarryOverCreditAmount = registration.CarryOverCreditAmount ?? carryOverCreditAmount,
            FinalTuitionAmount = registration.FinalTuitionAmount ?? Math.Max(0m, newTuitionPlan.TuitionAmount - carryOverCreditAmount),
            RolledOverMakeupCredits = rolloverCredits,
            Status = registration.Status.ToString()
        };
    }

}
