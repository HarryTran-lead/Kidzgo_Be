using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Enrollments.PauseEnrollment;

public sealed class PauseEnrollmentCommandHandler(
    IDbContext context,
    ClassLifecycleService classLifecycleService,
    StudentSessionAssignmentService studentSessionAssignmentService
) : ICommandHandler<PauseEnrollmentCommand, PauseEnrollmentResponse>
{
    public async Task<Result<PauseEnrollmentResponse>> Handle(PauseEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var enrollment = await context.ClassEnrollments
            .Include(e => e.Class)
            .Include(e => e.StudentProfile)
            .Include(e => e.TuitionPlan)
            .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure<PauseEnrollmentResponse>(
                EnrollmentErrors.NotFound(command.Id));
        }

        if (enrollment.Status != EnrollmentStatus.Active)
        {
            return Result.Failure<PauseEnrollmentResponse>(
                EnrollmentErrors.InvalidStatus);
        }

        var now = VietnamTime.UtcNow();
        enrollment.Status = EnrollmentStatus.Paused;
        enrollment.UpdatedAt = now;
        await studentSessionAssignmentService.CancelFutureAssignmentsForEnrollmentAsync(
            enrollment.Id,
            now,
            cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await classLifecycleService.RecalculateClassLifecycleAsync(enrollment.ClassId, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new PauseEnrollmentResponse
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

