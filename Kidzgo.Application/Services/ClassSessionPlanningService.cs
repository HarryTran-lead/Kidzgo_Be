using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class ClassSessionPlanningService(IDbContext context)
{
    public async Task AssignMetadataAsync(Guid classId, IReadOnlyCollection<Session> sessions, CancellationToken cancellationToken)
    {
        if (sessions.Count == 0)
        {
            return;
        }

        var classEntity = await context.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);
        if (classEntity is null)
        {
            return;
        }

        var progresses = await context.ClassModuleProgresses
            .AsNoTracking()
            .Where(x => x.ClassId == classId)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken);
        if (progresses.Count == 0)
        {
            return;
        }

        var plannableProgresses = progresses
            .Where(x => x.Status != ClassModuleProgressStatus.Skipped)
            .OrderBy(x => x.OrderIndex)
            .ToList();
        if (plannableProgresses.Count == 0)
        {
            return;
        }

        var templates = await context.LessonPlanTemplates
            .AsNoTracking()
            .Where(x => plannableProgresses.Select(p => p.ModuleId).Contains(x.ModuleId) && x.IsActive && !x.IsDeleted)
            .ToListAsync(cancellationToken);
        var templateLookup = templates.ToDictionary(x => (x.ModuleId, x.SessionIndex), x => x.Id);

        var existingSessionCount = await context.Sessions
            .AsNoTracking()
            .CountAsync(
                x => x.ClassId == classId &&
                     x.Status != SessionStatus.Cancelled,
                cancellationToken);

        var orderedNewSessions = sessions
            .OrderBy(x => x.PlannedDatetime)
            .ThenBy(x => x.CreatedAt)
            .ToList();

        var totalRequiredSessions = plannableProgresses.Sum(x => x.RequiredSessions);
        for (var index = 0; index < orderedNewSessions.Count; index++)
        {
            var sessionNumber = existingSessionCount + index + 1;
            if (sessionNumber > totalRequiredSessions)
            {
                orderedNewSessions[index].ModuleId = null;
                orderedNewSessions[index].LessonPlanTemplateId = null;
                orderedNewSessions[index].SessionIndexInModule = null;
                continue;
            }

            var running = 0;
            foreach (var progress in plannableProgresses)
            {
                var nextRunning = running + progress.RequiredSessions;
                if (sessionNumber <= nextRunning)
                {
                    var sessionIndexInModule = sessionNumber - running;
                    orderedNewSessions[index].ModuleId = progress.ModuleId;
                    orderedNewSessions[index].SessionIndexInModule = sessionIndexInModule;
                    orderedNewSessions[index].LessonPlanTemplateId = templateLookup.GetValueOrDefault((progress.ModuleId, sessionIndexInModule));
                    break;
                }

                running = nextRunning;
            }
        }
    }
}
