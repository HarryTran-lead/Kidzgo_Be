using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class PauseEnrollmentRequestHistoryConfiguration : IEntityTypeConfiguration<PauseEnrollmentRequestHistory>
{
    public void Configure(EntityTypeBuilder<PauseEnrollmentRequestHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.PauseEnrollmentRequestId)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.ClassId)
            .IsRequired();

        builder.Property(x => x.EnrollmentId);

        builder.Property(x => x.PreviousStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.NewStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.PauseFrom)
            .IsRequired();

        builder.Property(x => x.PauseTo)
            .IsRequired();

        builder.Property(x => x.ReservedSessionCount)
            .IsRequired();

        builder.Property(x => x.ChangedAt)
            .IsRequired();

        builder.Property(x => x.ChangedBy);

        builder.HasOne(x => x.PauseEnrollmentRequest)
            .WithMany()
            .HasForeignKey(x => x.PauseEnrollmentRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StudentProfile)
            .WithMany()
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Class)
            .WithMany()
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Enrollment)
            .WithMany()
            .HasForeignKey(x => x.EnrollmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ChangedByUser)
            .WithMany()
            .HasForeignKey(x => x.ChangedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
