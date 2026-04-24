using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.CreateClass;

public sealed class CreateClassCommandHandler(
    IDbContext context,
    SessionConflictChecker conflictChecker,
    ISchedulePatternParser patternParser
) : ICommandHandler<CreateClassCommand, CreateClassResponse>
{
    public async Task<Result<CreateClassResponse>> Handle(CreateClassCommand command, CancellationToken cancellationToken)
    {
        var normalizedPatternResult = SchedulePatternSupport.NormalizeWeeklyScheduleJson(
            command.WeeklyScheduleSlots,
            requireValue: false);
        if (normalizedPatternResult.IsFailure)
        {
            return Result.Failure<CreateClassResponse>(normalizedPatternResult.Error);
        }

        var normalizedWeeklyScheduleJson = normalizedPatternResult.Value;

        var branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<CreateClassResponse>(ClassErrors.BranchNotFound);
        }

        var programExists = await context.Programs
            .AnyAsync(p => p.Id == command.ProgramId && !p.IsDeleted && p.IsActive, cancellationToken);

        if (!programExists)
        {
            return Result.Failure<CreateClassResponse>(ClassErrors.ProgramNotFound);
        }

        var codeExists = await context.Classes
            .AnyAsync(c => c.Code == command.Code, cancellationToken);

        if (codeExists)
        {
            return Result.Failure<CreateClassResponse>(ClassErrors.CodeExists);
        }

        if (command.MainTeacherId.HasValue &&
            command.AssistantTeacherId.HasValue &&
            command.MainTeacherId.Value == command.AssistantTeacherId.Value)
        {
            return Result.Failure<CreateClassResponse>(ClassErrors.TeacherAndAssistantMustDiffer);
        }

        if (command.RoomId.HasValue)
        {
            var room = await context.Classrooms
                .FirstOrDefaultAsync(r => r.Id == command.RoomId.Value && r.IsActive, cancellationToken);

            if (room is null)
            {
                return Result.Failure<CreateClassResponse>(ClassErrors.RoomNotFound);
            }

            if (room.BranchId != command.BranchId)
            {
                return Result.Failure<CreateClassResponse>(ClassErrors.RoomBranchMismatch);
            }
        }

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
                return Result.Failure<CreateClassResponse>(ClassErrors.MainTeacherNotFound);
            }

            if (mainTeacher.BranchId != command.BranchId)
            {
                return Result.Failure<CreateClassResponse>(ClassErrors.MainTeacherBranchMismatch);
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
                return Result.Failure<CreateClassResponse>(ClassErrors.AssistantTeacherNotFound);
            }

            if (assistantTeacher.BranchId != command.BranchId)
            {
                return Result.Failure<CreateClassResponse>(ClassErrors.AssistantTeacherBranchMismatch);
            }
        }

        var endDate = command.EndDate;

        if (!string.IsNullOrWhiteSpace(normalizedWeeklyScheduleJson) && endDate.HasValue)
        {
            var parseResult = patternParser.ParseAndGenerateOccurrenceDetails(
                normalizedWeeklyScheduleJson,
                command.StartDate,
                endDate.Value);

            if (parseResult.IsSuccess && parseResult.Value.Count > 0)
            {
                foreach (var occurrence in parseResult.Value.Take(10))
                {
                    var conflictResult = await conflictChecker.CheckConflictsAsync(
                        Guid.Empty,
                        occurrence.PlannedDatetime,
                        occurrence.DurationMinutes,
                        command.RoomId,
                        command.MainTeacherId,
                        command.AssistantTeacherId,
                        cancellationToken);

                    if (!conflictResult.HasConflicts)
                    {
                        continue;
                    }

                    var firstConflict = conflictResult.Conflicts.First();
                    return firstConflict.Type switch
                    {
                        ConflictType.Room => Result.Failure<CreateClassResponse>(
                            ClassErrors.RoomConflict(
                                firstConflict.ClassCode,
                                firstConflict.ClassTitle,
                                firstConflict.ConflictDatetime)),
                        ConflictType.Teacher => Result.Failure<CreateClassResponse>(
                            ClassErrors.TeacherConflict(
                                firstConflict.ClassCode,
                                firstConflict.ClassTitle,
                                firstConflict.ConflictDatetime,
                                firstConflict.RoomName)),
                        ConflictType.Assistant => Result.Failure<CreateClassResponse>(
                            ClassErrors.AssistantConflict(
                                firstConflict.ClassCode,
                                firstConflict.ClassTitle,
                                firstConflict.ConflictDatetime)),
                        _ => Result.Failure<CreateClassResponse>(ClassErrors.NotFound(null))
                    };
                }
            }
        }

        var now = VietnamTime.UtcNow();
        var classEntity = new Class
        {
            Id = Guid.NewGuid(),
            BranchId = command.BranchId,
            ProgramId = command.ProgramId,
            Code = command.Code,
            Title = command.Title,
            RoomId = command.RoomId,
            MainTeacherId = command.MainTeacherId,
            AssistantTeacherId = command.AssistantTeacherId,
            StartDate = command.StartDate,
            EndDate = endDate,
            Status = ClassStatus.Active,
            Capacity = command.Capacity,
            WeeklyScheduleJson = normalizedWeeklyScheduleJson,
            Description = command.Description,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Classes.Add(classEntity);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateClassResponse
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
            WeeklyScheduleSlots = ParseSlots(classEntity.WeeklyScheduleJson),
            Description = classEntity.Description
        };
    }

    private List<ScheduleSlot> ParseSlots(string? weeklyScheduleJson)
    {
        if (string.IsNullOrWhiteSpace(weeklyScheduleJson))
        {
            return [];
        }

        var parseResult = patternParser.ParseScheduleSlots(weeklyScheduleJson);
        return parseResult.IsSuccess ? parseResult.Value : [];
    }

}
