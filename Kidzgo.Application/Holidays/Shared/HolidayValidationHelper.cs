using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools.Errors;

namespace Kidzgo.Application.Holidays.Shared;

internal static class HolidayValidationHelper
{
    public static Result Validate(
        string? name,
        DateOnly startDate,
        DateOnly endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(HolidayErrors.NameRequired);
        }

        if (endDate < startDate)
        {
            return Result.Failure(HolidayErrors.InvalidDateRange);
        }

        return Result.Success();
    }
}
