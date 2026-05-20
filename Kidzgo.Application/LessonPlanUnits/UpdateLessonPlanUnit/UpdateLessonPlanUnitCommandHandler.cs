using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LessonPlanTemplates.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanUnits.UpdateLessonPlanUnit;

public sealed class UpdateLessonPlanUnitCommandHandler(IDbContext context)
    : ICommandHandler<UpdateLessonPlanUnitCommand, UpdateLessonPlanUnitResponse>
{
    public async Task<Result<UpdateLessonPlanUnitResponse>> Handle(
        UpdateLessonPlanUnitCommand command,
        CancellationToken cancellationToken)
    {
        var unit = await context.LessonPlanUnits
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (unit is null)
        {
            return Result.Failure<UpdateLessonPlanUnitResponse>(
                LessonPlanUnitErrors.NotFound(command.Id));
        }

        if (command.Name is not null)
        {
            var normalized = LessonPlanUnitNameNormalizer.Normalize(command.Name);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return Result.Failure<UpdateLessonPlanUnitResponse>(LessonPlanUnitErrors.NameRequired);
            }

            var duplicateExists = await context.LessonPlanUnits
                .AnyAsync(
                    x => x.ModuleId == unit.ModuleId &&
                         x.NameNormalized == normalized &&
                         x.Id != unit.Id,
                    cancellationToken);
            if (duplicateExists)
            {
                return Result.Failure<UpdateLessonPlanUnitResponse>(
                    LessonPlanUnitErrors.DuplicateName(unit.ModuleId, normalized));
            }

            unit.Name = normalized;
            unit.NameNormalized = normalized;
        }

        if (command.IsActive.HasValue)
        {
            unit.IsActive = command.IsActive.Value;
        }

        unit.UpdatedAt = VietnamTime.UtcNow();
        await context.SaveChangesAsync(cancellationToken);

        return new UpdateLessonPlanUnitResponse
        {
            Id = unit.Id,
            ModuleId = unit.ModuleId,
            Name = unit.Name,
            OrderIndex = unit.OrderIndex,
            IsActive = unit.IsActive,
            UpdatedAt = unit.UpdatedAt
        };
    }
}
