using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Website.Errors;

public static class LandingPageErrors
{
    public static Error InvalidFeaturedPrograms(IEnumerable<Guid> missingIds) => Error.Validation(
        "LandingPage.FeaturedProgramsInvalid",
        $"Some featured programs are invalid or unavailable: {string.Join(", ", missingIds)}");

    public static Error InvalidFeaturedClasses(IEnumerable<Guid> missingIds) => Error.Validation(
        "LandingPage.FeaturedClassesInvalid",
        $"Some featured classes are invalid or unavailable: {string.Join(", ", missingIds)}");

    public static Error InvalidFeaturedTeachers(IEnumerable<Guid> missingIds) => Error.Validation(
        "LandingPage.FeaturedTeachersInvalid",
        $"Some featured teachers are invalid or unavailable: {string.Join(", ", missingIds)}");
}
