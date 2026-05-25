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
            var identity = LessonPlanUnitNameNormalizer.ExtractUnitIdentity(command.Name)
                           ?? new LessonPlanUnitIdentity(
                               CanonicalDisplayName: LessonPlanUnitNameNormalizer.Normalize(command.Name),
                               NormalizedKey: LessonPlanUnitNameNormalizer.Normalize(command.Name),
                               UnitNumber: null,
                               UnitTitle: null);
            if (string.IsNullOrWhiteSpace(identity.NormalizedKey))
            {
                return Result.Failure<UpdateLessonPlanUnitResponse>(LessonPlanUnitErrors.NameRequired);
            }

            var duplicateExists = await context.LessonPlanUnits
                .AnyAsync(
                    x => x.ModuleId == unit.ModuleId &&
                         x.NameNormalized == identity.NormalizedKey &&
                         x.Id != unit.Id,
                    cancellationToken);
            if (duplicateExists)
            {
                return Result.Failure<UpdateLessonPlanUnitResponse>(
                    LessonPlanUnitErrors.DuplicateName(unit.ModuleId, identity.NormalizedKey));
            }

            unit.Name = identity.CanonicalDisplayName;
            unit.NameNormalized = identity.NormalizedKey;
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
