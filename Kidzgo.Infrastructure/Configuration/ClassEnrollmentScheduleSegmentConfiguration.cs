using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class ClassEnrollmentScheduleSegmentConfiguration : IEntityTypeConfiguration<ClassEnrollmentScheduleSegment>
{
    public void Configure(EntityTypeBuilder<ClassEnrollmentScheduleSegment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ClassEnrollmentId)
            .IsRequired();

        builder.Property(x => x.EffectiveFrom)
            .IsRequired();

        builder.Property(x => x.EffectiveTo);

        builder.Property(x => x.SessionSelectionPattern)
            .HasColumnType("text");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.ClassEnrollment)
            .WithMany(x => x.ScheduleSegments)
            .HasForeignKey(x => x.ClassEnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ClassEnrollmentId, x.EffectiveFrom })
            .IsUnique();
    }
}
