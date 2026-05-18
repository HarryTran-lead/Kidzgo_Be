using Kidzgo.Domain.AcademicProgression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class RemedialPlanConfiguration : IEntityTypeConfiguration<RemedialPlan>
{
    public void Configure(EntityTypeBuilder<RemedialPlan> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.WeakSkills)
            .IsRequired();

        builder.Property(x => x.Notes);

        builder.HasIndex(x => new { x.StudentProfileId, x.ModuleId, x.CreatedAt });

        builder.HasOne(x => x.StudentProfile)
            .WithMany()
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.RemedialPlans)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
