using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class SessionGenerationService
{
    private const int RollingWindowWeeks = 8;
    private const int MaxHolidayCompensationDays = 366;

    private readonly IDbContext _context;
    private readonly ISchedulePatternParser _patternParser;
    private readonly StudentSessionAssignmentService _studentSessionAssignmentService;
    private readonly SessionConflictChecker _conflictChecker;
    private readonly ClassSessionPlanningService _classSessionPlanningService;

    public SessionGenerationService(
        IDbContext context,
        ISchedulePatternParser patternParser,
        StudentSessionAssignmentService studentSessionAssignmentService,
        SessionConflictChecker conflictChecker,
        ClassSessionPlanningService classSessionPlanningService)
    {
        _context = context;
        _patternParser = patternParser;
        _studentSessionAssignmentService = studentSessionAssignmentService;
        _conflictChecker = conflictChecker;
        _classSessionPlanningService = classSessionPlanningService;
    }

    public async Task<Result<int>> GenerateSessionsFromPatternAsync(
        Class classEntity,
        bool onlyFutureSessions = true,
        CancellationToken cancellationToken = default)
    {
        var roomId = classEntity.RoomId;

        if (classEntity.Status is not ClassStatus.Planned and not ClassStatus.Recruiting and not ClassStatus.Active)
        {
            return Result.Failure<int>(SessionErrors.InvalidClassStatus);
        }

        var today = VietnamTime.TodayDateOnly();
        var generationStart = onlyFutureSessions && today > classEntity.StartDate
            ? today
            : classEntity.StartDate;
        var rollingWindowEnd = generationStart.AddDays((RollingWindowWeeks * 7) - 1);
        var generationEnd = classEntity.EndDate.HasValue && classEntity.EndDate.Value < rollingWindowEnd
            ? classEntity.EndDate.Value
            : rollingWindowEnd;

        var scheduleWindowsResult = await GetScheduleGenerationWindowsAsync(
            classEntity,
            generationStart,
            generationEnd,
            cancellationToken);
        if (scheduleWindowsResult.IsFailure)
        {
            return Result.Failure<int>(scheduleWindowsResult.Error);
        }

        if (scheduleWindowsResult.Value.Count == 0)
        {
            return Result.Success(0);
        }

        var holidays = await _context.Holidays
            .AsNoTracking()
            .Where(h =>
                h.IsActive &&
                h.StartDate <= generationEnd &&
                h.EndDate >= generationStart)
            .Select(h => new HolidayWindow(h.StartDate, h.EndDate))
            .ToListAsync(cancellationToken);

        var occurrenceCandidates = new List<ScheduleOccurrenceCandidate>();
        var skippedHolidayOccurrenceCount = 0;
        foreach (var window in scheduleWindowsResult.Value)
        {
            var parseResult = _patternParser.ParseAndGenerateOccurrenceDetails(
                window.WeeklyScheduleJson,
                window.EffectiveFrom,
                window.EffectiveTo);

            if (parseResult.IsFailure)
            {
                return Result.Failure<int>(parseResult.Error);
            }

            if (parseResult.Value.Any(occurrence => occurrence.DurationMinutes <= 0))
            {
                var invalidDuration = parseResult.Value.First(occurrence => occurrence.DurationMinutes <= 0).DurationMinutes;
                return Result.Failure<int>(SessionErrors.InvalidDuration(invalidDuration));
            }

            foreach (var occurrence in parseResult.Value)
            {
                if (IsHoliday(occurrence.PlannedDatetime, holidays))
                {
                    skippedHolidayOccurrenceCount++;
                    continue;
                }

                occurrenceCandidates.Add(
                    new ScheduleOccurrenceCandidate(occurrence.PlannedDatetime, occurrence.DurationMinutes));
            }
        }

        if (skippedHolidayOccurrenceCount > 0 && !classEntity.EndDate.HasValue)
        {
            var compensationCandidatesResult = await GenerateHolidayCompensationCandidatesAsync(
                classEntity,
                generationEnd.AddDays(1),
                skippedHolidayOccurrenceCount,
                holidays,
                cancellationToken);

            if (compensationCandidatesResult.IsFailure)
            {
                return Result.Failure<int>(compensationCandidatesResult.Error);
            }

            occurrenceCandidates.AddRange(compensationCandidatesResult.Value);
        }

        if (occurrenceCandidates.Count == 0)
        {
            return Result.Success(0);
        }

        var branchExists = await _context.Branches
            .AnyAsync(b => b.Id == classEntity.BranchId && b.IsActive, cancellationToken);
        if (!branchExists)
        {
            return Result.Failure<int>(SessionErrors.InvalidBranch(classEntity.BranchId));
        }

        if (roomId.HasValue)
        {
            var roomExists = await _context.Classrooms
                .AnyAsync(r => r.Id == roomId.Value && r.BranchId == classEntity.BranchId, cancellationToken);
            if (!roomExists)
            {
                return Result.Failure<int>(SessionErrors.InvalidRoom(roomId.Value));
            }
        }

        if (classEntity.MainTeacherId.HasValue)
        {
            var teacherExists = await _context.Users
                .AnyAsync(
                    u => u.Id == classEntity.MainTeacherId.Value &&
                         u.Role == UserRole.Teacher &&
                         u.BranchId == classEntity.BranchId,
                    cancellationToken);
            if (!teacherExists)
            {
                return Result.Failure<int>(SessionErrors.InvalidTeacher(classEntity.MainTeacherId.Value));
            }
        }

        if (classEntity.AssistantTeacherId.HasValue)
        {
            var assistantExists = await _context.Users
                .AnyAsync(
                    u => u.Id == classEntity.AssistantTeacherId.Value &&
                         u.Role == UserRole.Teacher &&
                         u.BranchId == classEntity.BranchId,
                    cancellationToken);
            if (!assistantExists)
            {
                return Result.Failure<int>(SessionErrors.InvalidAssistant(classEntity.AssistantTeacherId.Value));
            }
        }

        var generatedSectionType = SectionType.Normal;

        var existingSessions = await _context.Sessions
            .Where(s => s.ClassId == classEntity.Id)
            .ToListAsync(cancellationToken);

        var now = VietnamTime.UtcNow();
        var sessionsToCreate = new List<Session>();

        foreach (var candidate in occurrenceCandidates
                     .OrderBy(candidate => candidate.PlannedDatetime)
                     .DistinctBy(candidate => candidate.PlannedDatetime))
        {
            var occurrence = candidate.PlannedDatetime;
            if (onlyFutureSessions && occurrence < now)
            {
                continue;
            }

            var existingSession = existingSessions
                .FirstOrDefault(s => Math.Abs((s.PlannedDatetime - occurrence).TotalMinutes) < 1);

            if (existingSession != null)
            {
                continue;
            }

            var conflictResult = await _conflictChecker.CheckConflictsAsync(
                Guid.Empty,
                occurrence,
                candidate.DurationMinutes,
                roomId,
                classEntity.MainTeacherId,
                classEntity.AssistantTeacherId,
                cancellationToken);

            if (conflictResult.HasConflicts)
            {
                var firstConflict = conflictResult.Conflicts.First();
                return firstConflict.Type switch
                {
                    ConflictType.Room => Result.Failure<int>(
                        ClassErrors.RoomConflict(
                            firstConflict.ClassCode,
                            firstConflict.ClassTitle,
                            firstConflict.ConflictDatetime)),
                    ConflictType.Teacher => Result.Failure<int>(
                        ClassErrors.TeacherConflict(
                            firstConflict.ClassCode,
                            firstConflict.ClassTitle,
                            firstConflict.ConflictDatetime,
                            firstConflict.RoomName)),
                    ConflictType.Assistant => Result.Failure<int>(
                        ClassErrors.AssistantConflict(
                            firstConflict.ClassCode,
                            firstConflict.ClassTitle,
                            firstConflict.ConflictDatetime)),
                    _ => Result.Failure<int>(Error.Validation(
                        "Session.ConflictDetected",
                        "A conflicting session was detected during generation"))
                };
            }

            sessionsToCreate.Add(new Session
            {
                Id = Guid.NewGuid(),
                ClassId = classEntity.Id,
                BranchId = classEntity.BranchId,
                Color = SessionColorPalette.GetRandomColor(),
                PlannedDatetime = occurrence,
                PlannedRoomId = roomId,
                PlannedTeacherId = classEntity.MainTeacherId,
                PlannedAssistantId = classEntity.AssistantTeacherId,
                DurationMinutes = candidate.DurationMinutes,
                ParticipationType = ParticipationType.Main,
                SectionType = generatedSectionType,
                Status = SessionStatus.Scheduled,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (sessionsToCreate.Count == 0)
        {
            return Result.Success(0);
        }

        try
        {
            var planningResult = await _classSessionPlanningService.AssignMetadataAsync(
                classEntity.Id,
                sessionsToCreate,
                strictCurriculumCoverage: false,
                cancellationToken);
            if (planningResult.IsFailure)
            {
                return Result.Failure<int>(planningResult.Error);
            }
            _context.Sessions.AddRange(sessionsToCreate);
            foreach (var session in sessionsToCreate)
            {
                await _studentSessionAssignmentService.SyncAssignmentsForSessionAsync(session, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            var errorMessage = ex.Message;
            var innerException = ex.InnerException;

            if (innerException != null)
            {
                var innerType = innerException.GetType();
                var innerTypeName = innerType.Name;

                if (innerTypeName.Contains("Postgres") || innerTypeName.Contains("Npgsql"))
                {
                    errorMessage = $"{ex.Message} | Inner: {innerException.Message}";

                    var constraintNameProperty = innerType.GetProperty("ConstraintName");
                    if (constraintNameProperty != null)
                    {
                        var constraintName = constraintNameProperty.GetValue(innerException)?.ToString();
                        if (!string.IsNullOrEmpty(constraintName))
                        {
                            errorMessage += $" | Constraint: {constraintName}";

                            if (constraintName.Contains("PlannedRoomId"))
                            {
                                if (roomId.HasValue)
                                {
                                    return Result.Failure<int>(SessionErrors.InvalidRoom(roomId.Value));
                                }

                                errorMessage = "Room with ID does not exist. Please verify roomId.";
                            }
                            else if (constraintName.Contains("BranchId"))
                            {
                                return Result.Failure<int>(SessionErrors.InvalidBranch(classEntity.BranchId));
                            }
                            else if (constraintName.Contains("ClassId"))
                            {
                                return Result.Failure<int>(ClassErrors.NotFound(classEntity.Id));
                            }
                            else if (constraintName.Contains("PlannedTeacherId") && classEntity.MainTeacherId.HasValue)
                            {
                                return Result.Failure<int>(SessionErrors.InvalidTeacher(classEntity.MainTeacherId.Value));
                            }
                            else if (constraintName.Contains("PlannedAssistantId") && classEntity.AssistantTeacherId.HasValue)
                            {
                                return Result.Failure<int>(SessionErrors.InvalidAssistant(classEntity.AssistantTeacherId.Value));
                            }
                        }
                    }

                    var detailProperty = innerType.GetProperty("Detail");
                    if (detailProperty != null)
                    {
                        var detail = detailProperty.GetValue(innerException)?.ToString();
                        if (!string.IsNullOrEmpty(detail))
                        {
                            errorMessage += $" | Detail: {detail}";
                        }
                    }
                }
                else
                {
                    errorMessage = $"{ex.Message} | Inner: {innerException.Message}";
                }
            }

            var entries = ex.Entries?.Select(e => $"{e.Entity.GetType().Name} - {e.State}").ToList();
            if (entries != null && entries.Any())
            {
                errorMessage += $" | Entries: {string.Join(", ", entries)}";
            }

            return Result.Failure<int>(SessionErrors.SaveFailed(errorMessage));
        }
        catch (Exception ex)
        {
            var stackTrace = ex.StackTrace != null
                ? ex.StackTrace.Substring(0, Math.Min(500, ex.StackTrace.Length))
                : "";

            return Result.Failure<int>(Error.Validation(
                "Session.SaveFailed",
                $"Unexpected error while saving sessions: {ex.Message} | Type: {ex.GetType().Name} | StackTrace: {stackTrace}"));
        }

        return Result.Success(sessionsToCreate.Count);
    }

    private async Task<Result<List<ScheduleGenerationWindow>>> GetScheduleGenerationWindowsAsync(
        Class classEntity,
        DateOnly generationStart,
        DateOnly generationEnd,
        CancellationToken cancellationToken)
    {
        var scheduleSegments = await _context.ClassScheduleSegments
            .AsNoTracking()
            .Where(segment => segment.ClassId == classEntity.Id)
            .OrderBy(segment => segment.EffectiveFrom)
            .ToListAsync(cancellationToken);

        if (scheduleSegments.Count == 0)
        {
            if (string.IsNullOrWhiteSpace(classEntity.WeeklyScheduleJson))
            {
                return Result.Failure<List<ScheduleGenerationWindow>>(
                    SessionErrors.MissingSchedulePattern(classEntity.Id));
            }

            return Result.Success(new List<ScheduleGenerationWindow>
            {
                new(generationStart, generationEnd, classEntity.WeeklyScheduleJson)
            });
        }

        var windows = new List<ScheduleGenerationWindow>();
        var firstSegment = scheduleSegments[0];

        if (firstSegment.EffectiveFrom > generationStart &&
            !string.IsNullOrWhiteSpace(classEntity.WeeklyScheduleJson))
        {
            windows.Add(new ScheduleGenerationWindow(
                generationStart,
                firstSegment.EffectiveFrom.AddDays(-1),
                classEntity.WeeklyScheduleJson));
        }

        foreach (var segment in scheduleSegments)
        {
            if (segment.EffectiveFrom > generationEnd ||
                (segment.EffectiveTo.HasValue && segment.EffectiveTo.Value < generationStart))
            {
                continue;
            }

            var effectiveFrom = segment.EffectiveFrom > generationStart
                ? segment.EffectiveFrom
                : generationStart;
            var effectiveTo = segment.EffectiveTo.HasValue && segment.EffectiveTo.Value < generationEnd
                ? segment.EffectiveTo.Value
                : generationEnd;

            if (effectiveTo < effectiveFrom)
            {
                continue;
            }

            windows.Add(new ScheduleGenerationWindow(
                effectiveFrom,
                effectiveTo,
                segment.WeeklyScheduleJson));
        }

        return Result.Success(windows);
    }

    private async Task<Result<List<ScheduleOccurrenceCandidate>>> GenerateHolidayCompensationCandidatesAsync(
        Class classEntity,
        DateOnly compensationStart,
        int targetCount,
        IReadOnlyCollection<HolidayWindow> initialHolidays,
        CancellationToken cancellationToken)
    {
        var candidates = new List<ScheduleOccurrenceCandidate>();
        var searchStart = compensationStart;
        var searchEnd = compensationStart.AddDays(MaxHolidayCompensationDays - 1);
        var holidays = initialHolidays.ToList();

        while (candidates.Count < targetCount && searchStart <= searchEnd)
        {
            var batchEnd = searchStart.AddDays((RollingWindowWeeks * 7) - 1);
            if (batchEnd > searchEnd)
            {
                batchEnd = searchEnd;
            }

            var extraHolidays = await _context.Holidays
                .AsNoTracking()
                .Where(h =>
                    h.IsActive &&
                    h.StartDate <= batchEnd &&
                    h.EndDate >= searchStart)
                .Select(h => new HolidayWindow(h.StartDate, h.EndDate))
                .ToListAsync(cancellationToken);

            holidays.AddRange(extraHolidays);

            var windowsResult = await GetScheduleGenerationWindowsAsync(
                classEntity,
                searchStart,
                batchEnd,
                cancellationToken);

            if (windowsResult.IsFailure)
            {
                return Result.Failure<List<ScheduleOccurrenceCandidate>>(windowsResult.Error);
            }

            foreach (var window in windowsResult.Value)
            {
                var parseResult = _patternParser.ParseAndGenerateOccurrenceDetails(
                    window.WeeklyScheduleJson,
                    window.EffectiveFrom,
                    window.EffectiveTo);

                if (parseResult.IsFailure)
                {
                    return Result.Failure<List<ScheduleOccurrenceCandidate>>(parseResult.Error);
                }

                foreach (var occurrence in parseResult.Value.OrderBy(x => x.PlannedDatetime))
                {
                    if (occurrence.DurationMinutes <= 0)
                    {
                        return Result.Failure<List<ScheduleOccurrenceCandidate>>(
                            SessionErrors.InvalidDuration(occurrence.DurationMinutes));
                    }

                    if (IsHoliday(occurrence.PlannedDatetime, holidays))
                    {
                        continue;
                    }

                    candidates.Add(new ScheduleOccurrenceCandidate(
                        occurrence.PlannedDatetime,
                        occurrence.DurationMinutes));

                    if (candidates.Count == targetCount)
                    {
                        break;
                    }
                }

                if (candidates.Count == targetCount)
                {
                    break;
                }
            }

            searchStart = batchEnd.AddDays(1);
        }

        return Result.Success(candidates);
    }

    private static bool IsHoliday(DateTime plannedDatetime, IReadOnlyCollection<HolidayWindow> holidays)
    {
        if (holidays.Count == 0)
        {
            return false;
        }

        var plannedDate = VietnamTime.ToVietnamDateOnly(plannedDatetime);
        return holidays.Any(h => h.StartDate <= plannedDate && h.EndDate >= plannedDate);
    }

    private sealed record ScheduleGenerationWindow(
        DateOnly EffectiveFrom,
        DateOnly EffectiveTo,
        string WeeklyScheduleJson);

    private sealed record ScheduleOccurrenceCandidate(
        DateTime PlannedDatetime,
        int DurationMinutes);

    private sealed record HolidayWindow(DateOnly StartDate, DateOnly EndDate);
}
