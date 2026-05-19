using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlans.Shared;

internal static class LessonPlanTemplateResolver
{
    public static async Task<LessonPlanTemplate?> ResolveForSessionAsync(
        IDbContext context,
        Guid classId,
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var sessionMetadata = await context.Sessions
            .Where(s => s.ClassId == classId && s.Id == sessionId)
            .Select(s => new
            {
                s.ModuleId,
                s.LessonPlanTemplateId,
                s.SessionIndexInModule
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (sessionMetadata is null)
        {
            return null;
        }

        if (sessionMetadata.LessonPlanTemplateId.HasValue)
        {
            return await context.LessonPlanTemplates
                .FirstOrDefaultAsync(
                    t => t.Id == sessionMetadata.LessonPlanTemplateId.Value &&
                         t.IsActive &&
                         !t.IsDeleted,
                    cancellationToken);
        }

        if (!sessionMetadata.ModuleId.HasValue || !sessionMetadata.SessionIndexInModule.HasValue)
        {
            return null;
        }

        return await context.LessonPlanTemplates
            .FirstOrDefaultAsync(
                t => t.ModuleId == sessionMetadata.ModuleId.Value &&
                     t.SessionIndex == sessionMetadata.SessionIndexInModule.Value &&
                     t.IsActive &&
                     !t.IsDeleted,
                cancellationToken);
    }
}
