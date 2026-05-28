using Kidzgo.Domain.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class StudentReportConfiguration : IEntityTypeConfiguration<StudentReport>
{
    public void Configure(EntityTypeBuilder<StudentReport> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReportType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.IsParentPublished)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.ParentPublishedAt);

        builder.Property(x => x.ParentPublishedBy);

        builder.Property(x => x.SnapshotJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.SummaryText)
            .HasColumnType("text");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.StudentId, x.ReportPeriodId, x.ReportType, x.CreatedAt });

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
            .WithMany(x => x.StudentReports)
            .HasForeignKey(x => x.ReportPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReportRun)
            .WithMany(x => x.StudentReports)
            .HasForeignKey(x => x.ReportRunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ParentPublishedByUser)
            .WithMany()
            .HasForeignKey(x => x.ParentPublishedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.AIInsights)
            .WithOne(x => x.StudentReport)
            .HasForeignKey(x => x.StudentReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ShareLogs)
            .WithOne(x => x.StudentReport)
            .HasForeignKey(x => x.StudentReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
