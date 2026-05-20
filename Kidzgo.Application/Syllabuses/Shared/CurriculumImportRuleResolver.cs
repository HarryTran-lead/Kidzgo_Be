using System.Text.RegularExpressions;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Programs;

namespace Kidzgo.Application.Syllabuses.Shared;

internal static class CurriculumImportRuleResolver
{
    private static readonly Regex TaggedNumberRegex = new(
        @"(UNIT|REVISION)\s*(\d+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static Module? Resolve(
        IReadOnlyList<Module> modules,
        IReadOnlyCollection<CurriculumImportModuleRule> rules,
        params string?[] hints)
    {
        var lookupTexts = hints
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(static x => Normalize(x!))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (lookupTexts.Count == 0)
        {
            return null;
        }

        var matchedRule = ResolveRule(rules, hints);
        if (matchedRule is not null)
        {
            return modules.FirstOrDefault(x => x.Id == matchedRule.ModuleId);
        }

        return modules.FirstOrDefault(module =>
            lookupTexts.Any(text =>
                module.Name.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                module.Code.Contains(text, StringComparison.OrdinalIgnoreCase)));
    }

    public static CurriculumImportModuleRule? ResolveRule(
        IReadOnlyCollection<CurriculumImportModuleRule> rules,
        params string?[] hints)
    {
        var lookupTexts = hints
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(static x => Normalize(x!))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return lookupTexts
            .Select(text => ResolveRuleFromText(rules, text))
            .FirstOrDefault(x => x is not null);
    }

    public static int? ResolveSessionIndex(
        CurriculumImportConfiguration configuration,
        CurriculumImportModuleRule rule,
        params string?[] hints)
    {
        var lookupTexts = hints
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(static x => Normalize(x!))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(static x => ExtractLessonIndex(x).HasValue)
            .ToList();

        foreach (var text in lookupTexts)
        {
            var unitMatch = Regex.Match(
                text,
                @"\bUNIT\s*(STARTER|0*\d+)\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (unitMatch.Success)
            {
                var lessonIndex = ExtractLessonIndex(text) ?? 1;
                if (unitMatch.Groups[1].Value.Equals("STARTER", StringComparison.OrdinalIgnoreCase))
                {
                    if (!rule.IncludeStarterUnit ||
                        lessonIndex < 1 ||
                        lessonIndex > configuration.StarterUnitLessonPlanCount)
                    {
                        return null;
                    }

                    return lessonIndex;
                }

                if (!int.TryParse(unitMatch.Groups[1].Value, out var unitNumber) ||
                    !rule.UnitFrom.HasValue ||
                    !rule.UnitTo.HasValue ||
                    unitNumber < rule.UnitFrom.Value ||
                    unitNumber > rule.UnitTo.Value)
                {
                    continue;
                }

                if (lessonIndex < 1 || lessonIndex > configuration.RegularUnitLessonPlanCount)
                {
                    return null;
                }

                var offset = rule.IncludeStarterUnit ? configuration.StarterUnitLessonPlanCount : 0;
                offset += (unitNumber - rule.UnitFrom.Value) * configuration.RegularUnitLessonPlanCount;

                return offset + lessonIndex;
            }

            var revisionMatch = Regex.Match(
                text,
                @"\bREVISION\s*0*(\d+)\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (revisionMatch.Success)
            {
                if (!int.TryParse(revisionMatch.Groups[1].Value, out var revisionNumber) ||
                    rule.RevisionNumber != revisionNumber)
                {
                    continue;
                }

                var lessonIndex = ExtractLessonIndex(text) ?? 1;
                if (lessonIndex < 1 || lessonIndex > configuration.RevisionLessonPlanCount)
                {
                    return null;
                }

                var offset = rule.IncludeStarterUnit ? configuration.StarterUnitLessonPlanCount : 0;
                if (rule.UnitFrom.HasValue && rule.UnitTo.HasValue)
                {
                    offset += (rule.UnitTo.Value - rule.UnitFrom.Value + 1) *
                              configuration.RegularUnitLessonPlanCount;
                }

                return offset + lessonIndex;
            }
        }

        return null;
    }

    private static CurriculumImportModuleRule? ResolveRuleFromText(
        IReadOnlyCollection<CurriculumImportModuleRule> rules,
        string text)
    {
        if (rules.Count == 0)
        {
            return null;
        }

        if (text.Contains("unit starter", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("unit hello", StringComparison.OrdinalIgnoreCase) ||
            text.Equals("starter", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("hello", StringComparison.OrdinalIgnoreCase))
        {
            return rules
                .OrderBy(x => x.OrderIndex)
                .FirstOrDefault(x => x.IncludeStarterUnit);
        }

        var taggedMatch = TaggedNumberRegex.Match(text);
        if (!taggedMatch.Success)
        {
            return null;
        }

        var kind = taggedMatch.Groups[1].Value;
        var number = int.Parse(taggedMatch.Groups[2].Value);

        if (kind.Equals("REVISION", StringComparison.OrdinalIgnoreCase))
        {
            return rules
                .OrderBy(x => x.OrderIndex)
                .FirstOrDefault(x => x.RevisionNumber == number);
        }

        return rules
            .OrderBy(x => x.OrderIndex)
            .FirstOrDefault(x =>
                x.UnitFrom.HasValue &&
                x.UnitTo.HasValue &&
                x.UnitFrom.Value <= number &&
                number <= x.UnitTo.Value);
    }

    private static int? ExtractLessonIndex(string text)
    {
        var match = Regex.Match(
            text,
            @"\bLESSON\s*0*(\d+)\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        return match.Success && int.TryParse(match.Groups[1].Value, out var lessonIndex)
            ? lessonIndex
            : null;
    }

    private static string Normalize(string value)
    {
        return Regex.Replace(value, @"\s+", " ").Trim();
    }
}
