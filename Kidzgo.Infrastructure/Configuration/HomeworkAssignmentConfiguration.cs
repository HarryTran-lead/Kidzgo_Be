using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class HomeworkAssignmentConfiguration : IEntityTypeConfiguration<HomeworkAssignment>
{
    public void Configure(EntityTypeBuilder<HomeworkAssignment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ClassId)
            .IsRequired();

        builder.Property(x => x.SessionId);

        builder.Property(x => x.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Description);

        builder.Property(x => x.StartDate);

        builder.Property(x => x.DueAt);

        builder.Property(x => x.Book)
            .HasMaxLength(255);

        builder.Property(x => x.Pages)
            .HasMaxLength(50);

        builder.Property(x => x.Skills)
            .HasMaxLength(100);

        builder.Property(x => x.Topic)
            .HasMaxLength(100);

        builder.Property(x => x.GrammarTags)
            .HasColumnType("jsonb");

        builder.Property(x => x.VocabularyTags)
            .HasColumnType("jsonb");

        builder.Property(x => x.SubmissionType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.MaxScore)
            .HasColumnType("numeric");

        builder.Property(x => x.RewardStars);

        builder.Property(x => x.TimeLimitMinutes);

        builder.Property(x => x.MaxAttempts)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.AiHintEnabled)
            .HasDefaultValue(false);

        builder.Property(x => x.AiRecommendEnabled)
            .HasDefaultValue(false);

        builder.Property(x => x.MissionId);

        builder.Property(x => x.Instructions);

        builder.Property(x => x.ExpectedAnswer);

        builder.Property(x => x.Rubric);

        builder.Property(x => x.SpeakingMode)
            .HasMaxLength(20);

        builder.Property(x => x.TargetWords)
            .HasColumnType("jsonb");

        builder.Property(x => x.SpeakingExpectedText);

        builder.Property(x => x.AttachmentUrl);

        builder.Property(x => x.CreatedBy);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Class)
            .WithMany(x => x.HomeworkAssignments)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Session)
            .WithMany(x => x.HomeworkAssignments)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Mission)
            .WithMany(x => x.HomeworkAssignments)
            .HasForeignKey(x => x.MissionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.CreatedHomeworkAssignments)
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.HomeworkStudents)
            .WithOne(x => x.Assignment)
            .HasForeignKey(x => x.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
