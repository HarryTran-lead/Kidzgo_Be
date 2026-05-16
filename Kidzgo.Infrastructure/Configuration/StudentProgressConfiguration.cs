using Kidzgo.Domain.AcademicProgression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class StudentProgressConfiguration : IEntityTypeConfiguration<StudentProgress>
{
    public void Configure(EntityTypeBuilder<StudentProgress> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompletionPercent)
            .HasPrecision(5, 2);

        builder.HasIndex(x => new { x.StudentProfileId, x.ModuleId }).IsUnique();

        builder.HasOne(x => x.StudentProfile)
            .WithMany()
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.StudentProgresses)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LastAssessment)
            .WithMany()
            .HasForeignKey(x => x.LastAssessmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.CurrentLessonPlanTemplate)
            .WithMany()
            .HasForeignKey(x => x.CurrentLessonPlanTemplateId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
