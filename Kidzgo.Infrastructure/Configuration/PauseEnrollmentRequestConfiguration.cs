using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class PauseEnrollmentRequestConfiguration : IEntityTypeConfiguration<PauseEnrollmentRequest>
{
    public void Configure(EntityTypeBuilder<PauseEnrollmentRequest> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.ClassId)
            .IsRequired(false);

        builder.Property(x => x.PauseFrom)
            .IsRequired();

        builder.Property(x => x.PauseTo)
            .IsRequired();

        builder.Property(x => x.Reason);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.RequestedAt)
            .IsRequired();

        builder.Property(x => x.ApprovedBy);

        builder.Property(x => x.ApprovedAt);

        builder.Property(x => x.CancelledBy);

        builder.Property(x => x.CancelledAt);

        builder.Property(x => x.Outcome)
            .HasConversion<string>()
            .HasMaxLength(40);

        builder.Property(x => x.OutcomeNote);

        builder.Property(x => x.OutcomeBy);

        builder.Property(x => x.OutcomeAt);

        builder.Property(x => x.ReassignedClassId);

        builder.Property(x => x.ReassignedEnrollmentId);

        builder.Property(x => x.OutcomeCompletedBy);

        builder.Property(x => x.OutcomeCompletedAt);

        builder.Property(x => x.ReservedSessionCount)
            .IsRequired();

        builder.Property(x => x.ReservationExpiresOn);

        builder.Property(x => x.ReservationSnapshotAt);

        builder.HasOne(x => x.StudentProfile)
            .WithMany()
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Class)
            .WithMany()
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ReassignedClass)
            .WithMany()
            .HasForeignKey(x => x.ReassignedClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReassignedEnrollment)
            .WithMany()
            .HasForeignKey(x => x.ReassignedEnrollmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ApprovedByUser)
            .WithMany()
            .HasForeignKey(x => x.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CancelledByUser)
            .WithMany()
            .HasForeignKey(x => x.CancelledBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OutcomeByUser)
            .WithMany()
            .HasForeignKey(x => x.OutcomeBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OutcomeCompletedByUser)
            .WithMany()
            .HasForeignKey(x => x.OutcomeCompletedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
