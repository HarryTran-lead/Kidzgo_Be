using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Registrations.Notifications;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class RegistrationSessionConsumptionService(IDbContext context)
{
    public async Task ApplyAttendanceTransitionAsync(
        Guid? registrationId,
        AttendanceStatus? previousStatus,
        AbsenceType? previousAbsenceType,
        AttendanceStatus newStatus,
        AbsenceType? newAbsenceType,
        CancellationToken cancellationToken)
    {
        if (!registrationId.HasValue)
        {
            return;
        }

        var consumedBefore = ConsumesRegularSession(previousStatus, previousAbsenceType);
        var consumedAfter = ConsumesRegularSession(newStatus, newAbsenceType);

        if (consumedBefore == consumedAfter)
        {
            return;
        }

        var registration = await context.Registrations
            .FirstOrDefaultAsync(r => r.Id == registrationId.Value, cancellationToken);

        if (registration is null)
        {
            return;
        }

        var now = VietnamTime.UtcNow();

        if (consumedAfter)
        {
            if (registration.RemainingSessions <= 0)
            {
                return;
            }

            registration.UsedSessions++;
            registration.RemainingSessions--;
            registration.UpdatedAt = now;

            if (registration.RemainingSessions == 0)
            {
                registration.Status = RegistrationStatus.Completed;
            }

            await LowRemainingSessionsNotificationHelper.QueueAsync(context, registration, cancellationToken);

            if (registration.ClassId.HasValue)
            {
                await CheckAndUpdateClassCompletionAsync(registration.ClassId.Value, cancellationToken);
            }

            if (registration.SecondaryClassId.HasValue)
            {
                await CheckAndUpdateClassCompletionAsync(registration.SecondaryClassId.Value, cancellationToken);
            }

            return;
        }

        registration.UsedSessions = Math.Max(0, registration.UsedSessions - 1);
        registration.RemainingSessions++;
        registration.UpdatedAt = now;

        if (registration.Status == RegistrationStatus.Completed)
        {
            registration.Status = RegistrationStatus.Studying;
        }
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

    private async Task CheckAndUpdateClassCompletionAsync(Guid classId, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .Include(c => c.ClassEnrollments)
            .FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);

        if (classEntity == null)
        {
            return;
        }

        var activeRegistrations = await context.Registrations
            .Where(r => (r.ClassId == classId || r.SecondaryClassId == classId)
                && r.Status == RegistrationStatus.Studying)
            .ToListAsync(cancellationToken);

        if (activeRegistrations.Count != 0)
        {
            return;
        }

        var activeEnrollments = classEntity.ClassEnrollments
            .Count(ce => ce.Status == EnrollmentStatus.Active);

        if (activeEnrollments == 0 && classEntity.Status == ClassStatus.Active)
        {
            classEntity.Status = ClassStatus.Completed;
            classEntity.UpdatedAt = VietnamTime.UtcNow();
        }
    }
}
