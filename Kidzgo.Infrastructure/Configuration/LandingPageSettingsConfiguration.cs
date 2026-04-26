using Kidzgo.Domain.Website;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class LandingPageSettingsConfiguration : IEntityTypeConfiguration<LandingPageSettings>
{
    public void Configure(EntityTypeBuilder<LandingPageSettings> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LogoUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.FeaturedProgramsSectionTitle)
            .HasMaxLength(255);

        builder.Property(x => x.FeaturedProgramsSectionSubtitle)
            .HasMaxLength(1000);

        builder.Property(x => x.FeaturedClassesSectionTitle)
            .HasMaxLength(255);

        builder.Property(x => x.FeaturedClassesSectionSubtitle)
            .HasMaxLength(1000);

        builder.Property(x => x.FeaturedTeachersSectionTitle)
            .HasMaxLength(255);

        builder.Property(x => x.FeaturedTeachersSectionSubtitle)
            .HasMaxLength(1000);

        builder.Property(x => x.FooterAddress)
            .HasMaxLength(500);

        builder.Property(x => x.FooterContactPhone)
            .HasMaxLength(100);

        builder.Property(x => x.FooterContactPhonesJson)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.FooterContactEmail)
            .HasMaxLength(255);

        builder.Property(x => x.FooterAddressesJson)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.FooterSocialLinksJson)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.FeaturedProgramIdsJson)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.FeaturedClassIdsJson)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.FeaturedProgramConfigsJson)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.FeaturedClassConfigsJson)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.FeaturedTeacherIdsJson)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);
    }
}
