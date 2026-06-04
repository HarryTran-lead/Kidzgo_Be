using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Services;

internal static class TicketCompatibilityRuleSupport
{
    internal static SlotDayGroup CombineDayGroups(IEnumerable<SlotDayGroup>? values)
    {
        return Combine(values, SlotDayGroup.None);
    }

    internal static SlotTimeBand CombineTimeBands(IEnumerable<SlotTimeBand>? values)
    {
        return Combine(values, SlotTimeBand.None);
    }

    internal static SlotTeacherType CombineTeacherTypes(IEnumerable<SlotTeacherType>? values)
    {
        return Combine(values, SlotTeacherType.None);
    }

    internal static SlotUsageType CombineUsageTypes(IEnumerable<SlotUsageType>? values)
    {
        return Combine(values, SlotUsageType.None);
    }

    internal static List<SlotDayGroup> ExpandDayGroups(SlotDayGroup mask)
    {
        return Expand(mask)
            .Where(x => x != SlotDayGroup.None)
            .ToList();
    }

    internal static List<SlotTimeBand> ExpandTimeBands(SlotTimeBand mask)
    {
        return Expand(mask)
            .Where(x => x != SlotTimeBand.None)
            .ToList();
    }

    internal static List<SlotTeacherType> ExpandTeacherTypes(SlotTeacherType mask)
    {
        return Expand(mask)
            .Where(x => x != SlotTeacherType.None)
            .ToList();
    }

    internal static List<SlotUsageType> ExpandUsageTypes(SlotUsageType mask)
    {
        return Expand(mask)
            .Where(x => x != SlotUsageType.None)
            .ToList();
    }

    internal static bool IsValidSingleDayGroup(SlotDayGroup value)
    {
        return value is SlotDayGroup.None or SlotDayGroup.Weekday or SlotDayGroup.Weekend;
    }

    internal static bool IsValidSingleTimeBand(SlotTimeBand value)
    {
        return value is SlotTimeBand.None or SlotTimeBand.Morning or SlotTimeBand.Afternoon or SlotTimeBand.Evening;
    }

    internal static bool IsValidSingleTeacherType(SlotTeacherType value)
    {
        return value is SlotTeacherType.None or SlotTeacherType.Standard or SlotTeacherType.Native;
    }

    internal static bool IsValidSingleUsageType(SlotUsageType value)
    {
        return value is SlotUsageType.None or
               SlotUsageType.Standard or
               SlotUsageType.Makeup or
               SlotUsageType.Remedial or
               SlotUsageType.Review or
               SlotUsageType.Custom;
    }

    internal static bool Matches(SlotDayGroup allowedMask, SlotDayGroup slotValue)
    {
        return MatchesEnum(allowedMask, slotValue, SlotDayGroup.None);
    }

    internal static bool Matches(SlotTimeBand allowedMask, SlotTimeBand slotValue)
    {
        return MatchesEnum(allowedMask, slotValue, SlotTimeBand.None);
    }

    internal static bool Matches(SlotTeacherType allowedMask, SlotTeacherType slotValue)
    {
        return MatchesEnum(allowedMask, slotValue, SlotTeacherType.None);
    }

    internal static bool Matches(SlotUsageType allowedMask, SlotUsageType slotValue)
    {
        return MatchesEnum(allowedMask, slotValue, SlotUsageType.None);
    }

    private static T Combine<T>(IEnumerable<T>? values, T none)
        where T : struct, Enum
    {
        if (values is null)
        {
            return none;
        }

        long mask = 0;
        foreach (var value in values)
        {
            mask |= Convert.ToInt64(value);
        }

        return (T)Enum.ToObject(typeof(T), mask);
    }

    private static IEnumerable<T> Expand<T>(T mask)
        where T : struct, Enum
    {
        var rawValue = Convert.ToInt64(mask);
        foreach (var value in Enum.GetValues<T>())
        {
            var candidate = Convert.ToInt64(value);
            if (candidate == 0)
            {
                continue;
            }

            if ((rawValue & candidate) == candidate)
            {
                yield return value;
            }
        }
    }

    private static bool MatchesEnum<T>(T allowedMask, T slotValue, T none)
        where T : struct, Enum
    {
        var noneValue = Convert.ToInt64(none);
        var allowedValue = Convert.ToInt64(allowedMask);
        var slotRawValue = Convert.ToInt64(slotValue);

        if (allowedValue == noneValue || slotRawValue == noneValue)
        {
            return true;
        }

        return (allowedValue & slotRawValue) == slotRawValue;
    }
}
