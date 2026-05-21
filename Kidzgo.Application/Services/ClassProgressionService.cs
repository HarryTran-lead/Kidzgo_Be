using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Sessions;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class ClassProgressionService(
    IDbContext context,
    ClassSessionPlanningService classSessionPlanningService)
{
    public Task AdvanceAsync(Guid classId, Guid? moduleId, CancellationToken cancellationToken)
    {
        return ApplySessionProgressAsync(
            classId,
            moduleId,
            countClassSession: true,
            consumeLesson: true,
            cancellationToken);
    }

    public async Task ApplySessionProgressAsync(
        Guid classId,
        Guid? moduleId,
        bool countClassSession,
        bool consumeLesson,
        CancellationToken cancellationToken)
    {
        if (!moduleId.HasValue)
        {
            return;
        }

        var classEntity = await context.Classes
            .Include(x => x.ModuleProgresses)
            .FirstOrDefaultAsync(x => x.Id == classId, cancellationToken);
        if (classEntity is null)
        {
            return;
        }

        var progress = classEntity.ModuleProgresses
            .OrderBy(x => x.OrderIndex)
            .FirstOrDefault(x => x.ModuleId == moduleId.Value);
        if (progress is null)
        {
            return;
        }

        var now = VietnamTime.UtcNow();
        if (progress.Status == ClassModuleProgressStatus.Pending)
        {
            progress.Status = ClassModuleProgressStatus.Active;
            progress.StartedAt ??= now;
        }

        if (countClassSession)
        {
            progress.CompletedClassSessions = Math.Min(progress.RequiredSessions, progress.CompletedClassSessions + 1);
        }

        if (consumeLesson)
        {
            progress.CompletedLessonPlans = Math.Min(progress.RequiredSessions, progress.CompletedLessonPlans + 1);
            progress.CurrentSessionIndex = Math.Min(progress.RequiredSessions + 1, progress.CurrentSessionIndex + 1);
        }

        progress.UpdatedAt = now;

        if (!consumeLesson)
        {
            await UpdateCurrentCursorAsync(classEntity, progress.ModuleId, progress.CurrentSessionIndex, now, cancellationToken);
            return;
        }

        if (progress.CompletedLessonPlans < progress.RequiredSessions)
        {
            await UpdateCurrentCursorAsync(classEntity, progress.ModuleId, progress.CurrentSessionIndex, now, cancellationToken);
            return;
        }

        progress.Status = ClassModuleProgressStatus.Completed;
        progress.CompletedAt ??= now;

        var nextProgress = classEntity.ModuleProgresses
            .Where(x => x.OrderIndex > progress.OrderIndex && x.Status != ClassModuleProgressStatus.Skipped)
            .OrderBy(x => x.OrderIndex)
            .FirstOrDefault();

        if (nextProgress is null)
        {
            classEntity.Status = ClassStatus.Completed;
            classEntity.CurrentModuleId = progress.ModuleId;
            classEntity.CurrentSessionIndex = progress.RequiredSessions + 1;
            classEntity.CurrentLessonPlanTemplateId = null;
            classEntity.ActualEndDate = VietnamTime.ToVietnamDateOnly(now);
            classEntity.UpdatedAt = now;
            return;
        }

        if (nextProgress.Status == ClassModuleProgressStatus.Pending)
        {
            nextProgress.Status = ClassModuleProgressStatus.Active;
            nextProgress.StartedAt ??= now;
            nextProgress.UpdatedAt = now;
        }

        await UpdateCurrentCursorAsync(classEntity, nextProgress.ModuleId, nextProgress.CurrentSessionIndex, now, cancellationToken);
    }

    public async Task<ClassRuntimeSyncResult?> RecalculateAndResyncAsync(
        Guid classId,
        bool resyncFutureSessions,
        CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .Include(x => x.ModuleProgresses)
            .FirstOrDefaultAsync(x => x.Id == classId, cancellationToken);
        if (classEntity is null)
        {
            return null;
        }

        var modules = await context.Modules
            .AsNoTracking()
            .Where(x => x.LevelId == classEntity.LevelId && x.IsActive)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);

        var orderedProgresses = classEntity.ModuleProgresses
            .OrderBy(x => x.OrderIndex)
            .ToList();
        if (orderedProgresses.Count == 0)
        {
            return new ClassRuntimeSyncResult
            {
                ClassId = classEntity.Id,
                CurrentModuleId = classEntity.CurrentModuleId,
                CurrentSessionIndex = classEntity.CurrentSessionIndex,
                CurrentLessonPlanTemplateId = classEntity.CurrentLessonPlanTemplateId
            };
        }

        var progressByModuleId = orderedProgresses.ToDictionary(x => x.ModuleId);
        var startModule = modules.FirstOrDefault(x => x.Id == classEntity.StartModuleId);
        if (startModule is null)
        {
            return new ClassRuntimeSyncResult
            {
                ClassId = classEntity.Id,
                CurrentModuleId = classEntity.CurrentModuleId,
                CurrentSessionIndex = classEntity.CurrentSessionIndex,
                CurrentLessonPlanTemplateId = classEntity.CurrentLessonPlanTemplateId
            };
        }

        var now = VietnamTime.UtcNow();
        ResetProgressState(classEntity, orderedProgresses, modules, startModule, now);

        var completedSessions = await context.Sessions
            .AsNoTracking()
            .Include(x => x.TeachingLog)
                .ThenInclude(x => x!.Lessons)
            .Where(x => x.ClassId == classId && x.Status == SessionStatus.Completed)
            .OrderBy(x => x.PlannedDatetime)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        foreach (var session in completedSessions)
        {
            var operationalProgress = ResolveOperationalProgress(progressByModuleId, session.ModuleId, classEntity.CurrentModuleId);
            if (operationalProgress is not null)
            {
                EnsureActive(operationalProgress, session.ActualDatetime ?? session.PlannedDatetime);
                operationalProgress.CompletedClassSessions = Math.Min(
                    operationalProgress.RequiredSessions,
                    operationalProgress.CompletedClassSessions + 1);
                operationalProgress.UpdatedAt = now;
            }

            var activeProgress = GetCurrentCurriculumProgress(orderedProgresses);
            if (activeProgress is null)
            {
                continue;
            }

            if (session.TeachingLog is null)
            {
                ConsumeCurrentLesson(activeProgress, orderedProgresses, session.ActualDatetime ?? session.PlannedDatetime, now);
                continue;
            }

            var lessonProgress = session.TeachingLog.Lessons
                .OrderBy(x => x.OrderIndex)
                .FirstOrDefault();
            if (lessonProgress is null)
            {
                continue;
            }

            if (TeachingLogProgressSupport.ShouldConsumeLesson(lessonProgress.ProgressStatus))
            {
                ConsumeCurrentLesson(activeProgress, orderedProgresses, session.ActualDatetime ?? session.PlannedDatetime, now);
            }
        }

        ApplyClassStatus(classEntity, orderedProgresses, now);
        await UpdateCurrentLessonTemplateAsync(classEntity, cancellationToken);

        var updatedSessionCount = 0;
        if (resyncFutureSessions)
        {
            updatedSessionCount = await ResyncFutureSessionsAsync(classEntity, now, cancellationToken);
        }

        return new ClassRuntimeSyncResult
        {
            ClassId = classEntity.Id,
            CurrentModuleId = classEntity.CurrentModuleId,
            CurrentSessionIndex = classEntity.CurrentSessionIndex,
            CurrentLessonPlanTemplateId = classEntity.CurrentLessonPlanTemplateId,
            UpdatedFutureSessionCount = updatedSessionCount
        };
    }

    public async Task<int> ResyncFutureSessionsAsync(Guid classId, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .FirstOrDefaultAsync(x => x.Id == classId, cancellationToken);
        if (classEntity is null)
        {
            return 0;
        }

        return await ResyncFutureSessionsAsync(classEntity, VietnamTime.UtcNow(), cancellationToken);
    }

    private async Task<int> ResyncFutureSessionsAsync(
        Class classEntity,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var futureSessions = await context.Sessions
            .Where(x => x.ClassId == classEntity.Id &&
                        x.Status != SessionStatus.Cancelled &&
                        x.Status != SessionStatus.Completed &&
                        x.PlannedDatetime >= now)
            .OrderBy(x => x.PlannedDatetime)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        if (futureSessions.Count == 0)
        {
            return 0;
        }

        var planResult = await classSessionPlanningService.PlanAsync(
            classEntity.LevelId,
            classEntity.CurrentModuleId,
            classEntity.CurrentSessionIndex,
            existingSessionCount: 0,
            newSessionCount: futureSessions.Count,
            strictCurriculumCoverage: false,
            cancellationToken);
        if (planResult.IsFailure)
        {
            return 0;
        }

        for (var index = 0; index < futureSessions.Count; index++)
        {
            futureSessions[index].ModuleId = planResult.Value[index].ModuleId;
            futureSessions[index].LessonPlanTemplateId = planResult.Value[index].LessonPlanTemplateId;
            futureSessions[index].SessionIndexInModule = planResult.Value[index].SessionIndexInModule;
            futureSessions[index].UpdatedAt = now;
        }

        classEntity.CurrentLessonPlanTemplateId = planResult.Value[0].LessonPlanTemplateId;
        classEntity.UpdatedAt = now;
        return futureSessions.Count;
    }

    private static void ResetProgressState(
        Class classEntity,
        IReadOnlyCollection<ClassModuleProgress> orderedProgresses,
        IReadOnlyCollection<Domain.Programs.Module> orderedModules,
        Domain.Programs.Module startModule,
        DateTime now)
    {
        foreach (var progress in orderedProgresses)
        {
            progress.CompletedClassSessions = 0;
            progress.CompletedLessonPlans = 0;
            progress.StartSessionIndex = progress.ModuleId == classEntity.StartModuleId
                ? classEntity.StartSessionIndex
                : 1;
            progress.CurrentSessionIndex = progress.StartSessionIndex;
            progress.StartedAt = null;
            progress.CompletedAt = null;
            progress.UpdatedAt = now;

            var moduleOrder = orderedModules.FirstOrDefault(x => x.Id == progress.ModuleId)?.Order ?? progress.OrderIndex;
            progress.Status = moduleOrder < startModule.Order
                ? ClassModuleProgressStatus.Skipped
                : progress.ModuleId == classEntity.StartModuleId
                    ? ClassModuleProgressStatus.Active
                    : ClassModuleProgressStatus.Pending;
        }

        classEntity.CurrentModuleId = classEntity.StartModuleId;
        classEntity.CurrentSessionIndex = classEntity.StartSessionIndex;
        classEntity.CurrentLessonPlanTemplateId = null;
        classEntity.ActualEndDate = null;
        classEntity.UpdatedAt = now;
    }

    private static ClassModuleProgress? ResolveOperationalProgress(
        IReadOnlyDictionary<Guid, ClassModuleProgress> progressByModuleId,
        Guid? sessionModuleId,
        Guid currentModuleId)
    {
        if (sessionModuleId.HasValue && progressByModuleId.TryGetValue(sessionModuleId.Value, out var progress))
        {
            return progress;
        }

        return progressByModuleId.TryGetValue(currentModuleId, out var currentProgress)
            ? currentProgress
            : null;
    }

    private static void EnsureActive(ClassModuleProgress progress, DateTime startedAt)
    {
        if (progress.Status == ClassModuleProgressStatus.Pending)
        {
            progress.Status = ClassModuleProgressStatus.Active;
        }

        progress.StartedAt ??= startedAt;
    }

    private static ClassModuleProgress? GetCurrentCurriculumProgress(
        IReadOnlyCollection<ClassModuleProgress> orderedProgresses)
    {
        return orderedProgresses
            .Where(x => x.Status != ClassModuleProgressStatus.Skipped && x.Status != ClassModuleProgressStatus.Completed)
            .OrderBy(x => x.OrderIndex)
            .FirstOrDefault();
    }

    private static void ConsumeCurrentLesson(
        ClassModuleProgress activeProgress,
        IReadOnlyCollection<ClassModuleProgress> orderedProgresses,
        DateTime completedAt,
        DateTime now)
    {
        EnsureActive(activeProgress, completedAt);
        activeProgress.CompletedLessonPlans = Math.Min(activeProgress.RequiredSessions, activeProgress.CompletedLessonPlans + 1);
        activeProgress.CurrentSessionIndex = Math.Min(activeProgress.RequiredSessions + 1, activeProgress.CurrentSessionIndex + 1);
        activeProgress.UpdatedAt = now;

        if (activeProgress.CompletedLessonPlans < activeProgress.RequiredSessions)
        {
            return;
        }

        activeProgress.Status = ClassModuleProgressStatus.Completed;
        activeProgress.CompletedAt ??= completedAt;

        var nextProgress = orderedProgresses
            .Where(x => x.OrderIndex > activeProgress.OrderIndex && x.Status != ClassModuleProgressStatus.Skipped)
            .OrderBy(x => x.OrderIndex)
            .FirstOrDefault();
        if (nextProgress is null)
        {
            return;
        }

        EnsureActive(nextProgress, completedAt);
        nextProgress.UpdatedAt = now;
    }

    private static void ApplyClassStatus(
        Class classEntity,
        IReadOnlyCollection<ClassModuleProgress> orderedProgresses,
        DateTime now)
    {
        var currentProgress = GetCurrentCurriculumProgress(orderedProgresses);
        if (currentProgress is null)
        {
            var lastProgress = orderedProgresses
                .Where(x => x.Status != ClassModuleProgressStatus.Skipped)
                .OrderBy(x => x.OrderIndex)
                .LastOrDefault();
            if (lastProgress is null)
            {
                return;
            }

            classEntity.CurrentModuleId = lastProgress.ModuleId;
            classEntity.CurrentSessionIndex = lastProgress.RequiredSessions + 1;
            classEntity.CurrentLessonPlanTemplateId = null;
            classEntity.ActualEndDate = VietnamTime.ToVietnamDateOnly(now);
            if (classEntity.Status != ClassStatus.Cancelled)
            {
                classEntity.Status = ClassStatus.Completed;
            }
            classEntity.UpdatedAt = now;
            return;
        }

        classEntity.CurrentModuleId = currentProgress.ModuleId;
        classEntity.CurrentSessionIndex = currentProgress.CurrentSessionIndex;
        classEntity.ActualEndDate = null;
        if (classEntity.Status != ClassStatus.Cancelled)
        {
            var hasOperationalProgress = orderedProgresses.Any(x => x.CompletedClassSessions > 0);
            classEntity.Status = hasOperationalProgress || classEntity.StartDate <= VietnamTime.TodayDateOnly()
                ? ClassStatus.Active
                : ClassStatus.Planned;
        }
        classEntity.UpdatedAt = now;
    }

    private async Task UpdateCurrentCursorAsync(
        Class classEntity,
        Guid moduleId,
        int currentSessionIndex,
        DateTime now,
        CancellationToken cancellationToken)
    {
        classEntity.CurrentModuleId = moduleId;
        classEntity.CurrentSessionIndex = currentSessionIndex;
        classEntity.CurrentLessonPlanTemplateId = await ResolveLessonTemplateIdAsync(
            moduleId,
            currentSessionIndex,
            cancellationToken);
        classEntity.UpdatedAt = now;
    }

    private async Task UpdateCurrentLessonTemplateAsync(
        Class classEntity,
        CancellationToken cancellationToken)
    {
        classEntity.CurrentLessonPlanTemplateId = await ResolveLessonTemplateIdAsync(
            classEntity.CurrentModuleId,
            classEntity.CurrentSessionIndex,
            cancellationToken);
    }

    private async Task<Guid?> ResolveLessonTemplateIdAsync(
        Guid moduleId,
        int sessionIndex,
        CancellationToken cancellationToken)
    {
        return await context.LessonPlanTemplates
            .AsNoTracking()
            .Where(x => x.ModuleId == moduleId &&
                        x.SessionIndex == sessionIndex &&
                        x.IsActive &&
                        !x.IsDeleted)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public sealed class ClassRuntimeSyncResult
{
    public Guid ClassId { get; init; }
    public Guid CurrentModuleId { get; init; }
    public int CurrentSessionIndex { get; init; }
    public Guid? CurrentLessonPlanTemplateId { get; init; }
    public int UpdatedFutureSessionCount { get; init; }
}
