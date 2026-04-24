using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ClassEnrollmentConfiguration : IEntityTypeConfiguration<ClassEnrollment>
{
    public void Configure(EntityTypeBuilder<ClassEnrollment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ClassId)
            .IsRequired();

        builder.Property(x => x.StudentProfileId)
            .IsRequired();

        builder.Property(x => x.EnrollDate)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.TuitionPlanId);

        builder.Property(x => x.RegistrationId);

        builder.Property(x => x.Track)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.SessionSelectionPattern)
            .HasColumnType("text");

        builder.Property(x => x.EnrollmentConfirmationPdfUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.EnrollmentConfirmationPdfGeneratedAt);

        builder.Property(x => x.EnrollmentConfirmationPdfGeneratedBy);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Class)
            .WithMany(x => x.ClassEnrollments)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StudentProfile)
            .WithMany(x => x.ClassEnrollments)
            .HasForeignKey(x => x.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TuitionPlan)
            .WithMany(x => x.ClassEnrollments)
            .HasForeignKey(x => x.TuitionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Registration)
            .WithMany()
            .HasForeignKey(x => x.RegistrationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.RegistrationId);
    }
}
