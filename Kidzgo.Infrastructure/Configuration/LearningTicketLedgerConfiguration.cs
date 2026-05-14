using Kidzgo.Domain.LearningTickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class LearningTicketLedgerConfiguration : IEntityTypeConfiguration<LearningTicketLedger>
{
    public void Configure(EntityTypeBuilder<LearningTicketLedger> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StudentProfileId).IsRequired();
        builder.Property(x => x.RegistrationId).IsRequired();
        builder.Property(x => x.TransactionType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.Reason)
            .HasMaxLength(500)
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

        builder.HasOne(x => x.LearningTicketItem)
            .WithMany()
            .HasForeignKey(x => x.LearningTicketItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Session)
            .WithMany()
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Attendance)
            .WithMany()
            .HasForeignKey(x => x.AttendanceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.RegistrationId, x.CreatedAt });
        builder.HasIndex(x => x.StudentProfileId);
    }
}
