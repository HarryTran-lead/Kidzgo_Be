using Kidzgo.Domain.AcademicProgression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class TeacherEvaluationConfiguration : IEntityTypeConfiguration<TeacherEvaluation>
{
    public void Configure(EntityTypeBuilder<TeacherEvaluation> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Notes);

        builder.HasIndex(x => new { x.StudentProfileId, x.ModuleId, x.EvaluatedAt });

        builder.HasOne(x => x.StudentProfile)
            .WithMany()
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.TeacherEvaluations)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EvaluatedByUser)
            .WithMany()
            .HasForeignKey(x => x.EvaluatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
