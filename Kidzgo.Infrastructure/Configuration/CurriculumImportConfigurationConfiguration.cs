using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class CurriculumImportConfigurationConfiguration : IEntityTypeConfiguration<CurriculumImportConfiguration>
{
    public void Configure(EntityTypeBuilder<CurriculumImportConfiguration> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RegularUnitLessonPlanCount).IsRequired();
        builder.Property(x => x.StarterUnitLessonPlanCount).IsRequired();
        builder.Property(x => x.RevisionLessonPlanCount).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => new { x.ProgramId, x.LevelId }).IsUnique();

        builder.HasOne(x => x.Program)
            .WithMany()
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Level)
            .WithMany()
            .HasForeignKey(x => x.LevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ModuleRules)
            .WithOne(x => x.CurriculumImportConfiguration)
            .HasForeignKey(x => x.CurriculumImportConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
