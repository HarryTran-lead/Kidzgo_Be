using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.PauseEnrollmentRequests;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.GetPauseEnrollmentRequestById;

public sealed class GetPauseEnrollmentRequestByIdQueryHandler(IDbContext context)
    : IQueryHandler<GetPauseEnrollmentRequestByIdQuery, PauseEnrollmentRequestResponse>
{
    public async Task<Result<PauseEnrollmentRequestResponse>> Handle(
        GetPauseEnrollmentRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var item = await context.PauseEnrollmentRequests
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (item is null)
        {
            return Result.Failure<PauseEnrollmentRequestResponse>(
                PauseEnrollmentRequestErrors.NotFound(request.Id));
        }

        var enrollmentClassIds = await context.ClassEnrollments
            .Where(e => e.StudentProfileId == item.StudentProfileId
                        && e.Status != EnrollmentStatus.Dropped)
            .Select(e => e.ClassId)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<PauseEnrollmentClassDto> classes = new();
        if (enrollmentClassIds.Count > 0)
        {
            var pauseFromUtc = VietnamTime.TreatAsVietnamLocal(item.PauseFrom.ToDateTime(TimeOnly.MinValue));
            var pauseToUtc = VietnamTime.EndOfVietnamDayUtc(VietnamTime.TreatAsVietnamLocal(item.PauseTo.ToDateTime(TimeOnly.MinValue)));
            var classIdsInRange = await context.Sessions
                .Where(s => enrollmentClassIds.Contains(s.ClassId)
                            && s.PlannedDatetime >= pauseFromUtc
                            && s.PlannedDatetime <= pauseToUtc)
                .Select(s => s.ClassId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (classIdsInRange.Count > 0)
            {
                classes = await context.Classes
                    .Where(c => classIdsInRange.Contains(c.Id))
                    .Select(c => new PauseEnrollmentClassDto
                    {
                        Id = c.Id,
                        Code = c.Code,
                        Title = c.Title,
                        ProgramId = c.ProgramId,
                        ProgramName = c.Program.Name,
                        BranchId = c.BranchId,
                        BranchName = c.Branch.Name,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Status = c.Status.ToString()
                    })
                    .ToListAsync(cancellationToken);
            }
        }

        return new PauseEnrollmentRequestResponse
        {
            Id = item.Id,
            StudentProfileId = item.StudentProfileId,
            ClassId = item.ClassId,
            PauseFrom = item.PauseFrom,
            PauseTo = item.PauseTo,
            Reason = item.Reason,
            Status = item.Status.ToString(),
            RequestedAt = item.RequestedAt,
            ApprovedBy = item.ApprovedBy,
            ApprovedAt = item.ApprovedAt,
            CancelledBy = item.CancelledBy,
            CancelledAt = item.CancelledAt,
            Outcome = item.Outcome.HasValue ? item.Outcome.Value.ToString() : null,
            OutcomeNote = item.OutcomeNote,
            OutcomeBy = item.OutcomeBy,
            OutcomeAt = item.OutcomeAt,
            ReassignedClassId = item.ReassignedClassId,
            ReassignedEnrollmentId = item.ReassignedEnrollmentId,
            OutcomeCompletedBy = item.OutcomeCompletedBy,
            OutcomeCompletedAt = item.OutcomeCompletedAt,
            ReservedSessionCount = item.ReservedSessionCount,
            ReservationExpiresOn = item.ReservationExpiresOn,
            ReservationSnapshotAt = item.ReservationSnapshotAt,
            Classes = classes
        };
    }
}
