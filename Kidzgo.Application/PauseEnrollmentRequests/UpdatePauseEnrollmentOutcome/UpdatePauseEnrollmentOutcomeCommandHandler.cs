using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Classes;
using Kidzgo.Application.PauseEnrollmentRequests.Notifications;
using Kidzgo.Application.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.UpdatePauseEnrollmentOutcome;

public sealed class UpdatePauseEnrollmentOutcomeCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ITemplateRenderer templateRenderer,
    StudentSessionAssignmentService studentSessionAssignmentService)
    : ICommandHandler<UpdatePauseEnrollmentOutcomeCommand>
{
    public async Task<Result> Handle(UpdatePauseEnrollmentOutcomeCommand request, CancellationToken cancellationToken)
    {
        var pauseRequest = await context.PauseEnrollmentRequests
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (pauseRequest is null)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.NotFound(request.Id));
        }

        if (pauseRequest.Status != PauseEnrollmentRequestStatus.Approved)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.OutcomeNotAllowed);
        }

        if (pauseRequest.OutcomeCompletedAt.HasValue)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.OutcomeAlreadyCompleted);
        }

        if (request.Outcome == PauseEnrollmentOutcome.ReassignEquivalentClass)
        {
            await DropEnrollmentsFromPauseAsync(
                pauseRequest,
                studentSessionAssignmentService,
                cancellationToken);
        }
        else if (request.Outcome == PauseEnrollmentOutcome.ContinueWithTutoring)
        {
            await CancelAssignmentsAfterPauseAsync(pauseRequest, studentSessionAssignmentService, cancellationToken);
        }

        pauseRequest.Outcome = request.Outcome;
        pauseRequest.OutcomeNote = request.OutcomeNote;
        pauseRequest.OutcomeBy = userContext.UserId;
        pauseRequest.OutcomeAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        await PauseEnrollmentRequestNotificationHelper.NotifyAsync(
            context,
            templateRenderer,
            pauseRequest.StudentProfileId,
            pauseRequest.Id,
            PauseEnrollmentRequestNotificationHelper.NotificationType.OutcomeUpdated,
            pauseRequest.PauseFrom,
            pauseRequest.PauseTo,
            VietnameseEnumText.ForPauseEnrollmentOutcome(request.Outcome),
            request.OutcomeNote,
            cancellationToken);

        if (request.Outcome == PauseEnrollmentOutcome.ReassignEquivalentClass)
        {
            await PauseEnrollmentRequestNotificationHelper.NotifyStaffFollowUpAsync(
                context,
                templateRenderer,
                pauseRequest.StudentProfileId,
                pauseRequest.Id,
                pauseRequest.PauseFrom,
                pauseRequest.PauseTo,
                PauseEnrollmentRequestNotificationHelper.StaffFollowUpType.ReassignEquivalentClass,
                request.OutcomeNote,
                cancellationToken);
        }
        else if (request.Outcome == PauseEnrollmentOutcome.ContinueWithTutoring)
        {
            await PauseEnrollmentRequestNotificationHelper.NotifyStaffFollowUpAsync(
                context,
                templateRenderer,
                pauseRequest.StudentProfileId,
                pauseRequest.Id,
                pauseRequest.PauseFrom,
                pauseRequest.PauseTo,
                PauseEnrollmentRequestNotificationHelper.StaffFollowUpType.ContinueWithTutoring,
                request.OutcomeNote,
                cancellationToken);
        }

        return Result.Success();
    }

    private async Task DropEnrollmentsFromPauseAsync(
        PauseEnrollmentRequest pauseRequest,
        StudentSessionAssignmentService studentSessionAssignmentService,
        CancellationToken cancellationToken)
    {
        var enrollmentIds = await context.PauseEnrollmentRequestHistories
            .Where(history => history.PauseEnrollmentRequestId == pauseRequest.Id &&
                              history.EnrollmentId.HasValue &&
                              history.NewStatus == EnrollmentStatus.Paused)
            .Select(history => history.EnrollmentId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (enrollmentIds.Count == 0)
        {
            return;
        }

        var enrollments = await context.ClassEnrollments
            .Where(enrollment => enrollmentIds.Contains(enrollment.Id))
            .ToListAsync(cancellationToken);

        if (enrollments.Count == 0)
        {
            return;
        }

        var now = VietnamTime.UtcNow();
        var today = VietnamTime.ToVietnamDateOnly(now);
        var effectiveFrom = pauseRequest.PauseFrom;
        if (today > effectiveFrom)
        {
            effectiveFrom = today;
        }

        var effectiveFromUtc = VietnamTime.TreatAsVietnamLocal(effectiveFrom.ToDateTime(TimeOnly.MinValue));
        var affectedClassIds = new HashSet<Guid>();

        foreach (var enrollment in enrollments)
        {
            await studentSessionAssignmentService.CancelFutureAssignmentsForEnrollmentAsync(
                enrollment.Id,
                effectiveFromUtc,
                cancellationToken);

            if (enrollment.Status == EnrollmentStatus.Dropped)
            {
                continue;
            }

            var previousStatus = enrollment.Status;
            enrollment.Status = EnrollmentStatus.Dropped;
            enrollment.UpdatedAt = now;
            affectedClassIds.Add(enrollment.ClassId);

            context.PauseEnrollmentRequestHistories.Add(new PauseEnrollmentRequestHistory
            {
                Id = Guid.NewGuid(),
                PauseEnrollmentRequestId = pauseRequest.Id,
                StudentProfileId = enrollment.StudentProfileId,
                ClassId = enrollment.ClassId,
                EnrollmentId = enrollment.Id,
                PreviousStatus = previousStatus,
                NewStatus = EnrollmentStatus.Dropped,
                PauseFrom = pauseRequest.PauseFrom,
                PauseTo = pauseRequest.PauseTo,
                ChangedAt = now,
                ChangedBy = userContext.UserId
            });
        }

        foreach (var classId in affectedClassIds)
        {
            await ClassCapacityStatusHelper.SyncAvailabilityStatusAsync(
                context,
                classId,
                now,
                cancellationToken);
        }
    }

    private async Task CancelAssignmentsAfterPauseAsync(
        PauseEnrollmentRequest pauseRequest,
        StudentSessionAssignmentService studentSessionAssignmentService,
        CancellationToken cancellationToken)
    {
        var enrollmentIds = await context.PauseEnrollmentRequestHistories
            .Where(history => history.PauseEnrollmentRequestId == pauseRequest.Id &&
                              history.EnrollmentId.HasValue &&
                              history.NewStatus == EnrollmentStatus.Paused)
            .Select(history => history.EnrollmentId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (enrollmentIds.Count == 0)
        {
            return;
        }

        var effectiveFrom = pauseRequest.PauseTo.AddDays(1);
        var today = VietnamTime.ToVietnamDateOnly(VietnamTime.UtcNow());
        if (today > effectiveFrom)
        {
            effectiveFrom = today;
        }

        var effectiveFromUtc = VietnamTime.TreatAsVietnamLocal(effectiveFrom.ToDateTime(TimeOnly.MinValue));

        foreach (var enrollmentId in enrollmentIds)
        {
            await studentSessionAssignmentService.CancelFutureAssignmentsForEnrollmentAsync(
                enrollmentId,
                effectiveFromUtc,
                cancellationToken);
        }
    }
}
