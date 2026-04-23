using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Programs.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.UpdateClass;

public sealed class UpdateClassCommandHandler(
    IDbContext context,
    SessionConflictChecker conflictChecker,
    ISchedulePatternParser patternParser
) : ICommandHandler<UpdateClassCommand, UpdateClassResponse>
{
    public async Task<Result<UpdateClassResponse>> Handle(UpdateClassCommand command, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<UpdateClassResponse>(
                ClassErrors.NotFound(command.Id));
        }

        // Check if branch exists
        bool branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<UpdateClassResponse>(
                ClassErrors.BranchNotFound);
        }

        // Check if program exists
        bool programExists = await context.Programs
            .AnyAsync(p => p.Id == command.ProgramId && !p.IsDeleted && p.IsActive, cancellationToken);

        if (!programExists)
        {
            return Result.Failure<UpdateClassResponse>(
                ClassErrors.ProgramNotFound);
        }

        var programAssignedToBranch = await BranchProgramAccessHelper.IsProgramAssignedToBranchAsync(
            context,
            command.BranchId,
            command.ProgramId,
            cancellationToken);

        if (!programAssignedToBranch)
        {
            return Result.Failure<UpdateClassResponse>(
                ClassErrors.ProgramNotAvailableInBranch);
        }

        // Check if code is unique (excluding current class)
        bool codeExists = await context.Classes
            .AnyAsync(c => c.Code == command.Code && c.Id != command.Id, cancellationToken);

        if (codeExists)
        {
            return Result.Failure<UpdateClassResponse>(
                ClassErrors.CodeExists);
        }

        bool branchOrProgramChanged = classEntity.BranchId != command.BranchId ||
                                      classEntity.ProgramId != command.ProgramId;
        bool scheduleChanged = classEntity.StartDate != command.StartDate ||
                               classEntity.EndDate != command.EndDate ||
                               classEntity.SchedulePattern != command.SchedulePattern;

        if (branchOrProgramChanged)
        {
            bool hasOperationalDependencies = await context.ClassEnrollments
                                                 .AnyAsync(e => e.ClassId == command.Id, cancellationToken) ||
                                             await context.Sessions
                                                 .AnyAsync(s => s.ClassId == command.Id, cancellationToken);

            if (hasOperationalDependencies)
            {
                return Result.Failure<UpdateClassResponse>(
                    ClassErrors.HasOperationalDependencies);
            }
        }

        int activeEnrollmentCount = await context.ClassEnrollments
            .CountAsync(
                e => e.ClassId == command.Id && e.Status == EnrollmentStatus.Active,
                cancellationToken);

        if (command.Capacity < activeEnrollmentCount)
        {
            return Result.Failure<UpdateClassResponse>(
                ClassErrors.CapacityBelowActiveEnrollments);
        }

        var futureSessions = await context.Sessions
            .Where(s => s.ClassId == command.Id &&
                        s.Status == SessionStatus.Scheduled &&
                        s.PlannedDatetime >= VietnamTime.UtcNow())
            .Select(s => new
            {
                s.Id,
                s.PlannedDatetime,
                s.DurationMinutes,
                s.PlannedRoomId
            })
            .ToListAsync(cancellationToken);

        if (scheduleChanged && futureSessions.Count > 0)
        {
            return Result.Failure<UpdateClassResponse>(
                ClassErrors.HasFutureSessions);
        }

        if (command.MainTeacherId.HasValue &&
            command.AssistantTeacherId.HasValue &&
            command.MainTeacherId.Value == command.AssistantTeacherId.Value)
        {
            return Result.Failure<UpdateClassResponse>(
                ClassErrors.TeacherAndAssistantMustDiffer);
        }

        if (command.RoomId.HasValue)
        {
            var room = await context.Classrooms
                .FirstOrDefaultAsync(r => r.Id == command.RoomId.Value && r.IsActive, cancellationToken);

            if (room is null)
            {
                return Result.Failure<UpdateClassResponse>(
                    ClassErrors.RoomNotFound);
            }

            if (room.BranchId != command.BranchId)
            {
                return Result.Failure<UpdateClassResponse>(
                    ClassErrors.RoomBranchMismatch);
            }
        }

        // Check if teachers exist, are TEACHER role, and belong to the same branch
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
                return Result.Failure<UpdateClassResponse>(
                    ClassErrors.MainTeacherNotFound);
            }

            // Check if teacher belongs to the same branch as the class
            if (mainTeacher.BranchId != command.BranchId)
            {
                return Result.Failure<UpdateClassResponse>(
                    ClassErrors.MainTeacherBranchMismatch);
            }
        }

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
                return Result.Failure<UpdateClassResponse>(
                    ClassErrors.AssistantTeacherNotFound);
            }

            // Check if teacher belongs to the same branch as the class
            if (assistantTeacher.BranchId != command.BranchId)
            {
                return Result.Failure<UpdateClassResponse>(
                    ClassErrors.AssistantTeacherBranchMismatch);
            }
        }

        bool resourcesChanged = classEntity.RoomId != command.RoomId ||
                                classEntity.MainTeacherId != command.MainTeacherId ||
                                classEntity.AssistantTeacherId != command.AssistantTeacherId;

        if (resourcesChanged && futureSessions.Count > 0)
        {
            foreach (var session in futureSessions)
            {
                var conflictResult = await conflictChecker.CheckConflictsAsync(
                    session.Id,
                    session.PlannedDatetime,
                    session.DurationMinutes,
                    command.RoomId ?? session.PlannedRoomId,
                    command.MainTeacherId,
                    command.AssistantTeacherId,
                    cancellationToken);

                if (conflictResult.HasConflicts)
                {
                    return Result.Failure<UpdateClassResponse>(
                        ToClassConflictError(conflictResult.Conflicts.First()));
                }
            }
        }

        if (futureSessions.Count == 0 &&
            !string.IsNullOrWhiteSpace(command.SchedulePattern) &&
            command.EndDate.HasValue)
        {
            var durationMinutes = patternParser.ParseDuration(command.SchedulePattern) ?? 90;
            var parseResult = patternParser.ParseAndGenerateOccurrences(
                command.SchedulePattern,
                command.StartDate,
                command.EndDate.Value);

            if (parseResult.IsSuccess)
            {
                foreach (var sessionDateTime in parseResult.Value.Take(10))
                {
                    var conflictResult = await conflictChecker.CheckConflictsAsync(
                        Guid.Empty,
                        sessionDateTime,
                        durationMinutes,
                        command.RoomId,
                        command.MainTeacherId,
                        command.AssistantTeacherId,
                        cancellationToken);

                    if (conflictResult.HasConflicts)
                    {
                        return Result.Failure<UpdateClassResponse>(
                            ToClassConflictError(conflictResult.Conflicts.First()));
                    }
                }
            }
        }

        classEntity.BranchId = command.BranchId;
        classEntity.ProgramId = command.ProgramId;
        classEntity.Code = command.Code;
        classEntity.Title = command.Title;
        classEntity.RoomId = command.RoomId;
        classEntity.MainTeacherId = command.MainTeacherId;
        classEntity.AssistantTeacherId = command.AssistantTeacherId;
        classEntity.StartDate = command.StartDate;
        classEntity.EndDate = command.EndDate;
        classEntity.Capacity = command.Capacity;
        classEntity.SchedulePattern = command.SchedulePattern;
        classEntity.Description = command.Description;
        classEntity.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateClassResponse
        {
            Id = classEntity.Id,
            BranchId = classEntity.BranchId,
            ProgramId = classEntity.ProgramId,
            Code = classEntity.Code,
            Title = classEntity.Title,
            RoomId = classEntity.RoomId,
            MainTeacherId = classEntity.MainTeacherId,
            AssistantTeacherId = classEntity.AssistantTeacherId,
            StartDate = classEntity.StartDate,
            EndDate = classEntity.EndDate,
            Status = classEntity.Status.ToString(),
            Capacity = classEntity.Capacity,
            SchedulePattern = classEntity.SchedulePattern,
            Description = classEntity.Description
        };
    }

    private static Error ToClassConflictError(SessionConflict conflict)
    {
        return conflict.Type switch
        {
            ConflictType.Room => ClassErrors.RoomConflict(
                conflict.ClassCode,
                conflict.ClassTitle,
                conflict.ConflictDatetime),
            ConflictType.Teacher => ClassErrors.TeacherConflict(
                conflict.ClassCode,
                conflict.ClassTitle,
                conflict.ConflictDatetime,
                conflict.RoomName),
            ConflictType.Assistant => ClassErrors.AssistantConflict(
                conflict.ClassCode,
                conflict.ClassTitle,
                conflict.ConflictDatetime),
            _ => ClassErrors.NotFound(null)
        };
    }
}

