using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Schools.Errors;

public static class HolidayErrors
{
    public static Error NotFound(Guid? holidayId) => Error.NotFound(
        "Holiday.NotFound",
        $"Holiday with Id = '{holidayId}' was not found");

    public static readonly Error NameRequired = Error.Validation(
        "Holiday.NameRequired",
        "Holiday name is required");

    public static readonly Error InvalidDateRange = Error.Validation(
        "Holiday.InvalidDateRange",
        "EndDate must be greater than or equal to StartDate");

    public static readonly Error BranchNotFound = Error.NotFound(
        "Holiday.BranchNotFound",
        "Branch not found or inactive");
}
