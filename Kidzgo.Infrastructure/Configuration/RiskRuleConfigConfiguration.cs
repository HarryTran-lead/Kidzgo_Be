using Kidzgo.Domain.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class RiskRuleConfigConfiguration : IEntityTypeConfiguration<RiskRuleConfig>
{
    public void Configure(EntityTypeBuilder<RiskRuleConfig> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RiskType)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.Score)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.ParametersJson)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.RiskType)
            .IsUnique();

        builder.HasOne(x => x.UpdatedByUser)
            .WithMany(x => x.UpdatedRiskRuleConfigs)
            .HasForeignKey(x => x.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
