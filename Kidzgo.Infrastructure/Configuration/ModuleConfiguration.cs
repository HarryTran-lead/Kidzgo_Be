using Kidzgo.Domain.Programs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ModuleType.Core)
            .IsRequired();

        builder.Property(x => x.Description);

        builder.HasIndex(x => new { x.LevelId, x.Code }).IsUnique();
        builder.HasIndex(x => new { x.LevelId, x.Order }).IsUnique();

        builder.HasOne(x => x.Level)
            .WithMany(x => x.Modules)
            .HasForeignKey(x => x.LevelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
