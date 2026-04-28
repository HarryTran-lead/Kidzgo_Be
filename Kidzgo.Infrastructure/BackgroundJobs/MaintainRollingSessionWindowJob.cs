using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Kidzgo.Infrastructure.BackgroundJobs;

public sealed class MaintainRollingSessionWindowJob(
    IServiceScopeFactory scopeFactory,
    ILogger<MaintainRollingSessionWindowJob> logger
) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDbContext>();
        var sessionGenerationService = scope.ServiceProvider.GetRequiredService<SessionGenerationService>();

        var classes = await db.Classes
            .Where(c =>
                c.Status == ClassStatus.Planned ||
                c.Status == ClassStatus.Recruiting ||
                c.Status == ClassStatus.Active)
            .ToListAsync(cancellationToken);

        var createdCount = 0;
        foreach (var classEntity in classes)
        {
            var result = await sessionGenerationService.GenerateSessionsFromPatternAsync(
                classEntity,
                onlyFutureSessions: true,
                cancellationToken);

            if (result.IsSuccess)
            {
                createdCount += result.Value;
                continue;
            }

            logger.LogWarning(
                "Rolling session generation skipped class {ClassId}: {ErrorCode} - {ErrorDescription}",
                classEntity.Id,
                result.Error.Code,
                result.Error.Description);
        }

        if (createdCount > 0)
        {
            logger.LogInformation("Rolling session generation created {Count} sessions", createdCount);
        }
    }
}
