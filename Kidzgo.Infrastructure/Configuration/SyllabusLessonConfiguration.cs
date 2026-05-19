using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class SyllabusLessonConfiguration : IEntityTypeConfiguration<SyllabusLesson>
{
    public void Configure(EntityTypeBuilder<SyllabusLesson> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Topic)
            .HasMaxLength(255);

        builder.Property(x => x.ContentSummary);
        builder.Property(x => x.StructureSummary);
        builder.Property(x => x.StudentBookPages)
            .HasMaxLength(100);
        builder.Property(x => x.TeacherBookPages)
            .HasMaxLength(100);

        builder.HasIndex(x => new { x.SyllabusId, x.OrderIndex })
            .IsUnique();

        builder.HasOne(x => x.Module)
            .WithMany(x => x.SyllabusLessons)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
