using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;
using Kidzgo.Domain.Common;
using AcademicModule = Kidzgo.Domain.Programs.Module;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AcademicProgression.Modules.CreateModule;

public sealed class CreateModuleCommandHandler(IDbContext context)
    : ICommandHandler<CreateModuleCommand, ModuleDto>
{
    public async Task<Result<ModuleDto>> Handle(CreateModuleCommand command, CancellationToken cancellationToken)
    {
        var level = await context.Levels.AsNoTracking().FirstOrDefaultAsync(x => x.Id == command.LevelId, cancellationToken);
        if (level is null)
        {
            return Result.Failure<ModuleDto>(Kidzgo.Domain.AcademicProgression.AcademicProgressionErrors.LevelNotFound(command.LevelId));
        }

        var normalizedCode = command.Code.Trim().ToUpperInvariant();
        var duplicateExists = await context.Modules.AnyAsync(
            x => x.LevelId == command.LevelId && (x.Code == normalizedCode || x.Order == command.Order),
            cancellationToken);
        if (duplicateExists)
        {
            return Result.Failure<ModuleDto>(
                Error.Conflict("AcademicProgression.ModuleDuplicate", "Module code or order already exists in the level."));
        }

        var now = VietnamTime.UtcNow();
        var module = new AcademicModule
        {
            Id = Guid.NewGuid(),
            LevelId = command.LevelId,
            Code = normalizedCode,
            Name = command.Name.Trim(),
            Order = command.Order,
            Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim(),
            PlannedSessionCount = command.PlannedSessionCount,
            IsActive = command.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Modules.Add(module);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new ModuleDto
        {
            Id = module.Id,
            LevelId = module.LevelId,
            LevelCode = level.Code,
            Code = module.Code,
            Name = module.Name,
            Order = module.Order,
            Description = module.Description,
            PlannedSessionCount = module.PlannedSessionCount,
            IsActive = module.IsActive
        });
    }
}
