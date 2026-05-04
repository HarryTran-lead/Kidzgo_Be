using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Enrollments.AssignTuitionPlan;

public sealed class AssignTuitionPlanCommandHandler(
    IDbContext context
) : ICommandHandler<AssignTuitionPlanCommand, AssignTuitionPlanResponse>
{
    public async Task<Result<AssignTuitionPlanResponse>> Handle(AssignTuitionPlanCommand command, CancellationToken cancellationToken)
    {
        var enrollment = await context.ClassEnrollments
            .Include(e => e.Class)
                .ThenInclude(c => c.Program)
            .Include(e => e.StudentProfile)
            .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure<AssignTuitionPlanResponse>(
                EnrollmentErrors.NotFound(command.Id));
        }

        var tuitionPlan = await context.TuitionPlans
            .FirstOrDefaultAsync(tp => tp.Id == command.TuitionPlanId, cancellationToken);

        if (tuitionPlan is null)
        {
            return Result.Failure<AssignTuitionPlanResponse>(
                EnrollmentErrors.TuitionPlanNotFound);
        }

        if (!tuitionPlan.IsActive || tuitionPlan.IsDeleted)
        {
            return Result.Failure<AssignTuitionPlanResponse>(
                EnrollmentErrors.TuitionPlanNotAvailable);
        }

        // Check if tuition plan belongs to the same program as the class
        if (tuitionPlan.ProgramId != enrollment.Class.ProgramId)
        {
            return Result.Failure<AssignTuitionPlanResponse>(
                EnrollmentErrors.TuitionPlanProgramMismatch);
        }

        enrollment.TuitionPlanId = command.TuitionPlanId;
        enrollment.UpdatedAt = VietnamTime.UtcNow();
        await context.SaveChangesAsync(cancellationToken);

        // Query enrollment with navigation properties for response
        var enrollmentWithNav = await context.ClassEnrollments
            .Include(e => e.Class)
            .Include(e => e.StudentProfile)
            .Include(e => e.TuitionPlan)
            .FirstOrDefaultAsync(e => e.Id == enrollment.Id, cancellationToken);

        return new AssignTuitionPlanResponse
        {
            Id = enrollmentWithNav!.Id,
            ClassId = enrollmentWithNav.ClassId,
            ClassCode = enrollmentWithNav.Class.Code,
            ClassTitle = enrollmentWithNav.Class.Title,
            StudentProfileId = enrollmentWithNav.StudentProfileId,
            StudentName = enrollmentWithNav.StudentProfile.DisplayName,
            EnrollDate = enrollmentWithNav.EnrollDate,
            Status = enrollmentWithNav.Status,
            TuitionPlanId = enrollmentWithNav.TuitionPlanId,
            TuitionPlanName = enrollmentWithNav.TuitionPlan?.Name
        };
    }
}

