using Kidzgo.Application.LessonPlanTemplates.Shared;

namespace Kidzgo.Application.Syllabuses.Shared;

internal sealed record OrderedSyllabusUnitSession(
    Guid ModuleId,
    string NormalizedKey,
    int OrderIndex,
    int? LessonCount);

internal static class SyllabusUnitSessionIndexResolver
{
    public static IReadOnlyDictionary<Guid, IReadOnlyList<OrderedSyllabusUnitSession>> BuildLookup(
        IEnumerable<OrderedSyllabusUnitSession> units)
    {
        return units
            .Where(x =>
                x.ModuleId != Guid.Empty &&
                !string.IsNullOrWhiteSpace(x.NormalizedKey))
            .GroupBy(x => x.ModuleId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<OrderedSyllabusUnitSession>)x
                    .OrderBy(unit => unit.OrderIndex)
                    .ThenBy(unit => unit.NormalizedKey, StringComparer.OrdinalIgnoreCase)
                    .ToList());
    }

    public static int? ResolveSessionIndex(
        IReadOnlyDictionary<Guid, IReadOnlyList<OrderedSyllabusUnitSession>> lookup,
        Guid moduleId,
        string normalizedUnitKey,
        int lessonNumber)
    {
        if (moduleId == Guid.Empty ||
            string.IsNullOrWhiteSpace(normalizedUnitKey) ||
            lessonNumber <= 0 ||
            !lookup.TryGetValue(moduleId, out var orderedUnits))
        {
            return null;
        }

        var targetUnit = orderedUnits.FirstOrDefault(x =>
            string.Equals(x.NormalizedKey, normalizedUnitKey, StringComparison.OrdinalIgnoreCase));
        if (targetUnit is null)
        {
            return null;
        }

        if (targetUnit.LessonCount.HasValue &&
            targetUnit.LessonCount.Value > 0 &&
            lessonNumber > targetUnit.LessonCount.Value)
        {
            return null;
        }

        var offset = 0;
        foreach (var unit in orderedUnits)
        {
            if (string.Equals(unit.NormalizedKey, normalizedUnitKey, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            offset += Math.Max(unit.LessonCount ?? 0, 0);
        }

        return offset + lessonNumber;
    }

    public static IReadOnlyDictionary<Guid, IReadOnlyList<OrderedSyllabusUnitSession>> BuildLookupFromSyllabusUnits(
        IEnumerable<Domain.LessonPlans.SyllabusUnit> units)
    {
        return BuildLookup(
            units
                .Where(x => x.ModuleId.HasValue)
                .Select(x => new
                {
                    Unit = x,
                    Identity = LessonPlanUnitNameNormalizer.ExtractUnitIdentity(x.Name)
                })
                .Where(x => x.Identity is not null)
                .Select(x => new OrderedSyllabusUnitSession(
                    x.Unit.ModuleId!.Value,
                    x.Identity!.NormalizedKey,
                    x.Unit.OrderIndex,
                    x.Unit.LessonCount ?? x.Unit.AllocatedPeriods)));
    }
}
