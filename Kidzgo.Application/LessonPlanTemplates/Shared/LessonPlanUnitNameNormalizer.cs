using System.Text.RegularExpressions;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanTemplates.Shared;

public static class LessonPlanUnitNameNormalizer
{
    private const string IntroUnitAliasPattern = @"STARTER|STARTERS|MOVER|MOVERS|FLYER|FLYERS|HELLO";

    public static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        return ExtractUnitIdentity(raw)?.NormalizedKey ?? NormalizeLoose(raw);
    }

    public static string? ExtractUnitName(params string?[] hints)
    {
        return ExtractUnitIdentity(hints)?.CanonicalDisplayName;
    }

    public static LessonPlanUnitIdentity? ExtractUnitIdentity(params string?[] hints)
    {
        foreach (var hint in hints.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var normalizedHint = NormalizeHint(hint!);

            var lessonSuffixMatch = Regex.Match(
                normalizedHint,
                @"^(?<unit>.+?)\s*(?:[-–]\s*)?LESSON\s*0*\d+\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (lessonSuffixMatch.Success)
            {
                var unit = ExtractUnitIdentity(lessonSuffixMatch.Groups["unit"].Value);
                if (unit is not null)
                {
                    return unit;
                }
            }

            var starterMatch = Regex.Match(
                normalizedHint,
                $@"\bUNIT\s+(?:{IntroUnitAliasPattern})\b(?<name>.*?)(?=\bLESSON\b|$)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (starterMatch.Success)
            {
                return BuildIntroUnitIdentity(starterMatch.Groups["name"].Value);
            }

            var unitMatch = Regex.Match(
                normalizedHint,
                @"\bUNIT\s*0*(?<number>\d+)\b(?<name>.*?)(?=\bLESSON\b|$)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (unitMatch.Success)
            {
                return BuildNumberedUnitIdentity(unitMatch.Groups["number"].Value, unitMatch.Groups["name"].Value);
            }

            var revisionMatch = Regex.Match(
                normalizedHint,
                @"\bREVISION\s*0*(?<number>\d+)\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (revisionMatch.Success)
            {
                return BuildRevisionIdentity(revisionMatch.Groups["number"].Value);
            }

            var directIdentity = TryParseUnitIdentity(normalizedHint);
            if (directIdentity is not null)
            {
                return directIdentity;
            }
        }

        return null;
    }

    public static int? ExtractUnitNumber(params string?[] hints)
    {
        return ExtractUnitIdentity(hints)?.UnitNumber;
    }

    public static string? ExtractUnitTitle(params string?[] hints)
    {
        return ExtractUnitIdentity(hints)?.UnitTitle;
    }

    public static int? ExtractLessonNumber(params string?[] hints)
    {
        foreach (var hint in hints.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var match = Regex.Match(
                hint!,
                @"\bLESSON\s*0*(\d+)\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (match.Success && int.TryParse(match.Groups[1].Value, out var lessonNumber))
            {
                return lessonNumber;
            }
        }

        return null;
    }

    public static bool IsIntroUnitAliasText(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var normalizedHint = NormalizeHint(raw);
        if (Regex.IsMatch(
                normalizedHint,
                $@"\bUNIT\s+(?:{IntroUnitAliasPattern})\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            return true;
        }

        var normalized = NormalizeLoose(raw);
        return Regex.IsMatch(
            normalized,
            $@"^(?:{IntroUnitAliasPattern})$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static LessonPlanUnitIdentity? TryParseUnitIdentity(string raw)
    {
        var normalized = NormalizeLoose(raw);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        var starterMatch = Regex.Match(
            normalized,
            $@"\bUNIT\s+(?:{IntroUnitAliasPattern})\b(?<name>.*)$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (starterMatch.Success)
        {
            return BuildIntroUnitIdentity(starterMatch.Groups["name"].Value);
        }

        var unitMatch = Regex.Match(
            normalized,
            @"\bUNIT\s*0*(?<number>\d+)\b(?<name>.*)$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (unitMatch.Success)
        {
            return BuildNumberedUnitIdentity(unitMatch.Groups["number"].Value, unitMatch.Groups["name"].Value);
        }

        var revisionMatch = Regex.Match(
            normalized,
            @"\bREVISION\s*0*(?<number>\d+)\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (revisionMatch.Success)
        {
            return BuildRevisionIdentity(revisionMatch.Groups["number"].Value);
        }

        var cleanedTitle = CleanDisplayTitle(normalized);
        return new LessonPlanUnitIdentity(
            CanonicalDisplayName: cleanedTitle,
            NormalizedKey: $"TEXT|{Regex.Replace(cleanedTitle, @"[^A-Z0-9]+", string.Empty)}",
            UnitNumber: null,
            UnitTitle: cleanedTitle);
    }

    private static LessonPlanUnitIdentity BuildIntroUnitIdentity(string rawTitle)
    {
        var title = CleanDisplayTitle(rawTitle);
        var displayName = string.IsNullOrWhiteSpace(title)
            ? "UNIT 0"
            : $"UNIT 0: {title}";

        return new LessonPlanUnitIdentity(
            CanonicalDisplayName: displayName,
            NormalizedKey: "UNIT|0",
            UnitNumber: 0,
            UnitTitle: title);
    }

    private static LessonPlanUnitIdentity BuildNumberedUnitIdentity(string rawNumber, string rawTitle)
    {
        var number = int.Parse(rawNumber);
        if (number == 0)
        {
            return BuildIntroUnitIdentity(rawTitle);
        }

        var title = CleanDisplayTitle(rawTitle);
        var displayName = string.IsNullOrWhiteSpace(title)
            ? $"UNIT {number}"
            : $"UNIT {number}: {title}";

        return new LessonPlanUnitIdentity(
            CanonicalDisplayName: displayName,
            NormalizedKey: $"UNIT|{number}",
            UnitNumber: number,
            UnitTitle: title);
    }

    private static LessonPlanUnitIdentity BuildRevisionIdentity(string rawNumber)
    {
        var number = int.Parse(rawNumber);
        return new LessonPlanUnitIdentity(
            CanonicalDisplayName: $"REVISION {number}",
            NormalizedKey: $"REVISION|{number}",
            UnitNumber: number,
            UnitTitle: $"REVISION {number}");
    }

    private static string NormalizeHint(string hint)
    {
        var normalized = hint.Replace('\\', '/');
        var lastSegment = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? normalized;
        lastSegment = Regex.Replace(lastSegment, @"\.[A-Za-z0-9]+$", string.Empty);
        return Regex.Replace(lastSegment, @"\s+", " ").Trim();
    }

    private static string NormalizeLoose(string raw)
    {
        var value = Regex.Replace(raw.Trim().ToUpperInvariant(), @"\s+", " ");
        value = value.Replace("–", "-").Replace("—", "-");
        value = Regex.Replace(value, @"\s+([:;,.!?])", "$1");
        value = Regex.Replace(value, @"([:;,.!?])(?=\S)", "$1 ");
        value = Regex.Replace(
            value,
            @"\b0+(\d+)\b",
            match => int.Parse(match.Groups[1].Value).ToString());
        value = Regex.Replace(value, @"[!?.;,]+$", string.Empty).Trim();
        return Regex.Replace(value, @"\s+", " ").Trim();
    }

    private static string CleanDisplayTitle(string raw)
    {
        var title = NormalizeLoose(raw);
        title = Regex.Replace(title, @"^(?:[:\-]\s*)+", string.Empty).Trim();
        title = Regex.Replace(title, @"\s+", " ").Trim();
        return title;
    }
}

public sealed record LessonPlanUnitIdentity(
    string CanonicalDisplayName,
    string NormalizedKey,
    int? UnitNumber,
    string? UnitTitle);

public static class LessonPlanUnitResolver
{
    public static async Task<LessonPlanUnit> FindOrCreateAsync(
        IDbContext context,
        Guid moduleId,
        LessonPlanUnitIdentity identity,
        int? orderIndexOverride,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var candidate = new LessonPlanUnit
        {
            Id = Guid.NewGuid(),
            ModuleId = moduleId,
            Name = identity.CanonicalDisplayName,
            NameNormalized = identity.NormalizedKey,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        EntityStringLengthTrimmer.TrimToModelLimits(context, candidate);

        var existing = await context.LessonPlanUnits
            .FirstOrDefaultAsync(
                x => x.ModuleId == moduleId &&
                     x.NameNormalized == candidate.NameNormalized,
                cancellationToken);
        if (existing is not null)
        {
            if (!string.Equals(existing.Name, candidate.Name, StringComparison.Ordinal) ||
                !existing.IsActive ||
                (orderIndexOverride.HasValue && existing.OrderIndex != orderIndexOverride.Value))
            {
                existing.Name = candidate.Name;
                existing.IsActive = true;
                if (orderIndexOverride.HasValue)
                {
                    existing.OrderIndex = orderIndexOverride.Value;
                }

                existing.UpdatedAt = now;
            }

            return existing;
        }

        var nextOrder = await context.LessonPlanUnits
            .Where(x => x.ModuleId == moduleId)
            .Select(x => (int?)x.OrderIndex)
            .MaxAsync(cancellationToken) ?? -1;

        var unit = new LessonPlanUnit
        {
            Id = candidate.Id,
            ModuleId = moduleId,
            Name = candidate.Name,
            NameNormalized = candidate.NameNormalized,
            OrderIndex = orderIndexOverride ?? (nextOrder + 1),
            IsActive = candidate.IsActive,
            CreatedAt = candidate.CreatedAt,
            UpdatedAt = candidate.UpdatedAt
        };

        context.LessonPlanUnits.Add(unit);
        return unit;
    }

    public static async Task<LessonPlanUnit> FindOrCreateAsync(
        IDbContext context,
        Guid moduleId,
        string rawName,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var identity = LessonPlanUnitNameNormalizer.ExtractUnitIdentity(rawName)
                       ?? new LessonPlanUnitIdentity(
                           CanonicalDisplayName: LessonPlanUnitNameNormalizer.Normalize(rawName),
                           NormalizedKey: LessonPlanUnitNameNormalizer.Normalize(rawName),
                           UnitNumber: null,
                           UnitTitle: null);
        return await FindOrCreateAsync(context, moduleId, identity, orderIndexOverride: null, now, cancellationToken);
    }

    public static async Task<int> GetNextOrderInUnitAsync(
        IDbContext context,
        Guid unitId,
        CancellationToken cancellationToken)
    {
        var currentMax = await context.LessonPlanTemplates
            .Where(x => x.LessonPlanUnitId == unitId && !x.IsDeleted)
            .Select(x => (int?)x.OrderIndexInUnit)
            .MaxAsync(cancellationToken);

        return currentMax.HasValue ? currentMax.Value + 1 : 0;
    }
}
