using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TicketTypeCompatibilities.GetTicketTypeCompatibilities;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LearningTickets;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TicketTypeCompatibilities.CreateTicketTypeCompatibility;

public sealed class CreateTicketTypeCompatibilityCommandHandler(
    IDbContext context)
    : ICommandHandler<CreateTicketTypeCompatibilityCommand, TicketTypeCompatibilityDto>
{
    public async Task<Result<TicketTypeCompatibilityDto>> Handle(
        CreateTicketTypeCompatibilityCommand command,
        CancellationToken cancellationToken)
    {
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

        var exists = await context.TicketTypeCompatibilities
            .AnyAsync(
                x => x.LearningTicketTypeId == command.LearningTicketTypeId &&
                     x.SlotTypeId == command.SlotTypeId,
                cancellationToken);
        if (exists)
        {
            return Result.Failure<TicketTypeCompatibilityDto>(
                Error.Conflict(
                    "TicketTypeCompatibility.MappingExists",
                    "Compatibility mapping already exists for this ticket type and slot type."));
        }

        var now = VietnamTime.UtcNow();
        var item = new TicketTypeCompatibility
        {
            Id = Guid.NewGuid(),
            LearningTicketTypeId = command.LearningTicketTypeId,
            SlotTypeId = command.SlotTypeId,
            IsCompatible = command.IsCompatible,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.TicketTypeCompatibilities.Add(item);
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

