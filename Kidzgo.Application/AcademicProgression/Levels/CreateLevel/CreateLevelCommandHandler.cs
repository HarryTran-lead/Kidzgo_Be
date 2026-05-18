using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AcademicProgression.Levels.CreateLevel;

public sealed class CreateLevelCommandHandler(IDbContext context)
    : ICommandHandler<CreateLevelCommand, LevelDto>
{
    public async Task<Result<LevelDto>> Handle(CreateLevelCommand command, CancellationToken cancellationToken)
    {
        var programExists = await context.Programs.AnyAsync(x => x.Id == command.ProgramId && !x.IsDeleted, cancellationToken);
        if (!programExists)
        {
            return Result.Failure<LevelDto>(
                Error.NotFound("AcademicProgression.ProgramNotFound", $"Program '{command.ProgramId}' was not found."));
        }

        var normalizedCode = command.Code.Trim().ToUpperInvariant();
        var duplicateExists = await context.Levels.AnyAsync(
            x => x.ProgramId == command.ProgramId && (x.Code == normalizedCode || x.Order == command.Order),
            cancellationToken);
        if (duplicateExists)
        {
            return Result.Failure<LevelDto>(
                Error.Conflict("AcademicProgression.LevelDuplicate", "Level code or order already exists in the program."));
        }

        var now = VietnamTime.UtcNow();
        var level = new Kidzgo.Domain.Programs.Level
        {
            Id = Guid.NewGuid(),
            ProgramId = command.ProgramId,
            Code = normalizedCode,
            Name = command.Name.Trim(),
            Order = command.Order,
            Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim(),
            IsActive = command.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Levels.Add(level);
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
