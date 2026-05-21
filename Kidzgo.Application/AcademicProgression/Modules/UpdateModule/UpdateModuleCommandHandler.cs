using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AcademicProgression.Modules.UpdateModule;

public sealed class UpdateModuleCommandHandler(IDbContext context)
    : ICommandHandler<UpdateModuleCommand, ModuleDto>
{
    public async Task<Result<ModuleDto>> Handle(UpdateModuleCommand command, CancellationToken cancellationToken)
    {
        var module = await context.Modules
            .Include(x => x.Level)
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (module is null)
        {
            return Result.Failure<ModuleDto>(Kidzgo.Domain.AcademicProgression.AcademicProgressionErrors.ModuleNotFound(command.Id));
        }

        var normalizedCode = command.Code.Trim().ToUpperInvariant();
        var duplicateExists = await context.Modules.AnyAsync(
            x => x.LevelId == module.LevelId
                 && x.Id != command.Id
                 && (x.Code == normalizedCode || x.Order == command.Order),
            cancellationToken);
        if (duplicateExists)
        {
            return Result.Failure<ModuleDto>(
                Error.Conflict("AcademicProgression.ModuleDuplicate", "Module code or order already exists in the level."));
        }

        module.Code = normalizedCode;
        module.Name = command.Name.Trim();
        module.Order = command.Order;
        module.Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();
        module.PlannedSessionCount = command.PlannedSessionCount;
        module.IsActive = command.IsActive;
        module.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        var lessonPlanCount = await context.LessonPlanTemplates
            .CountAsync(
                x => x.ModuleId == module.Id &&
                     x.IsActive &&
                     !x.IsDeleted,
                cancellationToken);

        return Result.Success(new ModuleDto
        {
            Id = module.Id,
            LevelId = module.LevelId,
            LevelCode = module.Level.Code,
            Code = module.Code,
            Name = module.Name,
            Order = module.Order,
            Description = module.Description,
            PlannedSessionCount = module.PlannedSessionCount,
            LessonPlanCount = lessonPlanCount,
            IsActive = module.IsActive
        });
    }
}
