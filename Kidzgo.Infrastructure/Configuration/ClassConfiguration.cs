using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Schools;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.ProgramId)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.RoomId);

        builder.Property(x => x.MainTeacherId);

        builder.Property(x => x.AssistantTeacherId);

        builder.Property(x => x.SlotTypeId);

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Capacity)
            .IsRequired();

        builder.Property(x => x.WeeklyScheduleJson)
            .IsRequired();

        builder.Property(x => x.Description);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Classes)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Program)
            .WithMany(x => x.Classes)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.MainTeacher)
            .WithMany(x => x.MainTeacherClasses)
            .HasForeignKey(x => x.MainTeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AssistantTeacher)
            .WithMany(x => x.AssistantTeacherClasses)
            .HasForeignKey(x => x.AssistantTeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SlotType)
            .WithMany()
            .HasForeignKey(x => x.SlotTypeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.ClassEnrollments)
            .WithOne(x => x.Class)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Sessions)
            .WithOne(x => x.Class)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.LeaveRequests)
            .WithOne(x => x.Class)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.HomeworkAssignments)
            .WithOne(x => x.Class)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Exams)
            .WithOne(x => x.Class)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.TargetMissions)
            .WithOne(x => x.TargetClass)
            .HasForeignKey(x => x.TargetClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.PlacementTests)
            .WithOne(x => x.Class)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.MediaAssets)
            .WithOne(x => x.Class)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Invoices)
            .WithOne(x => x.Class)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Tickets)
            .WithOne(x => x.Class)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
