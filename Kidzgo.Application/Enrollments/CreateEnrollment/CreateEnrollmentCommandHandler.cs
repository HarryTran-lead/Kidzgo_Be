using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Services;
using Kidzgo.Application.Students.Shared;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Enrollments.CreateEnrollment;

public sealed class CreateEnrollmentCommandHandler(
    IDbContext context,
    StudentSessionAssignmentService studentSessionAssignmentService,
    StudentEnrollmentScheduleConflictService studentEnrollmentScheduleConflictService
) : ICommandHandler<CreateEnrollmentCommand, CreateEnrollmentResponse>
{
    public async Task<Result<CreateEnrollmentResponse>> Handle(CreateEnrollmentCommand command, CancellationToken cancellationToken)
    {
        // Check if class exists and is active
        var classEntity = await context.Classes
            .Include(c => c.ClassEnrollments)
            .Include(c => c.Program)
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<CreateEnrollmentResponse>(
                EnrollmentErrors.ClassNotFound);
        }

        if (classEntity.Status != ClassStatus.Active && classEntity.Status != ClassStatus.Planned)
        {
            return Result.Failure<CreateEnrollmentResponse>(
                EnrollmentErrors.ClassNotAvailable);
        }

        var studentProfile = await context.Profiles
            .FirstOrDefaultAsync(
                p => p.Id == command.StudentProfileId &&
                     p.ProfileType == Domain.Users.ProfileType.Student &&
                     p.IsActive &&
                     !p.IsDeleted,
                cancellationToken);

        if (studentProfile is null)
        {
            return Result.Failure<CreateEnrollmentResponse>(
                EnrollmentErrors.StudentNotFound);
        }

        var branchAccessResult = await StudentBranchAccessHelper.ValidateBranchAccessAsync(
            context,
            command.StudentProfileId,
            classEntity.BranchId,
            command.AllowCrossBranchEnrollment,
            cancellationToken);
        if (branchAccessResult.IsFailure)
        {
            return Result.Failure<CreateEnrollmentResponse>(branchAccessResult.Error);
        }

        bool alreadyEnrolled = await context.ClassEnrollments
            .AnyAsync(ce => ce.ClassId == command.ClassId 
                && ce.StudentProfileId == command.StudentProfileId 
                && (ce.Status == EnrollmentStatus.Active || ce.Status == EnrollmentStatus.Paused),
                cancellationToken);

        if (alreadyEnrolled)
        {
            return Result.Failure<CreateEnrollmentResponse>(
                EnrollmentErrors.AlreadyEnrolled);
        }

        // Check class capacity
        int currentEnrollmentCount = classEntity.ClassEnrollments
            .Count(ce => ce.Status == EnrollmentStatus.Active);

        if (currentEnrollmentCount >= classEntity.Capacity)
        {
            return Result.Failure<CreateEnrollmentResponse>(
                EnrollmentErrors.ClassFull);
        }

        var weeklyPatternResult = SchedulePatternSupport.NormalizeWeeklyPatternJson(
            command.WeeklyPattern,
            requireValue: false);
        if (weeklyPatternResult.IsFailure)
        {
            return Result.Failure<CreateEnrollmentResponse>(weeklyPatternResult.Error);
        }

        var sessionSelectionPattern = weeklyPatternResult.Value;

        var selectionPatternValidation = await studentSessionAssignmentService
            .ValidateSelectionPatternAsync(classEntity, sessionSelectionPattern, cancellationToken);
        if (selectionPatternValidation.IsFailure)
        {
            return Result.Failure<CreateEnrollmentResponse>(selectionPatternValidation.Error);
        }

        // Check if tuition plan exists and is active (if provided)
        if (command.TuitionPlanId.HasValue)
        {
            var tuitionPlan = await context.TuitionPlans
                .FirstOrDefaultAsync(tp => tp.Id == command.TuitionPlanId.Value, cancellationToken);

            if (tuitionPlan is null)
            {
                return Result.Failure<CreateEnrollmentResponse>(
                    EnrollmentErrors.TuitionPlanNotFound);
            }

            if (!tuitionPlan.IsActive || tuitionPlan.IsDeleted)
            {
                return Result.Failure<CreateEnrollmentResponse>(
                    EnrollmentErrors.TuitionPlanNotAvailable);
            }

            // Check if tuition plan belongs to the same program as the class
            if (tuitionPlan.ProgramId != classEntity.ProgramId)
            {
                return Result.Failure<CreateEnrollmentResponse>(
                    EnrollmentErrors.TuitionPlanProgramMismatch);
            }

            if (tuitionPlan.LevelId != classEntity.LevelId)
            {
                return Result.Failure<CreateEnrollmentResponse>(
                    EnrollmentErrors.TuitionPlanLevelMismatch);
            }

            if (tuitionPlan.ModuleId.HasValue && tuitionPlan.ModuleId != classEntity.StartModuleId)
            {
                return Result.Failure<CreateEnrollmentResponse>(
                    EnrollmentErrors.TuitionPlanModuleMismatch);
            }

            if (await IsExplicitlyIncompatibleAsync(
                    tuitionPlan.LearningTicketTypeId,
                    classEntity.SlotTypeId,
                    cancellationToken))
            {
                return Result.Failure<CreateEnrollmentResponse>(
                    EnrollmentErrors.TuitionPlanIncompatibleWithClassSlotType(
                        tuitionPlan.LearningTicketTypeId,
                        classEntity.SlotTypeId));
            }
        }

        var conflictResult = await studentEnrollmentScheduleConflictService.EnsureNoConflictsAsync(
            command.StudentProfileId,
            classEntity.Id,
            command.EnrollDate,
            sessionSelectionPattern,
            cancellationToken);
        if (conflictResult.IsFailure)
        {
            return Result.Failure<CreateEnrollmentResponse>(conflictResult.Error);
        }

        var now = VietnamTime.UtcNow();
        var enrollment = new ClassEnrollment
        {
            Id = Guid.NewGuid(),
            ClassId = command.ClassId,
            StudentProfileId = command.StudentProfileId,
            EnrollDate = command.EnrollDate,
            Status = EnrollmentStatus.Active,
            TuitionPlanId = command.TuitionPlanId,
            Track = RegistrationTrackHelper.ToTrackType(command.Track),
            SessionSelectionPattern = sessionSelectionPattern,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.ClassEnrollments.Add(enrollment);
        if (classEntity.Program.IsSupplementary)
        {
            context.ClassEnrollmentScheduleSegments.Add(new ClassEnrollmentScheduleSegment
            {
                Id = Guid.NewGuid(),
                ClassEnrollmentId = enrollment.Id,
                EffectiveFrom = enrollment.EnrollDate,
                SessionSelectionPattern = enrollment.SessionSelectionPattern,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await studentSessionAssignmentService.SyncAssignmentsForEnrollmentAsync(enrollment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // Query enrollment with navigation properties for response
        var enrollmentWithNav = await context.ClassEnrollments
            .Include(e => e.Class)
            .Include(e => e.StudentProfile)
            .Include(e => e.TuitionPlan)
            .FirstOrDefaultAsync(e => e.Id == enrollment.Id, cancellationToken);

        return new CreateEnrollmentResponse
        {
            Id = enrollmentWithNav!.Id,
            ClassId = enrollmentWithNav.ClassId,
            ClassCode = enrollmentWithNav.Class.Code,
            ClassTitle = enrollmentWithNav.Class.Title,
            StudentProfileId = enrollmentWithNav.StudentProfileId,
            StudentName = enrollmentWithNav.StudentProfile.DisplayName,
            StudentHomeBranchId = branchAccessResult.Value.State.HomeBranchId,
            StudentActiveBranchId = branchAccessResult.Value.State.ActiveBranchId,
            IsCrossBranchEnrollment = branchAccessResult.Value.IsCrossBranch,
            EnrollDate = enrollmentWithNav.EnrollDate,
            Status = enrollmentWithNav.Status,
            TuitionPlanId = enrollmentWithNav.TuitionPlanId,
            TuitionPlanName = enrollmentWithNav.TuitionPlan?.Name
        };
    }

    private async Task<bool> IsExplicitlyIncompatibleAsync(
        Guid? learningTicketTypeId,
        Guid? slotTypeId,
        CancellationToken cancellationToken)
    {
        if (!learningTicketTypeId.HasValue || !slotTypeId.HasValue)
        {
            return false;
        }

        var mapping = await context.TicketTypeCompatibilities
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.LearningTicketTypeId == learningTicketTypeId.Value &&
                     x.SlotTypeId == slotTypeId.Value,
                cancellationToken);

        return mapping is not null && !mapping.IsCompatible;
    }
}

