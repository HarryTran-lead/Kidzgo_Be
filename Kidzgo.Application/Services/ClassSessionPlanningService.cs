using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class ClassSessionPlanningService(IDbContext context)
{
    public async Task<Result<List<PlannedSessionMetadata>>> PlanForClassAsync(
        Guid classId,
        int newSessionCount,
        bool strictCurriculumCoverage,
        CancellationToken cancellationToken)
    {
        if (newSessionCount <= 0)
        {
            return Result.Success(new List<PlannedSessionMetadata>());
        }

        var classEntity = await context.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);
        if (classEntity is null)
        {
            return Result.Failure<List<PlannedSessionMetadata>>(ClassErrors.NotFound(classId));
        }

        var existingSessionCount = await context.Sessions
            .AsNoTracking()
            .CountAsync(
                x => x.ClassId == classId &&
                     x.Status != SessionStatus.Cancelled,
                cancellationToken);

        return await PlanAsync(
            classEntity.LevelId,
            classEntity.StartModuleId,
            classEntity.StartSessionIndex,
            existingSessionCount,
            newSessionCount,
            strictCurriculumCoverage,
            cancellationToken);
    }

    public async Task<Result<List<PlannedSessionMetadata>>> PlanAsync(
        Guid levelId,
        Guid startModuleId,
        int startSessionIndex,
        int existingSessionCount,
        int newSessionCount,
        bool strictCurriculumCoverage,
        CancellationToken cancellationToken)
    {
        if (newSessionCount <= 0)
        {
            return Result.Success(new List<PlannedSessionMetadata>());
        }

        var modules = await context.Modules
            .AsNoTracking()
            .Where(x => x.LevelId == levelId && x.IsActive)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);

        var startModule = modules.FirstOrDefault(x => x.Id == startModuleId);
        if (startModule is null)
        {
            return Result.Failure<List<PlannedSessionMetadata>>(ClassErrors.StartModuleNotFound);
        }

        if (startSessionIndex < 1 || startSessionIndex > startModule.PlannedSessionCount)
        {
            return Result.Failure<List<PlannedSessionMetadata>>(
                ClassErrors.InvalidStartSessionIndex(startSessionIndex, startModule.PlannedSessionCount));
        }

        var orderedModules = modules
            .Where(x => x.Order >= startModule.Order)
            .OrderBy(x => x.Order)
            .ToList();

        var templates = await context.LessonPlanTemplates
            .AsNoTracking()
            .Where(x => orderedModules.Select(m => m.Id).Contains(x.ModuleId) && x.IsActive && !x.IsDeleted)
            .Select(x => new
            {
                x.Id,
                x.ModuleId,
                x.LessonPlanUnitId,
                UnitName = x.LessonPlanUnit != null ? x.LessonPlanUnit.Name : null,
                x.SessionIndex,
                x.Title
            })
            .ToListAsync(cancellationToken);

        var templateLookup = templates.ToDictionary(x => (x.ModuleId, x.SessionIndex));
        var totalRemainingSessions = GetTotalRemainingSessions(orderedModules, startModuleId, startSessionIndex);

        if (strictCurriculumCoverage && existingSessionCount + newSessionCount > totalRemainingSessions)
        {
            return Result.Failure<List<PlannedSessionMetadata>>(
                ClassErrors.NotEnoughCurriculumSessions(existingSessionCount + newSessionCount, totalRemainingSessions));
        }

        var planned = new List<PlannedSessionMetadata>(newSessionCount);
        for (var index = 0; index < newSessionCount; index++)
        {
            var curriculumOffset = existingSessionCount + index;
            if (!TryResolveCurriculumPosition(
                    orderedModules,
                    startModuleId,
                    startSessionIndex,
                    curriculumOffset,
                    out var resolvedModule,
                    out var sessionIndexInModule))
            {
                if (strictCurriculumCoverage)
                {
                    return Result.Failure<List<PlannedSessionMetadata>>(
                        ClassErrors.NotEnoughCurriculumSessions(existingSessionCount + newSessionCount, totalRemainingSessions));
                }

                planned.Add(new PlannedSessionMetadata
                {
                    ClassSessionNo = existingSessionCount + index + 1
                });

                continue;
            }

            if (!templateLookup.TryGetValue((resolvedModule.Id, sessionIndexInModule), out var template))
            {
                return Result.Failure<List<PlannedSessionMetadata>>(
                    ClassErrors.MissingLessonPlanTemplate(resolvedModule.Code, [sessionIndexInModule]));
            }

            planned.Add(new PlannedSessionMetadata
            {
                ClassSessionNo = existingSessionCount + index + 1,
                ModuleId = resolvedModule.Id,
                ModuleCode = resolvedModule.Code,
                ModuleName = resolvedModule.Name,
                LessonPlanTemplateId = template.Id,
                LessonPlanUnitId = template.LessonPlanUnitId,
                UnitName = template.UnitName,
                SessionIndexInModule = sessionIndexInModule,
                LessonTitle = template.Title
            });
        }

        return Result.Success(planned);
    }

    public async Task<Result> AssignMetadataAsync(
        Guid classId,
        IReadOnlyCollection<Session> sessions,
        bool strictCurriculumCoverage,
        CancellationToken cancellationToken)
    {
        if (sessions.Count == 0)
        {
            return Result.Success();
        }

        var orderedSessions = sessions
            .OrderBy(x => x.PlannedDatetime)
            .ThenBy(x => x.CreatedAt)
            .ToList();

        var planResult = await PlanForClassAsync(
            classId,
            orderedSessions.Count,
            strictCurriculumCoverage,
            cancellationToken);

        if (planResult.IsFailure)
        {
            return Result.Failure(planResult.Error);
        }

        for (var index = 0; index < orderedSessions.Count; index++)
        {
            var planned = planResult.Value[index];
            orderedSessions[index].ModuleId = planned.ModuleId;
            orderedSessions[index].LessonPlanTemplateId = planned.LessonPlanTemplateId;
            orderedSessions[index].SessionIndexInModule = planned.SessionIndexInModule;
        }

        return Result.Success();
    }

    private static int GetTotalRemainingSessions(
        IReadOnlyCollection<Module> orderedModules,
        Guid startModuleId,
        int startSessionIndex)
    {
        var total = 0;
        foreach (var module in orderedModules.OrderBy(x => x.Order))
        {
            if (module.Id == startModuleId)
            {
                total += module.PlannedSessionCount - startSessionIndex + 1;
                continue;
            }

            total += module.PlannedSessionCount;
        }

        return total;
    }

    private static bool TryResolveCurriculumPosition(
        IReadOnlyCollection<Module> orderedModules,
        Guid startModuleId,
        int startSessionIndex,
        int curriculumOffset,
        out Module module,
        out int sessionIndexInModule)
    {
        var remainingOffset = curriculumOffset;
        foreach (var currentModule in orderedModules.OrderBy(x => x.Order))
        {
            var firstSessionIndex = currentModule.Id == startModuleId ? startSessionIndex : 1;
            var availableSessions = currentModule.PlannedSessionCount - firstSessionIndex + 1;
            if (availableSessions <= 0)
            {
                continue;
            }

            if (remainingOffset < availableSessions)
            {
                module = currentModule;
                sessionIndexInModule = firstSessionIndex + remainingOffset;
                return true;
            }

            remainingOffset -= availableSessions;
        }

        module = null!;
        sessionIndexInModule = 0;
        return false;
    }
}

public sealed class PlannedSessionMetadata
{
    public int ClassSessionNo { get; init; }
    public Guid? ModuleId { get; init; }
    public string? ModuleCode { get; init; }
    public string? ModuleName { get; init; }
    public Guid? LessonPlanUnitId { get; init; }
    public string? UnitName { get; init; }
    public Guid? LessonPlanTemplateId { get; init; }
    public int? SessionIndexInModule { get; init; }
    public string? LessonTitle { get; init; }
}
