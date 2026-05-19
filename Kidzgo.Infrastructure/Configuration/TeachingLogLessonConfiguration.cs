using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class TeachingLogLessonConfiguration : IEntityTypeConfiguration<TeachingLogLesson>
{
    public void Configure(EntityTypeBuilder<TeachingLogLesson> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CoveragePercent)
            .HasPrecision(5, 2);

        builder.Property(x => x.ProgressStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.TeachingLog)
            .WithMany(x => x.Lessons)
            .HasForeignKey(x => x.TeachingLogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SessionTemplate)
            .WithMany(x => x.TeachingLogLessons)
            .HasForeignKey(x => x.SessionTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.LessonPlanTemplate)
            .WithMany()
            .HasForeignKey(x => x.LessonPlanTemplateId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
