using System.Text.RegularExpressions;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanTemplates.Shared;

public static class LessonPlanUnitNameNormalizer
{
    public static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var value = Regex.Replace(raw.Trim().ToUpperInvariant(), @"\s+", " ");
        return Regex.Replace(
            value,
            @"\b0+(\d+)\b",
            match => int.Parse(match.Groups[1].Value).ToString());
    }

    public static string? ExtractUnitName(params string?[] hints)
    {
        foreach (var hint in hints.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var normalizedHint = NormalizeHint(hint!);

            var lessonSuffixMatch = Regex.Match(
                normalizedHint,
                @"^(?<unit>.+?)\s*(?:[-–]\s*)?Lesson\s*0*\d+\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (lessonSuffixMatch.Success)
            {
                return Normalize(lessonSuffixMatch.Groups["unit"].Value);
            }

            var starterMatch = Regex.Match(
                normalizedHint,
                @"\bUNIT\s+STARTER\b(?<name>.*?)(?=\bLESSON\b|$)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (starterMatch.Success)
            {
                return Normalize($"Unit Starter{starterMatch.Groups["name"].Value}");
            }

            var unitMatch = Regex.Match(
                normalizedHint,
                @"\bUNIT\s*0*(?<number>\d+)\b(?<name>.*?)(?=\bLESSON\b|$)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (unitMatch.Success)
            {
                return Normalize($"Unit {unitMatch.Groups["number"].Value}{unitMatch.Groups["name"].Value}");
            }

            var revisionMatch = Regex.Match(
                normalizedHint,
                @"\bREVISION\s*0*(?<number>\d+)\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (revisionMatch.Success)
            {
                return Normalize($"Revision {revisionMatch.Groups["number"].Value}");
            }
        }

        return null;
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

    private static string NormalizeHint(string hint)
    {
        var normalized = hint.Replace('\\', '/');
        var lastSegment = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? normalized;
        lastSegment = Regex.Replace(lastSegment, @"\.[A-Za-z0-9]+$", string.Empty);
        return Regex.Replace(lastSegment, @"\s+", " ").Trim();
    }
}

public static class LessonPlanUnitResolver
{
    public static async Task<LessonPlanUnit> FindOrCreateAsync(
        IDbContext context,
        Guid moduleId,
        string rawName,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var normalized = LessonPlanUnitNameNormalizer.Normalize(rawName);

        var existing = await context.LessonPlanUnits
            .FirstOrDefaultAsync(
                x => x.ModuleId == moduleId &&
                     x.NameNormalized == normalized,
                cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var nextOrder = await context.LessonPlanUnits
            .Where(x => x.ModuleId == moduleId)
            .Select(x => (int?)x.OrderIndex)
            .MaxAsync(cancellationToken) ?? -1;

        var unit = new LessonPlanUnit
        {
            Id = Guid.NewGuid(),
            ModuleId = moduleId,
            Name = normalized,
            NameNormalized = normalized,
            OrderIndex = nextOrder + 1,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.LessonPlanUnits.Add(unit);
        return unit;
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
