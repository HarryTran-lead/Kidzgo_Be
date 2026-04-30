using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Classes;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class PauseEnrollmentReactivationService(
    IDbContext context,
    ClassLifecycleService classLifecycleService,
    StudentSessionAssignmentService studentSessionAssignmentService,
    StudentEnrollmentScheduleConflictService studentEnrollmentScheduleConflictService)
{
    public async Task<Result<int>> ReactivateIfDueAsync(
        Guid pauseRequestId,
        Guid? changedBy,
        CancellationToken cancellationToken)
    {
        var pauseRequest = await context.PauseEnrollmentRequests
            .FirstOrDefaultAsync(r => r.Id == pauseRequestId, cancellationToken);

        if (pauseRequest is null)
        {
            return Result.Failure<int>(PauseEnrollmentRequestErrors.NotFound(pauseRequestId));
        }

        if (pauseRequest.Status != PauseEnrollmentRequestStatus.Approved ||
            !RequiresAutomaticReactivation(pauseRequest.Outcome) ||
            !pauseRequest.OutcomeAt.HasValue)
        {
            return Result.Success(0);
        }

        var today = VietnamTime.ToVietnamDateOnly(VietnamTime.UtcNow());
        if (pauseRequest.PauseTo >= today)
        {
            return Result.Success(0);
        }

        var alreadyReactivated = await context.PauseEnrollmentRequestHistories
            .AnyAsync(h => h.PauseEnrollmentRequestId == pauseRequest.Id &&
                           h.NewStatus == EnrollmentStatus.Active,
                cancellationToken);

        if (alreadyReactivated)
        {
            return Result.Success(0);
        }

        return await ReactivatePausedEnrollmentsAsync(
            pauseRequest,
            changedBy,
            cancellationToken);
    }

    private async Task<Result<int>> ReactivatePausedEnrollmentsAsync(
        PauseEnrollmentRequest pauseRequest,
        Guid? changedBy,
        CancellationToken cancellationToken)
    {
        var enrollmentIds = await context.PauseEnrollmentRequestHistories
            .Where(h => h.PauseEnrollmentRequestId == pauseRequest.Id &&
                        h.EnrollmentId.HasValue &&
                        h.NewStatus == EnrollmentStatus.Paused)
            .Select(h => h.EnrollmentId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (enrollmentIds.Count == 0)
        {
            return Result.Success(0);
        }

        var enrollments = await context.ClassEnrollments
            .Include(e => e.Class)
            .Where(e => enrollmentIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        if (enrollments.Any(e => e.Status == EnrollmentStatus.Dropped))
        {
            return Result.Failure<int>(EnrollmentErrors.CannotReactivateDropped);
        }

        var now = VietnamTime.UtcNow();
        var today = VietnamTime.ToVietnamDateOnly(now);
        var effectiveFrom = pauseRequest.PauseTo.AddDays(1);
        if (today > effectiveFrom)
        {
            effectiveFrom = today;
        }

        var pausedEnrollments = enrollments
            .Where(e => e.Status == EnrollmentStatus.Paused)
            .ToList();

        if (pausedEnrollments.Count == 0)
        {
            return Result.Success(0);
        }

        var pendingReactivatedSlots = new List<StudentBookedSlot>();

        var reactivationCountsByClass = pausedEnrollments
            .GroupBy(e => e.ClassId)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var classId in reactivationCountsByClass.Keys)
        {
            var classEntity = enrollments
                .First(e => e.ClassId == classId)
                .Class;

            var currentEnrollmentCount = await context.ClassEnrollments
                .CountAsync(ce => ce.ClassId == classId && ce.Status == EnrollmentStatus.Active, cancellationToken);

            ClassCapacityStatusHelper.SyncAvailabilityStatus(classEntity, currentEnrollmentCount, now);

            if (classEntity.Status != ClassStatus.Active &&
                classEntity.Status != ClassStatus.Planned &&
                classEntity.Status != ClassStatus.Recruiting)
            {
                return Result.Failure<int>(EnrollmentErrors.ClassNotAvailable);
            }

            if (currentEnrollmentCount + reactivationCountsByClass[classId] > classEntity.Capacity)
            {
                return Result.Failure<int>(EnrollmentErrors.ClassFull);
            }
        }

        foreach (var enrollment in pausedEnrollments)
        {
            var assignmentStartDate = effectiveFrom > enrollment.EnrollDate
                ? effectiveFrom
                : enrollment.EnrollDate;

            var conflictResult = await studentEnrollmentScheduleConflictService.EnsureNoConflictsAsync(
                enrollment.StudentProfileId,
                enrollment.ClassId,
                assignmentStartDate,
                enrollment.SessionSelectionPattern,
                cancellationToken,
                additionalBookedSlots: pendingReactivatedSlots,
                excludeEnrollmentId: enrollment.Id);
            if (conflictResult.IsFailure)
            {
                return Result.Failure<int>(conflictResult.Error);
            }

            var candidateSlots = await studentEnrollmentScheduleConflictService.GetCandidateSlotsAsync(
                enrollment.ClassId,
                assignmentStartDate,
                enrollment.SessionSelectionPattern,
                cancellationToken);
            pendingReactivatedSlots.AddRange(candidateSlots);

            enrollment.Status = EnrollmentStatus.Active;
            enrollment.UpdatedAt = now;
            await studentSessionAssignmentService.RestoreAssignmentsForEnrollmentAsync(
                enrollment,
                effectiveFrom,
                cancellationToken);

            context.PauseEnrollmentRequestHistories.Add(new PauseEnrollmentRequestHistory
            {
                Id = Guid.NewGuid(),
                PauseEnrollmentRequestId = pauseRequest.Id,
                StudentProfileId = enrollment.StudentProfileId,
                ClassId = enrollment.ClassId,
                EnrollmentId = enrollment.Id,
                PreviousStatus = EnrollmentStatus.Paused,
                NewStatus = EnrollmentStatus.Active,
                PauseFrom = pauseRequest.PauseFrom,
                PauseTo = pauseRequest.PauseTo,
                ChangedAt = now,
                ChangedBy = changedBy
            });
        }

        foreach (var classId in reactivationCountsByClass.Keys)
        {
            var classEntity = enrollments
                .First(e => e.ClassId == classId)
                .Class;
            var currentEnrollmentCount = await context.ClassEnrollments
                .CountAsync(ce => ce.ClassId == classId && ce.Status == EnrollmentStatus.Active, cancellationToken);
            ClassCapacityStatusHelper.SyncAvailabilityStatus(classEntity, currentEnrollmentCount, now);
        }

        await context.SaveChangesAsync(cancellationToken);

        foreach (var classId in reactivationCountsByClass.Keys)
        {
            await classLifecycleService.RecalculateClassLifecycleAsync(classId, cancellationToken);
        }

        if (reactivationCountsByClass.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(pausedEnrollments.Count);
    }

    private static bool RequiresAutomaticReactivation(PauseEnrollmentOutcome? outcome)
    {
        return outcome is PauseEnrollmentOutcome.ContinueSameClass;
    }
}
