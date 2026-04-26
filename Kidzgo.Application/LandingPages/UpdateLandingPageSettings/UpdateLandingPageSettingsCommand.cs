using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LandingPages.UpdateLandingPageSettings;

public sealed class UpdateLandingPageSettingsCommand : ICommand<UpdateLandingPageSettingsResponse>
{
    public string? LogoUrl { get; init; }
    public string? FeaturedProgramsSectionTitle { get; init; }
    public string? FeaturedProgramsSectionSubtitle { get; init; }
    public string? FeaturedClassesSectionTitle { get; init; }
    public string? FeaturedClassesSectionSubtitle { get; init; }
    public string? FeaturedTeachersSectionTitle { get; init; }
    public string? FeaturedTeachersSectionSubtitle { get; init; }
    public string? FooterAddress { get; init; }
    public string? FooterContactPhone { get; init; }
    public IReadOnlyList<string> FooterContactPhones { get; init; } = Array.Empty<string>();
    public string? FooterContactEmail { get; init; }
    public IReadOnlyList<string> FooterAddresses { get; init; } = Array.Empty<string>();
    public IReadOnlyList<LandingPageFooterSocialLinkInput> FooterSocialLinks { get; init; } = Array.Empty<LandingPageFooterSocialLinkInput>();
    public IReadOnlyList<LandingPageFeaturedItemInput> FeaturedPrograms { get; init; } = Array.Empty<LandingPageFeaturedItemInput>();
    public IReadOnlyList<LandingPageFeaturedItemInput> FeaturedClasses { get; init; } = Array.Empty<LandingPageFeaturedItemInput>();
    public IReadOnlyList<Guid> FeaturedTeacherIds { get; init; } = Array.Empty<Guid>();
}

public sealed class LandingPageFeaturedItemInput
{
    public Guid Id { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

public sealed class LandingPageFooterSocialLinkInput
{
    public string Label { get; init; } = null!;
    public string Url { get; init; } = null!;
    public string? IconKey { get; init; }
}
