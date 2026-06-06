using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Kidzgo.Infrastructure.BackgroundJobs;

public sealed class SyncPlannedToActualSessionsJob(
    IServiceScopeFactory scopeFactory,
    ILogger<SyncPlannedToActualSessionsJob> logger
) : IJob
{
    private static readonly TimeSpan LookbackWindow = TimeSpan.FromDays(1);

    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDbContext>();

        var now = VietnamTime.UtcNow();
        var from = now.Subtract(LookbackWindow);

        var sessions = await db.Sessions
            .Where(s =>
                s.Status == SessionStatus.Scheduled &&
                !s.ActualDatetime.HasValue &&
                s.PlannedDatetime <= now &&
                s.PlannedDatetime >= from)
            .Include(s => s.Class)
            .ToListAsync(cancellationToken);

        if (sessions.Count == 0)
        {
            return;
        }

        foreach (var s in sessions)
        {
            s.ActualDatetime ??= s.PlannedDatetime;
            s.ActualRoomId ??= s.PlannedRoomId;
            s.ActualTeacherId ??= s.PlannedTeacherId;
            s.ActualAssistantId ??= s.PlannedAssistantId;
            s.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Quartz job synced Planned* -> Actual* for {Count} sessions", sessions.Count);

        // Update class status from upcoming statuses to Active when the first session date arrives.
        var plannedClassIds = await db.Classes
            .Where(c => c.Status == ClassStatus.Planned || c.Status == ClassStatus.Recruiting)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (plannedClassIds.Count == 0)
        {
            return;
        }

        // Get first session date for each upcoming class.
        var firstSessionDates = await db.Sessions
            .Where(s => plannedClassIds.Contains(s.ClassId))
            .GroupBy(s => s.ClassId)
            .Select(g => new
            {
                ClassId = g.Key,
                FirstSessionDate = g.Min(s => s.PlannedDatetime)
            })
            .Where(x => x.FirstSessionDate <= now)
            .ToListAsync(cancellationToken);

        if (firstSessionDates.Count == 0)
        {
            return;
        }

        var classIdsToActivate = firstSessionDates.Select(x => x.ClassId).ToList();
        var classesToActivate = await db.Classes
            .Where(c => classIdsToActivate.Contains(c.Id))
            .ToListAsync(cancellationToken);

        foreach (var classEntity in classesToActivate)
        {
            classEntity.Status = ClassStatus.Active;
            classEntity.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Quartz job updated {Count} classes from upcoming statuses to Active", classesToActivate.Count);
    }
}


