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
using System.Text.Json;

namespace Kidzgo.Application.Classes.CreateClass;

public sealed class CreateClassCommandHandler(
    IDbContext context,
    SessionConflictChecker conflictChecker,
    ISchedulePatternParser patternParser,
    ClassSessionPlanningService classSessionPlanningService,
    StudentSessionAssignmentService studentSessionAssignmentService
) : ICommandHandler<CreateClassCommand, CreateClassResponse>
{
    private const int OccurrenceBatchDays = 56;
    private const int MaxOccurrenceSearchDays = 730;

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

        var programAssignedToBranch = await context.BranchPrograms
            .AnyAsync(
                bp => bp.BranchId == command.BranchId &&
                      bp.ProgramId == command.ProgramId &&
                      bp.IsActive,
                cancellationToken);
        if (!programAssignedToBranch)
        {
            return Result.Failure<CreateClassResponse>(ClassErrors.ProgramNotAvailableInBranch);
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
            return Result.Failure<CreateClassResponse>(ClassErrors.SyllabusNotFound);
        }

        if (syllabus.ProgramId != command.ProgramId || syllabus.LevelId != command.LevelId)
        {
            return Result.Failure<CreateClassResponse>(ClassErrors.SyllabusProgramLevelMismatch);
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
            return Result.Failure<CreateClassResponse>(ClassErrors.SyllabusNotAvailableInBranch);
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

        if (command.StartSessionIndex < 1 || command.StartSessionIndex > startModule.PlannedSessionCount)
        {
            return Result.Failure<CreateClassResponse>(
                ClassErrors.InvalidStartSessionIndex(command.StartSessionIndex, startModule.PlannedSessionCount));
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

        var defaultSectionType = SectionType.Normal;

        var sessionsToGenerate = command.SessionsToGenerate.GetValueOrDefault();
        if (sessionsToGenerate > 0 && string.IsNullOrWhiteSpace(normalizedWeeklyScheduleJson))
        {
            return Result.Failure<CreateClassResponse>(Error.Validation(
                "Class.WeeklyScheduleRequired",
                "Weekly schedule is required when sessionsToGenerate is provided."));
        }

        List<ScheduleOccurrence> plannedOccurrences = [];
        List<PlannedSessionMetadata> plannedMetadata = [];
        if (sessionsToGenerate > 0)
        {
            var occurrenceResult = await BuildOccurrencesAsync(
                normalizedWeeklyScheduleJson!,
                command.StartDate,
                command.EndDate,
                sessionsToGenerate,
                command.SkipHolidays,
                cancellationToken);
            if (occurrenceResult.IsFailure)
            {
                return Result.Failure<CreateClassResponse>(occurrenceResult.Error);
            }

            plannedOccurrences = occurrenceResult.Value;

            var metadataResult = await classSessionPlanningService.PlanAsync(
                command.SyllabusId,
                command.LevelId,
                command.StartModuleId,
                command.StartSessionIndex,
                existingSessionCount: 0,
                newSessionCount: sessionsToGenerate,
                strictCurriculumCoverage: false,
                cancellationToken);
            if (metadataResult.IsFailure)
            {
                return Result.Failure<CreateClassResponse>(metadataResult.Error);
            }

            plannedMetadata = metadataResult.Value;

            foreach (var occurrence in plannedOccurrences)
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
            SyllabusId = command.SyllabusId,
            StartModuleId = command.StartModuleId,
            StartSessionIndex = command.StartSessionIndex,
            CurrentModuleId = command.StartModuleId,
            CurrentSessionIndex = command.StartSessionIndex,
            CurrentLessonPlanTemplateId = plannedMetadata.FirstOrDefault()?.LessonPlanTemplateId,
            Code = command.Code,
            Title = command.Title,
            RoomId = command.RoomId,
            MainTeacherId = command.MainTeacherId,
            AssistantTeacherId = command.AssistantTeacherId,
            StartDate = command.StartDate,
            ExpectedEndDate = plannedOccurrences.Count > 0
                ? VietnamTime.ToVietnamDateOnly(plannedOccurrences[^1].PlannedDatetime)
                : null,
            ActualEndDate = null,
            EndDate = command.EndDate,
            Status = ClassLifecycleStatusHelper.ResolveInitialStatus(command.StartDate, now),
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

        if (plannedOccurrences.Count > 0)
        {
            var sessions = plannedOccurrences
                .Select((occurrence, index) => new Session
                {
                    Id = Guid.NewGuid(),
                    ClassId = classEntity.Id,
                    BranchId = classEntity.BranchId,
                    ModuleId = plannedMetadata[index].ModuleId,
                    LessonPlanTemplateId = plannedMetadata[index].LessonPlanTemplateId,
                    SessionIndexInModule = plannedMetadata[index].SessionIndexInModule,
                    Color = SessionColorPalette.GetRandomColor(),
                    PlannedDatetime = occurrence.PlannedDatetime,
                    PlannedRoomId = classEntity.RoomId,
                    PlannedTeacherId = classEntity.MainTeacherId,
                    PlannedAssistantId = classEntity.AssistantTeacherId,
                    DurationMinutes = occurrence.DurationMinutes,
                    ParticipationType = ParticipationType.Main,
                    SectionType = defaultSectionType,
                    Status = SessionStatus.Scheduled,
                    CurriculumSnapshotJson = BuildCurriculumSnapshotJson(plannedMetadata[index]),
                    CreatedAt = now,
                    UpdatedAt = now
                })
                .ToList();

            context.Sessions.AddRange(sessions);
            foreach (var session in sessions)
            {
                await studentSessionAssignmentService.SyncAssignmentsForSessionAsync(session, cancellationToken);
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return new CreateClassResponse
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

    private async Task<Result<List<ScheduleOccurrence>>> BuildOccurrencesAsync(
        string weeklyScheduleJson,
        DateOnly startDate,
        DateOnly? endDate,
        int sessionsToGenerate,
        bool skipHolidays,
        CancellationToken cancellationToken)
    {
        var occurrences = new List<ScheduleOccurrence>(sessionsToGenerate);
        var searchStart = startDate;
        var absoluteSearchEnd = endDate ?? startDate.AddDays(MaxOccurrenceSearchDays);

        while (occurrences.Count < sessionsToGenerate && searchStart <= absoluteSearchEnd)
        {
            var batchEnd = searchStart.AddDays(OccurrenceBatchDays - 1);
            if (batchEnd > absoluteSearchEnd)
            {
                batchEnd = absoluteSearchEnd;
            }

            var parseResult = patternParser.ParseAndGenerateOccurrenceDetails(
                weeklyScheduleJson,
                searchStart,
                batchEnd);

            if (parseResult.IsFailure)
            {
                return Result.Failure<List<ScheduleOccurrence>>(parseResult.Error);
            }

            HashSet<DateOnly> holidayDates = [];
            if (skipHolidays)
            {
                var holidays = await context.Holidays
                    .AsNoTracking()
                    .Where(h => h.IsActive && h.StartDate <= batchEnd && h.EndDate >= searchStart)
                    .ToListAsync(cancellationToken);

                foreach (var holiday in holidays)
                {
                    for (var date = holiday.StartDate; date <= holiday.EndDate; date = date.AddDays(1))
                    {
                        holidayDates.Add(date);
                    }
                }
            }

            foreach (var occurrence in parseResult.Value.OrderBy(x => x.PlannedDatetime))
            {
                var occurrenceDate = VietnamTime.ToVietnamDateOnly(occurrence.PlannedDatetime);
                if (skipHolidays && holidayDates.Contains(occurrenceDate))
                {
                    continue;
                }

                if (occurrences.Any(x => x.PlannedDatetime == occurrence.PlannedDatetime))
                {
                    continue;
                }

                occurrences.Add(occurrence);
                if (occurrences.Count == sessionsToGenerate)
                {
                    break;
                }
            }

            searchStart = batchEnd.AddDays(1);
        }

        if (occurrences.Count < sessionsToGenerate)
        {
            return Result.Failure<List<ScheduleOccurrence>>(Error.Validation(
                "Class.NotEnoughScheduleOccurrences",
                $"Could only generate {occurrences.Count} scheduled occurrence(s) from {startDate:yyyy-MM-dd} but {sessionsToGenerate} were requested."));
        }

        return Result.Success(occurrences);
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

    private static string BuildCurriculumSnapshotJson(PlannedSessionMetadata metadata)
    {
        return JsonSerializer.Serialize(new
        {
            metadata.SyllabusId,
            metadata.SyllabusCode,
            metadata.SyllabusVersion,
            metadata.SyllabusTitle,
            metadata.ModuleId,
            metadata.ModuleCode,
            metadata.ModuleName,
            metadata.LessonPlanUnitId,
            metadata.UnitName,
            metadata.LessonPlanTemplateId,
            metadata.SessionIndexInModule,
            metadata.LessonTitle,
            metadata.Objectives,
            metadata.Procedure
        });
    }
}
