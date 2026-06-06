using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Classes;
using Kidzgo.Application.Services;
using Kidzgo.Application.Syllabuses.Shared;
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
        var normalizedPatternResult = SchedulePatternSupport.NormalizeWeeklyScheduleJson(
            command.WeeklyScheduleSlots,
            requireValue: false);
        if (normalizedPatternResult.IsFailure)
        {
            return Result.Failure<UpdateClassResponse>(normalizedPatternResult.Error);
        }

        var normalizedWeeklyScheduleJson = normalizedPatternResult.Value;

        var classEntity = await context.Classes
            .Include(x => x.ModuleProgresses)
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

        bool programAssignedToBranch = await context.BranchPrograms
            .AnyAsync(
                bp => bp.BranchId == command.BranchId &&
                      bp.ProgramId == command.ProgramId &&
                      bp.IsActive,
                cancellationToken);
        if (!programAssignedToBranch)
        {
            return Result.Failure<UpdateClassResponse>(ClassErrors.ProgramNotAvailableInBranch);
        }

        var level = await context.Levels
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == command.LevelId, cancellationToken);
        if (level is null)
        {
            return Result.Failure<UpdateClassResponse>(ClassErrors.LevelNotFound);
        }

        if (level.ProgramId != command.ProgramId)
        {
            return Result.Failure<UpdateClassResponse>(ClassErrors.LevelProgramMismatch);
        }

        var syllabus = await context.Syllabuses
            .AsNoTracking()
            .Where(x => x.Id == command.SyllabusId && x.IsActive && !x.IsDeleted)
            .Select(x => new
            {
                x.Id,
                x.ProgramId,
                x.LevelId,
                x.Code,
                x.Version,
                x.Title
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (syllabus is null)
        {
            return Result.Failure<UpdateClassResponse>(ClassErrors.SyllabusNotFound);
        }

        if (syllabus.ProgramId != command.ProgramId || syllabus.LevelId != command.LevelId)
        {
            return Result.Failure<UpdateClassResponse>(ClassErrors.SyllabusProgramLevelMismatch);
        }

        var syllabusAssignedToBranch = await CurriculumAssignmentAccessHelper.IsSyllabusAssignedToBranchAsync(
            context,
            command.BranchId,
            command.ProgramId,
            command.LevelId,
            command.SyllabusId,
            command.StartDate,
            cancellationToken);
        if (!syllabusAssignedToBranch)
        {
            return Result.Failure<UpdateClassResponse>(ClassErrors.SyllabusNotAvailableInBranch);
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
            return Result.Failure<UpdateClassResponse>(moduleExists
                ? ClassErrors.StartModuleLevelMismatch
                : ClassErrors.StartModuleNotFound);
        }

        if (command.StartSessionIndex < 1 || command.StartSessionIndex > startModule.PlannedSessionCount)
        {
            return Result.Failure<UpdateClassResponse>(
                ClassErrors.InvalidStartSessionIndex(command.StartSessionIndex, startModule.PlannedSessionCount));
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
                                      classEntity.ProgramId != command.ProgramId ||
                                      classEntity.LevelId != command.LevelId;
        bool syllabusChanged = classEntity.SyllabusId != command.SyllabusId;
        bool startModuleChanged = classEntity.StartModuleId != command.StartModuleId;
        bool startSessionChanged = classEntity.StartSessionIndex != command.StartSessionIndex;
        bool scheduleChanged = classEntity.StartDate != command.StartDate ||
                               classEntity.EndDate != command.EndDate ||
                               classEntity.WeeklyScheduleJson != normalizedWeeklyScheduleJson;

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

        if (startModuleChanged || startSessionChanged)
        {
            var hasSessions = await context.Sessions.AnyAsync(x => x.ClassId == command.Id, cancellationToken);
            if (hasSessions)
            {
                return Result.Failure<UpdateClassResponse>(ClassErrors.StartModuleImmutableAfterSessions);
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

        string? slotTypeCode = null;
        var resolvedSlotTypeId = command.SlotTypeId;
        if (command.SlotTypeId.HasValue)
        {
            var slotType = await context.SlotTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == command.SlotTypeId.Value && x.IsActive,
                    cancellationToken);

            if (slotType is null)
            {
                return Result.Failure<UpdateClassResponse>(
                    Error.Validation(
                        "Class.SlotTypeNotFound",
                        $"Slot type '{command.SlotTypeId.Value}' was not found or inactive."));
            }

            slotTypeCode = slotType.Code;
        }
        else if (classEntity.SlotTypeId.HasValue)
        {
            var existingSlotType = await context.SlotTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == classEntity.SlotTypeId.Value, cancellationToken);
            slotTypeCode = existingSlotType?.Code;
            resolvedSlotTypeId = classEntity.SlotTypeId;
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
            !string.IsNullOrWhiteSpace(normalizedWeeklyScheduleJson) &&
            command.EndDate.HasValue)
        {
            var parseResult = patternParser.ParseAndGenerateOccurrenceDetails(
                normalizedWeeklyScheduleJson,
                command.StartDate,
                command.EndDate.Value);

            if (parseResult.IsSuccess)
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
        classEntity.LevelId = command.LevelId;
        classEntity.StartModuleId = command.StartModuleId;
        classEntity.StartSessionIndex = command.StartSessionIndex;
        classEntity.Code = command.Code;
        classEntity.Title = command.Title;
        classEntity.RoomId = command.RoomId;
        classEntity.MainTeacherId = command.MainTeacherId;
        classEntity.AssistantTeacherId = command.AssistantTeacherId;
        classEntity.SlotTypeId = resolvedSlotTypeId;
        classEntity.StartDate = command.StartDate;
        classEntity.EndDate = command.EndDate;
        classEntity.Capacity = command.Capacity;
        classEntity.WeeklyScheduleJson = normalizedWeeklyScheduleJson;
        classEntity.Description = command.Description;
        classEntity.UpdatedAt = VietnamTime.UtcNow();
        classEntity.Status = ClassLifecycleStatusHelper.ResolveScheduledStatus(
            classEntity.Status,
            classEntity.StartDate,
            VietnamTime.ToVietnamDateOnly(classEntity.UpdatedAt));

        classEntity.SyllabusId = command.SyllabusId;

        if (startModuleChanged || startSessionChanged || branchOrProgramChanged || syllabusChanged)
        {
            classEntity.CurrentModuleId = command.StartModuleId;
            classEntity.CurrentSessionIndex = command.StartSessionIndex;
            classEntity.CurrentLessonPlanTemplateId = null;
            context.ClassModuleProgresses.RemoveRange(classEntity.ModuleProgresses);
            var now = classEntity.UpdatedAt;
            context.ClassModuleProgresses.AddRange(
                modules.Select(module => new ClassModuleProgress
                {
                    Id = Guid.NewGuid(),
                    ClassId = classEntity.Id,
                    ModuleId = module.Id,
                    OrderIndex = module.Order,
                    RequiredSessions = module.PlannedSessionCount,
                    CompletedClassSessions = 0,
                    CompletedLessonPlans = 0,
                    StartSessionIndex = module.Id == startModule.Id ? command.StartSessionIndex : 1,
                    CurrentSessionIndex = module.Id == startModule.Id ? command.StartSessionIndex : 1,
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
        }

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateClassResponse
        {
            Id = classEntity.Id,
            BranchId = classEntity.BranchId,
            ProgramId = classEntity.ProgramId,
            LevelId = classEntity.LevelId,
            SyllabusId = classEntity.SyllabusId,
            SyllabusCode = syllabus.Code,
            SyllabusVersion = syllabus.Version,
            SyllabusTitle = syllabus.Title,
            StartModuleId = classEntity.StartModuleId,
            StartSessionIndex = classEntity.StartSessionIndex,
            CurrentModuleId = classEntity.CurrentModuleId,
            CurrentSessionIndex = classEntity.CurrentSessionIndex,
            CurrentLessonPlanTemplateId = classEntity.CurrentLessonPlanTemplateId,
            Code = classEntity.Code,
            Title = classEntity.Title,
            RoomId = classEntity.RoomId,
            MainTeacherId = classEntity.MainTeacherId,
            AssistantTeacherId = classEntity.AssistantTeacherId,
            SlotTypeId = classEntity.SlotTypeId,
            SlotTypeCode = slotTypeCode,
            StartDate = classEntity.StartDate,
            ExpectedEndDate = classEntity.ExpectedEndDate,
            ActualEndDate = classEntity.ActualEndDate,
            EndDate = classEntity.EndDate,
            Status = classEntity.Status.ToString(),
            Capacity = classEntity.Capacity,
            WeeklyScheduleSlots = ParseSlots(classEntity.WeeklyScheduleJson),
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

