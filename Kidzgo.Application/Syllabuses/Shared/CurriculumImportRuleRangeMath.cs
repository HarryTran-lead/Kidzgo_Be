using Kidzgo.Application.Syllabuses.UpsertCurriculumImportConfiguration;
using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Application.Syllabuses.Shared;

internal static class CurriculumImportRuleRangeMath
{
    public static bool HasUnitRange(CurriculumImportModuleRule rule)
        => HasUnitRange(rule.UnitFrom, rule.UnitTo);

    public static bool HasUnitRange(UpsertCurriculumImportModuleRuleModel rule)
        => HasUnitRange(rule.UnitFrom, rule.UnitTo);

    public static int GetUnitCount(CurriculumImportModuleRule rule)
        => GetUnitCount(rule.UnitFrom, rule.UnitTo);

    public static int GetUnitCount(UpsertCurriculumImportModuleRuleModel rule)
        => GetUnitCount(rule.UnitFrom, rule.UnitTo);

    public static bool ContainsUnit(CurriculumImportModuleRule rule, int unitNumber)
        => ContainsUnit(rule.UnitFrom, rule.UnitTo, unitNumber);

    public static bool ContainsUnit(UpsertCurriculumImportModuleRuleModel rule, int unitNumber)
        => ContainsUnit(rule.UnitFrom, rule.UnitTo, unitNumber);

    public static int GetUnitOffset(CurriculumImportModuleRule rule, int unitNumber, int lessonPlanCountPerUnit)
    {
        return GetUnitOffset(rule.UnitFrom, rule.UnitTo, unitNumber, lessonPlanCountPerUnit);
    }

    private static bool HasUnitRange(int? unitFrom, int? unitTo)
    {
        return unitFrom.HasValue && unitTo.HasValue && unitFrom.Value <= unitTo.Value;
    }

    private static int GetUnitCount(int? unitFrom, int? unitTo)
    {
        if (!HasUnitRange(unitFrom, unitTo))
        {
            return 0;
        }

        return Math.Max(0, unitTo!.Value - unitFrom!.Value + 1);
    }

    private static bool ContainsUnit(int? unitFrom, int? unitTo, int unitNumber)
    {
        return HasUnitRange(unitFrom, unitTo) &&
               unitFrom!.Value <= unitNumber &&
               unitNumber <= unitTo!.Value;
    }

    private static int GetUnitOffset(int? unitFrom, int? unitTo, int unitNumber, int lessonPlanCountPerUnit)
    {
        if (!ContainsUnit(unitFrom, unitTo, unitNumber))
        {
            throw new ArgumentOutOfRangeException(nameof(unitNumber));
        }

        return (unitNumber - unitFrom!.Value) * lessonPlanCountPerUnit;
    }
}
