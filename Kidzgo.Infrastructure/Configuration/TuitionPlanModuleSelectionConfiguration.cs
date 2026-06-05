using Kidzgo.Domain.Programs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class TuitionPlanModuleSelectionConfiguration : IEntityTypeConfiguration<TuitionPlanModuleSelection>
{
    public void Configure(EntityTypeBuilder<TuitionPlanModuleSelection> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TuitionPlanId)
            .IsRequired();

        builder.Property(x => x.ModuleId)
            .IsRequired();

        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.TuitionPlanId, x.ModuleId })
            .IsUnique();

        builder.HasIndex(x => new { x.TuitionPlanId, x.OrderIndex })
            .IsUnique();

        builder.HasOne(x => x.TuitionPlan)
            .WithMany(x => x.SelectedModules)
            .HasForeignKey(x => x.TuitionPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.TuitionPlanSelections)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
