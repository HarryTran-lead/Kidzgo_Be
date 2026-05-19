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

        var matchedRule = lookupTexts
            .Select(text => ResolveRule(rules, text))
            .FirstOrDefault(x => x is not null);
        if (matchedRule is not null)
        {
            return modules.FirstOrDefault(x => x.Id == matchedRule.ModuleId);
        }

        return modules.FirstOrDefault(module =>
            lookupTexts.Any(text =>
                module.Name.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                module.Code.Contains(text, StringComparison.OrdinalIgnoreCase)));
    }

    private static CurriculumImportModuleRule? ResolveRule(
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

    private static string Normalize(string value)
    {
        return Regex.Replace(value, @"\s+", " ").Trim();
    }
}
