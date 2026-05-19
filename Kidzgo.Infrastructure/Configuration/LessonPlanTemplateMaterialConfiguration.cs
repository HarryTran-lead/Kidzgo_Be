using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class LessonPlanTemplateMaterialConfiguration : IEntityTypeConfiguration<LessonPlanTemplateMaterial>
{
    public void Configure(EntityTypeBuilder<LessonPlanTemplateMaterial> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.MaterialType)
            .HasMaxLength(100);

        builder.Property(x => x.ReferenceCode)
            .HasMaxLength(100);

        builder.Property(x => x.Url)
            .HasMaxLength(1000);

        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.LessonPlanTemplate)
            .WithMany(x => x.Materials)
            .HasForeignKey(x => x.LessonPlanTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
