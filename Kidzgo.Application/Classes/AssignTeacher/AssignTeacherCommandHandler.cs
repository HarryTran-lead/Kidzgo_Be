using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.AssignTeacher;

public sealed class AssignTeacherCommandHandler(
    IDbContext context,
    SessionConflictChecker conflictChecker
) : ICommandHandler<AssignTeacherCommand, AssignTeacherResponse>
{
    public async Task<Result<AssignTeacherResponse>> Handle(AssignTeacherCommand command, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .Include(c => c.MainTeacher)
            .Include(c => c.AssistantTeacher)
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<AssignTeacherResponse>(
                ClassErrors.NotFound(command.ClassId));
        }

        if (command.MainTeacherId.HasValue &&
            command.AssistantTeacherId.HasValue &&
            command.MainTeacherId.Value == command.AssistantTeacherId.Value)
        {
            return Result.Failure<AssignTeacherResponse>(
                ClassErrors.TeacherAndAssistantMustDiffer);
        }

        // Check if main teacher exists, is TEACHER role, and belongs to the same branch
        if (command.MainTeacherId.HasValue)
        {
            var mainTeacher = await context.Users
                .FirstOrDefaultAsync(
                    u => u.Id == command.MainTeacherId.Value &&
                         u.Role == UserRole.Teacher &&
                         u.IsActive &&
                         !u.IsDeleted,
                    cancellationToken);

            if (mainTeacher is null)
            {
                return Result.Failure<AssignTeacherResponse>(
                    ClassErrors.MainTeacherNotFound);
            }

            // Check if teacher belongs to the same branch as the class
            if (mainTeacher.BranchId != classEntity.BranchId)
            {
                return Result.Failure<AssignTeacherResponse>(
                    ClassErrors.MainTeacherBranchMismatch);
            }

            classEntity.MainTeacherId = command.MainTeacherId.Value;
        }
        else
        {
            classEntity.MainTeacherId = null;
        }

        // Check if assistant teacher exists, is TEACHER role, and belongs to the same branch
        if (command.AssistantTeacherId.HasValue)
        {
            var assistantTeacher = await context.Users
                .FirstOrDefaultAsync(
                    u => u.Id == command.AssistantTeacherId.Value &&
                         u.Role == UserRole.Teacher &&
                         u.IsActive &&
                         !u.IsDeleted,
                    cancellationToken);

            if (assistantTeacher is null)
            {
                return Result.Failure<AssignTeacherResponse>(
                    ClassErrors.AssistantTeacherNotFound);
            }

            // Check if teacher belongs to the same branch as the class
            if (assistantTeacher.BranchId != classEntity.BranchId)
            {
                return Result.Failure<AssignTeacherResponse>(
                    ClassErrors.AssistantTeacherBranchMismatch);
            }

            classEntity.AssistantTeacherId = command.AssistantTeacherId.Value;
        }
        else
        {
            classEntity.AssistantTeacherId = null;
        }

        var futureSessions = await context.Sessions
            .Where(s => s.ClassId == classEntity.Id &&
                        s.Status == SessionStatus.Scheduled &&
                        s.PlannedDatetime >= VietnamTime.UtcNow())
            .ToListAsync(cancellationToken);

        foreach (var session in futureSessions)
        {
            var conflictResult = await conflictChecker.CheckConflictsAsync(
                session.Id,
                session.PlannedDatetime,
                session.DurationMinutes,
                null,
                classEntity.MainTeacherId,
                classEntity.AssistantTeacherId,
                cancellationToken);

            if (!conflictResult.HasConflicts)
            {
                continue;
            }

            var conflict = conflictResult.Conflicts.First();
            return Result.Failure<AssignTeacherResponse>(
                conflict.Type == ConflictType.Assistant
                    ? ClassErrors.AssistantConflict(
                        conflict.ClassCode,
                        conflict.ClassTitle,
                        conflict.ConflictDatetime)
                    : ClassErrors.TeacherConflict(
                        conflict.ClassCode,
                        conflict.ClassTitle,
                        conflict.ConflictDatetime,
                        conflict.RoomName));
        }

        classEntity.UpdatedAt = VietnamTime.UtcNow();
        await context.SaveChangesAsync(cancellationToken);

        // Re-query to get teacher names
        var updatedClass = await context.Classes
            .Include(c => c.MainTeacher)
            .Include(c => c.AssistantTeacher)
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        return new AssignTeacherResponse
        {
            ClassId = updatedClass!.Id,
            MainTeacherId = updatedClass.MainTeacherId,
            MainTeacherName = updatedClass.MainTeacher?.Name,
            AssistantTeacherId = updatedClass.AssistantTeacherId,
            AssistantTeacherName = updatedClass.AssistantTeacher?.Name
        };
    }
}

