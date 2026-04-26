using Kidzgo.Application.LandingPages.Shared;

namespace Kidzgo.Application.LandingPages.GetLandingPage;

public sealed class GetLandingPageResponse
{
    public string? LogoUrl { get; init; }
    public LandingPageSectionDto FeaturedProgramsSection { get; init; } = new();
    public LandingPageSectionDto FeaturedClassesSection { get; init; } = new();
    public LandingPageSectionDto FeaturedTeachersSection { get; init; } = new();
    public LandingPageFooterDto Footer { get; init; } = new();
    public IReadOnlyList<LandingPageProgramDto> FeaturedPrograms { get; init; } = Array.Empty<LandingPageProgramDto>();
    public IReadOnlyList<LandingPageClassDto> FeaturedClasses { get; init; } = Array.Empty<LandingPageClassDto>();
    public IReadOnlyList<LandingPageTeacherDto> FeaturedTeachers { get; init; } = Array.Empty<LandingPageTeacherDto>();
    public DateTime? UpdatedAt { get; init; }
}
