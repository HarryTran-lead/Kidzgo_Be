using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LessonPlanTemplates.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanUnits.CreateLessonPlanUnit;

public sealed class CreateLessonPlanUnitCommandHandler(IDbContext context)
    : ICommandHandler<CreateLessonPlanUnitCommand, CreateLessonPlanUnitResponse>
{
    public async Task<Result<CreateLessonPlanUnitResponse>> Handle(
        CreateLessonPlanUnitCommand command,
        CancellationToken cancellationToken)
    {
        var identity = LessonPlanUnitNameNormalizer.ExtractUnitIdentity(command.Name)
                       ?? new LessonPlanUnitIdentity(
                           CanonicalDisplayName: LessonPlanUnitNameNormalizer.Normalize(command.Name),
                           NormalizedKey: LessonPlanUnitNameNormalizer.Normalize(command.Name),
                           UnitNumber: null,
                           UnitTitle: null);
        if (string.IsNullOrWhiteSpace(identity.NormalizedKey))
        {
            return Result.Failure<CreateLessonPlanUnitResponse>(LessonPlanUnitErrors.NameRequired);
        }

        var moduleExists = await context.Modules
            .AnyAsync(x => x.Id == command.ModuleId, cancellationToken);
        if (!moduleExists)
        {
            return Result.Failure<CreateLessonPlanUnitResponse>(
                LessonPlanUnitErrors.ModuleNotFound(command.ModuleId));
        }

        var duplicateExists = await context.LessonPlanUnits
            .AnyAsync(
                x => x.ModuleId == command.ModuleId &&
                     x.NameNormalized == identity.NormalizedKey,
                cancellationToken);
        if (duplicateExists)
        {
            return Result.Failure<CreateLessonPlanUnitResponse>(
                LessonPlanUnitErrors.DuplicateName(command.ModuleId, identity.NormalizedKey));
        }

        var nextOrder = await context.LessonPlanUnits
            .Where(x => x.ModuleId == command.ModuleId)
            .Select(x => (int?)x.OrderIndex)
            .MaxAsync(cancellationToken) ?? -1;

        var now = VietnamTime.UtcNow();
        var unit = new LessonPlanUnit
        {
            Id = Guid.NewGuid(),
            ModuleId = command.ModuleId,
            Name = identity.CanonicalDisplayName,
            NameNormalized = identity.NormalizedKey,
            OrderIndex = nextOrder + 1,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.LessonPlanUnits.Add(unit);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateLessonPlanUnitResponse
        {
            Id = unit.Id,
            ModuleId = unit.ModuleId,
            Name = unit.Name,
            OrderIndex = unit.OrderIndex,
            IsActive = unit.IsActive
        };
    }
}
