using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Users;
using DomainProgram = Kidzgo.Domain.Programs.Program;

namespace Kidzgo.Domain.ProgramProgressions;

public class ProgramProgressionSchedule : Entity
{
    public Guid Id { get; set; }
    public Guid SourceClassId { get; set; }
    public Guid SourceProgramId { get; set; }
    public Guid BranchId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public Guid? RoomId { get; set; }
    public Guid AssignedTeacherUserId { get; set; }
    public ProgramProgressionScheduleStatus Status { get; set; }
    public string? Notes { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Class SourceClass { get; set; } = null!;
    public DomainProgram SourceProgram { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public Classroom? Room { get; set; }
    public User AssignedTeacherUser { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public ICollection<ProgramProgressionScheduleParticipant> Participants { get; set; } = new List<ProgramProgressionScheduleParticipant>();
}
