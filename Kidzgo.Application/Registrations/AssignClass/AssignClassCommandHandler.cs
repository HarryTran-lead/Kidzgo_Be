using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Classes;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.AssignClass.Handler;

public sealed class AssignClassCommandHandler(
    IDbContext context,
    StudentSessionAssignmentService studentSessionAssignmentService,
    StudentEnrollmentScheduleConflictService studentEnrollmentScheduleConflictService
) : ICommandHandler<AssignClassCommand, AssignClassResponse>
{
    public async Task<Result<AssignClassResponse>> Handle(
        AssignClassCommand command,
        CancellationToken cancellationToken)
    {
        var now = VietnamTime.UtcNow();
        var track = RegistrationTrackHelper.NormalizeTrack(command.Track);
        var isSecondaryTrack = track == RegistrationTrackHelper.SecondaryTrack;

        if (!RegistrationTrackHelper.TryParseEntryType(command.EntryType, out var entryType))
        {
            return Result.Failure<AssignClassResponse>(
                RegistrationErrors.InvalidEntryType(command.EntryType));
        }

        var registration = await context.Registrations
            .Include(r => r.Program)
            .Include(r => r.TuitionPlan)
            .FirstOrDefaultAsync(r => r.Id == command.RegistrationId, cancellationToken);

        if (registration == null)
        {
            return Result.Failure<AssignClassResponse>(RegistrationErrors.NotFound(command.RegistrationId));
        }

        if (registration.Status == RegistrationStatus.Completed ||
            registration.Status == RegistrationStatus.Cancelled)
        {
            return Result.Failure<AssignClassResponse>(
                RegistrationErrors.InvalidStatus(registration.Status.ToString(), "assign-class"));
        }

        if (isSecondaryTrack && !registration.SecondaryLevelId.HasValue)
        {
            return Result.Failure<AssignClassResponse>(
                Error.Validation(
                    "Registration.SecondaryLevelMissing",
                    "Registration does not have a secondary level to assign"));
        }

        var currentEntryType = isSecondaryTrack ? registration.SecondaryEntryType : registration.EntryType;
        var currentClassId = isSecondaryTrack ? registration.SecondaryClassId : registration.ClassId;
        var targetProgramId = registration.ProgramId;

        if (currentEntryType != null &&
            currentEntryType != EntryType.Wait &&
            entryType == EntryType.Wait)
        {
            return Result.Failure<AssignClassResponse>(
                RegistrationErrors.InvalidStatus(
                    $"Cannot change track '{track}' back to 'Wait' after enrollment has been created.",
                    "assign-class"));
        }

        if (currentClassId.HasValue && entryType != EntryType.Wait)
        {
            return Result.Failure<AssignClassResponse>(
                Error.Validation(
                    "Registration.ClassAlreadyAssigned",
                    $"Track '{track}' already has a class assigned. Use transfer-class instead."));
        }

        var isWait = entryType == EntryType.Wait;
        var classId = command.ClassId;
        var currentActiveEnrollmentCount = 0;
        var today = VietnamTime.ToVietnamDateOnly(now);
        var assignmentStartDate = command.FirstStudyDate ?? today;
        DateTime? firstStudySessionAt = null;
        DateOnly? resolvedFirstStudyDate = null;
        var weeklyPatternResult = SchedulePatternSupport.NormalizeWeeklyPatternJson(
            command.WeeklyPattern,
            requireValue: false);
        if (weeklyPatternResult.IsFailure)
        {
            return Result.Failure<AssignClassResponse>(weeklyPatternResult.Error);
        }

        var sessionSelectionPattern = weeklyPatternResult.Value;

        if (isWait && command.FirstStudyDate.HasValue)
        {
            return Result.Failure<AssignClassResponse>(
                Error.Validation(
                    "Registration.FirstStudyDateNotAllowed",
                    "FirstStudyDate can only be used when assigning a class."));
        }

        if (!isWait && !classId.HasValue)
        {
            return Result.Failure<AssignClassResponse>(
                Error.Validation("Registration.ClassIdRequired", "ClassId is required when assigning a class"));
        }

        var classEntity = !isWait && classId.HasValue
            ? await context.Classes
                .Include(c => c.ClassEnrollments)
                .FirstOrDefaultAsync(c => c.Id == classId.Value, cancellationToken)
            : null;

        if (classEntity == null && !isWait)
        {
            return Result.Failure<AssignClassResponse>(RegistrationErrors.ClassNotFound(classId ?? Guid.Empty));
        }

        if (classEntity != null &&
            await IsExplicitlyIncompatibleAsync(
                registration.TuitionPlan?.LearningTicketTypeId,
                classEntity.SlotTypeId,
                cancellationToken))
        {
            return Result.Failure<AssignClassResponse>(
                RegistrationErrors.TicketTypeIncompatibleWithClassSlotType(
                    registration.TuitionPlan?.LearningTicketTypeId,
                    classEntity.SlotTypeId));
        }

        if (classEntity != null && classEntity.BranchId != registration.BranchId)
        {
            return Result.Failure<AssignClassResponse>(
                RegistrationErrors.ClassNotMatchingBranch(classEntity.Id, registration.BranchId));
        }

        if (classEntity != null && classEntity.ProgramId != targetProgramId)
        {
            return Result.Failure<AssignClassResponse>(
                RegistrationErrors.ClassNotMatchingProgram(classEntity.Id, targetProgramId));
        }

        if (classEntity != null)
        {
            var selectionPatternValidation = await studentSessionAssignmentService
                .ValidateSelectionPatternAsync(classEntity, sessionSelectionPattern, cancellationToken);
            if (selectionPatternValidation.IsFailure)
            {
                return Result.Failure<AssignClassResponse>(selectionPatternValidation.Error);
            }

            if (command.FirstStudyDate.HasValue)
            {
                if (command.FirstStudyDate.Value < today)
                {
                    return Result.Failure<AssignClassResponse>(
                        Error.Validation(
                            "Registration.FirstStudyDateInPast",
                            "FirstStudyDate cannot be earlier than today."));
                }

                if (command.FirstStudyDate.Value < classEntity.StartDate)
                {
                    return Result.Failure<AssignClassResponse>(
                        Error.Validation(
                            "Registration.FirstStudyDateBeforeClassStart",
                            "FirstStudyDate cannot be earlier than the class start date."));
                }

                if (classEntity.EndDate.HasValue &&
                    command.FirstStudyDate.Value > classEntity.EndDate.Value)
                {
                    return Result.Failure<AssignClassResponse>(
                        Error.Validation(
                            "Registration.FirstStudyDateAfterClassEnd",
                            "FirstStudyDate cannot be later than the class end date."));
                }
            }

            var candidateSlots = await studentEnrollmentScheduleConflictService.GetCandidateSlotsAsync(
                classEntity.Id,
                assignmentStartDate,
                sessionSelectionPattern,
                cancellationToken);

            if (candidateSlots.Count > 0)
            {
                firstStudySessionAt = candidateSlots[0].Start;
                resolvedFirstStudyDate = VietnamTime.ToVietnamDateOnly(firstStudySessionAt.Value);
            }

            if (command.FirstStudyDate.HasValue &&
                !candidateSlots.Any(slot => VietnamTime.ToVietnamDateOnly(slot.Start) == command.FirstStudyDate.Value))
            {
                return Result.Failure<AssignClassResponse>(
                    Error.Validation(
                        "Registration.FirstStudyDateNoSession",
                        "FirstStudyDate must match an available class session for the selected class and schedule pattern."));
            }
        }

        if (classEntity != null)
        {
            currentActiveEnrollmentCount = classEntity.ClassEnrollments
                .Count(ce => ce.Status == EnrollmentStatus.Active);

            ClassCapacityStatusHelper.SyncAvailabilityStatus(classEntity, currentActiveEnrollmentCount, now);

            if (classEntity.Status == ClassStatus.Completed ||
                classEntity.Status == ClassStatus.Cancelled ||
                classEntity.Status == ClassStatus.Suspended)
            {
                return Result.Failure<AssignClassResponse>(
                    Error.Validation("ClassNotAvailable", $"Class is {classEntity.Status} and cannot accept new students"));
            }

            if (currentActiveEnrollmentCount >= classEntity.Capacity)
            {
                return Result.Failure<AssignClassResponse>(RegistrationErrors.ClassFull(classEntity.Id));
            }

            var alreadyEnrolled = await context.ClassEnrollments
                .AnyAsync(
                    ce => ce.ClassId == classEntity.Id &&
                          ce.StudentProfileId == registration.StudentProfileId &&
                          (ce.Status == EnrollmentStatus.Active || ce.Status == EnrollmentStatus.Paused),
                    cancellationToken);

            if (alreadyEnrolled)
            {
                return Result.Failure<AssignClassResponse>(
                    Error.Conflict("AlreadyEnrolled", "Student is already enrolled in this class"));
            }

            var conflictResult = await studentEnrollmentScheduleConflictService.EnsureNoConflictsAsync(
                registration.StudentProfileId,
                classEntity.Id,
                assignmentStartDate,
                sessionSelectionPattern,
                cancellationToken);
            if (conflictResult.IsFailure)
            {
                return Result.Failure<AssignClassResponse>(conflictResult.Error);
            }
        }

        string? warningMessage = null;

        if (entryType != EntryType.Wait)
        {
            var enrollment = new ClassEnrollment
            {
                Id = Guid.NewGuid(),
                ClassId = classEntity!.Id,
                StudentProfileId = registration.StudentProfileId,
                EnrollDate = assignmentStartDate,
                Status = EnrollmentStatus.Active,
                TuitionPlanId = registration.TuitionPlanId,
                RegistrationId = registration.Id,
                Track = RegistrationTrackHelper.ToTrackType(track),
                SessionSelectionPattern = sessionSelectionPattern,
                CreatedAt = now,
                UpdatedAt = now
            };

            context.ClassEnrollments.Add(enrollment);

            var targetProgram = registration.Program;
            if (targetProgram?.IsSupplementary == true)
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

            if (classEntity!.Status == ClassStatus.Active)
            {
                warningMessage = "Class da bat dau. Hoc vien se tham gia giua chung.";
            }

            var previousClassStatus = classEntity.Status;
            ClassCapacityStatusHelper.SyncAvailabilityStatus(classEntity, currentActiveEnrollmentCount + 1, now);
            if (classEntity.Status == ClassStatus.Full && previousClassStatus != ClassStatus.Full)
            {
                warningMessage = string.IsNullOrEmpty(warningMessage)
                    ? "Class da day sau khi them hoc vien nay."
                    : warningMessage + " Class da day sau khi them hoc vien nay.";
            }
        }
        else
        {
            warningMessage = "Hoc vien da duoc them vao danh sach cho lop moi.";
        }

        if (isSecondaryTrack)
        {
            registration.SecondaryClassId = entryType == EntryType.Wait ? null : classEntity?.Id;
            registration.SecondaryClassAssignedDate = entryType == EntryType.Wait ? null : now;
            registration.SecondaryEntryType = entryType;
        }
        else
        {
            registration.ClassId = entryType == EntryType.Wait ? null : classEntity?.Id;
            registration.ClassAssignedDate = entryType == EntryType.Wait ? null : now;
            registration.EntryType = entryType;
        }

        registration.Status = RegistrationTrackHelper.ResolveStatus(registration);
        if (entryType == EntryType.Immediate && !registration.ActualStartDate.HasValue)
        {
            registration.ActualStartDate = firstStudySessionAt
                ?? VietnamTime.TreatAsVietnamLocal(assignmentStartDate.ToDateTime(TimeOnly.MinValue));
        }

        registration.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return new AssignClassResponse
        {
            RegistrationId = registration.Id,
            RegistrationStatus = registration.Status.ToString(),
            ClassId = classEntity?.Id ?? Guid.Empty,
            ClassCode = classEntity?.Code ?? string.Empty,
            ClassTitle = classEntity?.Title ?? string.Empty,
            Track = track,
            EntryType = RegistrationTrackHelper.ToApiEntryType(entryType) ?? nameof(EntryType.Immediate),
            ClassAssignedDate = isSecondaryTrack
                ? registration.SecondaryClassAssignedDate ?? now
                : registration.ClassAssignedDate ?? now,
            FirstStudyDate = resolvedFirstStudyDate,
            FirstStudySessionAt = firstStudySessionAt,
            WarningMessage = warningMessage
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
