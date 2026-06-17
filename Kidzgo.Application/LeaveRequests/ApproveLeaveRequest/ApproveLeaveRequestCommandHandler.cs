using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LeaveRequests.ApproveLeaveRequest;

public sealed class ApproveLeaveRequestCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ClassLifecycleService classLifecycleService,
    ApprovedLeaveAttendanceService approvedLeaveAttendanceService)
    : ICommandHandler<ApproveLeaveRequestCommand>
{
    public async Task<Result> Handle(ApproveLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leave = await context.LeaveRequests
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (leave is null)
        {
            return Result.Failure(LeaveRequestErrors.NotFound(request.Id));
        }

        if (leave.Status == LeaveRequestStatus.Approved)
        {
            return Result.Failure(LeaveRequestErrors.AlreadyApproved);
        }

        leave.Status = LeaveRequestStatus.Approved;
        leave.ApprovedAt = VietnamTime.UtcNow();
        leave.ApprovedBy = userContext.UserId;

        var leaveRangeFromUtc = VietnamTime.TreatAsVietnamLocal(leave.SessionDate.ToDateTime(TimeOnly.MinValue));
        var leaveRangeToUtc = VietnamTime.EndOfVietnamDayUtc(
            VietnamTime.TreatAsVietnamLocal((leave.EndDate ?? leave.SessionDate).ToDateTime(TimeOnly.MinValue)));

        var sessionsInRange = leave.SessionId.HasValue
            ? await context.Sessions
                .Where(s => s.Id == leave.SessionId.Value)
                .ToListAsync(cancellationToken)
            : await context.Sessions
                .Where(s => s.ClassId == leave.ClassId
                            && s.PlannedDatetime >= leaveRangeFromUtc
                            && s.PlannedDatetime <= leaveRangeToUtc)
                .ToListAsync(cancellationToken);

        if (!sessionsInRange.Any())
        {
            return Result.Failure(LeaveRequestErrors.SessionNotFound(leave.ClassId, leave.SessionDate));
        }

        var impactedClassIds = new HashSet<Guid>();

        foreach (var session in sessionsInRange)
        {
            var creditExists = await context.MakeupCredits
                .AnyAsync(
                    c => c.StudentProfileId == leave.StudentProfileId &&
                         c.CreatedReason == CreatedReason.ApprovedLeave24H &&
                         c.SourceSessionId == session.Id,
                    cancellationToken);

            if (!creditExists)
            {
                var credit = new MakeupCredit
                {
                    Id = Guid.NewGuid(),
                    StudentProfileId = leave.StudentProfileId,
                    SourceSessionId = session.Id,
                    Status = MakeupCreditStatus.Available,
                    CreatedReason = CreatedReason.ApprovedLeave24H,
                    ExpiresAt = null,
                    CreatedAt = VietnamTime.UtcNow()
                };
                context.MakeupCredits.Add(credit);
            }

            var transitionClassIds = await approvedLeaveAttendanceService.ApplyApprovedLeaveActivationAsync(
                leave.StudentProfileId,
                session,
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

}
