using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LessonPlanTemplates.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanTemplates.MoveLessonPlanTemplateUnit;

public sealed class MoveLessonPlanTemplateUnitCommandHandler(IDbContext context)
    : ICommandHandler<MoveLessonPlanTemplateUnitCommand, MoveLessonPlanTemplateUnitResponse>
{
    public async Task<Result<MoveLessonPlanTemplateUnitResponse>> Handle(
        MoveLessonPlanTemplateUnitCommand command,
        CancellationToken cancellationToken)
    {
        var template = await context.LessonPlanTemplates
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, cancellationToken);
        if (template is null)
        {
            return Result.Failure<MoveLessonPlanTemplateUnitResponse>(
                LessonPlanTemplateErrors.NotFound(command.Id));
        }

        if (command.LessonPlanUnitId.HasValue)
        {
            var unit = await context.LessonPlanUnits
                .FirstOrDefaultAsync(x => x.Id == command.LessonPlanUnitId.Value, cancellationToken);
            if (unit is null)
            {
                return Result.Failure<MoveLessonPlanTemplateUnitResponse>(
                    LessonPlanUnitErrors.NotFound(command.LessonPlanUnitId.Value));
            }

            if (unit.ModuleId != template.ModuleId)
            {
                return Result.Failure<MoveLessonPlanTemplateUnitResponse>(
                    LessonPlanUnitErrors.LessonMustStayInSameModule);
            }
        }

        template.LessonPlanUnitId = command.LessonPlanUnitId;
        template.OrderIndexInUnit = command.OrderIndexInUnit.HasValue
            ? Math.Max(command.OrderIndexInUnit.Value, 0)
            : command.LessonPlanUnitId.HasValue
                ? await LessonPlanUnitResolver.GetNextOrderInUnitAsync(context, command.LessonPlanUnitId.Value, cancellationToken)
                : 0;
        template.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return new MoveLessonPlanTemplateUnitResponse
        {
            Id = template.Id,
            ModuleId = template.ModuleId,
            LessonPlanUnitId = template.LessonPlanUnitId,
            OrderIndexInUnit = template.OrderIndexInUnit,
            UpdatedAt = template.UpdatedAt
        };
    }
}
