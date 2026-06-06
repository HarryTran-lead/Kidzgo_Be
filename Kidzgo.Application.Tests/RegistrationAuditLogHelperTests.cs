using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Domain.Registrations;
using Xunit;

namespace Kidzgo.Application.Tests;

public sealed class RegistrationAuditLogHelperTests
{
    [Fact]
    public void CreateSnapshot_captures_primary_registration_fields()
    {
        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            StudentProfileId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            ProgramId = Guid.NewGuid(),
            LevelId = Guid.NewGuid(),
            TuitionPlanId = Guid.NewGuid(),
            RegistrationDate = new DateTime(2026, 6, 6, 8, 0, 0, DateTimeKind.Utc),
            ExpectedStartDate = new DateTime(2026, 6, 15, 8, 0, 0, DateTimeKind.Utc),
            PreferredSchedule = "T4-T6 18:00",
            Note = "Needs weekday class",
            Status = RegistrationStatus.WaitingForClass,
            ClassId = Guid.NewGuid(),
            EntryType = EntryType.Immediate,
            OperationType = OperationType.TransferBranch,
            TotalSessions = 32,
            UsedSessions = 4,
            RemainingSessions = 28,
            CreatedAt = new DateTime(2026, 6, 6, 8, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 6, 6, 9, 0, 0, DateTimeKind.Utc)
        };

        var snapshot = RegistrationAuditLogHelper.CreateSnapshot(registration);

        Assert.Equal(registration.Id, snapshot.Id);
        Assert.Equal(registration.BranchId, snapshot.BranchId);
        Assert.Equal(registration.TuitionPlanId, snapshot.TuitionPlanId);
        Assert.Equal(RegistrationStatus.WaitingForClass.ToString(), snapshot.Status);
        Assert.Equal(EntryType.Immediate.ToString(), snapshot.EntryType);
        Assert.Equal(OperationType.TransferBranch.ToString(), snapshot.OperationType);
        Assert.Equal(28, snapshot.RemainingSessions);
    }

    [Fact]
    public void CreateSnapshot_keeps_secondary_assignment_fields()
    {
        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            StudentProfileId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            ProgramId = Guid.NewGuid(),
            LevelId = Guid.NewGuid(),
            SecondaryLevelId = Guid.NewGuid(),
            TuitionPlanId = Guid.NewGuid(),
            RegistrationDate = new DateTime(2026, 6, 6, 8, 0, 0, DateTimeKind.Utc),
            Status = RegistrationStatus.ClassAssigned,
            SecondaryClassId = Guid.NewGuid(),
            SecondaryClassAssignedDate = new DateTime(2026, 6, 10, 8, 0, 0, DateTimeKind.Utc),
            SecondaryEntryType = EntryType.Retake,
            SecondaryProgramSkillFocus = "Speaking",
            TotalSessions = 20,
            UsedSessions = 0,
            RemainingSessions = 20,
            CreatedAt = new DateTime(2026, 6, 6, 8, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 6, 6, 9, 0, 0, DateTimeKind.Utc)
        };

        var snapshot = RegistrationAuditLogHelper.CreateSnapshot(registration);

        Assert.Equal(registration.SecondaryLevelId, snapshot.SecondaryLevelId);
        Assert.Equal(registration.SecondaryClassId, snapshot.SecondaryClassId);
        Assert.Equal(EntryType.Retake.ToString(), snapshot.SecondaryEntryType);
        Assert.Equal("Speaking", snapshot.SecondaryProgramSkillFocus);
    }
}
