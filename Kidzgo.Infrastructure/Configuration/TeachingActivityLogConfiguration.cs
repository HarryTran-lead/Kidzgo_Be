using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class TeachingActivityLogConfiguration : IEntityTypeConfiguration<TeachingActivityLog>
{
    public void Configure(EntityTypeBuilder<TeachingActivityLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.WasCompleted)
            .IsRequired();

        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.TeachingLog)
            .WithMany(x => x.ActivityLogs)
            .HasForeignKey(x => x.TeachingLogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PlannedActivity)
            .WithMany(x => x.TeachingActivityLogs)
            .HasForeignKey(x => x.PlannedActivityId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
