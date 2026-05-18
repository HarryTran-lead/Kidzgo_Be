using Kidzgo.Domain.Registrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class RegistrationConfiguration : IEntityTypeConfiguration<Registration>
{
    public void Configure(EntityTypeBuilder<Registration> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.ProgramId)
            .IsRequired();

        builder.Property(x => x.LevelId)
            .IsRequired();

        builder.Property(x => x.SecondaryLevelId);

        builder.Property(x => x.TuitionPlanId)
            .IsRequired();

        builder.Property(x => x.SecondaryProgramId);

        builder.Property(x => x.RegistrationDate)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.EntryType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.SecondaryEntryType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.OperationType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.DiscountCampaignName)
            .HasMaxLength(200);

        builder.Property(x => x.DiscountType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.DiscountValue)
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.OriginalTuitionAmount)
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.DiscountAmount)
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.CarryOverCreditAmount)
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.FinalTuitionAmount)
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.SecondaryProgramSkillFocus)
            .HasMaxLength(50);

        builder.Property(x => x.TotalSessions)
            .IsRequired();

        builder.Property(x => x.UsedSessions)
            .IsRequired();

        builder.Property(x => x.RemainingSessions)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.StudentProfile)
            .WithMany()
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Program)
            .WithMany()
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Level)
            .WithMany()
            .HasForeignKey(x => x.LevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SecondaryLevel)
            .WithMany()
            .HasForeignKey(x => x.SecondaryLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SecondaryProgram)
            .WithMany()
            .HasForeignKey(x => x.SecondaryProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TuitionPlan)
            .WithMany()
            .HasForeignKey(x => x.TuitionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Class)
            .WithMany()
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SecondaryClass)
            .WithMany()
            .HasForeignKey(x => x.SecondaryClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OriginalRegistration)
            .WithMany()
            .HasForeignKey(x => x.OriginalRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DiscountCampaign)
            .WithMany()
            .HasForeignKey(x => x.DiscountCampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.StudentProfileId);
        builder.HasIndex(x => x.BranchId);
        builder.HasIndex(x => x.ProgramId);
        builder.HasIndex(x => x.LevelId);
        builder.HasIndex(x => x.SecondaryLevelId);
        builder.HasIndex(x => x.SecondaryProgramId);
        builder.HasIndex(x => x.TuitionPlanId);
        builder.HasIndex(x => x.ClassId);
        builder.HasIndex(x => x.SecondaryClassId);
        builder.HasIndex(x => x.OriginalRegistrationId);
        builder.HasIndex(x => x.DiscountCampaignId);
    }
}
