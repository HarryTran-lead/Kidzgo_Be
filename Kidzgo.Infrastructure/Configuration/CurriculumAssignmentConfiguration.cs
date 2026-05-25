using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class CurriculumAssignmentConfiguration : IEntityTypeConfiguration<CurriculumAssignment>
{
    public void Configure(EntityTypeBuilder<CurriculumAssignment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.ProgramId)
            .IsRequired();

        builder.Property(x => x.LevelId)
            .IsRequired();

        builder.Property(x => x.SyllabusId)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.BranchId, x.ProgramId, x.LevelId, x.SyllabusId, x.EffectiveFrom })
            .IsUnique();

        builder.HasIndex(x => new { x.BranchId, x.ProgramId, x.LevelId, x.IsActive });

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Program)
            .WithMany()
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Level)
            .WithMany()
            .HasForeignKey(x => x.LevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Syllabus)
            .WithMany()
            .HasForeignKey(x => x.SyllabusId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
