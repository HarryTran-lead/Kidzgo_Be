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

                await AutoScheduleMakeupForWeekendAsync(
                    context,
                    credit,
                    session,
                    VietnamTime.ToVietnamDateOnly(session.PlannedDatetime),
                    cancellationToken);
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

    private async Task AutoScheduleMakeupForWeekendAsync(
        IDbContext context,
        MakeupCredit credit,
        Session sourceSession,
        DateOnly leaveDate,
        CancellationToken cancellationToken)
    {
        var eligibleFromDate = MakeupSessionRuleHelper.GetFirstEligibleMakeupDate(leaveDate);
        var eligibleFromUtc = VietnamTime.TreatAsVietnamLocal(eligibleFromDate.ToDateTime(TimeOnly.MinValue));

        var candidateSessions = (await context.Sessions
            .Include(s => s.Class)
            .ThenInclude(c => c.Program)
            .Where(s => s.BranchId == sourceSession.BranchId)
            .Where(s => s.ClassId != sourceSession.ClassId)
            .Where(s => s.Class.Status == ClassStatus.Active)
            .Where(s => s.Class.Program.IsMakeup)
            .Where(s => s.Status == SessionStatus.Scheduled)
            .Where(s => s.PlannedDatetime >= eligibleFromUtc)
            .OrderBy(s => s.PlannedDatetime)
            .ToListAsync(cancellationToken))
            .Where(s => MakeupSessionRuleHelper.IsEligibleMakeupDate(
                leaveDate,
                VietnamTime.ToVietnamDateOnly(s.PlannedDatetime)))
            .ToList();

        foreach (var candidateSession in candidateSessions)
        {
            if (await HasActiveAllocationForSessionAsync(context, credit.StudentProfileId, candidateSession.Id, cancellationToken))
            {
                continue;
            }

            var participantCount = await GetScheduledParticipantCountAsync(context, candidateSession, cancellationToken);
            if (participantCount >= candidateSession.Class.Capacity)
            {
                continue;
            }

            var now = VietnamTime.UtcNow();
            context.MakeupAllocations.Add(new MakeupAllocation
            {
                Id = Guid.NewGuid(),
                MakeupCreditId = credit.Id,
                TargetSessionId = candidateSession.Id,
                Status = MakeupAllocationStatus.Pending,
                AssignedBy = userContext.UserId,
                AssignedAt = now,
                CreatedAt = now
            });

            credit.Status = MakeupCreditStatus.Used;
            credit.UsedSessionId = candidateSession.Id;
            break;
        }
    }

    private static async Task<int> GetScheduledParticipantCountAsync(
        IDbContext context,
        Session session,
        CancellationToken cancellationToken)
    {
        var hasAssignments = await context.StudentSessionAssignments
            .AnyAsync(a => a.SessionId == session.Id, cancellationToken);

        var regularCount = hasAssignments
            ? await context.StudentSessionAssignments
                .CountAsync(a => a.SessionId == session.Id && a.Status == StudentSessionAssignmentStatus.Assigned, cancellationToken)
            : await context.ClassEnrollments
                .Where(e => e.ClassId == session.ClassId
                    && e.Status == EnrollmentStatus.Active)
                .CountAsync(e => e.EnrollDate <= VietnamTime.ToVietnamDateOnly(session.PlannedDatetime), cancellationToken);

        var makeupCount = await context.MakeupAllocations
            .CountAsync(a => a.TargetSessionId == session.Id && a.Status != MakeupAllocationStatus.Cancelled, cancellationToken);

        var pendingTrackedMakeupCount = context.MakeupAllocations.Local
            .Count(a => a.TargetSessionId == session.Id && a.Status != MakeupAllocationStatus.Cancelled);

        return regularCount + makeupCount + pendingTrackedMakeupCount;
    }

    private static async Task<bool> HasActiveAllocationForSessionAsync(
        IDbContext context,
        Guid studentProfileId,
        Guid targetSessionId,
        CancellationToken cancellationToken)
    {
        var existsInDatabase = await context.MakeupAllocations
            .AnyAsync(
                a => a.TargetSessionId == targetSessionId &&
                     a.Status != MakeupAllocationStatus.Cancelled &&
                     a.MakeupCredit.StudentProfileId == studentProfileId,
                cancellationToken);

        if (existsInDatabase)
        {
            return true;
        }

        var pendingCreditIds = context.MakeupCredits.Local
            .Where(c => c.StudentProfileId == studentProfileId)
            .Select(c => c.Id)
            .ToHashSet();

        return context.MakeupAllocations.Local.Any(
            a => a.TargetSessionId == targetSessionId &&
                 a.Status != MakeupAllocationStatus.Cancelled &&
                 pendingCreditIds.Contains(a.MakeupCreditId));
    }
}
