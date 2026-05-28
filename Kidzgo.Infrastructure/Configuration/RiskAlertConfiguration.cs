using Kidzgo.Domain.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class RiskAlertConfiguration : IEntityTypeConfiguration<RiskAlert>
{
    public void Configure(EntityTypeBuilder<RiskAlert> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RiskType)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.Severity)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Source)
            .HasColumnType("text");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.StudentId, x.ClassId, x.BranchId, x.RiskType, x.ReportPeriodId, x.Status })
            .HasDatabaseName("risk_alert_dedup_idx");

        builder.HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Class)
            .WithMany()
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReportPeriod)
            .WithMany(x => x.RiskAlerts)
            .HasForeignKey(x => x.ReportPeriodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
