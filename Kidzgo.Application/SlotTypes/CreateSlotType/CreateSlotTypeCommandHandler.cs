using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.SlotTypes.GetSlotTypes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.SlotTypes.CreateSlotType;

public sealed class CreateSlotTypeCommandHandler(
    IDbContext context)
    : ICommandHandler<CreateSlotTypeCommand, SlotTypeDto>
{
    public async Task<Result<SlotTypeDto>> Handle(
        CreateSlotTypeCommand command,
        CancellationToken cancellationToken)
    {
        var normalizedCode = command.Code.Trim().ToUpperInvariant();

        var exists = await context.SlotTypes
            .AnyAsync(x => x.Code == normalizedCode, cancellationToken);

        if (exists)
        {
            return Result.Failure<SlotTypeDto>(
                Error.Conflict(
                    "SlotType.CodeExists",
                    $"Slot type code '{normalizedCode}' already exists."));
        }

        var now = VietnamTime.UtcNow();
        var item = new SlotType
        {
            Id = Guid.NewGuid(),
            Code = normalizedCode,
            Name = command.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim(),
            IsActive = command.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.SlotTypes.Add(item);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new SlotTypeDto
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

