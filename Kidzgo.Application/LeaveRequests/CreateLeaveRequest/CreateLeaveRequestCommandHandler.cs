using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Kidzgo.Application.LeaveRequests.CreateLeaveRequest;

public sealed class CreateLeaveRequestCommandHandler(
    IDbContext context,
    ClassLifecycleService classLifecycleService,
    SessionParticipantService sessionParticipantService,
    ApprovedLeaveAttendanceService approvedLeaveAttendanceService)
    : ICommandHandler<CreateLeaveRequestCommand, CreateLeaveRequestResponse>
{
    private const int MaxLeavesPerMonth = 2;

    public async Task<Result<CreateLeaveRequestResponse>> Handle(CreateLeaveRequestCommand command, CancellationToken cancellationToken)
    {
        var profile = await context.Profiles
            .Include(p => p.ClassEnrollments)
            .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId && !p.IsDeleted && p.IsActive, cancellationToken);

        if (profile is null)
        {
            return Result.Failure<CreateLeaveRequestResponse>(LeaveRequestErrors.NotFound(command.StudentProfileId));
        }

        var classInfo = await context.Classes
            .Include(c => c.Program)
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        if (classInfo is null)
        {
            return Result.Failure<CreateLeaveRequestResponse>(LeaveRequestErrors.ClassNotFound(command.ClassId));
        }

        List<Session> sessionsToLeave;
        if (command.SessionId.HasValue)
        {
            var targetSession = await context.Sessions
                .FirstOrDefaultAsync(
                    s => s.Id == command.SessionId.Value && s.ClassId == command.ClassId,
                    cancellationToken);

            if (targetSession is null)
            {
                return Result.Failure<CreateLeaveRequestResponse>(
                    LeaveRequestErrors.SessionNotFound(command.ClassId, command.SessionDate));
            }

            var assignmentCheck = await sessionParticipantService
                .EnsureStudentAssignedToSessionAsync(targetSession.Id, command.StudentProfileId, cancellationToken);

            if (assignmentCheck.IsFailure)
            {
                return Result.Failure<CreateLeaveRequestResponse>(assignmentCheck.Error);
            }

            sessionsToLeave = [targetSession];
        }
        else
        {
            bool enrolled = profile.ClassEnrollments.Any(e => e.ClassId == command.ClassId && e.Status == EnrollmentStatus.Active);
            if (!enrolled)
            {
                return Result.Failure<CreateLeaveRequestResponse>(LeaveRequestErrors.NotEnrolled(
                    command.ClassId,
                    command.StudentProfileId));
            }

            var endDate = command.EndDate ?? command.SessionDate;
            sessionsToLeave = await GetAssignedSessionsInRangeAsync(
                command.StudentProfileId,
                command.ClassId,
                command.SessionDate,
                endDate,
                cancellationToken);
        }

        if (!sessionsToLeave.Any())
        {
            return Result.Failure<CreateLeaveRequestResponse>(
                LeaveRequestErrors.SessionNotFound(command.ClassId, command.SessionDate));
        }

        var firstSessionDate = sessionsToLeave
            .Select(s => VietnamTime.ToVietnamDateOnly(s.PlannedDatetime))
            .Min();
        var sessionMonth = firstSessionDate.Month;
        var sessionYear = firstSessionDate.Year;

        // Get existing leave requests (Pending + Approved) for this student, class, and month
        var existingLeavesInMonth = await context.LeaveRequests
            .Where(lr => lr.StudentProfileId == command.StudentProfileId
                        && lr.ClassId == command.ClassId
                        && lr.SessionDate.Month == sessionMonth
                        && lr.SessionDate.Year == sessionYear
                        && (lr.Status == LeaveRequestStatus.Pending || lr.Status == LeaveRequestStatus.Approved)
                        && lr.Class.Status != ClassStatus.Closed)
            .ToListAsync(cancellationToken);

        var existingLeaveKeys = existingLeavesInMonth
            .Select(GetLeaveKey)
            .ToHashSet();
        var requestedLeaveKeys = sessionsToLeave
            .Select(GetSessionKey)
            .ToHashSet();

        if (requestedLeaveKeys.Any(existingLeaveKeys.Contains))
        {
            return Result.Failure<CreateLeaveRequestResponse>(Error.Validation(
                "LeaveRequest.AlreadyExists",
                "A leave request already exists for at least one selected session."));
        }

        var totalSessionDatesInMonth = existingLeaveKeys
            .Union(requestedLeaveKeys)
            .Count();

        var configuredMaxLeavesPerMonth = await context.ProgramLeavePolicies
            .Where(x => x.ProgramId == classInfo.ProgramId)
            .Select(x => (int?)x.MaxLeavesPerMonth)
            .FirstOrDefaultAsync(cancellationToken) ?? MaxLeavesPerMonth;

        if (totalSessionDatesInMonth > configuredMaxLeavesPerMonth)
        {
            return Result.Failure<CreateLeaveRequestResponse>(LeaveRequestErrors.ExceededMonthlyLeaveLimit(configuredMaxLeavesPerMonth));
        }

        var createdLeaves = new List<LeaveRequest>();
        var impactedClassIds = new HashSet<Guid>();
        var now = VietnamTime.UtcNow();

        // Create one LeaveRequest per session date (not per session)
        // sessionDate và endDate cách nhau bao nhiêu ngày thì tạo bấy nhiêu LeaveRequest
        foreach (var session in sessionsToLeave.OrderBy(s => s.PlannedDatetime))
        {
            var sessionDate = VietnamTime.ToVietnamDateOnly(session.PlannedDatetime);
            var noticeHours = (int)Math.Floor((session.PlannedDatetime - now).TotalHours);
            var status = noticeHours >= 24 ? LeaveRequestStatus.Approved : LeaveRequestStatus.Pending;

            var leave = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                StudentProfileId = command.StudentProfileId,
                ClassId = command.ClassId,
                SessionId = session.Id,
                SessionDate = sessionDate,
                EndDate = null,
                Reason = command.Reason,
                NoticeHours = noticeHours,
                Status = status,
                RequestedAt = now
            };

            context.LeaveRequests.Add(leave);
            createdLeaves.Add(leave);
            if (status == LeaveRequestStatus.Approved)
            {
                bool creditExists = await context.MakeupCredits
                    .AnyAsync(c => c.StudentProfileId == command.StudentProfileId &&
                                   c.SourceSessionId == session.Id &&
                                   c.CreatedReason == CreatedReason.ApprovedLeave24H,
                        cancellationToken);

                if (!creditExists)
                {
                    var credit = new MakeupCredit
                    {
                        Id = Guid.NewGuid(),
                        StudentProfileId = command.StudentProfileId,
                        SourceSessionId = session.Id,
                        Status = MakeupCreditStatus.Available,
                        CreatedReason = CreatedReason.ApprovedLeave24H,
                        ExpiresAt = null,
                        CreatedAt = now
                    };
                    context.MakeupCredits.Add(credit);
                }

                leave.ApprovedAt = now;

                var transitionClassIds = await approvedLeaveAttendanceService.ApplyApprovedLeaveActivationAsync(
                    command.StudentProfileId,
                    session,
                    cancellationToken);
                impactedClassIds.UnionWith(transitionClassIds);
            }
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

        return new CreateLeaveRequestResponse
        {
            LeaveRequests = createdLeaves.Select(l => new LeaveRequestItem
            {
                Id = l.Id,
                StudentProfileId = l.StudentProfileId,
                ClassId = l.ClassId,
                SessionId = l.SessionId,
                SessionDate = l.SessionDate,
                EndDate = l.EndDate,
                Reason = l.Reason,
                NoticeHours = l.NoticeHours,
                Status = l.Status.ToString(),
                RequestedAt = l.RequestedAt,
                ApprovedAt = l.ApprovedAt
            }).ToList()
        };
    }

    private async Task<List<Session>> GetAssignedSessionsInRangeAsync(
        Guid studentProfileId,
        Guid classId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        var fromUtc = VietnamTime.TreatAsVietnamLocal(fromDate.ToDateTime(TimeOnly.MinValue));
        var toUtc = VietnamTime.EndOfVietnamDayUtc(VietnamTime.TreatAsVietnamLocal(toDate.ToDateTime(TimeOnly.MinValue)));

        var assignedSessions = await context.StudentSessionAssignments
            .AsNoTracking()
            .Where(a => a.StudentProfileId == studentProfileId
                && a.Status == StudentSessionAssignmentStatus.Assigned
                && a.Session.ClassId == classId
                && a.Session.Status != SessionStatus.Cancelled
                && a.Session.PlannedDatetime >= fromUtc
                && a.Session.PlannedDatetime <= toUtc)
            .Select(a => a.Session)
            .ToListAsync(cancellationToken);

        var assignedSessionIds = assignedSessions
            .Select(session => session.Id)
            .ToHashSet();

        var candidateSessions = await context.Sessions
            .AsNoTracking()
            .Where(s => s.ClassId == classId
                && s.Status != SessionStatus.Cancelled
                && s.PlannedDatetime >= fromUtc
                && s.PlannedDatetime <= toUtc
                && !assignedSessionIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        var sessionsInRange = new List<Session>(assignedSessions);
        foreach (var session in candidateSessions)
        {
            var assignmentCheck = await sessionParticipantService
                .EnsureStudentAssignedToSessionAsync(session.Id, studentProfileId, cancellationToken);

            if (assignmentCheck.IsSuccess)
            {
                sessionsInRange.Add(session);
            }
        }

        return sessionsInRange;
    }

    private static string GetLeaveKey(LeaveRequest leave)
    {
        return leave.SessionId?.ToString() ?? $"{leave.ClassId}_{leave.SessionDate:yyyyMMdd}";
    }

    private static string GetSessionKey(Session session)
    {
        return session.Id.ToString();
    }
}

