using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Registrations.Notifications;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class RegistrationSessionConsumptionService(
    IDbContext context,
    StudentSessionAssignmentService studentSessionAssignmentService)
{
    public async Task<IReadOnlyCollection<Guid>> ApplyAttendanceTransitionAsync(
        Guid? registrationId,
        AttendanceStatus? previousStatus,
        AbsenceType? previousAbsenceType,
        AttendanceStatus newStatus,
        AbsenceType? newAbsenceType,
        DateTime sessionDateTimeUtc,
        CancellationToken cancellationToken)
    {
        var impactedClassIds = new HashSet<Guid>();

        if (!registrationId.HasValue)
        {
            return impactedClassIds;
        }

        var consumedBefore = ConsumesRegularSession(previousStatus, previousAbsenceType);
        var consumedAfter = ConsumesRegularSession(newStatus, newAbsenceType);

        if (consumedBefore == consumedAfter)
        {
            return impactedClassIds;
        }

        var registration = await context.Registrations
            .FirstOrDefaultAsync(r => r.Id == registrationId.Value, cancellationToken);

        if (registration is null)
        {
            return impactedClassIds;
        }

        var now = VietnamTime.UtcNow();
        CollectRegistrationClassIds(registration, impactedClassIds);

        if (consumedAfter)
        {
            if (registration.RemainingSessions <= 0)
            {
                return impactedClassIds;
            }

            registration.UsedSessions++;
            registration.RemainingSessions--;
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

            return impactedClassIds;
        }

        var wasCompletedBySessionExhaustion =
            registration.Status == RegistrationStatus.Completed &&
            registration.RemainingSessions == 0;

        registration.UsedSessions = Math.Max(0, registration.UsedSessions - 1);
        registration.RemainingSessions++;
        registration.UpdatedAt = now;

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

        return impactedClassIds;
    }

    public static bool ConsumesRegularSession(
        AttendanceStatus? attendanceStatus,
        AbsenceType? absenceType)
    {
        return attendanceStatus switch
        {
            AttendanceStatus.Present => true,
            AttendanceStatus.Absent when absenceType == AbsenceType.NoNotice => true,
            _ => false
        };
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
