using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Classes;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Enrollments.ReactivateEnrollment;

public sealed class ReactivateEnrollmentCommandHandler(
    IDbContext context,
    ClassLifecycleService classLifecycleService,
    StudentSessionAssignmentService studentSessionAssignmentService,
    StudentEnrollmentScheduleConflictService studentEnrollmentScheduleConflictService
) : ICommandHandler<ReactivateEnrollmentCommand, ReactivateEnrollmentResponse>
{
    public async Task<Result<ReactivateEnrollmentResponse>> Handle(ReactivateEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var enrollment = await context.ClassEnrollments
            .Include(e => e.Class)
            .Include(e => e.StudentProfile)
            .Include(e => e.TuitionPlan)
            .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure<ReactivateEnrollmentResponse>(
                EnrollmentErrors.NotFound(command.Id));
        }

        if (enrollment.Status == EnrollmentStatus.Active)
        {
            return Result.Failure<ReactivateEnrollmentResponse>(
                EnrollmentErrors.AlreadyActive);
        }

        if (enrollment.Status == EnrollmentStatus.Dropped)
        {
            return Result.Failure<ReactivateEnrollmentResponse>(
                EnrollmentErrors.CannotReactivateDropped);
        }

        if (enrollment.Status == EnrollmentStatus.Completed)
        {
            return Result.Failure<ReactivateEnrollmentResponse>(
                EnrollmentErrors.CannotReactivateCompleted);
        }
        
        var now = VietnamTime.UtcNow();

        // Check class capacity
        int currentEnrollmentCount = await context.ClassEnrollments
            .CountAsync(ce => ce.ClassId == enrollment.ClassId && ce.Status == EnrollmentStatus.Active, cancellationToken);

        ClassCapacityStatusHelper.SyncAvailabilityStatus(enrollment.Class, currentEnrollmentCount, now);

        // Check if class is still available
        if (enrollment.Class.Status != ClassStatus.Active &&
            enrollment.Class.Status != ClassStatus.Planned &&
            enrollment.Class.Status != ClassStatus.Recruiting)
        {
            return Result.Failure<ReactivateEnrollmentResponse>(
                EnrollmentErrors.ClassNotAvailable);
        }

        if (currentEnrollmentCount >= enrollment.Class.Capacity)
        {
            return Result.Failure<ReactivateEnrollmentResponse>(
                EnrollmentErrors.ClassFull);
        }

        var conflictResult = await studentEnrollmentScheduleConflictService.EnsureNoConflictsAsync(
            enrollment.StudentProfileId,
            enrollment.ClassId,
            enrollment.EnrollDate,
            enrollment.SessionSelectionPattern,
            cancellationToken,
            excludeEnrollmentId: enrollment.Id);
        if (conflictResult.IsFailure)
        {
            return Result.Failure<ReactivateEnrollmentResponse>(conflictResult.Error);
        }

        enrollment.Status = EnrollmentStatus.Active;
        enrollment.UpdatedAt = now;
        await studentSessionAssignmentService.SyncAssignmentsForEnrollmentAsync(enrollment, cancellationToken);
        ClassCapacityStatusHelper.SyncAvailabilityStatus(enrollment.Class, currentEnrollmentCount + 1, now);
        await context.SaveChangesAsync(cancellationToken);
        await classLifecycleService.RecalculateClassLifecycleAsync(enrollment.ClassId, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new ReactivateEnrollmentResponse
        {
            Id = enrollment.Id,
            ClassId = enrollment.ClassId,
            ClassCode = enrollment.Class.Code,
            ClassTitle = enrollment.Class.Title,
            StudentProfileId = enrollment.StudentProfileId,
            StudentName = enrollment.StudentProfile.DisplayName,
            EnrollDate = enrollment.EnrollDate,
            Status = enrollment.Status,
            TuitionPlanId = enrollment.TuitionPlanId,
            TuitionPlanName = enrollment.TuitionPlan?.Name
        };
    }
}

