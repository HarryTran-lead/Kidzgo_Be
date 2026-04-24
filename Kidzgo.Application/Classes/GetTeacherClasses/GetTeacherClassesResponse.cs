using Kidzgo.Application.Abstraction.Services;

using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Classes.GetTeacherClasses;

public sealed class GetTeacherClassesResponse
{
    public Page<TeacherClassDto> Classes { get; init; } = null!;
}

public sealed class TeacherClassDto
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
    public string Role { get; init; } = null!; // "MainTeacher" or "AssistantTeacher"
}

