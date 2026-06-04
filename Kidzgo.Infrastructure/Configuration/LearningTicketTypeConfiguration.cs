using Kidzgo.Domain.LearningTickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class LearningTicketTypeConfiguration : IEntityTypeConfiguration<LearningTicketType>
{
    public void Configure(EntityTypeBuilder<LearningTicketType> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.CompatibilityMode)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.AllowedDayGroups)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.AllowedTimeBands)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.AllowedTeacherTypes)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.AllowedUsageTypes)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();
    }
}
