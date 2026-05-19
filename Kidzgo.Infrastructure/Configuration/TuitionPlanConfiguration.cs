using Kidzgo.Domain.Programs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class TuitionPlanConfiguration : IEntityTypeConfiguration<TuitionPlan>
{
    public void Configure(EntityTypeBuilder<TuitionPlan> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ProgramId)
            .IsRequired();

        builder.Property(x => x.LevelId)
            .IsRequired();

        builder.Property(x => x.ModuleId);

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.TotalSessions)
            .IsRequired();

        builder.Property(x => x.TuitionAmount)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.UnitPriceSession)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.LearningTicketTypeId);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Program)
            .WithMany(x => x.TuitionPlans)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Level)
            .WithMany(x => x.TuitionPlans)
            .HasForeignKey(x => x.LevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.TuitionPlans)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.ClassEnrollments)
            .WithOne(x => x.TuitionPlan)
            .HasForeignKey(x => x.TuitionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LearningTicketType)
            .WithMany()
            .HasForeignKey(x => x.LearningTicketTypeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.ProgramId, x.LevelId, x.ModuleId, x.TotalSessions, x.Name })
            .IsUnique();
    }
}
