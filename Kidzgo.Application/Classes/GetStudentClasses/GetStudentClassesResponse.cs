using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Classes.GetStudentClasses;

public sealed class GetStudentClassesResponse
{
    public Page<StudentClassDto> Classes { get; init; } = null!;
}

public sealed class StudentClassDto
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public Guid? MainTeacherId { get; init; }
    public string? MainTeacherName { get; init; }
    public Guid? AssistantTeacherId { get; init; }
    public string? AssistantTeacherName { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string Status { get; init; } = null!;
    public int Capacity { get; init; }
    public int CurrentEnrollmentCount { get; init; }
    public List<ScheduleSlot> WeeklyScheduleSlots { get; init; } = [];
    public DateOnly EnrollDate { get; init; }
    public string EnrollmentStatus { get; init; } = null!;
    public Guid ClassId => Id;
    public string ClassName => Title;
    public string? TeacherName => MainTeacherName ?? AssistantTeacherName;
    public Guid? RoomId { get; init; }
    public string? RoomName { get; init; }
    public string? ScheduleText => SchedulePatternSupport.BuildDisplayText(WeeklyScheduleSlots);
    public int TotalSessions { get; init; }
    public int CompletedSessions { get; init; }
    public decimal ProgressPercent => TotalSessions <= 0
        ? 0
        : Math.Round((decimal)CompletedSessions * 100 / TotalSessions, 2);
}

