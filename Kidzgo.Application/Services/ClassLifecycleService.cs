using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Classes;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Registrations;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class ClassLifecycleService(IDbContext context)
{
    public async Task RecalculateClassLifecycleAsync(Guid classId, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .Include(c => c.ClassEnrollments)
            .FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);

        if (classEntity is null ||
            classEntity.Status is ClassStatus.Closed or ClassStatus.Cancelled or ClassStatus.Suspended)
        {
            return;
        }

        var now = VietnamTime.UtcNow();
        var activeEnrollmentCount = classEntity.ClassEnrollments
            .Count(enrollment => enrollment.Status == EnrollmentStatus.Active);

        var hasStudyingRegistrations = await context.Registrations
            .AsNoTracking()
            .AnyAsync(registration =>
                    (registration.ClassId == classId || registration.SecondaryClassId == classId) &&
                    registration.Status == RegistrationStatus.Studying,
                cancellationToken);

        if (!hasStudyingRegistrations && activeEnrollmentCount == 0)
        {
            if (classEntity.Status is ClassStatus.Active or ClassStatus.Full)
            {
                classEntity.Status = ClassStatus.Completed;
                classEntity.UpdatedAt = now;
            }

            return;
        }

        if (classEntity.Status == ClassStatus.Completed)
        {
            var reopenedStatus = ClassCapacityStatusHelper.ResolveAvailableStatus(
                classEntity,
                activeEnrollmentCount,
                now);

            if (reopenedStatus != classEntity.Status)
            {
                classEntity.Status = reopenedStatus;
                classEntity.UpdatedAt = now;
            }

            return;
        }

        ClassCapacityStatusHelper.SyncAvailabilityStatus(classEntity, activeEnrollmentCount, now);
    }
}
