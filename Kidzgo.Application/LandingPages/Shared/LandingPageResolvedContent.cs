namespace Kidzgo.Application.LandingPages.Shared;

internal sealed class LandingPageResolvedContent
{
    public string? LogoUrl { get; init; }
    public LandingPageSectionDto FeaturedProgramsSection { get; init; } = new();
    public LandingPageSectionDto FeaturedClassesSection { get; init; } = new();
    public LandingPageSectionDto FeaturedTeachersSection { get; init; } = new();
    public LandingPageFooterDto Footer { get; init; } = new();
    public IReadOnlyList<Guid> FeaturedProgramIds { get; init; } = Array.Empty<Guid>();
    public IReadOnlyList<Guid> FeaturedClassIds { get; init; } = Array.Empty<Guid>();
    public IReadOnlyList<Guid> FeaturedTeacherIds { get; init; } = Array.Empty<Guid>();
    public IReadOnlyList<LandingPageFeaturedItemConfigDto> FeaturedProgramConfigs { get; init; } = Array.Empty<LandingPageFeaturedItemConfigDto>();
    public IReadOnlyList<LandingPageFeaturedItemConfigDto> FeaturedClassConfigs { get; init; } = Array.Empty<LandingPageFeaturedItemConfigDto>();
    public IReadOnlyList<LandingPageProgramDto> FeaturedPrograms { get; init; } = Array.Empty<LandingPageProgramDto>();
    public IReadOnlyList<LandingPageClassDto> FeaturedClasses { get; init; } = Array.Empty<LandingPageClassDto>();
    public IReadOnlyList<LandingPageTeacherDto> FeaturedTeachers { get; init; } = Array.Empty<LandingPageTeacherDto>();
    public DateTime? UpdatedAt { get; init; }
}
