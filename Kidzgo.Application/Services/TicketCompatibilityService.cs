using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.LearningTickets;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public sealed record TicketSelectionResult(
    LearningTicketItem? TicketItem,
    bool IsCompatible,
    string Reason,
    Guid? TicketTypeId,
    string? TicketTypeCode);

public sealed record TicketCompatibilityEvaluation(
    bool IsCompatible,
    string Source,
    string Reason,
    bool? OverrideValue);

public sealed class TicketCompatibilityService(IDbContext context)
{
    public async Task<TicketCompatibilityEvaluation> EvaluateAsync(
        Guid? learningTicketTypeId,
        Guid? slotTypeId,
        CancellationToken cancellationToken)
    {
        if (!learningTicketTypeId.HasValue)
        {
            return new TicketCompatibilityEvaluation(
                true,
                "NoTicketType",
                "No learning ticket type configured.",
                null);
        }

        if (!slotTypeId.HasValue)
        {
            return new TicketCompatibilityEvaluation(
                true,
                "NoSlotType",
                "No slot type configured.",
                null);
        }

        var learningTicketType = await context.LearningTicketTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == learningTicketTypeId.Value, cancellationToken);

        var slotType = await context.SlotTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == slotTypeId.Value, cancellationToken);

        var compatibilityOverride = await context.TicketTypeCompatibilities
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.LearningTicketTypeId == learningTicketTypeId.Value &&
                     x.SlotTypeId == slotTypeId.Value,
                cancellationToken);

        return Evaluate(learningTicketType, slotType, compatibilityOverride);
    }

    public async Task<IReadOnlyDictionary<Guid, TicketCompatibilityEvaluation>> EvaluateForSlotTypesAsync(
        Guid? learningTicketTypeId,
        IReadOnlyCollection<Guid> slotTypeIds,
        CancellationToken cancellationToken)
    {
        if (slotTypeIds.Count == 0)
        {
            return new Dictionary<Guid, TicketCompatibilityEvaluation>();
        }

        var slotTypes = await context.SlotTypes
            .AsNoTracking()
            .Where(x => slotTypeIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (!learningTicketTypeId.HasValue)
        {
            return slotTypes.ToDictionary(
                x => x.Id,
                _ => new TicketCompatibilityEvaluation(
                    true,
                    "NoTicketType",
                    "No learning ticket type configured.",
                    null));
        }

        var learningTicketType = await context.LearningTicketTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == learningTicketTypeId.Value, cancellationToken);

        var overrides = await context.TicketTypeCompatibilities
            .AsNoTracking()
            .Where(x => x.LearningTicketTypeId == learningTicketTypeId.Value && slotTypeIds.Contains(x.SlotTypeId))
            .ToDictionaryAsync(x => x.SlotTypeId, cancellationToken);

        return slotTypes.ToDictionary(
            x => x.Id,
            x => Evaluate(
                learningTicketType,
                x,
                overrides.TryGetValue(x.Id, out var compatibilityOverride) ? compatibilityOverride : null));
    }

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

        var overrides = typeIds.Count == 0
            ? new Dictionary<Guid, TicketTypeCompatibility>()
            : await context.TicketTypeCompatibilities
                .AsNoTracking()
                .Where(x => typeIds.Contains(x.LearningTicketTypeId) && x.SlotTypeId == slotType.Id)
                .ToDictionaryAsync(x => x.LearningTicketTypeId, cancellationToken);

        var compatibleTickets = availableTickets
            .Select(ticket => new
            {
                Ticket = ticket,
                Evaluation = Evaluate(
                    ticket.LearningTicketType,
                    slotType,
                    ticket.LearningTicketTypeId.HasValue &&
                    overrides.TryGetValue(ticket.LearningTicketTypeId.Value, out var compatibilityOverride)
                        ? compatibilityOverride
                        : null)
            })
            .Where(x => x.Evaluation.IsCompatible)
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
            x => x.Ticket.LearningTicketType != null &&
                 string.Equals(x.Ticket.LearningTicketType.Code, slotType.Code, StringComparison.OrdinalIgnoreCase));

        var selected = exactMatch ?? compatibleTickets[0];

        var reason = exactMatch is not null
            ? $"Exact match ticket type '{selected.Ticket.LearningTicketType?.Code}' for slot '{slotType.Code}'"
            : $"{selected.Evaluation.Reason} Selected compatible ticket type '{selected.Ticket.LearningTicketType?.Code ?? "DEFAULT"}' for slot '{slotType.Code}'.";

        return new TicketSelectionResult(
            selected.Ticket,
            true,
            reason,
            selected.Ticket.LearningTicketTypeId,
            selected.Ticket.LearningTicketType?.Code);
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

    private static TicketCompatibilityEvaluation Evaluate(
        LearningTicketType? learningTicketType,
        SlotType? slotType,
        TicketTypeCompatibility? compatibilityOverride)
    {
        if (learningTicketType is null)
        {
            return new TicketCompatibilityEvaluation(
                true,
                "NoTicketType",
                "No learning ticket type configured.",
                null);
        }

        if (slotType is null)
        {
            return new TicketCompatibilityEvaluation(
                true,
                "NoSlotType",
                "No slot type configured.",
                null);
        }

        if (compatibilityOverride is not null)
        {
            return new TicketCompatibilityEvaluation(
                compatibilityOverride.IsCompatible,
                compatibilityOverride.IsCompatible ? "OverrideAllow" : "OverrideDeny",
                compatibilityOverride.IsCompatible
                    ? "Compatible by manual override."
                    : "Blocked by manual override.",
                compatibilityOverride.IsCompatible);
        }

        if (learningTicketType.CompatibilityMode == TicketCompatibilityMode.AllowAll)
        {
            return new TicketCompatibilityEvaluation(
                true,
                "AllowAll",
                "Compatible by AllowAll mode.",
                null);
        }

        var failures = new List<string>();
        if (!TicketCompatibilityRuleSupport.Matches(learningTicketType.AllowedDayGroups, slotType.DayGroup))
        {
            failures.Add($"Day group '{slotType.DayGroup}' is not allowed.");
        }

        if (!TicketCompatibilityRuleSupport.Matches(learningTicketType.AllowedTimeBands, slotType.TimeBand))
        {
            failures.Add($"Time band '{slotType.TimeBand}' is not allowed.");
        }

        if (!TicketCompatibilityRuleSupport.Matches(learningTicketType.AllowedTeacherTypes, slotType.TeacherType))
        {
            failures.Add($"Teacher type '{slotType.TeacherType}' is not allowed.");
        }

        if (!TicketCompatibilityRuleSupport.Matches(learningTicketType.AllowedUsageTypes, slotType.UsageType))
        {
            failures.Add($"Usage type '{slotType.UsageType}' is not allowed.");
        }

        if (failures.Count == 0)
        {
            return new TicketCompatibilityEvaluation(
                true,
                "Rule",
                "Compatible by default rule.",
                null);
        }

        return new TicketCompatibilityEvaluation(
            false,
            "Rule",
            string.Join(" ", failures),
            null);
    }
}
