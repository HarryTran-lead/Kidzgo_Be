using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.Media;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Tickets;

namespace Kidzgo.Domain.Classes;

public class Class : Entity
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid ProgramId { get; set; }
    public Guid LevelId { get; set; }
    public Guid? SyllabusId { get; set; }
    public Guid StartModuleId { get; set; }
    public Guid CurrentModuleId { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public Guid? RoomId { get; set; }
    public Guid? MainTeacherId { get; set; }
    public Guid? AssistantTeacherId { get; set; }
    public Guid? SlotTypeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public ClassStatus Status { get; set; }
    public int Capacity { get; set; }
    public string? WeeklyScheduleJson { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Branch Branch { get; set; } = null!;
    public Program Program { get; set; } = null!;
    public Level Level { get; set; } = null!;
    public Syllabus? Syllabus { get; set; }
    public Module StartModule { get; set; } = null!;
    public Module CurrentModule { get; set; } = null!;
    public Classroom? Room { get; set; }
    public User? MainTeacher { get; set; }
    public User? AssistantTeacher { get; set; }
    public SlotType? SlotType { get; set; }
    public ICollection<ClassEnrollment> ClassEnrollments { get; set; } = new List<ClassEnrollment>();
    public ICollection<ClassModuleProgress> ModuleProgresses { get; set; } = new List<ClassModuleProgress>();
    public ICollection<ClassScheduleSegment> ScheduleSegments { get; set; } = new List<ClassScheduleSegment>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<LessonPlan> LessonPlans { get; set; } = new List<LessonPlan>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<HomeworkAssignment> HomeworkAssignments { get; set; } = new List<HomeworkAssignment>();
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
    public ICollection<Mission> TargetMissions { get; set; } = new List<Mission>();
    public ICollection<PlacementTest> PlacementTests { get; set; } = new List<PlacementTest>();
    public ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
