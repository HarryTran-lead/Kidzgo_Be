using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.PauseEnrollmentRequests;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.CreatePauseEnrollmentRequest;

public sealed class CreatePauseEnrollmentRequestCommandHandler(
    IDbContext context,
    PauseEnrollmentEligibleClassResolver eligibleClassResolver)
    : ICommandHandler<CreatePauseEnrollmentRequestCommand, CreatePauseEnrollmentRequestResponse>
{
    public async Task<Result<CreatePauseEnrollmentRequestResponse>> Handle(
        CreatePauseEnrollmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var scope = PauseEnrollmentRequestScopeHelper.ResolveFromClassId(command.ClassId);

        var profileExists = await context.Profiles
            .AnyAsync(p => p.Id == command.StudentProfileId && !p.IsDeleted && p.IsActive, cancellationToken);

        if (!profileExists)
        {
            return Result.Failure<CreatePauseEnrollmentRequestResponse>(
                PauseEnrollmentRequestErrors.StudentNotFound(command.StudentProfileId));
        }

        var activeEnrollments = await context.ClassEnrollments
            .Where(e => e.StudentProfileId == command.StudentProfileId
                        && e.Status == EnrollmentStatus.Active)
            .ToListAsync(cancellationToken);

        if (activeEnrollments.Count == 0)
        {
            return Result.Failure<CreatePauseEnrollmentRequestResponse>(
                PauseEnrollmentRequestErrors.NoEnrollmentsInRange);
        }

        var classIdsInRange = await eligibleClassResolver.GetEligibleClassIdsAsync(
            command.StudentProfileId,
            command.PauseFrom,
            command.PauseTo,
            cancellationToken);

        if (classIdsInRange.Count == 0 && scope == PauseEnrollmentRequestScopeHelper.AllEligible)
        {
            return Result.Failure<CreatePauseEnrollmentRequestResponse>(
                PauseEnrollmentRequestErrors.NoEnrollmentsInRange);
        }

        Guid? targetClassId = null;
        List<Guid> targetClassIds;
        if (scope == PauseEnrollmentRequestScopeHelper.SingleClass)
        {
            var selectedClassId = command.ClassId.GetValueOrDefault();
            targetClassId = selectedClassId;
            var targetEnrollment = activeEnrollments
                .FirstOrDefault(e => e.ClassId == selectedClassId);

            if (targetEnrollment is null)
            {
                return Result.Failure<CreatePauseEnrollmentRequestResponse>(
                    PauseEnrollmentRequestErrors.NotEnrolled(selectedClassId, command.StudentProfileId));
            }

            if (!classIdsInRange.Contains(selectedClassId))
            {
                return Result.Failure<CreatePauseEnrollmentRequestResponse>(
                    PauseEnrollmentRequestErrors.ClassNotInPauseRange(
                        selectedClassId,
                        command.PauseFrom,
                        command.PauseTo));
            }

            targetClassIds = [selectedClassId];
        }
        else
        {
            targetClassIds = classIdsInRange;
        }

        var overlappingRequests = context.PauseEnrollmentRequests
            .Where(r => r.StudentProfileId == command.StudentProfileId
                        && (r.Status == PauseEnrollmentRequestStatus.Pending ||
                            r.Status == PauseEnrollmentRequestStatus.Approved)
                        && r.PauseFrom <= command.PauseTo
                        && r.PauseTo >= command.PauseFrom);

        bool hasActiveRequest = scope == PauseEnrollmentRequestScopeHelper.SingleClass
            ? await overlappingRequests.AnyAsync(
                r => !r.ClassId.HasValue || r.ClassId == targetClassId,
                cancellationToken)
            : await overlappingRequests.AnyAsync(cancellationToken);

        if (hasActiveRequest)
        {
            return Result.Failure<CreatePauseEnrollmentRequestResponse>(
                PauseEnrollmentRequestErrors.DuplicateActiveRequest);
        }

        var classesInRange = await context.Classes
            .Where(c => targetClassIds.Contains(c.Id))
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

        var request = new PauseEnrollmentRequest
        {
            Id = Guid.NewGuid(),
            StudentProfileId = command.StudentProfileId,
            ClassId = targetClassId,
            PauseFrom = command.PauseFrom,
            PauseTo = command.PauseTo,
            Reason = command.Reason,
            Status = PauseEnrollmentRequestStatus.Pending,
            RequestedAt = VietnamTime.UtcNow()
        };

        context.PauseEnrollmentRequests.Add(request);
        await context.SaveChangesAsync(cancellationToken);

        return new CreatePauseEnrollmentRequestResponse
        {
            Id = request.Id,
            StudentProfileId = request.StudentProfileId,
            ClassId = request.ClassId,
            PauseFrom = request.PauseFrom,
            PauseTo = request.PauseTo,
            Reason = request.Reason,
            Scope = PauseEnrollmentRequestScopeHelper.ResolveFromClassId(request.ClassId),
            Status = request.Status.ToString(),
            RequestedAt = request.RequestedAt,
            Classes = classesInRange
        };
    }
}
