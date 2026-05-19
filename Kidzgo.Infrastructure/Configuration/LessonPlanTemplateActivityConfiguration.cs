using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class LessonPlanTemplateActivityConfiguration : IEntityTypeConfiguration<LessonPlanTemplateActivity>
{
    public void Configure(EntityTypeBuilder<LessonPlanTemplateActivity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(255);

        builder.Property(x => x.Resources)
            .HasMaxLength(1000);

        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.LessonPlanTemplate)
            .WithMany(x => x.Activities)
            .HasForeignKey(x => x.LessonPlanTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
