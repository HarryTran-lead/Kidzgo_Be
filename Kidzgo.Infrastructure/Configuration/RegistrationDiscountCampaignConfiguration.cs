using Kidzgo.Domain.Registrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class RegistrationDiscountCampaignConfiguration : IEntityTypeConfiguration<RegistrationDiscountCampaign>
{
    public void Configure(EntityTypeBuilder<RegistrationDiscountCampaign> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.DiscountType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.DiscountValue)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.Priority)
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.EndDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

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

        builder.HasOne(x => x.TuitionPlan)
            .WithMany()
            .HasForeignKey(x => x.TuitionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.BranchId);
        builder.HasIndex(x => x.ProgramId);
        builder.HasIndex(x => x.LevelId);
        builder.HasIndex(x => x.TuitionPlanId);
        builder.HasIndex(x => new { x.IsActive, x.StartDate, x.EndDate, x.Priority });
    }
}
