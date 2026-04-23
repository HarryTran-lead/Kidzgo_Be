using Kidzgo.Domain.Homework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class QuestionBankItemConfiguration : IEntityTypeConfiguration<QuestionBankItem>
{
    public void Configure(EntityTypeBuilder<QuestionBankItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ProgramId)
            .IsRequired();

        builder.Property(x => x.QuestionText)
            .IsRequired();

        builder.Property(x => x.QuestionType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Options)
            .HasColumnType("jsonb");

        builder.Property(x => x.CorrectAnswer);

        builder.Property(x => x.Points)
            .IsRequired();

        builder.Property(x => x.Explanation);

        builder.Property(x => x.Topic)
            .HasMaxLength(100);

        builder.Property(x => x.Skill)
            .HasMaxLength(100);

        builder.Property(x => x.GrammarTags)
            .HasColumnType("jsonb");

        builder.Property(x => x.VocabularyTags)
            .HasColumnType("jsonb");

        builder.Property(x => x.Level)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.CreatedBy);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => new { x.ProgramId, x.Level, x.IsDeleted });

        builder.HasOne(x => x.Program)
            .WithMany(x => x.QuestionBankItems)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
