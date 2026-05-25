using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
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
            classEntity.SyllabusId,
            classEntity.LevelId,
            classEntity.StartModuleId,
            classEntity.StartSessionIndex,
            existingSessionCount,
            newSessionCount,
            strictCurriculumCoverage,
            cancellationToken);
    }

    public async Task<Result<List<PlannedSessionMetadata>>> PlanAsync(
        Guid? syllabusId,
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

        return syllabusId.HasValue
            ? await PlanBySyllabusAsync(
                syllabusId.Value,
                orderedModules,
                startModuleId,
                startSessionIndex,
                existingSessionCount,
                newSessionCount,
                strictCurriculumCoverage,
                cancellationToken)
            : await PlanLegacyAsync(
                orderedModules,
                startModuleId,
                startSessionIndex,
                existingSessionCount,
                newSessionCount,
                strictCurriculumCoverage,
                cancellationToken);
    }

    private async Task<Result<List<PlannedSessionMetadata>>> PlanBySyllabusAsync(
        Guid syllabusId,
        IReadOnlyCollection<Module> orderedModules,
        Guid startModuleId,
        int startSessionIndex,
        int existingSessionCount,
        int newSessionCount,
        bool strictCurriculumCoverage,
        CancellationToken cancellationToken)
    {
        var moduleIds = orderedModules.Select(x => x.Id).ToList();
        var sessionTemplates = await context.SessionTemplates
            .AsNoTracking()
            .Where(x => x.SyllabusId == syllabusId &&
                        x.IsActive &&
                        x.ModuleId.HasValue &&
                        x.SessionIndexInModule.HasValue &&
                        moduleIds.Contains(x.ModuleId.Value))
            .Select(x => new
            {
                x.Id,
                x.ModuleId,
                x.SessionIndexInModule,
                x.OrderIndex,
                x.Title,
                x.Topic,
                x.LessonPlanTemplateId,
                x.SyllabusId,
                SyllabusCode = x.Syllabus.Code,
                SyllabusVersion = x.Syllabus.Version,
                SyllabusTitle = x.Syllabus.Title
            })
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken);

        var startTemplate = sessionTemplates.FirstOrDefault(
            x => x.ModuleId == startModuleId && x.SessionIndexInModule == startSessionIndex);
        if (startTemplate is null)
        {
            return Result.Failure<List<PlannedSessionMetadata>>(
                ClassErrors.MissingLessonPlanTemplate(
                    orderedModules.First(x => x.Id == startModuleId).Code,
                    [startSessionIndex]));
        }

        var remainingTemplates = sessionTemplates
            .Where(x => x.OrderIndex >= startTemplate.OrderIndex)
            .OrderBy(x => x.OrderIndex)
            .ToList();

        if (strictCurriculumCoverage && existingSessionCount + newSessionCount > remainingTemplates.Count)
        {
            return Result.Failure<List<PlannedSessionMetadata>>(
                ClassErrors.NotEnoughCurriculumSessions(existingSessionCount + newSessionCount, remainingTemplates.Count));
        }

        var templates = await context.LessonPlanTemplates
            .AsNoTracking()
            .Where(x => x.SyllabusId == syllabusId &&
                        x.IsActive &&
                        !x.IsDeleted &&
                        moduleIds.Contains(x.ModuleId))
            .Select(x => new
            {
                x.Id,
                x.ModuleId,
                x.LessonPlanUnitId,
                UnitName = x.LessonPlanUnit != null ? x.LessonPlanUnit.Name : null,
                x.SessionIndex,
                x.Title,
                x.Objectives,
                x.Procedure
            })
            .ToListAsync(cancellationToken);

        var templatesById = templates.ToDictionary(x => x.Id);
        var templateLookup = templates.ToDictionary(x => (x.ModuleId, x.SessionIndex));

        var planned = new List<PlannedSessionMetadata>(newSessionCount);
        for (var index = 0; index < newSessionCount; index++)
        {
            var curriculumOffset = existingSessionCount + index;
            if (curriculumOffset >= remainingTemplates.Count)
            {
                if (strictCurriculumCoverage)
                {
                    return Result.Failure<List<PlannedSessionMetadata>>(
                        ClassErrors.NotEnoughCurriculumSessions(existingSessionCount + newSessionCount, remainingTemplates.Count));
                }

                planned.Add(new PlannedSessionMetadata
                {
                    ClassSessionNo = existingSessionCount + index + 1
                });
                continue;
            }

            var resolved = remainingTemplates[curriculumOffset];
            var resolvedModule = orderedModules.First(x => x.Id == resolved.ModuleId);
            var resolvedTemplate = resolved.LessonPlanTemplateId.HasValue &&
                                   templatesById.TryGetValue(resolved.LessonPlanTemplateId.Value, out var linkedTemplate)
                ? linkedTemplate
                : templateLookup.GetValueOrDefault((resolved.ModuleId!.Value, resolved.SessionIndexInModule!.Value));

            if (resolvedTemplate is null)
            {
                return Result.Failure<List<PlannedSessionMetadata>>(
                    ClassErrors.MissingLessonPlanTemplate(
                        resolvedModule.Code,
                        [resolved.SessionIndexInModule!.Value]));
            }

            planned.Add(new PlannedSessionMetadata
            {
                ClassSessionNo = existingSessionCount + index + 1,
                ModuleId = resolvedModule.Id,
                ModuleCode = resolvedModule.Code,
                ModuleName = resolvedModule.Name,
                LessonPlanTemplateId = resolvedTemplate.Id,
                LessonPlanUnitId = resolvedTemplate.LessonPlanUnitId,
                UnitName = resolvedTemplate.UnitName,
                SessionIndexInModule = resolved.SessionIndexInModule,
                LessonTitle = resolvedTemplate.Title ?? resolved.Title ?? resolved.Topic,
                SyllabusId = resolved.SyllabusId,
                SyllabusCode = resolved.SyllabusCode,
                SyllabusVersion = resolved.SyllabusVersion,
                SyllabusTitle = resolved.SyllabusTitle,
                Objectives = resolvedTemplate.Objectives,
                Procedure = resolvedTemplate.Procedure
            });
        }

        return Result.Success(planned);
    }

    private async Task<Result<List<PlannedSessionMetadata>>> PlanLegacyAsync(
        IReadOnlyCollection<Module> orderedModules,
        Guid startModuleId,
        int startSessionIndex,
        int existingSessionCount,
        int newSessionCount,
        bool strictCurriculumCoverage,
        CancellationToken cancellationToken)
    {
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
                x.Title,
                x.SyllabusId,
                SyllabusCode = x.Syllabus.Code,
                SyllabusVersion = x.Syllabus.Version,
                SyllabusTitle = x.Syllabus.Title,
                x.Objectives,
                x.Procedure
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
                LessonTitle = template.Title,
                SyllabusId = template.SyllabusId,
                SyllabusCode = template.SyllabusCode,
                SyllabusVersion = template.SyllabusVersion,
                SyllabusTitle = template.SyllabusTitle,
                Objectives = template.Objectives,
                Procedure = template.Procedure
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
    public Guid? SyllabusId { get; init; }
    public string? SyllabusCode { get; init; }
    public string? SyllabusVersion { get; init; }
    public string? SyllabusTitle { get; init; }
    public string? Objectives { get; init; }
    public string? Procedure { get; init; }
}
