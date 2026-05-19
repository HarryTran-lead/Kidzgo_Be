using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class HomeworkTemplateConfiguration : IEntityTypeConfiguration<HomeworkTemplate>
{
    public void Configure(EntityTypeBuilder<HomeworkTemplate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.MaterialReference)
            .HasMaxLength(500);

        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.Property(x => x.IsRequired)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.LessonPlanTemplate)
            .WithMany(x => x.HomeworkTemplates)
            .HasForeignKey(x => x.LessonPlanTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
