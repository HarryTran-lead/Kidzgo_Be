using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Classes;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Services;
using Kidzgo.Application.Students.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.TransferClass.Handler;

public sealed class TransferClassCommandHandler(
    IDbContext context,
    ClassLifecycleService classLifecycleService,
    StudentSessionAssignmentService studentSessionAssignmentService,
    StudentEnrollmentScheduleConflictService studentEnrollmentScheduleConflictService
) : ICommandHandler<TransferClassCommand, TransferClassResponse>
{
    public async Task<Result<TransferClassResponse>> Handle(
        TransferClassCommand command,
        CancellationToken cancellationToken)
    {
        var now = VietnamTime.UtcNow();
        var effectiveDate = VietnamTime.ToVietnamDateOnly(command.EffectiveDate);
        var track = RegistrationTrackHelper.NormalizeTrack(command.Track);
        var isSecondaryTrack = track == RegistrationTrackHelper.SecondaryTrack;
        var weeklyPatternResult = SchedulePatternSupport.NormalizeWeeklyPatternJson(
            command.WeeklyPattern,
            requireValue: false);
        if (weeklyPatternResult.IsFailure)
        {
            return Result.Failure<TransferClassResponse>(weeklyPatternResult.Error);
        }

        var sessionSelectionPattern = weeklyPatternResult.Value;

        var registration = await context.Registrations
            .Include(r => r.Program)
            .Include(r => r.TuitionPlan)
            .Include(r => r.Class)
            .Include(r => r.SecondaryClass)
            .FirstOrDefaultAsync(r => r.Id == command.RegistrationId, cancellationToken);

        if (registration == null)
        {
            return Result.Failure<TransferClassResponse>(RegistrationErrors.NotFound(command.RegistrationId));
        }

        if (registration.Status == RegistrationStatus.Completed ||
            registration.Status == RegistrationStatus.Cancelled)
        {
            return Result.Failure<TransferClassResponse>(
                RegistrationErrors.InvalidStatus(registration.Status.ToString(), "transfer-class"));
        }

        var currentClassId = isSecondaryTrack ? registration.SecondaryClassId : registration.ClassId;
        var targetProgramId = registration.ProgramId;

        if (isSecondaryTrack && !registration.SecondaryLevelId.HasValue)
        {
            return Result.Failure<TransferClassResponse>(
                Error.Validation("Registration.SecondaryLevelMissing", "Registration does not have a secondary level to transfer"));
        }

        if (currentClassId == null)
        {
            return Result.Failure<TransferClassResponse>(
                Error.Validation("NoClassAssigned", $"Registration track '{track}' has no class assigned"));
        }

        var oldClassId = currentClassId.Value;

        // Early exit: cannot transfer to same class (avoids 2 unnecessary DB queries)
        if (oldClassId == command.NewClassId)
        {
            return Result.Failure<TransferClassResponse>(RegistrationErrors.CannotTransferToSameClass());
        }

        var oldClass = await context.Classes
            .Include(c => c.ClassEnrollments)
            .FirstOrDefaultAsync(c => c.Id == oldClassId, cancellationToken);

        var newClass = await context.Classes
            .Include(c => c.ClassEnrollments)
            .FirstOrDefaultAsync(c => c.Id == command.NewClassId, cancellationToken);

        if (newClass == null)
        {
            return Result.Failure<TransferClassResponse>(RegistrationErrors.ClassNotFound(command.NewClassId));
        }

        if (await IsExplicitlyIncompatibleAsync(
                registration.TuitionPlan?.LearningTicketTypeId,
                newClass.SlotTypeId,
                cancellationToken))
        {
            return Result.Failure<TransferClassResponse>(
                RegistrationErrors.TicketTypeIncompatibleWithClassSlotType(
                    registration.TuitionPlan?.LearningTicketTypeId,
                    newClass.SlotTypeId));
        }

        var branchAccessResult = await StudentBranchAccessHelper.ValidateBranchAccessAsync(
            context,
            registration.StudentProfileId,
            newClass.BranchId,
            allowCrossBranchEnrollment: false,
            cancellationToken);
        if (branchAccessResult.IsFailure)
        {
            return Result.Failure<TransferClassResponse>(branchAccessResult.Error);
        }

        if (newClass.BranchId != registration.BranchId)
        {
            return Result.Failure<TransferClassResponse>(
                RegistrationErrors.ClassNotMatchingBranch(command.NewClassId, registration.BranchId));
        }

        if (newClass.ProgramId != targetProgramId)
        {
            return Result.Failure<TransferClassResponse>(
                RegistrationErrors.ClassNotMatchingProgram(command.NewClassId, targetProgramId));
        }

        var selectionPatternValidation = await studentSessionAssignmentService
            .ValidateSelectionPatternAsync(newClass, sessionSelectionPattern, cancellationToken);
        if (selectionPatternValidation.IsFailure)
        {
            return Result.Failure<TransferClassResponse>(selectionPatternValidation.Error);
        }

        var newClassActiveEnrollmentCount = newClass.ClassEnrollments
            .Count(ce => ce.Status == EnrollmentStatus.Active);
        ClassCapacityStatusHelper.SyncAvailabilityStatus(newClass, newClassActiveEnrollmentCount, now);

        // Check new class capacity
        if (newClassActiveEnrollmentCount >= newClass.Capacity)
        {
            return Result.Failure<TransferClassResponse>(RegistrationErrors.ClassFull(command.NewClassId));
        }

        // Check new class status
        if (newClass.Status != ClassStatus.Active && newClass.Status != ClassStatus.Recruiting)
        {
            return Result.Failure<TransferClassResponse>(
                Error.Validation("ClassNotAvailable", $"Cannot transfer to class with status {newClass.Status}"));
        }

        var oldEnrollment = await context.ClassEnrollments
            .FirstOrDefaultAsync(ce => ce.ClassId == oldClassId
                && ce.StudentProfileId == registration.StudentProfileId
                && (!ce.RegistrationId.HasValue || ce.RegistrationId == registration.Id)
                && ce.Status == EnrollmentStatus.Active,
                cancellationToken);

        var conflictResult = await studentEnrollmentScheduleConflictService.EnsureNoConflictsAsync(
            registration.StudentProfileId,
            newClass.Id,
            effectiveDate,
            sessionSelectionPattern,
            cancellationToken,
            excludeEnrollmentId: oldEnrollment?.Id,
            excludeLegacyClassId: oldClassId,
            excludeSlotsFromUtc: command.EffectiveDate);
        if (conflictResult.IsFailure)
        {
            return Result.Failure<TransferClassResponse>(conflictResult.Error);
        }

        // Update old enrollment to dropped
        if (oldEnrollment != null)
        {
            oldEnrollment.Status = EnrollmentStatus.Dropped;
            oldEnrollment.UpdatedAt = now;
            await studentSessionAssignmentService.CancelFutureAssignmentsForEnrollmentAsync(
                oldEnrollment.Id,
                command.EffectiveDate,
                cancellationToken);
        }

        // Create new enrollment
        var newEnrollment = new ClassEnrollment
        {
            Id = Guid.NewGuid(),
            ClassId = command.NewClassId,
            StudentProfileId = registration.StudentProfileId,
            EnrollDate = effectiveDate,
            Status = EnrollmentStatus.Active,
            TuitionPlanId = registration.TuitionPlanId,
            RegistrationId = registration.Id,
            Track = RegistrationTrackHelper.ToTrackType(track),
            SessionSelectionPattern = sessionSelectionPattern,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.ClassEnrollments.Add(newEnrollment);
        var targetProgram = registration.Program;
        if (targetProgram?.IsSupplementary == true)
        {
            context.ClassEnrollmentScheduleSegments.Add(new ClassEnrollmentScheduleSegment
            {
                Id = Guid.NewGuid(),
                ClassEnrollmentId = newEnrollment.Id,
                EffectiveFrom = newEnrollment.EnrollDate,
                SessionSelectionPattern = newEnrollment.SessionSelectionPattern,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await studentSessionAssignmentService.SyncAssignmentsForEnrollmentAsync(newEnrollment, cancellationToken);
        if (oldClass != null)
        {
            await ClassCapacityStatusHelper.SyncAvailabilityStatusAsync(context, oldClass.Id, now, cancellationToken);
        }

        ClassCapacityStatusHelper.SyncAvailabilityStatus(newClass, newClassActiveEnrollmentCount + 1, now);

        if (isSecondaryTrack)
        {
            registration.SecondaryClassId = command.NewClassId;
            registration.SecondaryClassAssignedDate = now;
        }
        else
        {
            registration.ClassId = command.NewClassId;
            registration.ClassAssignedDate = now;
        }

        registration.OperationType = OperationType.Transfer;
        registration.Status = RegistrationTrackHelper.ResolveStatus(registration);
        registration.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);
        if (oldClass != null)
        {
            await classLifecycleService.RecalculateClassLifecycleAsync(oldClass.Id, cancellationToken);
        }

        await classLifecycleService.RecalculateClassLifecycleAsync(newClass.Id, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new TransferClassResponse
        {
            RegistrationId = registration.Id,
            OldClassId = oldClassId,
            OldClassName = oldClass?.Title ?? "Unknown",
            NewClassId = newClass.Id,
            NewClassName = newClass.Title,
            Track = track,
            EffectiveDate = command.EffectiveDate,
            Status = registration.Status.ToString()
        };
    }

    private async Task<bool> IsExplicitlyIncompatibleAsync(
        Guid? learningTicketTypeId,
        Guid? slotTypeId,
        CancellationToken cancellationToken)
    {
        if (!learningTicketTypeId.HasValue || !slotTypeId.HasValue)
        {
            return false;
        }

        var mapping = await context.TicketTypeCompatibilities
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.LearningTicketTypeId == learningTicketTypeId.Value &&
                     x.SlotTypeId == slotTypeId.Value,
                cancellationToken);

        return mapping is not null && !mapping.IsCompatible;
    }
}
