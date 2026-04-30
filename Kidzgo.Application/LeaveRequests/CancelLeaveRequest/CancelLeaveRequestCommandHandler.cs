using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LeaveRequests.CancelLeaveRequest;

public sealed class CancelLeaveRequestCommandHandler(
    IDbContext context,
    ClassLifecycleService classLifecycleService,
    ApprovedLeaveAttendanceService approvedLeaveAttendanceService)
    : ICommandHandler<CancelLeaveRequestCommand>
{
    public async Task<Result> Handle(CancelLeaveRequestCommand command, CancellationToken cancellationToken)
    {
        var leaveRequest = await context.LeaveRequests
            .Include(lr => lr.Class)
            .FirstOrDefaultAsync(lr => lr.Id == command.Id, cancellationToken);

        if (leaveRequest is null)
        {
            return Result.Failure(LeaveRequestErrors.NotFound(command.Id));
        }

        // Check if already cancelled
        if (leaveRequest.Status == LeaveRequestStatus.Cancelled)
        {
            return Result.Failure(LeaveRequestErrors.AlreadyCancelled);
        }

        // Check if session date has passed - cannot cancel past sessions
        if (leaveRequest.SessionDate < VietnamTime.TodayDateOnly())
        {
            return Result.Failure(LeaveRequestErrors.CannotCancelPastSession(leaveRequest.SessionDate));
        }

        // Check if was approved (to know if we need to delete makeup credits)
        bool wasApproved = await WasOriginallyApproved(leaveRequest, cancellationToken);

        // Update status to Cancelled
        leaveRequest.Status = LeaveRequestStatus.Cancelled;
        leaveRequest.CancelledAt = VietnamTime.UtcNow();

        // If was approved, also delete the makeup credits and allocations
        if (wasApproved)
        {
            var impactedClassIds = new HashSet<Guid>();
            var sessionDayStartUtc = VietnamTime.TreatAsVietnamLocal(leaveRequest.SessionDate.ToDateTime(TimeOnly.MinValue));
            var sessionDayEndUtc = VietnamTime.EndOfVietnamDayUtc(sessionDayStartUtc);
            var sourceSession = leaveRequest.SessionId.HasValue
                ? await context.Sessions
                    .FirstOrDefaultAsync(s => s.Id == leaveRequest.SessionId.Value, cancellationToken)
                : await context.Sessions
                    .FirstOrDefaultAsync(s => s.ClassId == leaveRequest.ClassId
                        && s.PlannedDatetime >= sessionDayStartUtc
                        && s.PlannedDatetime <= sessionDayEndUtc, cancellationToken);

            if (sourceSession != null)
            {
                var makeupCredits = await context.MakeupCredits
                    .Where(mc => mc.StudentProfileId == leaveRequest.StudentProfileId
                        && mc.SourceSessionId == sourceSession.Id
                        && mc.CreatedReason == CreatedReason.ApprovedLeave24H)
                    .Include(mc => mc.MakeupAllocations)
                    .ToListAsync(cancellationToken);

                foreach (var credit in makeupCredits)
                {
                    // Remove allocations first
                    context.MakeupAllocations.RemoveRange(credit.MakeupAllocations);
                    // Then remove credits
                    context.MakeupCredits.Remove(credit);
                }

                var transitionClassIds = await approvedLeaveAttendanceService.ApplyApprovedLeaveDeactivationAsync(
                    leaveRequest.StudentProfileId,
                    sourceSession,
                    cancellationToken);
                impactedClassIds.UnionWith(transitionClassIds);
            }

            await context.SaveChangesAsync(cancellationToken);

            foreach (var classId in impactedClassIds)
            {
                await classLifecycleService.RecalculateClassLifecycleAsync(classId, cancellationToken);
            }

            if (impactedClassIds.Count > 0)
            {
                await context.SaveChangesAsync(cancellationToken);
            }

            return Result.Success();
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<bool> WasOriginallyApproved(LeaveRequest leaveRequest, CancellationToken cancellationToken)
    {
        var sessionDayStartUtc = VietnamTime.TreatAsVietnamLocal(leaveRequest.SessionDate.ToDateTime(TimeOnly.MinValue));
        var sessionDayEndUtc = VietnamTime.EndOfVietnamDayUtc(sessionDayStartUtc);
        var sourceSession = leaveRequest.SessionId.HasValue
            ? await context.Sessions
                .FirstOrDefaultAsync(s => s.Id == leaveRequest.SessionId.Value, cancellationToken)
            : await context.Sessions
                .FirstOrDefaultAsync(s => s.ClassId == leaveRequest.ClassId
                    && s.PlannedDatetime >= sessionDayStartUtc
                    && s.PlannedDatetime <= sessionDayEndUtc, cancellationToken);

        if (sourceSession == null) return false;

        return await context.MakeupCredits
            .AnyAsync(mc => mc.StudentProfileId == leaveRequest.StudentProfileId
                && mc.SourceSessionId == sourceSession.Id
                && mc.CreatedReason == CreatedReason.ApprovedLeave24H, cancellationToken);
    }
}
