using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class TeachingLogConfiguration : IEntityTypeConfiguration<TeachingLog>
{
    public void Configure(EntityTypeBuilder<TeachingLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.SessionId)
            .IsUnique();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ActualTeachingType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.LessonPlan)
            .WithOne(x => x.TeachingLog)
            .HasForeignKey<TeachingLog>(x => x.LessonPlanId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.PlannedLessonPlanTemplate)
            .WithMany()
            .HasForeignKey(x => x.PlannedLessonPlanTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ActualLessonPlanTemplate)
            .WithMany()
            .HasForeignKey(x => x.ActualLessonPlanTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.SubmittedByUser)
            .WithMany(x => x.TeachingLogs)
            .HasForeignKey(x => x.SubmittedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
