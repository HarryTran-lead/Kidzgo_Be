using Kidzgo.Domain.LearningTickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class LearningTicketItemConfiguration : IEntityTypeConfiguration<LearningTicketItem>
{
    public void Configure(EntityTypeBuilder<LearningTicketItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StudentProfileId).IsRequired();
        builder.Property(x => x.RegistrationId).IsRequired();
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.StudentProfile)
            .WithMany()
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Registration)
            .WithMany()
            .HasForeignKey(x => x.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ConsumedBySession)
            .WithMany()
            .HasForeignKey(x => x.ConsumedBySessionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ConsumedByAttendance)
            .WithMany()
            .HasForeignKey(x => x.ConsumedByAttendanceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.RegistrationId, x.Status, x.CreatedAt });
        builder.HasIndex(x => x.StudentProfileId);
    }
}
