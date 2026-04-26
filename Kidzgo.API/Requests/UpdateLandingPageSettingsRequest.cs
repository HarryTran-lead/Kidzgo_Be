namespace Kidzgo.API.Requests;

public sealed class UpdateLandingPageSettingsRequest
{
    public string? LogoUrl { get; set; }
    public string? FeaturedProgramsSectionTitle { get; set; }
    public string? FeaturedProgramsSectionSubtitle { get; set; }
    public string? FeaturedClassesSectionTitle { get; set; }
    public string? FeaturedClassesSectionSubtitle { get; set; }
    public string? FeaturedTeachersSectionTitle { get; set; }
    public string? FeaturedTeachersSectionSubtitle { get; set; }
    public string? FooterAddress { get; set; }
    public string? FooterContactPhone { get; set; }
    public List<string>? FooterContactPhones { get; set; }
    public string? FooterContactEmail { get; set; }
    public List<string>? FooterAddresses { get; set; }
    public List<LandingPageFooterSocialLinkRequest>? FooterSocialLinks { get; set; }
    public List<Guid>? FeaturedProgramIds { get; set; }
    public List<Guid>? FeaturedClassIds { get; set; }
    public List<LandingPageFeaturedItemConfigRequest>? FeaturedPrograms { get; set; }
    public List<LandingPageFeaturedItemConfigRequest>? FeaturedClasses { get; set; }
    public List<Guid>? FeaturedTeacherIds { get; set; }
}

public sealed class LandingPageFeaturedItemConfigRequest
{
    public Guid Id { get; set; }
    public List<string>? Tags { get; set; }
}

public sealed class LandingPageFooterSocialLinkRequest
{
    public string? Label { get; set; }
    public string? Url { get; set; }
    public string? IconKey { get; set; }
}
