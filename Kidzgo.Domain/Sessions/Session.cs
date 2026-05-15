using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Payroll;

namespace Kidzgo.Domain.Sessions;

public class Session : Entity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid BranchId { get; set; }
    public DateTime PlannedDatetime { get; set; }
    public Guid? PlannedRoomId { get; set; }
    public Guid? PlannedTeacherId { get; set; }
    public Guid? PlannedAssistantId { get; set; }
    public Guid? SlotTypeId { get; set; }
    public int DurationMinutes { get; set; }
    public ParticipationType ParticipationType { get; set; }
    public SectionType SectionType { get; set; }
    public SessionStatus Status { get; set; }
    public DateTime? ActualDatetime { get; set; }
    public Guid? ActualRoomId { get; set; }
    public Guid? ActualTeacherId { get; set; }
    public Guid? ActualAssistantId { get; set; }
    public string? Color { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Class Class { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public Classroom? PlannedRoom { get; set; }
    public User? PlannedTeacher { get; set; }
    public User? PlannedAssistant { get; set; }
    public SlotType? SlotType { get; set; }
    public Classroom? ActualRoom { get; set; }
    public User? ActualTeacher { get; set; }
    public User? ActualAssistant { get; set; }
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<StudentSessionAssignment> StudentSessionAssignments { get; set; } = new List<StudentSessionAssignment>();
    public ICollection<MakeupCredit> SourceMakeupCredits { get; set; } = new List<MakeupCredit>();
    public ICollection<MakeupCredit> UsedMakeupCredits { get; set; } = new List<MakeupCredit>();
    public ICollection<MakeupAllocation> TargetMakeupAllocations { get; set; } = new List<MakeupAllocation>();
    public LessonPlan? LessonPlan { get; set; }
    public ICollection<HomeworkAssignment> HomeworkAssignments { get; set; } = new List<HomeworkAssignment>();
    public ICollection<SessionRole> SessionRoles { get; set; } = new List<SessionRole>();
}
