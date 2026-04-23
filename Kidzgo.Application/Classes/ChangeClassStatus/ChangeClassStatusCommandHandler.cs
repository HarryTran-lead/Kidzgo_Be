using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.ChangeClassStatus;

public sealed class ChangeClassStatusCommandHandler(
    IDbContext context
) : ICommandHandler<ChangeClassStatusCommand, ChangeClassStatusResponse>
{
    public async Task<Result<ChangeClassStatusResponse>> Handle(ChangeClassStatusCommand command, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<ChangeClassStatusResponse>(
                ClassErrors.NotFound(command.Id));
        }

        // Validate status transition
        if (classEntity.Status == command.Status)
        {
            return Result.Failure<ChangeClassStatusResponse>(
                ClassErrors.StatusUnchanged);
        }

        // Business rules for status transitions
        // PLANNED -> ACTIVE: OK
        // ACTIVE -> CLOSED: OK
        // CLOSED -> ACTIVE: OK (reopen)
        // CLOSED -> PLANNED: Not allowed
        if (classEntity.Status == Domain.Classes.ClassStatus.Closed && command.Status == Domain.Classes.ClassStatus.Planned)
        {
            return Result.Failure<ChangeClassStatusResponse>(
                ClassErrors.InvalidStatusTransition);
        }

        if (IsTerminalOrPausedStatus(command.Status))
        {
            bool hasActiveEnrollments = await context.ClassEnrollments.AnyAsync(
                e => e.ClassId == command.Id &&
                     (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Paused),
                cancellationToken);

            if (hasActiveEnrollments)
            {
                return Result.Failure<ChangeClassStatusResponse>(
                    ClassErrors.CannotCloseWithActiveEnrollments);
            }

            bool hasFutureSessions = await context.Sessions.AnyAsync(
                s => s.ClassId == command.Id &&
                     s.Status == SessionStatus.Scheduled &&
                     s.PlannedDatetime >= VietnamTime.UtcNow(),
                cancellationToken);

            if (hasFutureSessions)
            {
                return Result.Failure<ChangeClassStatusResponse>(
                    ClassErrors.CannotCloseWithFutureSessions);
            }
        }

        if (IsOperationalStatus(command.Status))
        {
            bool dependenciesAreActive = await context.Branches.AnyAsync(
                                             b => b.Id == classEntity.BranchId && b.IsActive,
                                             cancellationToken) &&
                                         await context.Programs.AnyAsync(
                                             p => p.Id == classEntity.ProgramId &&
                                                  p.IsActive &&
                                                  !p.IsDeleted,
                                             cancellationToken);

            if (dependenciesAreActive && classEntity.RoomId.HasValue)
            {
                dependenciesAreActive = await context.Classrooms.AnyAsync(
                    r => r.Id == classEntity.RoomId.Value &&
                         r.BranchId == classEntity.BranchId &&
                         r.IsActive,
                    cancellationToken);
            }

            if (dependenciesAreActive && classEntity.MainTeacherId.HasValue)
            {
                dependenciesAreActive = await context.Users.AnyAsync(
                    u => u.Id == classEntity.MainTeacherId.Value &&
                         u.BranchId == classEntity.BranchId &&
                         u.IsActive &&
                         !u.IsDeleted,
                    cancellationToken);
            }

            if (dependenciesAreActive && classEntity.AssistantTeacherId.HasValue)
            {
                dependenciesAreActive = await context.Users.AnyAsync(
                    u => u.Id == classEntity.AssistantTeacherId.Value &&
                         u.BranchId == classEntity.BranchId &&
                         u.IsActive &&
                         !u.IsDeleted,
                    cancellationToken);
            }

            if (!dependenciesAreActive)
            {
                return Result.Failure<ChangeClassStatusResponse>(
                    ClassErrors.InvalidActiveDependencies);
            }
        }

        classEntity.Status = command.Status;
        classEntity.UpdatedAt = VietnamTime.UtcNow();
        await context.SaveChangesAsync(cancellationToken);

        return new ChangeClassStatusResponse
        {
            Id = classEntity.Id,
            Status = classEntity.Status.ToString()
        };
    }

    private static bool IsTerminalOrPausedStatus(ClassStatus status)
    {
        return status is ClassStatus.Closed
            or ClassStatus.Completed
            or ClassStatus.Suspended
            or ClassStatus.Cancelled;
    }

    private static bool IsOperationalStatus(ClassStatus status)
    {
        return status is ClassStatus.Active
            or ClassStatus.Recruiting
            or ClassStatus.Full;
    }
}

