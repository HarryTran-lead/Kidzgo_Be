using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class StudentBranchTransferConfiguration : IEntityTypeConfiguration<StudentBranchTransfer>
{
    public void Configure(EntityTypeBuilder<StudentBranchTransfer> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.FromBranchId)
            .IsRequired();

        builder.Property(x => x.ToBranchId)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.StudentProfileId, x.CreatedAt });

        builder.HasOne(x => x.StudentProfile)
            .WithMany()
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FromBranch)
            .WithMany()
            .HasForeignKey(x => x.FromBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToBranch)
            .WithMany()
            .HasForeignKey(x => x.ToBranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
