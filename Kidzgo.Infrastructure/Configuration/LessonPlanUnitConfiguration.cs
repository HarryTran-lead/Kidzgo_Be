using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class LessonPlanUnitConfiguration : IEntityTypeConfiguration<LessonPlanUnit>
{
    public void Configure(EntityTypeBuilder<LessonPlanUnit> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ModuleId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.NameNormalized)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.OrderIndex)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.Module)
            .WithMany(x => x.LessonPlanUnits)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ModuleId, x.NameNormalized })
            .IsUnique();

        builder.HasIndex(x => new { x.ModuleId, x.OrderIndex });
    }
}
