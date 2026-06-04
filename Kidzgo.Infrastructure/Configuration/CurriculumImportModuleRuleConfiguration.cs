using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class CurriculumImportModuleRuleConfiguration : IEntityTypeConfiguration<CurriculumImportModuleRule>
{
    public void Configure(EntityTypeBuilder<CurriculumImportModuleRule> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderIndex).IsRequired();

        builder.HasIndex(x => new { x.CurriculumImportConfigurationId, x.ModuleId }).IsUnique();
        builder.HasIndex(x => new { x.CurriculumImportConfigurationId, x.OrderIndex }).IsUnique();

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
