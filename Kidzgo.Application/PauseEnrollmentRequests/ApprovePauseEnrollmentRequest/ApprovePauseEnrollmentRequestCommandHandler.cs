using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.PauseEnrollmentRequests.Notifications;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.ApprovePauseEnrollmentRequest;

public sealed class ApprovePauseEnrollmentRequestCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ITemplateRenderer templateRenderer,
    ClassLifecycleService classLifecycleService,
    StudentSessionAssignmentService studentSessionAssignmentService,
    PauseEnrollmentEligibleClassResolver eligibleClassResolver)
    : ICommandHandler<ApprovePauseEnrollmentRequestCommand>
{
    public async Task<Result> Handle(ApprovePauseEnrollmentRequestCommand request, CancellationToken cancellationToken)
    {
        var now = VietnamTime.UtcNow();
        var pauseRequest = await context.PauseEnrollmentRequests
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (pauseRequest is null)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.NotFound(request.Id));
        }

        if (pauseRequest.Status == PauseEnrollmentRequestStatus.Approved)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.AlreadyApproved);
        }

        if (pauseRequest.Status == PauseEnrollmentRequestStatus.Rejected)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.AlreadyRejected);
        }

        if (pauseRequest.Status == PauseEnrollmentRequestStatus.Cancelled)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.AlreadyCancelled);
        }

        var activeEnrollments = await context.ClassEnrollments
            .Where(e => e.StudentProfileId == pauseRequest.StudentProfileId
                        && e.Status == EnrollmentStatus.Active)
            .ToListAsync(cancellationToken);

        if (activeEnrollments.Count == 0)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.NoEnrollmentsInRange);
        }

        pauseRequest.Status = PauseEnrollmentRequestStatus.Approved;
        pauseRequest.ApprovedAt = now;
        pauseRequest.ApprovedBy = userContext.UserId;

        var pauseFromUtc = VietnamTime.TreatAsVietnamLocal(pauseRequest.PauseFrom.ToDateTime(TimeOnly.MinValue));
        var pauseToUtc = VietnamTime.EndOfVietnamDayUtc(
            VietnamTime.TreatAsVietnamLocal(pauseRequest.PauseTo.ToDateTime(TimeOnly.MinValue)));

        List<ClassEnrollment> affectedEnrollments;
        if (pauseRequest.ClassId.HasValue)
        {
            var requestEnrollment = activeEnrollments
                .FirstOrDefault(e => e.ClassId == pauseRequest.ClassId.Value);

            if (requestEnrollment is null)
            {
                return Result.Failure(PauseEnrollmentRequestErrors.NotEnrolled(
                    pauseRequest.ClassId.Value,
                    pauseRequest.StudentProfileId));
            }

            affectedEnrollments = [requestEnrollment];
        }
        else
        {
            var classIdsInRange = await eligibleClassResolver.GetEligibleClassIdsAsync(
                pauseRequest.StudentProfileId,
                pauseRequest.PauseFrom,
                pauseRequest.PauseTo,
                cancellationToken);

            if (classIdsInRange.Count == 0)
            {
                return Result.Failure(PauseEnrollmentRequestErrors.NoEnrollmentsInRange);
            }

            affectedEnrollments = activeEnrollments
                .Where(e => classIdsInRange.Contains(e.ClassId))
                .ToList();
        }

        var affectedClassIds = affectedEnrollments
            .Select(e => e.ClassId)
            .Distinct()
            .ToList();

        await ReconcileFutureMakeupAndLeaveDuringPauseAsync(
            pauseRequest.StudentProfileId,
            affectedClassIds,
            pauseRequest.PauseFrom,
            pauseRequest.PauseTo,
            pauseFromUtc,
            pauseToUtc,
            now,
            cancellationToken);

        var reservedSessionCount = 0;
        foreach (var enrollment in affectedEnrollments)
        {
            var previousStatus = enrollment.Status;
            enrollment.Status = EnrollmentStatus.Paused;
            enrollment.UpdatedAt = now;
            var enrollmentReservedSessionCount = await studentSessionAssignmentService.CancelAssignmentsForEnrollmentInRangeAsync(
                enrollment.Id,
                pauseFromUtc,
                pauseToUtc,
                cancellationToken);
            reservedSessionCount += enrollmentReservedSessionCount;

            var history = new PauseEnrollmentRequestHistory
            {
                Id = Guid.NewGuid(),
                PauseEnrollmentRequestId = pauseRequest.Id,
                StudentProfileId = enrollment.StudentProfileId,
                ClassId = enrollment.ClassId,
                EnrollmentId = enrollment.Id,
                PreviousStatus = previousStatus,
                NewStatus = EnrollmentStatus.Paused,
                PauseFrom = pauseRequest.PauseFrom,
                PauseTo = pauseRequest.PauseTo,
                ReservedSessionCount = enrollmentReservedSessionCount,
                ChangedAt = now,
                ChangedBy = userContext.UserId
            };

            context.PauseEnrollmentRequestHistories.Add(history);
        }

        pauseRequest.ReservedSessionCount = reservedSessionCount;
        pauseRequest.ReservationExpiresOn = pauseRequest.PauseFrom.AddMonths(3);
        pauseRequest.ReservationSnapshotAt = now;

        await context.SaveChangesAsync(cancellationToken);

        foreach (var classId in affectedClassIds)
        {
            await classLifecycleService.RecalculateClassLifecycleAsync(classId, cancellationToken);
        }

        if (affectedClassIds.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        await PauseEnrollmentRequestNotificationHelper.NotifyAsync(
            context,
            templateRenderer,
            pauseRequest.StudentProfileId,
            pauseRequest.Id,
            PauseEnrollmentRequestNotificationHelper.NotificationType.Approved,
            pauseRequest.PauseFrom,
            pauseRequest.PauseTo,
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    private async Task ReconcileFutureMakeupAndLeaveDuringPauseAsync(
        Guid studentProfileId,
        IReadOnlyCollection<Guid> affectedClassIds,
        DateOnly pauseFrom,
        DateOnly pauseTo,
        DateTime pauseFromUtc,
        DateTime pauseToUtc,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var removedCreditIds = await RemoveMakeupCreditsInPauseWindowAsync(
            studentProfileId,
            pauseFromUtc,
            pauseToUtc,
            cancellationToken);

        if (affectedClassIds.Count == 0)
        {
            return;
        }

        await CancelLeaveAndCreditsSupersededByPauseAsync(
            studentProfileId,
            affectedClassIds,
            pauseFrom,
            pauseTo,
            pauseFromUtc,
            pauseToUtc,
            removedCreditIds,
            cancellationToken);
    }

    private async Task<HashSet<Guid>> RemoveMakeupCreditsInPauseWindowAsync(
        Guid studentProfileId,
        DateTime pauseFromUtc,
        DateTime pauseToUtc,
        CancellationToken cancellationToken)
    {
        var creditIdsToRemove = await context.MakeupAllocations
            .AsNoTracking()
            .Where(allocation => allocation.MakeupCredit.StudentProfileId == studentProfileId
                && allocation.Status != MakeupAllocationStatus.Cancelled
                && allocation.TargetSession.Status != SessionStatus.Cancelled
                && allocation.TargetSession.PlannedDatetime >= pauseFromUtc
                && allocation.TargetSession.PlannedDatetime <= pauseToUtc)
            .Select(allocation => allocation.MakeupCreditId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (creditIdsToRemove.Count == 0)
        {
            return [];
        }

        var creditsToRemove = await context.MakeupCredits
            .Include(credit => credit.SourceSession)
            .Include(credit => credit.MakeupAllocations)
            .Where(credit => creditIdsToRemove.Contains(credit.Id))
            .ToListAsync(cancellationToken);

        foreach (var credit in creditsToRemove)
        {
            var sourceSessionDate = VietnamTime.ToVietnamDateOnly(credit.SourceSession.PlannedDatetime);

            var relatedLeaveRequests = await context.LeaveRequests
                .Where(leave => leave.StudentProfileId == studentProfileId
                    && (leave.Status == LeaveRequestStatus.Pending || leave.Status == LeaveRequestStatus.Approved)
                    && ((leave.SessionId.HasValue && leave.SessionId == credit.SourceSessionId)
                        || (!leave.SessionId.HasValue
                            && leave.ClassId == credit.SourceSession.ClassId
                            && leave.SessionDate == sourceSessionDate)))
                .ToListAsync(cancellationToken);

            foreach (var leaveRequest in relatedLeaveRequests)
            {
                leaveRequest.Status = LeaveRequestStatus.Cancelled;
                leaveRequest.CancelledAt = VietnamTime.UtcNow();
            }

            await RemoveAttendanceAsync(studentProfileId, credit.SourceSessionId, cancellationToken);

            foreach (var allocation in credit.MakeupAllocations)
            {
                await RemoveAttendanceAsync(studentProfileId, allocation.TargetSessionId, cancellationToken);
            }

            context.MakeupAllocations.RemoveRange(credit.MakeupAllocations);
            context.MakeupCredits.Remove(credit);
        }

        return creditIdsToRemove.ToHashSet();
    }

    private async Task CancelLeaveAndCreditsSupersededByPauseAsync(
        Guid studentProfileId,
        IReadOnlyCollection<Guid> affectedClassIds,
        DateOnly pauseFrom,
        DateOnly pauseTo,
        DateTime pauseFromUtc,
        DateTime pauseToUtc,
        IReadOnlySet<Guid> excludedCreditIds,
        CancellationToken cancellationToken)
    {
        var leaveRequestsInPause = await context.LeaveRequests
            .Where(leave => leave.StudentProfileId == studentProfileId
                && affectedClassIds.Contains(leave.ClassId)
                && (leave.Status == LeaveRequestStatus.Pending || leave.Status == LeaveRequestStatus.Approved)
                && leave.SessionDate >= pauseFrom
                && leave.SessionDate <= pauseTo)
            .ToListAsync(cancellationToken);

        foreach (var leaveRequest in leaveRequestsInPause)
        {
            leaveRequest.Status = LeaveRequestStatus.Cancelled;
            leaveRequest.CancelledAt = VietnamTime.UtcNow();
            if (leaveRequest.SessionId.HasValue)
            {
                await RemoveAttendanceAsync(studentProfileId, leaveRequest.SessionId.Value, cancellationToken);
            }
        }

        var creditsSupersededByPause = await context.MakeupCredits
            .Include(credit => credit.MakeupAllocations)
            .Where(credit => credit.StudentProfileId == studentProfileId
                && !excludedCreditIds.Contains(credit.Id)
                && credit.CreatedReason == CreatedReason.ApprovedLeave24H
                && credit.SourceSession.PlannedDatetime >= pauseFromUtc
                && credit.SourceSession.PlannedDatetime <= pauseToUtc
                && affectedClassIds.Contains(credit.SourceSession.ClassId))
            .ToListAsync(cancellationToken);

        foreach (var credit in creditsSupersededByPause)
        {
            await RemoveAttendanceAsync(studentProfileId, credit.SourceSessionId, cancellationToken);

            foreach (var allocation in credit.MakeupAllocations)
            {
                await RemoveAttendanceAsync(studentProfileId, allocation.TargetSessionId, cancellationToken);
            }

            context.MakeupAllocations.RemoveRange(credit.MakeupAllocations);
            context.MakeupCredits.Remove(credit);
        }
    }

    private async Task RemoveAttendanceAsync(
        Guid studentProfileId,
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var attendance = await context.Attendances
            .FirstOrDefaultAsync(a => a.StudentProfileId == studentProfileId && a.SessionId == sessionId, cancellationToken);

        if (attendance is not null)
        {
            context.Attendances.Remove(attendance);
        }
    }
}
