using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class StudentBranchStateConfiguration : IEntityTypeConfiguration<StudentBranchState>
{
    public void Configure(EntityTypeBuilder<StudentBranchState> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.HomeBranchId)
            .IsRequired();

        builder.Property(x => x.ActiveBranchId)
            .IsRequired();

        builder.Property(x => x.AllowCrossBranchEnrollment)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.StudentProfileId)
            .IsUnique();

        builder.HasOne(x => x.StudentProfile)
            .WithOne()
            .HasForeignKey<StudentBranchState>(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.HomeBranch)
            .WithMany()
            .HasForeignKey(x => x.HomeBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ActiveBranch)
            .WithMany()
            .HasForeignKey(x => x.ActiveBranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
