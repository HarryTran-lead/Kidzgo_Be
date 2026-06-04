using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.SlotTypes.GetSlotTypes;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.SlotTypes.UpdateSlotType;

public sealed class UpdateSlotTypeCommandHandler(
    IDbContext context)
    : ICommandHandler<UpdateSlotTypeCommand, SlotTypeDto>
{
    public async Task<Result<SlotTypeDto>> Handle(
        UpdateSlotTypeCommand command,
        CancellationToken cancellationToken)
    {
        var item = await context.SlotTypes
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (item is null)
        {
            return Result.Failure<SlotTypeDto>(
                Error.NotFound(
                    "SlotType.NotFound",
                    $"Slot type '{command.Id}' was not found."));
        }

        var normalizedCode = command.Code.Trim().ToUpperInvariant();
        var codeExists = await context.SlotTypes
            .AnyAsync(x => x.Id != command.Id && x.Code == normalizedCode, cancellationToken);

        if (codeExists)
        {
            return Result.Failure<SlotTypeDto>(
                Error.Conflict(
                    "SlotType.CodeExists",
                    $"Slot type code '{normalizedCode}' already exists."));
        }

        item.Code = normalizedCode;
        item.Name = command.Name.Trim();
        item.Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();
        item.DayGroup = command.DayGroup;
        item.TimeBand = command.TimeBand;
        item.TeacherType = command.TeacherType;
        item.UsageType = command.UsageType;
        item.IsActive = command.IsActive;
        item.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new SlotTypeDto
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            Description = item.Description,
            DayGroup = item.DayGroup,
            TimeBand = item.TimeBand,
            TeacherType = item.TeacherType,
            UsageType = item.UsageType,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        });
    }
}

