using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public readonly record struct ApprovedLeaveSessionCandidate(
    Guid StudentProfileId,
    Guid SessionId,
    Guid ClassId,
    DateOnly SessionDate);

public sealed class ApprovedLeaveAttendanceService(
    IDbContext context,
    SessionParticipantService sessionParticipantService,
    RegistrationSessionConsumptionService registrationSessionConsumptionService)
{
    public async Task<bool> HasApprovedLeaveAsync(
        Guid studentProfileId,
        Session session,
        CancellationToken cancellationToken)
    {
        var sessionDate = VietnamTime.ToVietnamDateOnly(session.PlannedDatetime);

        return await context.LeaveRequests
            .AsNoTracking()
            .AnyAsync(
                l => l.StudentProfileId == studentProfileId
                     && l.Status == LeaveRequestStatus.Approved
                     && (l.SessionId == session.Id ||
                         (l.ClassId == session.ClassId
                          && l.SessionDate <= sessionDate
                          && (l.EndDate == null || l.EndDate >= sessionDate))),
                cancellationToken);
    }

    public async Task<HashSet<Guid>> GetApprovedLeaveStudentIdsForSessionAsync(
        Session session,
        IEnumerable<Guid> studentProfileIds,
        CancellationToken cancellationToken)
    {
        var studentIdList = studentProfileIds
            .Distinct()
            .ToList();

        if (studentIdList.Count == 0)
        {
            return new HashSet<Guid>();
        }

        var sessionDate = VietnamTime.ToVietnamDateOnly(session.PlannedDatetime);

        return await context.LeaveRequests
            .AsNoTracking()
            .Where(
                l => studentIdList.Contains(l.StudentProfileId)
                     && l.Status == LeaveRequestStatus.Approved
                     && (l.SessionId == session.Id ||
                         (l.ClassId == session.ClassId
                          && l.SessionDate <= sessionDate
                          && (l.EndDate == null || l.EndDate >= sessionDate))))
            .Select(l => l.StudentProfileId)
            .Distinct()
            .ToHashSetAsync(cancellationToken);
    }

    public async Task<HashSet<(Guid StudentProfileId, Guid SessionId)>> GetApprovedLeavePairsAsync(
        IEnumerable<ApprovedLeaveSessionCandidate> candidates,
        CancellationToken cancellationToken)
    {
        var candidateList = candidates
            .Distinct()
            .ToList();

        if (candidateList.Count == 0)
        {
            return new HashSet<(Guid StudentProfileId, Guid SessionId)>();
        }

        var studentIds = candidateList
            .Select(c => c.StudentProfileId)
            .Distinct()
            .ToList();
        var sessionIds = candidateList
            .Select(c => c.SessionId)
            .Distinct()
            .ToList();
        var classIds = candidateList
            .Select(c => c.ClassId)
            .Distinct()
            .ToList();
        var minDate = candidateList.Min(c => c.SessionDate);
        var maxDate = candidateList.Max(c => c.SessionDate);

        var approvedLeaves = await context.LeaveRequests
            .AsNoTracking()
            .Where(
                l => studentIds.Contains(l.StudentProfileId)
                     && l.Status == LeaveRequestStatus.Approved
                     && ((l.SessionId.HasValue && sessionIds.Contains(l.SessionId.Value)) ||
                         (!l.SessionId.HasValue
                          && classIds.Contains(l.ClassId)
                          && l.SessionDate <= maxDate
                          && (l.EndDate == null || l.EndDate >= minDate))))
            .Select(l => new ApprovedLeaveMatch(
                l.StudentProfileId,
                l.SessionId,
                l.ClassId,
                l.SessionDate,
                l.EndDate))
            .ToListAsync(cancellationToken);

        var result = new HashSet<(Guid StudentProfileId, Guid SessionId)>();

        foreach (var candidate in candidateList)
        {
            var isApproved = approvedLeaves.Any(
                leave => leave.StudentProfileId == candidate.StudentProfileId
                         && (leave.SessionId == candidate.SessionId ||
                             (!leave.SessionId.HasValue
                              && leave.ClassId == candidate.ClassId
                              && leave.SessionDate <= candidate.SessionDate
                              && (leave.EndDate == null || leave.EndDate >= candidate.SessionDate))));

            if (isApproved)
            {
                result.Add((candidate.StudentProfileId, candidate.SessionId));
            }
        }

        return result;
    }

    public async Task ApplyApprovedLeaveActivationAsync(
        Guid studentProfileId,
        Session session,
        CancellationToken cancellationToken)
    {
        var attendance = await context.Attendances
            .FirstOrDefaultAsync(
                a => a.SessionId == session.Id && a.StudentProfileId == studentProfileId,
                cancellationToken);

        if (attendance is null)
        {
            return;
        }

        var participant = await GetRegularParticipantAsync(session.Id, studentProfileId, cancellationToken);
        if (participant is null)
        {
            return;
        }

        await registrationSessionConsumptionService.ApplyAttendanceTransitionAsync(
            participant.Value.RegistrationId,
            attendance.AttendanceStatus,
            attendance.AbsenceType,
            AttendanceStatus.Makeup,
            null,
            cancellationToken);
    }

    public async Task ApplyApprovedLeaveDeactivationAsync(
        Guid studentProfileId,
        Session session,
        CancellationToken cancellationToken)
    {
        var attendance = await context.Attendances
            .FirstOrDefaultAsync(
                a => a.SessionId == session.Id && a.StudentProfileId == studentProfileId,
                cancellationToken);

        if (attendance is null)
        {
            return;
        }

        var participant = await GetRegularParticipantAsync(session.Id, studentProfileId, cancellationToken);
        if (participant is null)
        {
            return;
        }

        await registrationSessionConsumptionService.ApplyAttendanceTransitionAsync(
            participant.Value.RegistrationId,
            AttendanceStatus.Makeup,
            null,
            attendance.AttendanceStatus,
            attendance.AbsenceType,
            cancellationToken);
    }

    private async Task<SessionParticipant?> GetRegularParticipantAsync(
        Guid sessionId,
        Guid studentProfileId,
        CancellationToken cancellationToken)
    {
        var participant = (await sessionParticipantService.GetParticipantsAsync(sessionId, cancellationToken))
            .FirstOrDefault(p => p.StudentProfileId == studentProfileId);

        if (participant.StudentProfileId == Guid.Empty || participant.IsMakeup)
        {
            return null;
        }

        return participant;
    }

    private sealed record ApprovedLeaveMatch(
        Guid StudentProfileId,
        Guid? SessionId,
        Guid ClassId,
        DateOnly SessionDate,
        DateOnly? EndDate);
}
