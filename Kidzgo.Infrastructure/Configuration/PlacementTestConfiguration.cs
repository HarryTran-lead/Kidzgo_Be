using Kidzgo.Domain.CRM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class PlacementTestConfiguration : IEntityTypeConfiguration<PlacementTest>
{
    public void Configure(EntityTypeBuilder<PlacementTest> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.LeadId);

        builder.Property(x => x.LeadChildId);

        builder.Property(x => x.StudentProfileId);

        builder.Property(x => x.ScheduledAt);

        builder.Property(x => x.DurationMinutes)
            .HasDefaultValue(60)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.RoomId);

        builder.Property(x => x.Room)
            .HasMaxLength(100);

        builder.Property(x => x.InvigilatorUserId);

        builder.Property(x => x.ResultScore)
            .HasColumnType("numeric");

        builder.Property(x => x.ListeningScore)
            .HasColumnType("numeric");

        builder.Property(x => x.SpeakingScore)
            .HasColumnType("numeric");

        builder.Property(x => x.ReadingScore)
            .HasColumnType("numeric");

        builder.Property(x => x.WritingScore)
            .HasColumnType("numeric");

        builder.Property(x => x.LevelRecommendation)
            .HasMaxLength(100);

        builder.Property(x => x.ProgramRecommendationId);

        builder.Property(x => x.PrimaryLevelRecommendationId);

        builder.Property(x => x.SecondaryLevelRecommendationId);

        builder.Property(x => x.SecondaryProgramRecommendationId);

        builder.Property(x => x.SecondaryProgramSkillFocus)
            .HasMaxLength(50);

        builder.Property(x => x.Notes);

        builder.Property(x => x.AttachmentUrl);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Lead)
            .WithMany(x => x.PlacementTests)
            .HasForeignKey(x => x.LeadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LeadChild)
            .WithMany(x => x.PlacementTests)
            .HasForeignKey(x => x.LeadChildId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StudentProfile)
            .WithMany(x => x.PlacementTests)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Class)
            .WithMany(x => x.PlacementTests)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PlacementRoom)
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.InvigilatorUser)
            .WithMany(x => x.InvigilatedPlacementTests)
            .HasForeignKey(x => x.InvigilatorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProgramRecommendationProgram)
            .WithMany(x => x.PrimaryPlacementTestRecommendations)
            .HasForeignKey(x => x.ProgramRecommendationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PrimaryLevelRecommendationLevel)
            .WithMany()
            .HasForeignKey(x => x.PrimaryLevelRecommendationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SecondaryLevelRecommendationLevel)
            .WithMany()
            .HasForeignKey(x => x.SecondaryLevelRecommendationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SecondaryProgramRecommendationProgram)
            .WithMany(x => x.SecondaryPlacementTestRecommendations)
            .HasForeignKey(x => x.SecondaryProgramRecommendationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
