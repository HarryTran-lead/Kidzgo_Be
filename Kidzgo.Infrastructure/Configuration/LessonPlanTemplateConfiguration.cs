using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class LessonPlanTemplateConfiguration : IEntityTypeConfiguration<LessonPlanTemplate>
{
    public void Configure(EntityTypeBuilder<LessonPlanTemplate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ProgramId)
            .IsRequired();

        builder.Property(x => x.ModuleId);

        builder.Property(x => x.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Level)
            .HasMaxLength(100);

        builder.Property(x => x.SessionIndex)
            .IsRequired();

        builder.Property(x => x.SessionOrder)
            .IsRequired();

        builder.Property(x => x.SyllabusMetadata);

        builder.Property(x => x.SyllabusContent);

        builder.Property(x => x.SourceFileName)
            .HasMaxLength(255);

        builder.Property(x => x.AttachmentUrl)
            .HasMaxLength(500);

        builder.Property(x => x.AttachmentMimeType)
            .HasMaxLength(100);

        builder.Property(x => x.AttachmentFileSize);

        builder.Property(x => x.AttachmentOriginalFileName)
            .HasMaxLength(255);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.Property(x => x.CreatedBy);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Program)
            .WithMany(x => x.LessonPlanTemplates)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.LessonPlanTemplates)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.CreatedLessonPlanTemplates)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.LessonPlans)
            .WithOne(x => x.Template)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
