using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Programs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.ClassId)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.ModuleId);

        builder.Property(x => x.LessonPlanTemplateId);

        builder.Property(x => x.SessionIndexInModule);

        builder.Property(x => x.RescheduledFromSessionId);

        builder.Property(x => x.PlannedDatetime)
            .IsRequired();

        builder.Property(x => x.PlannedRoomId);

        builder.Property(x => x.PlannedTeacherId);

        builder.Property(x => x.PlannedAssistantId);

        builder.Property(x => x.DurationMinutes)
            .IsRequired();

        builder.Property(x => x.ParticipationType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.SectionType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(SectionType.Normal)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ActualDatetime);

        builder.Property(x => x.ActualRoomId);

        builder.Property(x => x.ActualTeacherId);

        builder.Property(x => x.ActualAssistantId);

        builder.Property(x => x.Color)
            .HasMaxLength(50);

        builder.Property(x => x.CurriculumSnapshotJson);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Class)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.LessonPlanTemplate)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.LessonPlanTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.RescheduledFromSession)
            .WithMany(x => x.RescheduledSessions)
            .HasForeignKey(x => x.RescheduledFromSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PlannedRoom)
            .WithMany(x => x.PlannedRoomSessions)
            .HasForeignKey(x => x.PlannedRoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PlannedTeacher)
            .WithMany(x => x.PlannedTeacherSessions)
            .HasForeignKey(x => x.PlannedTeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PlannedAssistant)
            .WithMany(x => x.PlannedAssistantSessions)
            .HasForeignKey(x => x.PlannedAssistantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ActualRoom)
            .WithMany(x => x.ActualRoomSessions)
            .HasForeignKey(x => x.ActualRoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ActualTeacher)
            .WithMany(x => x.ActualTeacherSessions)
            .HasForeignKey(x => x.ActualTeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ActualAssistant)
            .WithMany(x => x.ActualAssistantSessions)
            .HasForeignKey(x => x.ActualAssistantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Attendances)
            .WithOne(x => x.Session)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.StudentSessionAssignments)
            .WithOne(x => x.Session)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.SourceMakeupCredits)
            .WithOne(x => x.SourceSession)
            .HasForeignKey(x => x.SourceSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.UsedMakeupCredits)
            .WithOne(x => x.UsedSession)
            .HasForeignKey(x => x.UsedSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.TargetMakeupAllocations)
            .WithOne(x => x.TargetSession)
            .HasForeignKey(x => x.TargetSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.SessionLessons)
            .WithOne(x => x.Session)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.LessonPlan)
            .WithOne(x => x.Session)
            .HasForeignKey<LessonPlan>(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.TeachingLog)
            .WithOne(x => x.Session)
            .HasForeignKey<TeachingLog>(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.HomeworkAssignments)
            .WithOne(x => x.Session)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.SessionRoles)
            .WithOne(x => x.Session)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
