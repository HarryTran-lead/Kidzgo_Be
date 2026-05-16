using Kidzgo.Domain.AcademicProgression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Score)
            .HasPrecision(5, 2);

        builder.Property(x => x.TeacherComment);

        builder.HasIndex(x => new { x.StudentProfileId, x.ModuleId, x.AssessedAt });

        builder.HasOne(x => x.StudentProfile)
            .WithMany()
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.Assessments)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Session)
            .WithMany()
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.AssessedByUser)
            .WithMany()
            .HasForeignKey(x => x.AssessedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
