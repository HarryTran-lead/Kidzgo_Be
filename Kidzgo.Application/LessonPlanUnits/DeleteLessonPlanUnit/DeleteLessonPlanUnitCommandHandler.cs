using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanUnits.DeleteLessonPlanUnit;

public sealed class DeleteLessonPlanUnitCommandHandler(IDbContext context)
    : ICommandHandler<DeleteLessonPlanUnitCommand>
{
    public async Task<Result> Handle(DeleteLessonPlanUnitCommand command, CancellationToken cancellationToken)
    {
        var unit = await context.LessonPlanUnits
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (unit is null)
        {
            return Result.Failure(LessonPlanUnitErrors.NotFound(command.Id));
        }

        var lessonCount = await context.LessonPlanTemplates
            .CountAsync(x => x.LessonPlanUnitId == command.Id && !x.IsDeleted, cancellationToken);
        if (lessonCount > 0)
        {
            return Result.Failure(LessonPlanUnitErrors.HasLessonPlanTemplates(lessonCount));
        }

        context.LessonPlanUnits.Remove(unit);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
