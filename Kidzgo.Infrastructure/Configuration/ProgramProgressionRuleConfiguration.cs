using Kidzgo.Domain.ProgramProgressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class ProgramProgressionRuleConfiguration : IEntityTypeConfiguration<ProgramProgressionRule>
{
    public void Configure(EntityTypeBuilder<ProgramProgressionRule> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Method)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.MinimumOverallScore)
            .HasColumnType("numeric");

        builder.Property(x => x.ShieldMappingJson);
        builder.Property(x => x.ClassificationBandsJson);
        builder.Property(x => x.Notes);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.CarryOverRemainingSessions)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.StopCurrentEnrollmentOnApproval)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOne(x => x.SourceProgram)
            .WithMany()
            .HasForeignKey(x => x.SourceProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetProgram)
            .WithMany()
            .HasForeignKey(x => x.TargetProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SourceProgramId);
        builder.HasIndex(x => x.TargetProgramId);
        builder.HasIndex(x => new { x.SourceProgramId, x.IsActive });
    }
}
