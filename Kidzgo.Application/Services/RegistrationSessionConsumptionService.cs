using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Registrations.Notifications;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LearningTickets;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed record AttendanceTransitionOutcome(
    IReadOnlyCollection<Guid> ImpactedClassIds,
    bool TicketChanged,
    int TicketDelta,
    int? TicketBalance,
    bool AdvanceLessonProgression,
    bool? CompatibilityPassed,
    string? CompatibilityReason);

public sealed class RegistrationSessionConsumptionService(
    IDbContext context,
    StudentSessionAssignmentService studentSessionAssignmentService,
    TicketConsumptionPolicyService ticketConsumptionPolicyService)
{
    public async Task<AttendanceTransitionOutcome> ApplyAttendanceTransitionAsync(
        Guid? sessionId,
        Guid? attendanceId,
        Guid? registrationId,
        AttendanceStatus? previousStatus,
        AbsenceType? previousAbsenceType,
        AttendanceStatus newStatus,
        AbsenceType? newAbsenceType,
        ParticipationType participationType,
        SectionType sectionType,
        DateTime sessionDateTimeUtc,
        CancellationToken cancellationToken)
    {
        var impactedClassIds = new HashSet<Guid>();

        if (!registrationId.HasValue)
        {
            return new AttendanceTransitionOutcome(impactedClassIds, false, 0, null, false, null, null);
        }

        var beforeDecision = ticketConsumptionPolicyService.Evaluate(
            previousStatus,
            previousAbsenceType,
            participationType,
            sectionType);
        var afterDecision = ticketConsumptionPolicyService.Evaluate(
            newStatus,
            newAbsenceType,
            participationType,
            sectionType);

        var consumedBefore = beforeDecision.ShouldConsumeTicket && beforeDecision.Quantity > 0;
        var consumedAfter = afterDecision.ShouldConsumeTicket && afterDecision.Quantity > 0;

        if (consumedBefore == consumedAfter)
        {
            return new AttendanceTransitionOutcome(
                impactedClassIds,
                false,
                0,
                null,
                afterDecision.AdvanceLessonProgression,
                null,
                null);
        }

        var registration = await context.Registrations
            .FirstOrDefaultAsync(r => r.Id == registrationId.Value, cancellationToken);

        if (registration is null)
        {
            return new AttendanceTransitionOutcome(
                impactedClassIds,
                false,
                0,
                null,
                afterDecision.AdvanceLessonProgression,
                null,
                null);
        }

        var now = VietnamTime.UtcNow();
        CollectRegistrationClassIds(registration, impactedClassIds);

        if (consumedAfter)
        {
            var availableTicket = await context.LearningTicketItems
                .Where(x => x.RegistrationId == registration.Id &&
                            x.Status == LearningTicketItemStatus.Available)
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (availableTicket is null)
            {
                return new AttendanceTransitionOutcome(
                    impactedClassIds,
                    false,
                    0,
                    registration.RemainingSessions,
                    afterDecision.AdvanceLessonProgression,
                    null,
                    null);
            }

            availableTicket.Status = LearningTicketItemStatus.Consumed;
            availableTicket.ConsumedBySessionId = sessionId;
            availableTicket.ConsumedByAttendanceId = attendanceId;
            availableTicket.ConsumedAt = now;

            context.LearningTicketLedgers.Add(new LearningTicketLedger
            {
                Id = Guid.NewGuid(),
                StudentProfileId = registration.StudentProfileId,
                RegistrationId = registration.Id,
                LearningTicketItemId = availableTicket.Id,
                SessionId = sessionId,
                AttendanceId = attendanceId,
                TransactionType = LearningTicketTransactionType.Consume,
                Quantity = -1,
                Reason = afterDecision.Reason,
                CreatedAt = now
            });

            registration.UsedSessions++;
            registration.RemainingSessions = Math.Max(0, registration.RemainingSessions - 1);
            registration.UpdatedAt = now;

            if (registration.RemainingSessions == 0)
            {
                registration.Status = RegistrationStatus.Completed;
                var completedEnrollmentClassIds = await CompleteActiveEnrollmentsAsync(
                    registration,
                    sessionDateTimeUtc,
                    now,
                    cancellationToken);
                impactedClassIds.UnionWith(completedEnrollmentClassIds);
            }

            await LowRemainingSessionsNotificationHelper.QueueAsync(context, registration, cancellationToken);

            return new AttendanceTransitionOutcome(
                impactedClassIds,
                true,
                -1,
                registration.RemainingSessions,
                afterDecision.AdvanceLessonProgression,
                null,
                null);
        }

        var wasCompletedBySessionExhaustion =
            registration.Status == RegistrationStatus.Completed &&
            registration.RemainingSessions == 0;

        var consumedTicket = await context.LearningTicketItems
            .Where(x => x.RegistrationId == registration.Id &&
                        x.Status == LearningTicketItemStatus.Consumed &&
                        (!attendanceId.HasValue || x.ConsumedByAttendanceId == attendanceId) &&
                        (!sessionId.HasValue || x.ConsumedBySessionId == sessionId))
            .OrderByDescending(x => x.ConsumedAt)
            .ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (consumedTicket is null)
        {
            consumedTicket = await context.LearningTicketItems
                .Where(x => x.RegistrationId == registration.Id && x.Status == LearningTicketItemStatus.Consumed)
                .OrderByDescending(x => x.ConsumedAt)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (consumedTicket is not null)
        {
            consumedTicket.Status = LearningTicketItemStatus.Available;
            consumedTicket.ConsumedBySessionId = null;
            consumedTicket.ConsumedByAttendanceId = null;
            consumedTicket.ConsumedAt = null;

            context.LearningTicketLedgers.Add(new LearningTicketLedger
            {
                Id = Guid.NewGuid(),
                StudentProfileId = registration.StudentProfileId,
                RegistrationId = registration.Id,
                LearningTicketItemId = consumedTicket.Id,
                SessionId = sessionId,
                AttendanceId = attendanceId,
                TransactionType = LearningTicketTransactionType.Refund,
                Quantity = 1,
                Reason = $"Rollback consumption for attendance {newStatus}",
                CreatedAt = now
            });
        }

        if (consumedTicket is not null)
        {
            registration.UsedSessions = Math.Max(0, registration.UsedSessions - 1);
            registration.RemainingSessions++;
            registration.UpdatedAt = now;
        }

        if (wasCompletedBySessionExhaustion)
        {
            var reopenedEnrollmentClassIds = await ReopenCompletedEnrollmentsAsync(
                registration,
                sessionDateTimeUtc,
                now,
                cancellationToken);
            impactedClassIds.UnionWith(reopenedEnrollmentClassIds);

            registration.Status = reopenedEnrollmentClassIds.Count > 0
                ? RegistrationStatus.Studying
                : ResolveRollbackStatus(registration);
        }

        return new AttendanceTransitionOutcome(
            impactedClassIds,
            consumedTicket is not null,
            consumedTicket is not null ? 1 : 0,
            registration.RemainingSessions,
            afterDecision.AdvanceLessonProgression,
            null,
            null);
    }

    private async Task<HashSet<Guid>> CompleteActiveEnrollmentsAsync(
        Registration registration,
        DateTime sessionDateTimeUtc,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var enrollments = await GetRelatedEnrollmentsAsync(
            registration,
            EnrollmentStatus.Active,
            includeClass: false,
            cancellationToken);

        if (enrollments.Count == 0)
        {
            return new HashSet<Guid>();
        }

        var cancelFromUtc = sessionDateTimeUtc.AddTicks(1);
        var impactedClassIds = new HashSet<Guid>();
        foreach (var enrollment in enrollments)
        {
            enrollment.Status = EnrollmentStatus.Completed;
            enrollment.UpdatedAt = now;
            impactedClassIds.Add(enrollment.ClassId);

            await studentSessionAssignmentService.CancelFutureAssignmentsForEnrollmentAsync(
                enrollment.Id,
                cancelFromUtc,
                cancellationToken);
        }

        return impactedClassIds;
    }

    private async Task<HashSet<Guid>> ReopenCompletedEnrollmentsAsync(
        Registration registration,
        DateTime sessionDateTimeUtc,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var enrollments = await GetRelatedEnrollmentsAsync(
            registration,
            EnrollmentStatus.Completed,
            includeClass: true,
            cancellationToken);

        if (enrollments.Count == 0)
        {
            return new HashSet<Guid>();
        }

        var restoreFromDate = VietnamTime.ToVietnamDateOnly(sessionDateTimeUtc);
        var impactedClassIds = new HashSet<Guid>();

        foreach (var enrollment in enrollments.Where(IsClassReopenAllowed))
        {
            enrollment.Status = EnrollmentStatus.Active;
            enrollment.UpdatedAt = now;
            impactedClassIds.Add(enrollment.ClassId);

            await studentSessionAssignmentService.RestoreAssignmentsForEnrollmentAsync(
                enrollment,
                restoreFromDate,
                cancellationToken);
        }

        return impactedClassIds;
    }

    private async Task<List<ClassEnrollment>> GetRelatedEnrollmentsAsync(
        Registration registration,
        EnrollmentStatus status,
        bool includeClass,
        CancellationToken cancellationToken)
    {
        var query = context.ClassEnrollments
            .Where(enrollment =>
                enrollment.StudentProfileId == registration.StudentProfileId &&
                enrollment.Status == status &&
                (enrollment.RegistrationId == registration.Id ||
                 (!enrollment.RegistrationId.HasValue &&
                  ((registration.ClassId.HasValue && enrollment.ClassId == registration.ClassId.Value) ||
                   (registration.SecondaryClassId.HasValue && enrollment.ClassId == registration.SecondaryClassId.Value)))));

        if (includeClass)
        {
            query = query.Include(enrollment => enrollment.Class);
        }

        return await query.ToListAsync(cancellationToken);
    }

    private static void CollectRegistrationClassIds(Registration registration, ISet<Guid> impactedClassIds)
    {
        if (registration.ClassId.HasValue)
        {
            impactedClassIds.Add(registration.ClassId.Value);
        }

        if (registration.SecondaryClassId.HasValue)
        {
            impactedClassIds.Add(registration.SecondaryClassId.Value);
        }
    }

    private static RegistrationStatus ResolveRollbackStatus(Registration registration)
    {
        var resolvedStatus = RegistrationTrackHelper.ResolveStatus(registration);
        return resolvedStatus == RegistrationStatus.Studying
            ? RegistrationStatus.ClassAssigned
            : resolvedStatus;
    }

    private static bool IsClassReopenAllowed(ClassEnrollment enrollment)
    {
        return enrollment.Class.Status is not ClassStatus.Closed
            and not ClassStatus.Cancelled
            and not ClassStatus.Suspended;
    }
}
