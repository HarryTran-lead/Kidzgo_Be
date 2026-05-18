using Kidzgo.Domain.ProgramProgressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class ProgramProgressionAssessmentConfiguration : IEntityTypeConfiguration<ProgramProgressionAssessment>
{
    public void Configure(EntityTypeBuilder<ProgramProgressionAssessment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Method)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.ListeningScore).HasColumnType("numeric");
        builder.Property(x => x.SpeakingScore).HasColumnType("numeric");
        builder.Property(x => x.ReadingWritingScore).HasColumnType("numeric");
        builder.Property(x => x.ReadingScore).HasColumnType("numeric");
        builder.Property(x => x.WritingScore).HasColumnType("numeric");
        builder.Property(x => x.OverallScore).HasColumnType("numeric");
        builder.Property(x => x.Comment);
        builder.Property(x => x.AttachmentUrls);
        builder.Property(x => x.ResultBand).HasMaxLength(200);
        builder.Property(x => x.ResultLevel).HasMaxLength(100);
        builder.Property(x => x.ApprovalNote);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOne(x => x.Rule)
            .WithMany()
            .HasForeignKey(x => x.RuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StudentProfile)
            .WithMany()
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SourceLevel)
            .WithMany()
            .HasForeignKey(x => x.SourceLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetLevel)
            .WithMany()
            .HasForeignKey(x => x.TargetLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SourceProgram)
            .WithMany()
            .HasForeignKey(x => x.SourceProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetProgram)
            .WithMany()
            .HasForeignKey(x => x.TargetProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SourceRegistration)
            .WithMany()
            .HasForeignKey(x => x.SourceRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SourceEnrollment)
            .WithMany()
            .HasForeignKey(x => x.SourceEnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RecordedByUser)
            .WithMany()
            .HasForeignKey(x => x.RecordedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ApprovedByUser)
            .WithMany()
            .HasForeignKey(x => x.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ApprovedTuitionPlan)
            .WithMany()
            .HasForeignKey(x => x.ApprovedTuitionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.GeneratedRegistration)
            .WithMany()
            .HasForeignKey(x => x.GeneratedRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.RuleId);
        builder.HasIndex(x => x.ScheduleParticipantId).IsUnique();
        builder.HasIndex(x => x.StudentProfileId);
        builder.HasIndex(x => x.SourceLevelId);
        builder.HasIndex(x => x.TargetLevelId);
        builder.HasIndex(x => x.SourceRegistrationId);
        builder.HasIndex(x => x.SourceProgramId);
        builder.HasIndex(x => x.TargetProgramId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IsEligible);
    }
}
