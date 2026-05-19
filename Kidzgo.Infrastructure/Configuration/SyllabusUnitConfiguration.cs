using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class SyllabusUnitConfiguration : IEntityTypeConfiguration<SyllabusUnit>
{
    public void Configure(EntityTypeBuilder<SyllabusUnit> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Notes);

        builder.HasIndex(x => new { x.SyllabusId, x.OrderIndex })
            .IsUnique();

        builder.HasOne(x => x.Module)
            .WithMany(x => x.SyllabusUnits)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
