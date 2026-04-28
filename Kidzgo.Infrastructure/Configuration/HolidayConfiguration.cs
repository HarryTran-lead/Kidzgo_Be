using Kidzgo.Domain.Schools;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.StartDate, x.EndDate });
    }
}
