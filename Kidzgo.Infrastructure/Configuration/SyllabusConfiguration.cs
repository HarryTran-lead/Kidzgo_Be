using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class SyllabusConfiguration : IEntityTypeConfiguration<Syllabus>
{
    public void Configure(EntityTypeBuilder<Syllabus> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Version)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Edition)
            .HasMaxLength(100);

        builder.Property(x => x.PacingSchemeJson);

        builder.Property(x => x.SourceFileName)
            .HasMaxLength(255);

        builder.Property(x => x.AttachmentUrl)
            .HasMaxLength(500);

        builder.Property(x => x.DocumentStatus)
            .HasMaxLength(50)
            .HasDefaultValue("Draft")
            .IsRequired();

        builder.Property(x => x.SourceType)
            .HasMaxLength(50)
            .HasDefaultValue("Manual")
            .IsRequired();

        builder.Property(x => x.ParserVersion)
            .HasMaxLength(100);

        builder.Property(x => x.DocumentVersion)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.SectionsJson);

        builder.Property(x => x.WarningsJson);

        builder.Property(x => x.ArchiveReason)
            .HasMaxLength(500);

        builder.HasIndex(x => new { x.ProgramId, x.LevelId, x.Code, x.Version })
            .IsUnique();

        builder.HasOne(x => x.Program)
            .WithMany(x => x.Syllabuses)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Level)
            .WithMany(x => x.Syllabuses)
            .HasForeignKey(x => x.LevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.CreatedSyllabuses)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Units)
            .WithOne(x => x.Syllabus)
            .HasForeignKey(x => x.SyllabusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Lessons)
            .WithOne(x => x.Syllabus)
            .HasForeignKey(x => x.SyllabusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Resources)
            .WithOne(x => x.Syllabus)
            .HasForeignKey(x => x.SyllabusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.SessionTemplates)
            .WithOne(x => x.Syllabus)
            .HasForeignKey(x => x.SyllabusId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
