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

        builder.Property(x => x.SyllabusId)
            .IsRequired();

        builder.Property(x => x.ModuleId)
            .IsRequired();

        builder.Property(x => x.LessonPlanUnitId);

        builder.Property(x => x.SessionTemplateId);

        builder.Property(x => x.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.SessionIndex)
            .IsRequired();

        builder.Property(x => x.SessionOrder)
            .IsRequired();

        builder.Property(x => x.OrderIndexInUnit)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.SyllabusMetadata);

        builder.Property(x => x.SyllabusContent);

        builder.Property(x => x.Objectives);

        builder.Property(x => x.LanguageContent);

        builder.Property(x => x.Vocabulary);

        builder.Property(x => x.Grammar);

        builder.Property(x => x.TeachingMethodology);

        builder.Property(x => x.TeacherMaterials);

        builder.Property(x => x.StudentMaterials);

        builder.Property(x => x.Procedure);

        builder.Property(x => x.Evaluation);

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

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Module)
            .WithMany(x => x.LessonPlanTemplates)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Syllabus)
            .WithMany()
            .HasForeignKey(x => x.SyllabusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.LessonPlanUnit)
            .WithMany(x => x.LessonPlanTemplates)
            .HasForeignKey(x => x.LessonPlanUnitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.SessionTemplate)
            .WithOne(x => x.LessonPlanTemplate)
            .HasForeignKey<LessonPlanTemplate>(x => x.SessionTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.CreatedLessonPlanTemplates)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.LessonPlans)
            .WithOne(x => x.Template)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Sessions)
            .WithOne(x => x.LessonPlanTemplate)
            .HasForeignKey(x => x.LessonPlanTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.SyllabusId, x.ModuleId, x.SessionIndex })
            .IsUnique();

        builder.HasIndex(x => new { x.SyllabusId, x.ModuleId, x.SessionOrder })
            .IsUnique();

        builder.HasIndex(x => x.LessonPlanUnitId);

        builder.HasIndex(x => new { x.LessonPlanUnitId, x.OrderIndexInUnit });

        builder.HasIndex(x => x.SessionTemplateId)
            .IsUnique()
            .HasFilter("\"SessionTemplateId\" IS NOT NULL");
    }
}
