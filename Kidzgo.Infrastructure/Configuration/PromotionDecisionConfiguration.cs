using Kidzgo.Domain.AcademicProgression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class PromotionDecisionConfiguration : IEntityTypeConfiguration<PromotionDecision>
{
    public void Configure(EntityTypeBuilder<PromotionDecision> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason);

        builder.HasIndex(x => new { x.StudentProfileId, x.ModuleId, x.ApprovedAt });

        builder.HasOne(x => x.StudentProfile)
            .WithMany()
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.PromotionDecisions)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ApprovedByUser)
            .WithMany()
            .HasForeignKey(x => x.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
