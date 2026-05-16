using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class LessonPlanConfiguration : IEntityTypeConfiguration<LessonPlan>
{
    public void Configure(EntityTypeBuilder<LessonPlan> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ClassId)
            .IsRequired();

        builder.Property(x => x.SessionId)
            .IsRequired();

        builder.HasIndex(x => x.ClassId)
            .HasDatabaseName("IX_LessonPlans_ClassId");

        builder.HasIndex(x => x.SessionId)
            .IsUnique()
            .HasDatabaseName("session_unique");

        builder.Property(x => x.TemplateId);

        builder.Property(x => x.PlannedContent);

        builder.Property(x => x.ActualContent);

        builder.Property(x => x.ActualHomework);

        builder.Property(x => x.TeacherNotes);

        builder.Property(x => x.CompletionPercent)
            .HasPrecision(5, 2);

        builder.Property(x => x.CarryForwardContent);

        // Media/Attachment fields
        builder.Property(x => x.CoverImageUrl)
            .HasMaxLength(500);

        builder.Property(x => x.CoverImageMimeType)
            .HasMaxLength(100);

        builder.Property(x => x.CoverImageFileSize);

        builder.Property(x => x.MediaUrl)
            .HasMaxLength(500);

        builder.Property(x => x.MediaMimeType)
            .HasMaxLength(100);

        builder.Property(x => x.MediaFileSize);

        builder.Property(x => x.MediaType)
            .HasMaxLength(20);

        builder.Property(x => x.SubmittedBy);

        builder.Property(x => x.SubmittedAt);

        builder.Property(x => x.IsDeleted);

        // Relationships
        builder.HasOne(x => x.Class)
            .WithMany(x => x.LessonPlans)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Session)
            .WithOne(x => x.LessonPlan)
            .HasForeignKey<LessonPlan>(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Template)
            .WithMany(x => x.LessonPlans)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SubmittedByUser)
            .WithMany(x => x.SubmittedLessonPlans)
            .HasForeignKey(x => x.SubmittedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
