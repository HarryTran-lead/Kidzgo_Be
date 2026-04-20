using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LeaveRequests.RejectLeaveRequest;

public sealed class RejectLeaveRequestCommandHandler(
    IDbContext context,
    ApprovedLeaveAttendanceService approvedLeaveAttendanceService)
    : ICommandHandler<RejectLeaveRequestCommand>
{
    public async Task<Result> Handle(RejectLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leave = await context.LeaveRequests
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (leave is null)
        {
            return Result.Failure(LeaveRequestErrors.NotFound(request.Id));
        }

        if (leave.Status == LeaveRequestStatus.Rejected)
        {
            return Result.Failure(LeaveRequestErrors.AlreadyRejected);
        }

        var wasApproved = leave.Status == LeaveRequestStatus.Approved;

        leave.Status = LeaveRequestStatus.Rejected;
        leave.ApprovedAt = null;
        leave.ApprovedBy = null;

        if (wasApproved)
        {
            var sourceSession = await ResolveSourceSessionAsync(leave, cancellationToken);
            if (sourceSession is not null)
            {
                await RemoveApprovedLeaveCreditsAsync(leave, sourceSession.Id, cancellationToken);
                await approvedLeaveAttendanceService.ApplyApprovedLeaveDeactivationAsync(
                    leave.StudentProfileId,
                    sourceSession,
                    cancellationToken);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Session?> ResolveSourceSessionAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken)
    {
        var sessionDayStartUtc = VietnamTime.TreatAsVietnamLocal(leaveRequest.SessionDate.ToDateTime(TimeOnly.MinValue));
        var sessionDayEndUtc = VietnamTime.EndOfVietnamDayUtc(sessionDayStartUtc);

        return leaveRequest.SessionId.HasValue
            ? await context.Sessions
                .FirstOrDefaultAsync(s => s.Id == leaveRequest.SessionId.Value, cancellationToken)
            : await context.Sessions
                .FirstOrDefaultAsync(
                    s => s.ClassId == leaveRequest.ClassId
                         && s.PlannedDatetime >= sessionDayStartUtc
                         && s.PlannedDatetime <= sessionDayEndUtc,
                    cancellationToken);
    }

    private async Task RemoveApprovedLeaveCreditsAsync(
        LeaveRequest leaveRequest,
        Guid sourceSessionId,
        CancellationToken cancellationToken)
    {
        var makeupCredits = await context.MakeupCredits
            .Where(mc => mc.StudentProfileId == leaveRequest.StudentProfileId
                && mc.SourceSessionId == sourceSessionId
                && mc.CreatedReason == CreatedReason.ApprovedLeave24H)
            .Include(mc => mc.MakeupAllocations)
            .ToListAsync(cancellationToken);

        foreach (var credit in makeupCredits)
        {
            context.MakeupAllocations.RemoveRange(credit.MakeupAllocations);
            context.MakeupCredits.Remove(credit);
        }
    }
}

