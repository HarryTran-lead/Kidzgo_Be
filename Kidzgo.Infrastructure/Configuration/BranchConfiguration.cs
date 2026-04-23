using Kidzgo.Domain.Schools;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.Programs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Address);

        builder.Property(x => x.ContactPhone)
            .HasMaxLength(32);

        builder.Property(x => x.ContactEmail)
            .HasMaxLength(255);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasMany(x => x.Users)
            .WithOne(x => x.Branch)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Classrooms)
            .WithOne(x => x.Branch)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Classes)
            .WithOne(x => x.Branch)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.BranchPrograms)
            .WithOne(x => x.Branch)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Sessions)
            .WithOne(x => x.Branch)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.MonthlyReportJobs)
            .WithOne(x => x.Branch)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.PreferredLeads)
            .WithOne(x => x.BranchPreferenceNavigation)
            .HasForeignKey(x => x.BranchPreference)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.MediaAssets)
            .WithOne(x => x.Branch)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Invoices)
            .WithOne(x => x.Branch)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.CashbookEntries)
            .WithOne(x => x.Branch)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Contracts)
            .WithOne(x => x.Branch)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.PayrollRuns)
            .WithOne(x => x.Branch)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Tickets)
            .WithOne(x => x.Branch)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
        
    }
}
