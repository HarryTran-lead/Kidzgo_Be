using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class SessionTemplateConfiguration : IEntityTypeConfiguration<SessionTemplate>
{
    public void Configure(EntityTypeBuilder<SessionTemplate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SyllabusId)
            .IsRequired();

        builder.Property(x => x.ProgramId)
            .IsRequired();

        builder.Property(x => x.LevelId)
            .IsRequired();

        builder.Property(x => x.SessionIndex)
            .IsRequired();

        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(255);

        builder.Property(x => x.Topic)
            .HasMaxLength(255);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.SyllabusId, x.SessionIndex })
            .IsUnique();

        builder.HasIndex(x => new { x.ModuleId, x.SessionIndexInModule });

        builder.HasOne(x => x.Syllabus)
            .WithMany(x => x.SessionTemplates)
            .HasForeignKey(x => x.SyllabusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Program)
            .WithMany(x => x.SessionTemplates)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Level)
            .WithMany(x => x.SessionTemplates)
            .HasForeignKey(x => x.LevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.SessionTemplates)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
