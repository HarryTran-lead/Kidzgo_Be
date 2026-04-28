using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class MakeupSettingsConfiguration : IEntityTypeConfiguration<MakeupSettings>
{
    public void Configure(EntityTypeBuilder<MakeupSettings> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreditExpiryDays)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);
    }
}
