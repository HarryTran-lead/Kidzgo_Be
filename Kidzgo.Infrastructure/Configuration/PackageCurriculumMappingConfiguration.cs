using Kidzgo.Domain.Programs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class PackageCurriculumMappingConfiguration : IEntityTypeConfiguration<PackageCurriculumMapping>
{
    public void Configure(EntityTypeBuilder<PackageCurriculumMapping> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TuitionPlanId)
            .IsRequired();

        builder.Property(x => x.SyllabusId)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.TuitionPlanId, x.SyllabusId })
            .IsUnique();

        builder.HasIndex(x => new { x.TuitionPlanId, x.IsActive });

        builder.HasOne(x => x.TuitionPlan)
            .WithMany(x => x.CurriculumMappings)
            .HasForeignKey(x => x.TuitionPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Syllabus)
            .WithMany(x => x.PackageCurriculumMappings)
            .HasForeignKey(x => x.SyllabusId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
