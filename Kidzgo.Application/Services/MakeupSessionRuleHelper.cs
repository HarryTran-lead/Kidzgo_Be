namespace Kidzgo.Application.Services;

public static class MakeupSessionRuleHelper
{
    public static DateOnly GetFirstEligibleMakeupDate(DateOnly sourceDate)
    {
        var candidate = sourceDate.AddDays(1);

        while (!IsEligibleMakeupDate(sourceDate, candidate))
        {
            candidate = candidate.AddDays(1);
        }

        return candidate;
    }

    public static bool IsEligibleMakeupDate(DateOnly sourceDate, DateOnly targetDate)
    {
        return targetDate > sourceDate;
    }
}
