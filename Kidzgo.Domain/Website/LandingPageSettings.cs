namespace Kidzgo.Domain.Website;

public class LandingPageSettings
{
    public int Id { get; set; }
    public string? LogoUrl { get; set; }
    public string? FeaturedProgramsSectionTitle { get; set; }
    public string? FeaturedProgramsSectionSubtitle { get; set; }
    public string? FeaturedClassesSectionTitle { get; set; }
    public string? FeaturedClassesSectionSubtitle { get; set; }
    public string? FeaturedTeachersSectionTitle { get; set; }
    public string? FeaturedTeachersSectionSubtitle { get; set; }
    public string? FooterAddress { get; set; }
    public string? FooterContactPhone { get; set; }
    public string FooterContactPhonesJson { get; set; } = "[]";
    public string? FooterContactEmail { get; set; }
    public string FooterAddressesJson { get; set; } = "[]";
    public string FooterSocialLinksJson { get; set; } = "[]";
    public string FeaturedProgramIdsJson { get; set; } = "[]";
    public string FeaturedClassIdsJson { get; set; } = "[]";
    public string FeaturedProgramConfigsJson { get; set; } = "[]";
    public string FeaturedClassConfigsJson { get; set; } = "[]";
    public string FeaturedTeacherIdsJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
