using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LearningTicketTypes.GetLearningTicketTypes;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LearningTicketTypes.UpdateLearningTicketType;

public sealed class UpdateLearningTicketTypeCommandHandler(
    IDbContext context)
    : ICommandHandler<UpdateLearningTicketTypeCommand, LearningTicketTypeDto>
{
    public async Task<Result<LearningTicketTypeDto>> Handle(
        UpdateLearningTicketTypeCommand command,
        CancellationToken cancellationToken)
    {
        var item = await context.LearningTicketTypes
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (item is null)
        {
            return Result.Failure<LearningTicketTypeDto>(
                Error.NotFound(
                    "LearningTicketType.NotFound",
                    $"Learning ticket type '{command.Id}' was not found."));
        }

        var normalizedCode = command.Code.Trim().ToUpperInvariant();
        var codeExists = await context.LearningTicketTypes
            .AnyAsync(x => x.Id != command.Id && x.Code == normalizedCode, cancellationToken);

        if (codeExists)
        {
            return Result.Failure<LearningTicketTypeDto>(
                Error.Conflict(
                    "LearningTicketType.CodeExists",
                    $"Learning ticket type code '{normalizedCode}' already exists."));
        }

        item.Code = normalizedCode;
        item.Name = command.Name.Trim();
        item.Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();
        item.IsActive = command.IsActive;
        item.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new LearningTicketTypeDto
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            Description = item.Description,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        });
    }
}

