using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class PauseEnrollmentEligibleClassResolver(
    IDbContext context,
    StudentSessionAssignmentService studentSessionAssignmentService)
{
    public async Task<List<Guid>> GetEligibleClassIdsAsync(
        Guid studentProfileId,
        DateOnly pauseFrom,
        DateOnly pauseTo,
        CancellationToken cancellationToken)
    {
        var activeEnrollments = await context.ClassEnrollments
            .AsNoTracking()
            .Where(e => e.StudentProfileId == studentProfileId && e.Status == EnrollmentStatus.Active)
            .Select(e => new ActiveEnrollmentSnapshot(
                e.Id,
                e.ClassId))
            .ToListAsync(cancellationToken);

        if (activeEnrollments.Count == 0)
        {
            return [];
        }

        var pauseFromUtc = VietnamTime.TreatAsVietnamLocal(pauseFrom.ToDateTime(TimeOnly.MinValue));
        var pauseToUtc = VietnamTime.EndOfVietnamDayUtc(
            VietnamTime.TreatAsVietnamLocal(pauseTo.ToDateTime(TimeOnly.MinValue)));

        var activeEnrollmentIds = activeEnrollments
            .Select(e => e.Id)
            .ToList();

        var assignedClassIds = await context.StudentSessionAssignments
            .AsNoTracking()
            .Where(a => a.StudentProfileId == studentProfileId
                && a.Status == StudentSessionAssignmentStatus.Assigned
                && activeEnrollmentIds.Contains(a.ClassEnrollmentId)
                && a.Session.Status != SessionStatus.Cancelled
                && a.Session.PlannedDatetime >= pauseFromUtc
                && a.Session.PlannedDatetime <= pauseToUtc)
            .Select(a => a.Session.ClassId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var activeClassIds = activeEnrollments
            .Select(e => e.ClassId)
            .Distinct()
            .ToList();

        var candidateClassIds = activeClassIds
            .Except(assignedClassIds)
            .ToList();

        if (candidateClassIds.Count == 0)
        {
            return assignedClassIds;
        }

        var candidateSessions = await context.Sessions
            .AsNoTracking()
            .Where(s => candidateClassIds.Contains(s.ClassId)
                && s.Status != SessionStatus.Cancelled
                && s.PlannedDatetime >= pauseFromUtc
                && s.PlannedDatetime <= pauseToUtc)
            .Select(s => new CandidateSessionSnapshot(
                s.Id,
                s.ClassId))
            .ToListAsync(cancellationToken);

        var derivedClassIds = new HashSet<Guid>(assignedClassIds);
        foreach (var session in candidateSessions)
        {
            var isAssigned = await studentSessionAssignmentService
                .IsStudentRegularlyAssignedToSessionAsync(
                    session.Id,
                    studentProfileId,
                    cancellationToken);

            if (isAssigned)
            {
                derivedClassIds.Add(session.ClassId);
            }
        }

        return derivedClassIds
            .ToList();
    }

    private sealed record ActiveEnrollmentSnapshot(
        Guid Id,
        Guid ClassId);

    private sealed record CandidateSessionSnapshot(
        Guid Id,
        Guid ClassId);
}
