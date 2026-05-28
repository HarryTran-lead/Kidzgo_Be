using Kidzgo.Domain.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class ReportShareLogConfiguration : IEntityTypeConfiguration<ReportShareLog>
{
    public void Configure(EntityTypeBuilder<ReportShareLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RecipientName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.RecipientContact)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Channel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ProviderMessageId)
            .HasMaxLength(200);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(x => x.SentAt)
            .IsRequired();

        builder.HasIndex(x => x.ProviderMessageId);

        builder.HasOne(x => x.StudentReport)
            .WithMany(x => x.ShareLogs)
            .HasForeignKey(x => x.StudentReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
