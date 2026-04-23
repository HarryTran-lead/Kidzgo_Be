using Kidzgo.Domain.Programs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class BranchProgramConfiguration : IEntityTypeConfiguration<BranchProgram>
{
    public void Configure(EntityTypeBuilder<BranchProgram> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.ProgramId)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.DefaultMakeupClassId);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.BranchId, x.ProgramId })
            .IsUnique();

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.BranchPrograms)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Program)
            .WithMany(x => x.BranchPrograms)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DefaultMakeupClass)
            .WithMany()
            .HasForeignKey(x => x.DefaultMakeupClassId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
