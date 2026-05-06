using Kidzgo.Domain.ProgramProgressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public sealed class ProgramProgressionScheduleConfiguration : IEntityTypeConfiguration<ProgramProgressionSchedule>
{
    public void Configure(EntityTypeBuilder<ProgramProgressionSchedule> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.DurationMinutes).IsRequired();
        builder.Property(x => x.Notes);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOne(x => x.SourceClass)
            .WithMany()
            .HasForeignKey(x => x.SourceClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SourceProgram)
            .WithMany()
            .HasForeignKey(x => x.SourceProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AssignedTeacherUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedTeacherUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SourceClassId);
        builder.HasIndex(x => x.SourceProgramId);
        builder.HasIndex(x => x.BranchId);
        builder.HasIndex(x => x.AssignedTeacherUserId);
        builder.HasIndex(x => x.RoomId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ScheduledAt);
    }
}
