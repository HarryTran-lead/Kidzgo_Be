using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class PauseEnrollmentSettingsConfiguration : IEntityTypeConfiguration<PauseEnrollmentSettings>
{
    public void Configure(EntityTypeBuilder<PauseEnrollmentSettings> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReservationLimitMonths)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);
    }
}
