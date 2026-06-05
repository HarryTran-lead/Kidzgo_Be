using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Classes;
using Kidzgo.Application.Programs.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.TransferRegistrationBranch.Handler;

public sealed class TransferRegistrationBranchCommandHandler(
    IDbContext context,
    ClassLifecycleService classLifecycleService,
    StudentSessionAssignmentService studentSessionAssignmentService,
    StudentEnrollmentScheduleConflictService studentEnrollmentScheduleConflictService,
    TicketCompatibilityService ticketCompatibilityService)
    : ICommandHandler<TransferRegistrationBranchCommand, TransferRegistrationBranchResponse>
{
    public async Task<Result<TransferRegistrationBranchResponse>> Handle(
        TransferRegistrationBranchCommand command,
        CancellationToken cancellationToken)
    {
        var now = VietnamTime.UtcNow();
        var effectiveDate = VietnamTime.ToVietnamDateOnly(command.EffectiveDate);
        var weeklyPatternResult = SchedulePatternSupport.NormalizeWeeklyPatternJson(
            command.WeeklyPattern,
            requireValue: false);
        if (weeklyPatternResult.IsFailure)
        {
            return Result.Failure<TransferRegistrationBranchResponse>(weeklyPatternResult.Error);
        }

        var sessionSelectionPattern = weeklyPatternResult.Value;

        var registration = await context.Registrations
            .Include(r => r.Program)
            .Include(r => r.Branch)
            .Include(r => r.TuitionPlan)
            .Include(r => r.Class)
            .Include(r => r.SecondaryClass)
            .FirstOrDefaultAsync(r => r.Id == command.RegistrationId, cancellationToken);

        if (registration == null)
        {
            return Result.Failure<TransferRegistrationBranchResponse>(
                RegistrationErrors.NotFound(command.RegistrationId));
        }

        if (registration.Status == RegistrationStatus.Completed ||
            registration.Status == RegistrationStatus.Cancelled)
        {
            return Result.Failure<TransferRegistrationBranchResponse>(
                RegistrationErrors.InvalidStatus(registration.Status.ToString(), "transfer-branch"));
        }

        if (registration.SecondaryClassId.HasValue)
        {
            return Result.Failure<TransferRegistrationBranchResponse>(
                RegistrationErrors.CannotTransferBranchWithSecondaryClass());
        }

        if (registration.BranchId == command.NewBranchId)
        {
            return Result.Failure<TransferRegistrationBranchResponse>(
                RegistrationErrors.CannotTransferToSameBranch());
        }

        var targetBranch = await context.Branches
            .FirstOrDefaultAsync(b => b.Id == command.NewBranchId && b.IsActive, cancellationToken);
        if (targetBranch == null)
        {
            return Result.Failure<TransferRegistrationBranchResponse>(
                RegistrationErrors.BranchNotFound(command.NewBranchId));
        }

        var programAssignedToBranch = await BranchProgramAccessHelper.IsProgramAssignedToBranchAsync(
            context,
            targetBranch.Id,
            registration.ProgramId,
            cancellationToken);
        if (!programAssignedToBranch)
        {
            return Result.Failure<TransferRegistrationBranchResponse>(
                RegistrationErrors.ProgramNotAvailableInBranch(registration.ProgramId, targetBranch.Id));
        }

        var oldBranchId = registration.Branch.Id;
        var oldBranchName = registration.Branch.Name;
        var oldClassId = registration.ClassId;
        var oldClassName = registration.Class?.Title;
        var oldEnrollment = await context.ClassEnrollments
            .FirstOrDefaultAsync(ce =>
                ce.StudentProfileId == registration.StudentProfileId &&
                ce.Status == EnrollmentStatus.Active &&
                ce.ClassId == registration.ClassId &&
                (!ce.RegistrationId.HasValue || ce.RegistrationId == registration.Id),
                cancellationToken);

        var hasOtherOperationalEnrollmentsOutsideTargetBranch =
            await HasOtherOperationalEnrollmentsOutsideBranchAsync(
                registration.StudentProfileId,
                targetBranch.Id,
                registration.Id,
                oldClassId,
                cancellationToken);
        if (hasOtherOperationalEnrollmentsOutsideTargetBranch)
        {
            return Result.Failure<TransferRegistrationBranchResponse>(
                StudentBranchErrors.ActiveEnrollmentsRequireResolution(targetBranch.Id));
        }

        var state = await context.StudentBranchStates
            .FirstOrDefaultAsync(x => x.StudentProfileId == registration.StudentProfileId, cancellationToken);

        if (state is not null && state.ActiveBranchId != registration.BranchId)
        {
            return Result.Failure<TransferRegistrationBranchResponse>(
                StudentBranchErrors.TransferCurrentBranchMismatch(registration.BranchId, state.ActiveBranchId));
        }

        Kidzgo.Domain.Classes.Class? newClass = null;
        var newClassActiveEnrollmentCount = 0;
        DateTime? firstStudySessionAt = null;
        string? warningMessage = null;

        if (command.NewClassId.HasValue)
        {
            newClass = await context.Classes
                .Include(c => c.ClassEnrollments)
                .FirstOrDefaultAsync(c => c.Id == command.NewClassId.Value, cancellationToken);

            if (newClass == null)
            {
                return Result.Failure<TransferRegistrationBranchResponse>(
                    RegistrationErrors.ClassNotFound(command.NewClassId.Value));
            }

            if (newClass.BranchId != targetBranch.Id)
            {
                return Result.Failure<TransferRegistrationBranchResponse>(
                    RegistrationErrors.ClassNotMatchingBranch(newClass.Id, targetBranch.Id));
            }

            if (newClass.ProgramId != registration.ProgramId)
            {
                return Result.Failure<TransferRegistrationBranchResponse>(
                    RegistrationErrors.ClassNotMatchingProgram(newClass.Id, registration.ProgramId));
            }

            if (newClass.LevelId != registration.LevelId)
            {
                return Result.Failure<TransferRegistrationBranchResponse>(
                    RegistrationErrors.ClassNotMatchingLevel(newClass.Id, registration.LevelId));
            }

            if (await IsExplicitlyIncompatibleAsync(
                    registration.TuitionPlan?.LearningTicketTypeId,
                    newClass.SlotTypeId,
                    cancellationToken))
            {
                return Result.Failure<TransferRegistrationBranchResponse>(
                    RegistrationErrors.TicketTypeIncompatibleWithClassSlotType(
                        registration.TuitionPlan?.LearningTicketTypeId,
                        newClass.SlotTypeId));
            }

            var selectionPatternValidation = await studentSessionAssignmentService
                .ValidateSelectionPatternAsync(newClass, sessionSelectionPattern, cancellationToken);
            if (selectionPatternValidation.IsFailure)
            {
                return Result.Failure<TransferRegistrationBranchResponse>(selectionPatternValidation.Error);
            }

            newClassActiveEnrollmentCount = newClass.ClassEnrollments
                .Count(ce => ce.Status == EnrollmentStatus.Active);
            ClassCapacityStatusHelper.SyncAvailabilityStatus(newClass, newClassActiveEnrollmentCount, now);

            if (newClass.Status is ClassStatus.Completed or ClassStatus.Closed or ClassStatus.Cancelled or ClassStatus.Suspended)
            {
                return Result.Failure<TransferRegistrationBranchResponse>(
                    Error.Validation("ClassNotAvailable", $"Cannot transfer to class with status {newClass.Status}"));
            }

            if (newClassActiveEnrollmentCount >= newClass.Capacity)
            {
                return Result.Failure<TransferRegistrationBranchResponse>(
                    RegistrationErrors.ClassFull(newClass.Id));
            }

            if (registration.TuitionPlan?.ModuleId.HasValue == true &&
                registration.TuitionPlan.ModuleId != newClass.StartModuleId)
            {
                return Result.Failure<TransferRegistrationBranchResponse>(
                    RegistrationErrors.TuitionPlanModuleMismatch(registration.TuitionPlanId, newClass.Id));
            }

            if (registration.TuitionPlan?.ModuleId.HasValue == true &&
                newClass.Status is not ClassStatus.Planned and not ClassStatus.Recruiting)
            {
                return Result.Failure<TransferRegistrationBranchResponse>(
                    RegistrationErrors.ModuleBasedTuitionPlanRequiresUpcomingClass(registration.TuitionPlanId));
            }

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
                return Result.Failure<TransferRegistrationBranchResponse>(conflictResult.Error);
            }

            var candidateSlots = await studentEnrollmentScheduleConflictService.GetCandidateSlotsAsync(
                newClass.Id,
                effectiveDate,
                sessionSelectionPattern,
                cancellationToken);

            if (candidateSlots.Count > 0)
            {
                firstStudySessionAt = candidateSlots[0].Start;
            }
        }

        if (oldEnrollment != null)
        {
            oldEnrollment.Status = EnrollmentStatus.Dropped;
            oldEnrollment.UpdatedAt = now;
            await studentSessionAssignmentService.CancelFutureAssignmentsForEnrollmentAsync(
                oldEnrollment.Id,
                command.EffectiveDate,
                cancellationToken);
        }

        var resolvedEntryType = registration.EntryType.HasValue && registration.EntryType.Value != EntryType.Wait
            ? registration.EntryType.Value
            : EntryType.Immediate;

        if (newClass != null)
        {
            var newEnrollment = new ClassEnrollment
            {
                Id = Guid.NewGuid(),
                ClassId = newClass.Id,
                StudentProfileId = registration.StudentProfileId,
                EnrollDate = effectiveDate,
                Status = EnrollmentStatus.Active,
                TuitionPlanId = registration.TuitionPlanId,
                RegistrationId = registration.Id,
                Track = RegistrationTrackType.Primary,
                SessionSelectionPattern = sessionSelectionPattern,
                CreatedAt = now,
                UpdatedAt = now
            };

            context.ClassEnrollments.Add(newEnrollment);

            if (registration.Program.IsSupplementary)
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

            if (newClass.Status == ClassStatus.Active)
            {
                warningMessage = "Class da bat dau. Hoc vien se tham gia theo tien do hien tai cua lop moi.";
            }

            var previousClassStatus = newClass.Status;
            ClassCapacityStatusHelper.SyncAvailabilityStatus(newClass, newClassActiveEnrollmentCount + 1, now);
            if (newClass.Status == ClassStatus.Full && previousClassStatus != ClassStatus.Full)
            {
                warningMessage = string.IsNullOrWhiteSpace(warningMessage)
                    ? "Class da day sau khi chuyen hoc vien nay."
                    : warningMessage + " Class da day sau khi chuyen hoc vien nay.";
            }

            registration.ClassId = newClass.Id;
            registration.ClassAssignedDate = now;
            registration.EntryType = resolvedEntryType;

            if (resolvedEntryType == EntryType.Immediate && !registration.ActualStartDate.HasValue)
            {
                registration.ActualStartDate = firstStudySessionAt
                    ?? VietnamTime.TreatAsVietnamLocal(effectiveDate.ToDateTime(TimeOnly.MinValue));
            }
        }
        else
        {
            registration.ClassId = null;
            registration.ClassAssignedDate = null;
            registration.EntryType = EntryType.Wait;
            warningMessage = "Hoc vien da duoc chuyen sang chi nhanh moi va quay lai danh sach cho lop.";
        }

        registration.BranchId = targetBranch.Id;
        registration.OperationType = OperationType.TransferBranch;
        registration.Status = RegistrationTrackHelper.ResolveStatus(registration);
        registration.UpdatedAt = now;

        UpdateStudentBranchState(
            state,
            registration.StudentProfileId,
            oldBranchId,
            targetBranch.Id,
            effectiveDate,
            now);

        AddStudentBranchTransferHistory(
            registration.StudentProfileId,
            oldBranchId,
            targetBranch.Id,
            effectiveDate,
            command.Reason,
            now);

        await context.SaveChangesAsync(cancellationToken);

        if (oldClassId.HasValue)
        {
            await classLifecycleService.RecalculateClassLifecycleAsync(oldClassId.Value, cancellationToken);
        }

        if (newClass != null)
        {
            await classLifecycleService.RecalculateClassLifecycleAsync(newClass.Id, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new TransferRegistrationBranchResponse
        {
            RegistrationId = registration.Id,
            OldBranchId = oldBranchId,
            OldBranchName = oldBranchName,
            NewBranchId = targetBranch.Id,
            NewBranchName = targetBranch.Name,
            OldClassId = oldClassId,
            OldClassName = oldClassName,
            NewClassId = newClass?.Id,
            NewClassName = newClass?.Title,
            EffectiveDate = command.EffectiveDate,
            Status = registration.Status.ToString(),
            EntryType = RegistrationTrackHelper.ToApiEntryType(registration.EntryType) ?? nameof(EntryType.Immediate),
            WarningMessage = warningMessage
        };
    }

    private void UpdateStudentBranchState(
        StudentBranchState? state,
        Guid studentProfileId,
        Guid oldBranchId,
        Guid targetBranchId,
        DateOnly effectiveDate,
        DateTime now)
    {
        if (state is null)
        {
            state = new StudentBranchState
            {
                Id = Guid.NewGuid(),
                StudentProfileId = studentProfileId,
                HomeBranchId = oldBranchId,
                ActiveBranchId = oldBranchId,
                AllowCrossBranchEnrollment = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            context.StudentBranchStates.Add(state);
        }

        state.HomeBranchId = targetBranchId;
        state.ActiveBranchId = targetBranchId;
        state.AllowCrossBranchEnrollment = false;
        state.LastTransferredAt = VietnamTime.TreatAsVietnamLocal(effectiveDate.ToDateTime(TimeOnly.MinValue));
        state.UpdatedAt = now;
    }

    private void AddStudentBranchTransferHistory(
        Guid studentProfileId,
        Guid oldBranchId,
        Guid targetBranchId,
        DateOnly effectiveDate,
        string? reason,
        DateTime now)
    {
        context.StudentBranchTransfers.Add(new StudentBranchTransfer
        {
            Id = Guid.NewGuid(),
            StudentProfileId = studentProfileId,
            FromBranchId = oldBranchId,
            ToBranchId = targetBranchId,
            EffectiveDate = effectiveDate,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            KeepCurrentClass = false,
            AllowCrossBranchEnrollment = false,
            CreatedAt = now,
            UpdatedAt = now
        });
    }

    private async Task<bool> HasOtherOperationalEnrollmentsOutsideBranchAsync(
        Guid studentProfileId,
        Guid targetBranchId,
        Guid currentRegistrationId,
        Guid? currentClassId,
        CancellationToken cancellationToken)
    {
        return await context.ClassEnrollments.AnyAsync(
            enrollment =>
                enrollment.StudentProfileId == studentProfileId &&
                (enrollment.Status == EnrollmentStatus.Active || enrollment.Status == EnrollmentStatus.Paused) &&
                enrollment.Class.BranchId != targetBranchId &&
                enrollment.Class.Status != ClassStatus.Closed &&
                enrollment.Class.Status != ClassStatus.Completed &&
                enrollment.Class.Status != ClassStatus.Cancelled &&
                enrollment.RegistrationId != currentRegistrationId &&
                (!currentClassId.HasValue || enrollment.ClassId != currentClassId.Value),
            cancellationToken);
    }

    private async Task<bool> IsExplicitlyIncompatibleAsync(
        Guid? learningTicketTypeId,
        Guid? slotTypeId,
        CancellationToken cancellationToken)
    {
        var evaluation = await ticketCompatibilityService.EvaluateAsync(
            learningTicketTypeId,
            slotTypeId,
            cancellationToken);
        return !evaluation.IsCompatible;
    }
}
