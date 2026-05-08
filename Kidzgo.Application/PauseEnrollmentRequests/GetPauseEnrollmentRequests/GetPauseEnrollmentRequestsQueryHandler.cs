using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.PauseEnrollmentRequests;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.GetPauseEnrollmentRequests;

public sealed class GetPauseEnrollmentRequestsQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetPauseEnrollmentRequestsQuery, Page<PauseEnrollmentRequestResponse>>
{
    public async Task<Result<Page<PauseEnrollmentRequestResponse>>> Handle(
        GetPauseEnrollmentRequestsQuery request,
        CancellationToken cancellationToken)
    {
        Guid? studentProfileId = request.StudentProfileId;
        if (!request.StudentProfileId.HasValue)
        {
            var currentUserRole = await context.Users
                .AsNoTracking()
                .Where(u => u.Id == userContext.UserId)
                .Select(u => u.Role)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentUserRole is UserRole.Parent)
            {
                if (!userContext.StudentId.HasValue)
                {
                    return new Page<PauseEnrollmentRequestResponse>(
                        new List<PauseEnrollmentRequestResponse>(),
                        0,
                        request.PageNumber,
                        request.PageSize);
                }

                studentProfileId = userContext.StudentId.Value;
            }
        }

        var query = context.PauseEnrollmentRequests.AsQueryable();

        if (studentProfileId.HasValue)
        {
            query = query.Where(r => r.StudentProfileId == studentProfileId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        var items = await query
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new
            {
                r.Id,
                r.StudentProfileId,
                r.ClassId,
                r.PauseFrom,
                r.PauseTo,
                r.Reason,
                r.Status,
                r.RequestedAt,
                r.ApprovedBy,
                r.ApprovedAt,
                r.CancelledBy,
                r.CancelledAt,
                r.Outcome,
                r.OutcomeNote,
                r.OutcomeBy,
                r.OutcomeAt,
                r.ReassignedClassId,
                r.ReassignedEnrollmentId,
                r.OutcomeCompletedBy,
                r.OutcomeCompletedAt,
                r.ReservedSessionCount,
                r.ReservationExpiresOn,
                r.ReservationSnapshotAt
            })
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
        {
            return new Page<PauseEnrollmentRequestResponse>(
                new List<PauseEnrollmentRequestResponse>(),
                0,
                request.PageNumber,
                request.PageSize);
        }

        var matchingClassIdsByRequest = items.ToDictionary(
            item => item.Id,
            item => item.ClassId.HasValue
                ? new List<Guid> { item.ClassId.Value }
                : new List<Guid>());

        var allEligibleItems = items
            .Where(item => !item.ClassId.HasValue)
            .ToList();

        if (allEligibleItems.Count > 0)
        {
            var studentProfileIds = allEligibleItems
                .Select(r => r.StudentProfileId)
                .Distinct()
                .ToList();
            var activeEnrollments = await context.ClassEnrollments
                .AsNoTracking()
                .Where(e =>
                    studentProfileIds.Contains(e.StudentProfileId) &&
                    (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Paused))
                .Select(e => new
                {
                    e.StudentProfileId,
                    e.ClassId
                })
                .ToListAsync(cancellationToken);

            var relevantClassIds = activeEnrollments.Select(e => e.ClassId).Distinct().ToList();
            if (relevantClassIds.Count > 0)
            {
                var minPauseFrom = allEligibleItems.Min(r => r.PauseFrom);
                var maxPauseTo = allEligibleItems.Max(r => r.PauseTo);
                var minPauseFromUtc = VietnamTime.TreatAsVietnamLocal(minPauseFrom.ToDateTime(TimeOnly.MinValue));
                var maxPauseToUtc = VietnamTime.EndOfVietnamDayUtc(VietnamTime.TreatAsVietnamLocal(maxPauseTo.ToDateTime(TimeOnly.MinValue)));

                var sessionsByClass = await context.Sessions
                    .AsNoTracking()
                    .Where(s => relevantClassIds.Contains(s.ClassId)
                        && s.PlannedDatetime >= minPauseFromUtc
                        && s.PlannedDatetime <= maxPauseToUtc)
                    .Select(s => new
                    {
                        s.ClassId,
                        s.PlannedDatetime
                    })
                    .ToListAsync(cancellationToken);

                foreach (var item in allEligibleItems)
                {
                    matchingClassIdsByRequest[item.Id] = activeEnrollments
                        .Where(e => e.StudentProfileId == item.StudentProfileId)
                        .Select(e => e.ClassId)
                        .Where(classId => sessionsByClass.Any(s =>
                            s.ClassId == classId &&
                            VietnamTime.ToVietnamDateOnly(s.PlannedDatetime) >= item.PauseFrom &&
                            VietnamTime.ToVietnamDateOnly(s.PlannedDatetime) <= item.PauseTo))
                        .Distinct()
                        .ToList();
                }
            }
        }

        var allMatchingClassIds = matchingClassIdsByRequest
            .SelectMany(kvp => kvp.Value)
            .Distinct()
            .ToList();

        var classDetails = await context.Classes
            .AsNoTracking()
            .Where(c => allMatchingClassIds.Contains(c.Id))
            .Select(c => new
            {
                c.Id,
                c.Code,
                c.Title,
                c.ProgramId,
                ProgramName = c.Program.Name,
                c.BranchId,
                BranchName = c.Branch.Name,
                c.StartDate,
                c.EndDate,
                Status = c.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        var classDetailLookup = classDetails.ToDictionary(c => c.Id);
        if (request.ClassId.HasValue)
        {
            items = items
                .Where(item => matchingClassIdsByRequest.TryGetValue(item.Id, out var classIds) &&
                    classIds.Contains(request.ClassId.Value))
                .ToList();
        }

        if (request.BranchId.HasValue)
        {
            items = items
                .Where(item => matchingClassIdsByRequest.TryGetValue(item.Id, out var classIds) &&
                    classIds.Any(classId =>
                        classDetailLookup.TryGetValue(classId, out var classDetail) &&
                        classDetail.BranchId == request.BranchId.Value))
                .ToList();
        }

        var total = items.Count;
        if (total == 0)
        {
            return new Page<PauseEnrollmentRequestResponse>(
                new List<PauseEnrollmentRequestResponse>(),
                0,
                request.PageNumber,
                request.PageSize);
        }

        items = items
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var classLookup = items.ToDictionary(
            item => item.Id,
            item => matchingClassIdsByRequest.TryGetValue(item.Id, out var classIds)
                ? classIds
                    .Where(classDetailLookup.ContainsKey)
                    .Select(classId =>
                    {
                        var c = classDetailLookup[classId];
                        return new PauseEnrollmentClassDto
                        {
                            Id = c.Id,
                            Code = c.Code,
                            Title = c.Title,
                            ProgramId = c.ProgramId,
                            ProgramName = c.ProgramName,
                            BranchId = c.BranchId,
                            BranchName = c.BranchName,
                            StartDate = c.StartDate,
                            EndDate = c.EndDate,
                            Status = c.Status
                        };
                    })
                    .ToList()
                : new List<PauseEnrollmentClassDto>());

        var responses = items
            .Select(r => new PauseEnrollmentRequestResponse
            {
                Id = r.Id,
                StudentProfileId = r.StudentProfileId,
                ClassId = r.ClassId,
                Scope = PauseEnrollmentRequestScopeHelper.ResolveFromClassId(r.ClassId),
                PauseFrom = r.PauseFrom,
                PauseTo = r.PauseTo,
                Reason = r.Reason,
                Status = r.Status.ToString(),
                RequestedAt = r.RequestedAt,
                ApprovedBy = r.ApprovedBy,
                ApprovedAt = r.ApprovedAt,
                CancelledBy = r.CancelledBy,
                CancelledAt = r.CancelledAt,
                Outcome = r.Outcome.HasValue ? r.Outcome.Value.ToString() : null,
                OutcomeNote = r.OutcomeNote,
                OutcomeBy = r.OutcomeBy,
                OutcomeAt = r.OutcomeAt,
                ReassignedClassId = r.ReassignedClassId,
                ReassignedEnrollmentId = r.ReassignedEnrollmentId,
                OutcomeCompletedBy = r.OutcomeCompletedBy,
                OutcomeCompletedAt = r.OutcomeCompletedAt,
                ReservedSessionCount = r.ReservedSessionCount,
                ReservationExpiresOn = r.ReservationExpiresOn,
                ReservationSnapshotAt = r.ReservationSnapshotAt,
                Classes = classLookup.TryGetValue(r.Id, out var classes) ? classes : new List<PauseEnrollmentClassDto>()
            })
            .ToList();

        return new Page<PauseEnrollmentRequestResponse>(responses, total, request.PageNumber, request.PageSize);
    }
}
