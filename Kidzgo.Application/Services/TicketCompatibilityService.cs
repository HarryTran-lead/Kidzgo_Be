using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.LearningTickets;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed record TicketSelectionResult(
    LearningTicketItem? TicketItem,
    bool IsCompatible,
    string Reason,
    Guid? TicketTypeId,
    string? TicketTypeCode);

public sealed class TicketCompatibilityService(IDbContext context)
{
    public async Task<TicketSelectionResult> SelectTicketForConsumptionAsync(
        Guid registrationId,
        Guid? slotTypeId,
        CancellationToken cancellationToken)
    {
        var availableTickets = await context.LearningTicketItems
            .Where(x => x.RegistrationId == registrationId && x.Status == LearningTicketItemStatus.Available)
            .Include(x => x.LearningTicketType)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        if (availableTickets.Count == 0)
        {
            return new TicketSelectionResult(
                null,
                false,
                "No available tickets",
                null,
                null);
        }

        if (!slotTypeId.HasValue)
        {
            var fallback = availableTickets[0];
            return new TicketSelectionResult(
                fallback,
                true,
                "Session has no slot type, fallback to oldest available ticket",
                fallback.LearningTicketTypeId,
                fallback.LearningTicketType?.Code);
        }

        var slotType = await context.SlotTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == slotTypeId.Value, cancellationToken);

        if (slotType is null)
        {
            var fallback = availableTickets[0];
            return new TicketSelectionResult(
                fallback,
                true,
                "Slot type not found, fallback to oldest available ticket",
                fallback.LearningTicketTypeId,
                fallback.LearningTicketType?.Code);
        }

        var typeIds = availableTickets
            .Where(x => x.LearningTicketTypeId.HasValue)
            .Select(x => x.LearningTicketTypeId!.Value)
            .Distinct()
            .ToList();

        var compatibilities = typeIds.Count == 0
            ? new List<TicketTypeCompatibility>()
            : await context.TicketTypeCompatibilities
                .AsNoTracking()
                .Where(x => typeIds.Contains(x.LearningTicketTypeId) && x.SlotTypeId == slotType.Id)
                .ToListAsync(cancellationToken);

        var compatibleTickets = availableTickets
            .Where(ticket => IsCompatibleWithCurrentPolicy(ticket, compatibilities, slotType.Id))
            .ToList();

        if (compatibleTickets.Count == 0)
        {
            return new TicketSelectionResult(
                null,
                false,
                $"No compatible tickets for slot type '{slotType.Code}'",
                null,
                null);
        }

        // Prefer exact code match first, then fallback to FIFO among compatible tickets.
        var exactMatch = compatibleTickets.FirstOrDefault(
            ticket => ticket.LearningTicketType != null &&
                      string.Equals(ticket.LearningTicketType.Code, slotType.Code, StringComparison.OrdinalIgnoreCase));

        var selected = exactMatch ?? compatibleTickets[0];

        var reason = exactMatch is not null
            ? $"Exact match ticket type '{selected.LearningTicketType?.Code}' for slot '{slotType.Code}'"
            : $"Compatible fallback ticket type '{selected.LearningTicketType?.Code ?? "DEFAULT"}' for slot '{slotType.Code}'";

        return new TicketSelectionResult(
            selected,
            true,
            reason,
            selected.LearningTicketTypeId,
            selected.LearningTicketType?.Code);
    }

    public async Task<TicketSelectionResult> ValidateStudentSessionCompatibilityAsync(
        Guid studentProfileId,
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var session = await context.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

        if (session is null)
        {
            return new TicketSelectionResult(
                null,
                false,
                "Session not found",
                null,
                null);
        }

        var registrations = await context.Registrations
            .AsNoTracking()
            .Where(x => x.StudentProfileId == studentProfileId)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var registrationId in registrations)
        {
            var selected = await SelectTicketForConsumptionAsync(registrationId, session.SlotTypeId, cancellationToken);
            if (selected.TicketItem is not null)
            {
                return selected;
            }
        }

        return new TicketSelectionResult(
            null,
            false,
            "No available tickets for student",
            null,
            null);
    }

    private static bool IsCompatibleWithCurrentPolicy(
        LearningTicketItem ticket,
        IReadOnlyCollection<TicketTypeCompatibility> compatibilities,
        Guid slotTypeId)
    {
        // Phase 1.5 runtime policy is default-pass:
        // if no explicit mapping exists, treat as compatible.
        if (!ticket.LearningTicketTypeId.HasValue)
        {
            return true;
        }

        var mapping = compatibilities.FirstOrDefault(
            x => x.LearningTicketTypeId == ticket.LearningTicketTypeId.Value &&
                 x.SlotTypeId == slotTypeId);

        return mapping?.IsCompatible ?? true;
    }
}
