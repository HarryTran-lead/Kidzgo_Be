using Kidzgo.Domain.Programs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class LevelConfiguration : IEntityTypeConfiguration<Level>
{
    public void Configure(EntityTypeBuilder<Level> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Description);

        builder.HasIndex(x => new { x.ProgramId, x.Code }).IsUnique();
        builder.HasIndex(x => new { x.ProgramId, x.Order }).IsUnique();

        builder.HasOne(x => x.Program)
            .WithMany(x => x.Levels)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
