using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Classes;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.ReassignEquivalentClass;

public sealed class ReassignEquivalentClassCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ClassLifecycleService classLifecycleService,
    StudentSessionAssignmentService studentSessionAssignmentService,
    StudentEnrollmentScheduleConflictService studentEnrollmentScheduleConflictService)
    : ICommandHandler<ReassignEquivalentClassCommand, ReassignEquivalentClassResponse>
{
    public async Task<Result<ReassignEquivalentClassResponse>> Handle(
        ReassignEquivalentClassCommand command,
        CancellationToken cancellationToken)
    {
        var now = VietnamTime.UtcNow();
        var track = RegistrationTrackHelper.NormalizeTrack(command.Track);
        var trackType = RegistrationTrackHelper.ToTrackType(track);
        var isSecondaryTrack = track == RegistrationTrackHelper.SecondaryTrack;
        var weeklyPatternResult = SchedulePatternSupport.NormalizeWeeklyPatternJson(
            command.WeeklyPattern,
            requireValue: false);
        if (weeklyPatternResult.IsFailure)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(weeklyPatternResult.Error);
        }

        var sessionSelectionPattern = weeklyPatternResult.Value;

        var pauseRequest = await context.PauseEnrollmentRequests
            .FirstOrDefaultAsync(r => r.Id == command.PauseEnrollmentRequestId, cancellationToken);

        if (pauseRequest is null)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(
                PauseEnrollmentRequestErrors.NotFound(command.PauseEnrollmentRequestId));
        }

        if (pauseRequest.Status != PauseEnrollmentRequestStatus.Approved)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(
                PauseEnrollmentRequestErrors.OutcomeNotAllowed);
        }

        if (pauseRequest.Outcome != PauseEnrollmentOutcome.ReassignEquivalentClass)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(
                PauseEnrollmentRequestErrors.OutcomeMustBeReassignEquivalentClass);
        }

        if (pauseRequest.OutcomeCompletedAt.HasValue)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(
                PauseEnrollmentRequestErrors.OutcomeAlreadyCompleted);
        }

        var registration = await context.Registrations
            .Include(r => r.Program)
            .Include(r => r.SecondaryProgram)
            .Include(r => r.Class)
            .Include(r => r.SecondaryClass)
            .FirstOrDefaultAsync(r => r.Id == command.RegistrationId, cancellationToken);

        if (registration is null)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(
                RegistrationErrors.NotFound(command.RegistrationId));
        }

        if (registration.StudentProfileId != pauseRequest.StudentProfileId)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(
                PauseEnrollmentRequestErrors.RegistrationStudentMismatch);
        }

        if (registration.Status is RegistrationStatus.Completed or RegistrationStatus.Cancelled)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(
                RegistrationErrors.InvalidStatus(registration.Status.ToString(), "reassign-equivalent-class"));
        }

        if (isSecondaryTrack && !registration.SecondaryProgramId.HasValue)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(
                Error.Validation("Registration.SecondaryProgramMissing", "Registration does not have a secondary program to reassign"));
        }

        var currentClassId = isSecondaryTrack ? registration.SecondaryClassId : registration.ClassId;
        var targetProgramId = isSecondaryTrack ? registration.SecondaryProgramId : registration.ProgramId;

        if (!currentClassId.HasValue)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(
                Error.Validation("NoClassAssigned", $"Registration track '{track}' has no class assigned"));
        }

        var effectiveDate = ResolveEffectiveDate(command.EffectiveDate, pauseRequest, now);
        if (effectiveDate <= pauseRequest.PauseTo)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(
                PauseEnrollmentRequestErrors.EffectiveDateBeforePauseEnd(pauseRequest.PauseTo));
        }

        var effectiveUtc = VietnamTime.TreatAsVietnamLocal(effectiveDate.ToDateTime(TimeOnly.MinValue));

        var pausedEnrollmentIds = await context.PauseEnrollmentRequestHistories
            .AsNoTracking()
            .Where(history =>
                history.PauseEnrollmentRequestId == pauseRequest.Id &&
                history.EnrollmentId.HasValue &&
                history.NewStatus == EnrollmentStatus.Paused)
            .Select(history => history.EnrollmentId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var oldEnrollment = await context.ClassEnrollments
            .Include(e => e.Class)
            .FirstOrDefaultAsync(e =>
                pausedEnrollmentIds.Contains(e.Id) &&
                e.StudentProfileId == registration.StudentProfileId &&
                e.Track == trackType &&
                e.Status == EnrollmentStatus.Paused &&
                e.ClassId == currentClassId.Value &&
                (!e.RegistrationId.HasValue || e.RegistrationId == registration.Id),
                cancellationToken);

        if (oldEnrollment is null)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(
                PauseEnrollmentRequestErrors.NoPausedEnrollmentToReassign);
        }

        var newClass = await context.Classes
            .Include(c => c.ClassEnrollments)
            .FirstOrDefaultAsync(c => c.Id == command.NewClassId, cancellationToken);

        if (newClass is null)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(
                RegistrationErrors.ClassNotFound(command.NewClassId));
        }

        if (newClass.ProgramId != targetProgramId)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(
                RegistrationErrors.ClassNotMatchingProgram(command.NewClassId, targetProgramId ?? Guid.Empty));
        }

        if (oldEnrollment.ClassId == command.NewClassId)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(
                RegistrationErrors.CannotTransferToSameClass());
        }

        var selectionPatternValidation = await studentSessionAssignmentService
            .ValidateSelectionPatternAsync(newClass, sessionSelectionPattern, cancellationToken);
        if (selectionPatternValidation.IsFailure)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(selectionPatternValidation.Error);
        }

        var alreadyEnrolledInNewClass = await context.ClassEnrollments
            .AnyAsync(e =>
                e.Id != oldEnrollment.Id &&
                e.ClassId == newClass.Id &&
                e.StudentProfileId == registration.StudentProfileId &&
                (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Paused),
                cancellationToken);

        if (alreadyEnrolledInNewClass)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(
                Error.Conflict("AlreadyEnrolled", "Student is already enrolled in the target class"));
        }

        var newClassActiveEnrollmentCount = newClass.ClassEnrollments
            .Count(ce => ce.Status == EnrollmentStatus.Active);
        ClassCapacityStatusHelper.SyncAvailabilityStatus(newClass, newClassActiveEnrollmentCount, now);

        if (newClassActiveEnrollmentCount >= newClass.Capacity)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(
                RegistrationErrors.ClassFull(command.NewClassId));
        }

        if (newClass.Status is not ClassStatus.Active and not ClassStatus.Recruiting)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(
                Error.Validation("ClassNotAvailable", $"Cannot reassign to class with status {newClass.Status}"));
        }

        var conflictResult = await studentEnrollmentScheduleConflictService.EnsureNoConflictsAsync(
            registration.StudentProfileId,
            newClass.Id,
            effectiveDate,
            sessionSelectionPattern,
            cancellationToken,
            excludeEnrollmentId: oldEnrollment.Id,
            excludeLegacyClassId: oldEnrollment.ClassId,
            excludeSlotsFromUtc: effectiveUtc);
        if (conflictResult.IsFailure)
        {
            return Result.Failure<ReassignEquivalentClassResponse>(conflictResult.Error);
        }

        oldEnrollment.Status = EnrollmentStatus.Dropped;
        oldEnrollment.UpdatedAt = now;
        await studentSessionAssignmentService.CancelFutureAssignmentsForEnrollmentAsync(
            oldEnrollment.Id,
            effectiveUtc,
            cancellationToken);

        var newEnrollment = new ClassEnrollment
        {
            Id = Guid.NewGuid(),
            ClassId = newClass.Id,
            StudentProfileId = registration.StudentProfileId,
            EnrollDate = effectiveDate,
            Status = EnrollmentStatus.Active,
            TuitionPlanId = registration.TuitionPlanId,
            RegistrationId = registration.Id,
            Track = trackType,
            SessionSelectionPattern = sessionSelectionPattern,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.ClassEnrollments.Add(newEnrollment);
        var targetProgram = isSecondaryTrack ? registration.SecondaryProgram : registration.Program;
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

        context.PauseEnrollmentRequestHistories.Add(new PauseEnrollmentRequestHistory
        {
            Id = Guid.NewGuid(),
            PauseEnrollmentRequestId = pauseRequest.Id,
            StudentProfileId = oldEnrollment.StudentProfileId,
            ClassId = oldEnrollment.ClassId,
            EnrollmentId = oldEnrollment.Id,
            PreviousStatus = EnrollmentStatus.Paused,
            NewStatus = EnrollmentStatus.Dropped,
            PauseFrom = pauseRequest.PauseFrom,
            PauseTo = pauseRequest.PauseTo,
            ChangedAt = now,
            ChangedBy = userContext.UserId
        });

        if (isSecondaryTrack)
        {
            registration.SecondaryClassId = newClass.Id;
            registration.SecondaryClassAssignedDate = now;
        }
        else
        {
            registration.ClassId = newClass.Id;
            registration.ClassAssignedDate = now;
        }

        registration.OperationType = OperationType.Transfer;
        registration.Status = RegistrationTrackHelper.ResolveStatus(registration);
        registration.UpdatedAt = now;

        pauseRequest.ReassignedClassId = newClass.Id;
        pauseRequest.ReassignedEnrollmentId = newEnrollment.Id;
        pauseRequest.OutcomeCompletedAt = now;
        pauseRequest.OutcomeCompletedBy = userContext.UserId;

        await ClassCapacityStatusHelper.SyncAvailabilityStatusAsync(
            context,
            oldEnrollment.ClassId,
            now,
            cancellationToken);
        ClassCapacityStatusHelper.SyncAvailabilityStatus(newClass, newClassActiveEnrollmentCount + 1, now);

        await context.SaveChangesAsync(cancellationToken);
        await classLifecycleService.RecalculateClassLifecycleAsync(oldEnrollment.ClassId, cancellationToken);
        await classLifecycleService.RecalculateClassLifecycleAsync(newClass.Id, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new ReassignEquivalentClassResponse
        {
            PauseEnrollmentRequestId = pauseRequest.Id,
            RegistrationId = registration.Id,
            OldClassId = oldEnrollment.ClassId,
            OldClassName = oldEnrollment.Class.Title,
            NewClassId = newClass.Id,
            NewClassName = newClass.Title,
            DroppedEnrollmentId = oldEnrollment.Id,
            NewEnrollmentId = newEnrollment.Id,
            Track = track,
            EffectiveDate = effectiveUtc,
            RegistrationStatus = registration.Status.ToString(),
            OutcomeCompletedAt = now
        };
    }

    private static DateOnly ResolveEffectiveDate(
        DateTime? requestedEffectiveDate,
        PauseEnrollmentRequest pauseRequest,
        DateTime nowUtc)
    {
        if (requestedEffectiveDate.HasValue)
        {
            return VietnamTime.ToVietnamDateOnly(VietnamTime.NormalizeToUtc(requestedEffectiveDate.Value));
        }

        var today = VietnamTime.ToVietnamDateOnly(nowUtc);
        var afterPause = pauseRequest.PauseTo.AddDays(1);
        return today > afterPause ? today : afterPause;
    }
}
