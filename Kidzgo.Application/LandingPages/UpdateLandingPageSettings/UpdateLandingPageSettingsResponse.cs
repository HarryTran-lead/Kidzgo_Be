using Kidzgo.Application.LandingPages.Shared;

namespace Kidzgo.Application.LandingPages.UpdateLandingPageSettings;

public sealed class UpdateLandingPageSettingsResponse
{
    public string? LogoUrl { get; init; }
    public LandingPageSectionDto FeaturedProgramsSection { get; init; } = new();
    public LandingPageSectionDto FeaturedClassesSection { get; init; } = new();
    public LandingPageSectionDto FeaturedTeachersSection { get; init; } = new();
    public string? FooterAddress { get; init; }
    public string? FooterContactPhone { get; init; }
    public IReadOnlyList<string> FooterContactPhones { get; init; } = Array.Empty<string>();
    public string? FooterContactEmail { get; init; }
    public IReadOnlyList<string> FooterAddresses { get; init; } = Array.Empty<string>();
    public IReadOnlyList<LandingPageFooterSocialLinkDto> FooterSocialLinks { get; init; } = Array.Empty<LandingPageFooterSocialLinkDto>();
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
