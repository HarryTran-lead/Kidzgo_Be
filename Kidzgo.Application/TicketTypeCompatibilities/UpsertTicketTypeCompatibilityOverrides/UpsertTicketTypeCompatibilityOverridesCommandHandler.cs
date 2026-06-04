using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TicketTypeCompatibilities.GetTicketTypeCompatibilities;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LearningTickets;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TicketTypeCompatibilities.UpsertTicketTypeCompatibilityOverrides;

public sealed class UpsertTicketTypeCompatibilityOverridesCommandHandler(
    IDbContext context)
    : ICommandHandler<UpsertTicketTypeCompatibilityOverridesCommand, UpsertTicketTypeCompatibilityOverridesResponse>
{
    public async Task<Result<UpsertTicketTypeCompatibilityOverridesResponse>> Handle(
        UpsertTicketTypeCompatibilityOverridesCommand command,
        CancellationToken cancellationToken)
    {
        var learningTicketType = await context.LearningTicketTypes
            .FirstOrDefaultAsync(x => x.Id == command.LearningTicketTypeId, cancellationToken);
        if (learningTicketType is null)
        {
            return Result.Failure<UpsertTicketTypeCompatibilityOverridesResponse>(
                Error.NotFound(
                    "TicketTypeCompatibility.LearningTicketTypeNotFound",
                    $"Learning ticket type '{command.LearningTicketTypeId}' was not found."));
        }

        var slotTypeIds = command.Items
            .Select(x => x.SlotTypeId)
            .Distinct()
            .ToList();

        var slotTypes = slotTypeIds.Count == 0
            ? new Dictionary<Guid, Domain.Sessions.SlotType>()
            : await context.SlotTypes
                .Where(x => slotTypeIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        var missingSlotTypeId = slotTypeIds.FirstOrDefault(slotTypeId => !slotTypes.ContainsKey(slotTypeId));
        if (missingSlotTypeId != Guid.Empty)
        {
            return Result.Failure<UpsertTicketTypeCompatibilityOverridesResponse>(
                Error.NotFound(
                    "TicketTypeCompatibility.SlotTypeNotFound",
                    $"Slot type '{missingSlotTypeId}' was not found."));
        }

        var existingMappings = slotTypeIds.Count == 0
            ? new Dictionary<Guid, TicketTypeCompatibility>()
            : await context.TicketTypeCompatibilities
                .Where(x => x.LearningTicketTypeId == command.LearningTicketTypeId &&
                            slotTypeIds.Contains(x.SlotTypeId))
                .ToDictionaryAsync(x => x.SlotTypeId, cancellationToken);

        var now = VietnamTime.UtcNow();
        var upsertedCount = 0;
        var removedCount = 0;

        foreach (var item in command.Items)
        {
            existingMappings.TryGetValue(item.SlotTypeId, out var existingMapping);

            if (!item.IsCompatible.HasValue)
            {
                if (existingMapping is not null)
                {
                    context.TicketTypeCompatibilities.Remove(existingMapping);
                    removedCount++;
                }

                continue;
            }

            if (existingMapping is null)
            {
                context.TicketTypeCompatibilities.Add(new TicketTypeCompatibility
                {
                    Id = Guid.NewGuid(),
                    LearningTicketTypeId = command.LearningTicketTypeId,
                    SlotTypeId = item.SlotTypeId,
                    IsCompatible = item.IsCompatible.Value,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else
            {
                existingMapping.IsCompatible = item.IsCompatible.Value;
                existingMapping.UpdatedAt = now;
            }

            upsertedCount++;
        }

        await context.SaveChangesAsync(cancellationToken);

        var items = await context.TicketTypeCompatibilities
            .AsNoTracking()
            .Where(x => x.LearningTicketTypeId == command.LearningTicketTypeId)
            .Include(x => x.LearningTicketType)
            .Include(x => x.SlotType)
            .OrderBy(x => x.SlotType.Code)
            .Select(x => new TicketTypeCompatibilityDto
            {
                Id = x.Id,
                LearningTicketTypeId = x.LearningTicketTypeId,
                LearningTicketTypeCode = x.LearningTicketType.Code,
                SlotTypeId = x.SlotTypeId,
                SlotTypeCode = x.SlotType.Code,
                IsCompatible = x.IsCompatible,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new UpsertTicketTypeCompatibilityOverridesResponse
        {
            LearningTicketTypeId = command.LearningTicketTypeId,
            UpsertedCount = upsertedCount,
            RemovedCount = removedCount,
            Items = items
        });
    }
}
