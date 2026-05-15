using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TicketTypeCompatibilities.GetTicketTypeCompatibilities;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TicketTypeCompatibilities.UpdateTicketTypeCompatibility;

public sealed class UpdateTicketTypeCompatibilityCommandHandler(
    IDbContext context)
    : ICommandHandler<UpdateTicketTypeCompatibilityCommand, TicketTypeCompatibilityDto>
{
    public async Task<Result<TicketTypeCompatibilityDto>> Handle(
        UpdateTicketTypeCompatibilityCommand command,
        CancellationToken cancellationToken)
    {
        var item = await context.TicketTypeCompatibilities
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (item is null)
        {
            return Result.Failure<TicketTypeCompatibilityDto>(
                Error.NotFound(
                    "TicketTypeCompatibility.NotFound",
                    $"Ticket type compatibility '{command.Id}' was not found."));
        }

        var learningTicketType = await context.LearningTicketTypes
            .FirstOrDefaultAsync(x => x.Id == command.LearningTicketTypeId, cancellationToken);
        if (learningTicketType is null)
        {
            return Result.Failure<TicketTypeCompatibilityDto>(
                Error.NotFound(
                    "TicketTypeCompatibility.LearningTicketTypeNotFound",
                    $"Learning ticket type '{command.LearningTicketTypeId}' was not found."));
        }

        var slotType = await context.SlotTypes
            .FirstOrDefaultAsync(x => x.Id == command.SlotTypeId, cancellationToken);
        if (slotType is null)
        {
            return Result.Failure<TicketTypeCompatibilityDto>(
                Error.NotFound(
                    "TicketTypeCompatibility.SlotTypeNotFound",
                    $"Slot type '{command.SlotTypeId}' was not found."));
        }

        var duplicate = await context.TicketTypeCompatibilities
            .AnyAsync(
                x => x.Id != command.Id &&
                     x.LearningTicketTypeId == command.LearningTicketTypeId &&
                     x.SlotTypeId == command.SlotTypeId,
                cancellationToken);
        if (duplicate)
        {
            return Result.Failure<TicketTypeCompatibilityDto>(
                Error.Conflict(
                    "TicketTypeCompatibility.MappingExists",
                    "Compatibility mapping already exists for this ticket type and slot type."));
        }

        item.LearningTicketTypeId = command.LearningTicketTypeId;
        item.SlotTypeId = command.SlotTypeId;
        item.IsCompatible = command.IsCompatible;
        item.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new TicketTypeCompatibilityDto
        {
            Id = item.Id,
            LearningTicketTypeId = item.LearningTicketTypeId,
            LearningTicketTypeCode = learningTicketType.Code,
            SlotTypeId = item.SlotTypeId,
            SlotTypeCode = slotType.Code,
            IsCompatible = item.IsCompatible,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        });
    }
}

