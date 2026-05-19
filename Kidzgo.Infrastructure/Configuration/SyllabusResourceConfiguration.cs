using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class SyllabusResourceConfiguration : IEntityTypeConfiguration<SyllabusResource>
{
    public void Configure(EntityTypeBuilder<SyllabusResource> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocumentName)
            .HasMaxLength(255);
        builder.Property(x => x.Abbreviation)
            .HasMaxLength(50);
        builder.Property(x => x.IntendedUsers)
            .HasMaxLength(255);
        builder.Property(x => x.Notes);

        builder.HasIndex(x => new { x.SyllabusId, x.OrderIndex })
            .IsUnique();
    }
}
