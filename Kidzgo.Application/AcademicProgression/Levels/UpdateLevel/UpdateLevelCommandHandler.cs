using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AcademicProgression.Levels.UpdateLevel;

public sealed class UpdateLevelCommandHandler(IDbContext context)
    : ICommandHandler<UpdateLevelCommand, LevelDto>
{
    public async Task<Result<LevelDto>> Handle(UpdateLevelCommand command, CancellationToken cancellationToken)
    {
        var level = await context.Levels.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (level is null)
        {
            return Result.Failure<LevelDto>(Kidzgo.Domain.AcademicProgression.AcademicProgressionErrors.LevelNotFound(command.Id));
        }

        var normalizedCode = command.Code.Trim().ToUpperInvariant();
        var duplicateExists = await context.Levels.AnyAsync(
            x => x.ProgramId == level.ProgramId
                 && x.Id != command.Id
                 && (x.Code == normalizedCode || x.Order == command.Order),
            cancellationToken);
        if (duplicateExists)
        {
            return Result.Failure<LevelDto>(
                Error.Conflict("AcademicProgression.LevelDuplicate", "Level code or order already exists in the program."));
        }

        level.Code = normalizedCode;
        level.Name = command.Name.Trim();
        level.Order = command.Order;
        level.Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();
        level.IsActive = command.IsActive;
        level.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new LevelDto
        {
            Id = level.Id,
            ProgramId = level.ProgramId,
            Code = level.Code,
            Name = level.Name,
            Order = level.Order,
            Description = level.Description,
            IsActive = level.IsActive
        });
    }
}
