using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ClassModuleProgressConfiguration : IEntityTypeConfiguration<ClassModuleProgress>
{
    public void Configure(EntityTypeBuilder<ClassModuleProgress> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.RequiredSessions)
            .IsRequired();

        builder.Property(x => x.CompletedSessions)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.ClassId, x.ModuleId })
            .IsUnique();

        builder.HasOne(x => x.Class)
            .WithMany(x => x.ModuleProgresses)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.ClassModuleProgresses)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
