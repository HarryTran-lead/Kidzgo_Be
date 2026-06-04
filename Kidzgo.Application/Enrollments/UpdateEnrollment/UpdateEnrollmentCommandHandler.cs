using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Enrollments.UpdateEnrollment;

public sealed class UpdateEnrollmentCommandHandler(
    IDbContext context,
    StudentSessionAssignmentService studentSessionAssignmentService,
    StudentEnrollmentScheduleConflictService studentEnrollmentScheduleConflictService,
    TicketCompatibilityService ticketCompatibilityService
) : ICommandHandler<UpdateEnrollmentCommand, UpdateEnrollmentResponse>
{
    public async Task<Result<UpdateEnrollmentResponse>> Handle(UpdateEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var enrollment = await context.ClassEnrollments
            .Include(e => e.Class)
                .ThenInclude(c => c.Program)
            .Include(e => e.StudentProfile)
            .Include(e => e.TuitionPlan)
            .Include(e => e.ScheduleSegments)
            .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure<UpdateEnrollmentResponse>(
                EnrollmentErrors.NotFound(command.Id));
        }

        // Update EnrollDate if provided
        if (command.EnrollDate.HasValue)
        {
            enrollment.EnrollDate = command.EnrollDate.Value;
        }

        if (command.Track is not null)
        {
            enrollment.Track = RegistrationTrackHelper.ToTrackType(command.Track);
        }

        if (command.ClearWeeklyPattern)
        {
            enrollment.SessionSelectionPattern = null;
        }
        else if (command.WeeklyPattern is not null)
        {
            var weeklyPatternResult = SchedulePatternSupport.NormalizeWeeklyPatternJson(
                command.WeeklyPattern,
                requireValue: false);
            if (weeklyPatternResult.IsFailure)
            {
                return Result.Failure<UpdateEnrollmentResponse>(weeklyPatternResult.Error);
            }

            var selectionPatternValidation = await studentSessionAssignmentService
                .ValidateSelectionPatternAsync(enrollment.Class, weeklyPatternResult.Value, cancellationToken);
            if (selectionPatternValidation.IsFailure)
            {
                return Result.Failure<UpdateEnrollmentResponse>(selectionPatternValidation.Error);
            }

            enrollment.SessionSelectionPattern = weeklyPatternResult.Value;
        }

        if (command.ClearWeeklyPattern || command.WeeklyPattern is not null)
        {
            var now = VietnamTime.UtcNow();
            var today = VietnamTime.TodayDateOnly();
            var effectiveDate = enrollment.EnrollDate > today ? enrollment.EnrollDate : today;

            if (enrollment.Class.Program.IsSupplementary)
            {
                var activeSegment = enrollment.ScheduleSegments
                    .Where(s => s.EffectiveFrom <= effectiveDate && (!s.EffectiveTo.HasValue || s.EffectiveTo.Value >= effectiveDate))
                    .OrderByDescending(s => s.EffectiveFrom)
                    .FirstOrDefault();

                if (activeSegment != null)
                {
                    if (activeSegment.EffectiveFrom == effectiveDate)
                    {
                        activeSegment.SessionSelectionPattern = enrollment.SessionSelectionPattern;
                        activeSegment.UpdatedAt = now;
                    }
                    else
                    {
                        activeSegment.EffectiveTo = effectiveDate.AddDays(-1);
                        activeSegment.UpdatedAt = now;

                        context.ClassEnrollmentScheduleSegments.Add(new ClassEnrollmentScheduleSegment
                        {
                            Id = Guid.NewGuid(),
                            ClassEnrollmentId = enrollment.Id,
                            EffectiveFrom = effectiveDate,
                            SessionSelectionPattern = enrollment.SessionSelectionPattern,
                            CreatedAt = now,
                            UpdatedAt = now
                        });
                    }
                }
            }
        }

        // Update TuitionPlan if provided
        if (command.TuitionPlanId.HasValue)
        {
            var tuitionPlan = await context.TuitionPlans
                .FirstOrDefaultAsync(tp => tp.Id == command.TuitionPlanId.Value, cancellationToken);

            if (tuitionPlan is null)
            {
                return Result.Failure<UpdateEnrollmentResponse>(
                    EnrollmentErrors.TuitionPlanNotFound);
            }

            if (!tuitionPlan.IsActive || tuitionPlan.IsDeleted)
            {
                return Result.Failure<UpdateEnrollmentResponse>(
                    EnrollmentErrors.TuitionPlanNotAvailable);
            }

            // Check if tuition plan belongs to the same program as the class
            if (tuitionPlan.ProgramId != enrollment.Class.ProgramId)
            {
                return Result.Failure<UpdateEnrollmentResponse>(
                    EnrollmentErrors.TuitionPlanProgramMismatch);
            }

            if (await IsExplicitlyIncompatibleAsync(
                    tuitionPlan.LearningTicketTypeId,
                    enrollment.Class.SlotTypeId,
                    cancellationToken))
            {
                return Result.Failure<UpdateEnrollmentResponse>(
                    EnrollmentErrors.TuitionPlanIncompatibleWithClassSlotType(
                        tuitionPlan.LearningTicketTypeId,
                        enrollment.Class.SlotTypeId));
            }

            enrollment.TuitionPlanId = command.TuitionPlanId.Value;
            enrollment.TuitionPlan = tuitionPlan;
        }

        var conflictResult = await studentEnrollmentScheduleConflictService.EnsureNoConflictsAsync(
            enrollment.StudentProfileId,
            enrollment.ClassId,
            enrollment.EnrollDate,
            enrollment.SessionSelectionPattern,
            cancellationToken,
            excludeEnrollmentId: enrollment.Id,
            excludeLegacyClassId: enrollment.ClassId);
        if (conflictResult.IsFailure)
        {
            return Result.Failure<UpdateEnrollmentResponse>(conflictResult.Error);
        }

        enrollment.UpdatedAt = VietnamTime.UtcNow();
        await studentSessionAssignmentService.SyncAssignmentsForEnrollmentAsync(enrollment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // Navigation properties are already loaded from the initial query
        return new UpdateEnrollmentResponse
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

    private async Task<bool> IsExplicitlyIncompatibleAsync(
        Guid? learningTicketTypeId,
        Guid? slotTypeId,
        CancellationToken cancellationToken)
    {
        var evaluation = await ticketCompatibilityService.EvaluateAsync(
            learningTicketTypeId,
            slotTypeId,
            cancellationToken);
        return !evaluation.IsCompatible;
    }
}
