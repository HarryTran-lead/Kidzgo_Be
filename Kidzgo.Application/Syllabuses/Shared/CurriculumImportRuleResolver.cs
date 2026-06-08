using System.Text.RegularExpressions;
using Kidzgo.Application.LessonPlanTemplates.Shared;
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
                @"\bUNIT\s*(STARTER|STARTERS|MOVER|MOVERS|FLYER|FLYERS|HELLO|0*\d+)\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (unitMatch.Success)
            {
                var lessonIndex = ExtractLessonIndex(text) ?? 1;
                var unitToken = unitMatch.Groups[1].Value;
                var unitNumber = int.TryParse(unitToken, out var parsedUnitNumber)
                    ? parsedUnitNumber
                    : 0;

                if (!CurriculumImportRuleRangeMath.ContainsUnit(rule, unitNumber))
                {
                    continue;
                }

                if (lessonIndex < 1 || lessonIndex > configuration.RegularUnitLessonPlanCount)
                {
                    return null;
                }

                var offset = CurriculumImportRuleRangeMath.GetUnitOffset(
                    rule,
                    unitNumber,
                    configuration.RegularUnitLessonPlanCount);

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

                var offset = CurriculumImportRuleRangeMath.GetUnitCount(rule) *
                             configuration.RegularUnitLessonPlanCount;

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

        if (LessonPlanUnitNameNormalizer.IsIntroUnitAliasText(text) ||
            Regex.IsMatch(text, @"\bUNIT\s*0+\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) ||
            text.Equals("starter", StringComparison.OrdinalIgnoreCase))
        {
            return rules
                .OrderBy(x => x.OrderIndex)
                .FirstOrDefault(x => CurriculumImportRuleRangeMath.ContainsUnit(x, 0));
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
                CurriculumImportRuleRangeMath.ContainsUnit(x, number));
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
