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

        var level = await context.Levels
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == command.LevelId, cancellationToken);
        if (level is null)
        {
            return Result.Failure<CreateClassResponse>(ClassErrors.LevelNotFound);
        }

        if (level.ProgramId != command.ProgramId)
        {
            return Result.Failure<CreateClassResponse>(ClassErrors.LevelProgramMismatch);
        }

        var modules = await context.Modules
            .AsNoTracking()
            .Where(x => x.LevelId == command.LevelId && x.IsActive)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);
        var startModule = modules.FirstOrDefault(x => x.Id == command.StartModuleId);
        if (startModule is null)
        {
            var moduleExists = await context.Modules.AnyAsync(x => x.Id == command.StartModuleId, cancellationToken);
            return Result.Failure<CreateClassResponse>(moduleExists
                ? ClassErrors.StartModuleLevelMismatch
                : ClassErrors.StartModuleNotFound);
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

        string? slotTypeCode = null;
        if (command.SlotTypeId.HasValue)
        {
            var slotType = await context.SlotTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == command.SlotTypeId.Value && x.IsActive,
                    cancellationToken);

            if (slotType is null)
            {
                return Result.Failure<CreateClassResponse>(
                    Error.Validation(
                        "Class.SlotTypeNotFound",
                        $"Slot type '{command.SlotTypeId.Value}' was not found or inactive."));
            }

            slotTypeCode = slotType.Code;
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
            LevelId = command.LevelId,
            StartModuleId = command.StartModuleId,
            CurrentModuleId = command.StartModuleId,
            Code = command.Code,
            Title = command.Title,
            RoomId = command.RoomId,
            MainTeacherId = command.MainTeacherId,
            AssistantTeacherId = command.AssistantTeacherId,
            SlotTypeId = command.SlotTypeId,
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
        context.ClassModuleProgresses.AddRange(
            modules.Select(module => new ClassModuleProgress
            {
                Id = Guid.NewGuid(),
                ClassId = classEntity.Id,
                ModuleId = module.Id,
                OrderIndex = module.Order,
                RequiredSessions = module.PlannedSessionCount,
                CompletedSessions = 0,
                Status = module.Order < startModule.Order
                    ? ClassModuleProgressStatus.Skipped
                    : module.Id == startModule.Id
                        ? ClassModuleProgressStatus.Active
                        : ClassModuleProgressStatus.Pending,
                StartedAt = module.Id == startModule.Id ? now : null,
                CompletedAt = null,
                CreatedAt = now,
                UpdatedAt = now
            }));
        await context.SaveChangesAsync(cancellationToken);

        return new CreateClassResponse
        {
            Id = classEntity.Id,
            BranchId = classEntity.BranchId,
            ProgramId = classEntity.ProgramId,
            LevelId = classEntity.LevelId,
            StartModuleId = classEntity.StartModuleId,
            CurrentModuleId = classEntity.CurrentModuleId,
            Code = classEntity.Code,
            Title = classEntity.Title,
            RoomId = classEntity.RoomId,
            MainTeacherId = classEntity.MainTeacherId,
            AssistantTeacherId = classEntity.AssistantTeacherId,
            SlotTypeId = classEntity.SlotTypeId,
            SlotTypeCode = slotTypeCode,
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
