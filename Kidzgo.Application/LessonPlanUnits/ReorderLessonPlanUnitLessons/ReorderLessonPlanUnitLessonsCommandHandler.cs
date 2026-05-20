using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanUnits.ReorderLessonPlanUnitLessons;

public sealed class ReorderLessonPlanUnitLessonsCommandHandler(IDbContext context)
    : ICommandHandler<ReorderLessonPlanUnitLessonsCommand>
{
    public async Task<Result> Handle(ReorderLessonPlanUnitLessonsCommand command, CancellationToken cancellationToken)
    {
        if (command.Items.Any(x => x.OrderIndexInUnit < 0))
        {
            return Result.Failure(LessonPlanUnitErrors.InvalidOrderIndex);
        }

        var unitExists = await context.LessonPlanUnits
            .AnyAsync(x => x.Id == command.UnitId, cancellationToken);
        if (!unitExists)
        {
            return Result.Failure(LessonPlanUnitErrors.NotFound(command.UnitId));
        }

        var ids = command.Items.Select(x => x.Id).Distinct().ToList();
        var lessons = await context.LessonPlanTemplates
            .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
            .ToListAsync(cancellationToken);

        if (lessons.Count != ids.Count || lessons.Any(x => x.LessonPlanUnitId != command.UnitId))
        {
            return Result.Failure(LessonPlanUnitErrors.LessonMustStayInSameModule);
        }

        var orderById = command.Items.ToDictionary(x => x.Id, x => x.OrderIndexInUnit);
        var now = VietnamTime.UtcNow();
        foreach (var lesson in lessons)
        {
            lesson.OrderIndexInUnit = orderById[lesson.Id];
            lesson.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
