using Kidzgo.Domain.Registrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class EnrollmentConfirmationPaymentSettingConfiguration
    : IEntityTypeConfiguration<EnrollmentConfirmationPaymentSetting>
{
    public void Configure(EntityTypeBuilder<EnrollmentConfirmationPaymentSetting> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ScopeKey)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.PaymentMethod)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.AccountName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.AccountNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.BankName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.BankCode)
            .HasMaxLength(50);

        builder.Property(x => x.BankBin)
            .HasMaxLength(20);

        builder.Property(x => x.VietQrTemplate)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.LogoUrl)
            .HasColumnType("text");

        builder.Property(x => x.NewStudentPolicyText)
            .HasColumnType("text");

        builder.Property(x => x.ReservationPolicyText)
            .HasColumnType("text");

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ScopeKey)
            .IsUnique();

        builder.HasIndex(x => x.BranchId);
    }
}
