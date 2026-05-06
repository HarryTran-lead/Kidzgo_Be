using Kidzgo.Domain.ProgramProgressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class ProgramProgressionScheduleParticipantConfiguration : IEntityTypeConfiguration<ProgramProgressionScheduleParticipant>
{
    public void Configure(EntityTypeBuilder<ProgramProgressionScheduleParticipant> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOne(x => x.Schedule)
            .WithMany(x => x.Participants)
            .HasForeignKey(x => x.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StudentProfile)
            .WithMany()
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SourceRegistration)
            .WithMany()
            .HasForeignKey(x => x.SourceRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SourceEnrollment)
            .WithMany()
            .HasForeignKey(x => x.SourceEnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Assessment)
            .WithOne(x => x.ScheduleParticipant)
            .HasForeignKey<ProgramProgressionAssessment>(x => x.ScheduleParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ScheduleId);
        builder.HasIndex(x => x.StudentProfileId);
        builder.HasIndex(x => x.SourceRegistrationId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.ScheduleId, x.SourceRegistrationId }).IsUnique();
    }
}
