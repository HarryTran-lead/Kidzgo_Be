using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions;
using Kidzgo.Domain.ProgramProgressions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.CreateProgramProgressionSchedule;

public sealed class CreateProgramProgressionScheduleCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ProgramProgressionScheduleNotificationService notificationService)
    : ICommandHandler<CreateProgramProgressionScheduleCommand, ProgramProgressionScheduleDto>
{
    public async Task<Result<ProgramProgressionScheduleDto>> Handle(
        CreateProgramProgressionScheduleCommand command,
        CancellationToken cancellationToken)
    {
        var sourceClass = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.SourceClassId, cancellationToken);

        if (sourceClass is null)
        {
            return Result.Failure<ProgramProgressionScheduleDto>(
                ProgramProgressionErrors.SourceClassNotFound(command.SourceClassId));
        }

        var hasActiveRule = await context.ProgramProgressionRules
            .AsNoTracking()
            .AnyAsync(rule => rule.SourceProgramId == sourceClass.ProgramId && rule.IsActive, cancellationToken);

        if (!hasActiveRule)
        {
            return Result.Failure<ProgramProgressionScheduleDto>(
                ProgramProgressionErrors.NoActiveRuleForProgram(sourceClass.ProgramId));
        }

        if (!command.AssignedTeacherUserId.HasValue)
        {
            return Result.Failure<ProgramProgressionScheduleDto>(ProgramProgressionErrors.AssignedTeacherRequired);
        }

        var duration = ProgramProgressionScheduleAvailability.NormalizeDuration(command.DurationMinutes);
        if (duration <= 0)
        {
            return Result.Failure<ProgramProgressionScheduleDto>(ProgramProgressionErrors.InvalidScheduleDuration);
        }

        var effectiveRoomId = command.RoomId ?? sourceClass.RoomId;

        var availability = await ProgramProgressionScheduleAvailability.EnsureScheduleAvailableAsync(
            context,
            sourceClass,
            command.AssignedTeacherUserId.Value,
            effectiveRoomId,
            command.ScheduledAt,
            duration,
            excludeScheduleId: null,
            cancellationToken);

        if (availability.IsFailure)
        {
            return Result.Failure<ProgramProgressionScheduleDto>(availability.Error);
        }

        var requestedStudentIds = command.StudentProfileIds?
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var eligibleEnrollmentsQuery = context.ClassEnrollments
            .Include(enrollment => enrollment.StudentProfile)
            .Include(enrollment => enrollment.Registration)
            .Where(enrollment =>
                enrollment.ClassId == sourceClass.Id &&
                enrollment.RegistrationId.HasValue &&
                (enrollment.Status == EnrollmentStatus.Active || enrollment.Status == EnrollmentStatus.Completed) &&
                enrollment.Registration != null &&
                enrollment.Registration.Status != Domain.Registrations.RegistrationStatus.Cancelled);

        if (requestedStudentIds is { Count: > 0 })
        {
            eligibleEnrollmentsQuery = eligibleEnrollmentsQuery
                .Where(enrollment => requestedStudentIds.Contains(enrollment.StudentProfileId));
        }

        var eligibleEnrollments = await eligibleEnrollmentsQuery
            .OrderBy(enrollment => enrollment.StudentProfile.DisplayName)
            .ToListAsync(cancellationToken);

        if (eligibleEnrollments.Count == 0)
        {
            return Result.Failure<ProgramProgressionScheduleDto>(
                ProgramProgressionErrors.ScheduleHasNoEligibleStudents(sourceClass.Id));
        }

        if (requestedStudentIds is { Count: > 0 })
        {
            var foundStudentIds = eligibleEnrollments
                .Select(enrollment => enrollment.StudentProfileId)
                .ToHashSet();

            var missingStudentId = requestedStudentIds
                .FirstOrDefault(studentId => !foundStudentIds.Contains(studentId));

            if (missingStudentId != Guid.Empty)
            {
                return Result.Failure<ProgramProgressionScheduleDto>(
                    ProgramProgressionErrors.StudentNotInSourceClass(missingStudentId, sourceClass.Id));
            }
        }

        var registrationIds = eligibleEnrollments
            .Select(enrollment => enrollment.RegistrationId!.Value)
            .Distinct()
            .ToList();

        var activeScheduledRegistrationId = await context.ProgramProgressionScheduleParticipants
            .AsNoTracking()
            .Where(participant =>
                registrationIds.Contains(participant.SourceRegistrationId) &&
                participant.Status == ProgramProgressionScheduleParticipantStatus.Scheduled &&
                participant.Schedule.Status == ProgramProgressionScheduleStatus.Scheduled)
            .Select(participant => (Guid?)participant.SourceRegistrationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeScheduledRegistrationId.HasValue)
        {
            return Result.Failure<ProgramProgressionScheduleDto>(
                ProgramProgressionErrors.ActiveScheduleAlreadyExists(activeScheduledRegistrationId.Value));
        }

        var now = VietnamTime.UtcNow();
        var normalizedScheduledAt = VietnamTime.NormalizeToUtc(command.ScheduledAt);
        var schedule = new ProgramProgressionSchedule
        {
            Id = Guid.NewGuid(),
            SourceClassId = sourceClass.Id,
            SourceProgramId = sourceClass.ProgramId,
            BranchId = sourceClass.BranchId,
            ScheduledAt = normalizedScheduledAt,
            DurationMinutes = duration,
            RoomId = effectiveRoomId,
            AssignedTeacherUserId = command.AssignedTeacherUserId.Value,
            Status = ProgramProgressionScheduleStatus.Scheduled,
            Notes = string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim(),
            CreatedByUserId = userContext.UserId,
            CreatedAt = now,
            UpdatedAt = now,
            Participants = eligibleEnrollments
                .Select(enrollment => new ProgramProgressionScheduleParticipant
                {
                    Id = Guid.NewGuid(),
                    StudentProfileId = enrollment.StudentProfileId,
                    SourceRegistrationId = enrollment.RegistrationId!.Value,
                    SourceEnrollmentId = enrollment.Id,
                    Status = ProgramProgressionScheduleParticipantStatus.Scheduled,
                    CreatedAt = now,
                    UpdatedAt = now
                })
                .ToList()
        };

        context.ProgramProgressionSchedules.Add(schedule);
        await context.SaveChangesAsync(cancellationToken);

        var createdSchedule = await ProgramProgressionScheduleReadQuery.Build(context)
            .FirstAsync(s => s.Id == schedule.Id, cancellationToken);

        await notificationService.NotifyCreatedAsync(createdSchedule, cancellationToken);

        return Result.Success(createdSchedule.ToDto());
    }
}
