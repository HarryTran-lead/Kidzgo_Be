using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed class ClassProgressionService(IDbContext context)
{
    public async Task AdvanceAsync(Guid classId, Guid? moduleId, CancellationToken cancellationToken)
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

        progress.CompletedSessions = Math.Min(progress.RequiredSessions, progress.CompletedSessions + 1);
        progress.UpdatedAt = now;

        if (progress.CompletedSessions < progress.RequiredSessions)
        {
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
            classEntity.UpdatedAt = now;
            return;
        }

        classEntity.CurrentModuleId = nextProgress.ModuleId;
        classEntity.UpdatedAt = now;
        if (nextProgress.Status == ClassModuleProgressStatus.Pending)
        {
            nextProgress.Status = ClassModuleProgressStatus.Active;
            nextProgress.StartedAt ??= now;
            nextProgress.UpdatedAt = now;
        }
    }
}
