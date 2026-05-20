using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanUnits.ReorderLessonPlanUnits;

public sealed class ReorderLessonPlanUnitsCommandHandler(IDbContext context)
    : ICommandHandler<ReorderLessonPlanUnitsCommand>
{
    public async Task<Result> Handle(ReorderLessonPlanUnitsCommand command, CancellationToken cancellationToken)
    {
        if (command.Items.Any(x => x.OrderIndex < 0))
        {
            return Result.Failure(LessonPlanUnitErrors.InvalidOrderIndex);
        }

        var ids = command.Items.Select(x => x.Id).Distinct().ToList();
        var units = await context.LessonPlanUnits
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (units.Count != ids.Count || units.Any(x => x.ModuleId != command.ModuleId))
        {
            return Result.Failure(LessonPlanUnitErrors.ModuleNotFound(command.ModuleId));
        }

        var orderById = command.Items.ToDictionary(x => x.Id, x => x.OrderIndex);
        var now = VietnamTime.UtcNow();
        foreach (var unit in units)
        {
            unit.OrderIndex = orderById[unit.Id];
            unit.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
