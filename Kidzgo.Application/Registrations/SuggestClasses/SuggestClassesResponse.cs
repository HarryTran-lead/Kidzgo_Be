using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.Application.Registrations.SuggestClasses;

public sealed class SuggestedClassDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string Status { get; init; } = null!;
    public int Capacity { get; init; }
    public int CurrentEnrollment { get; init; }
    public int RemainingSlots => Capacity - CurrentEnrollment;
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public List<ScheduleSlot> WeeklyScheduleSlots { get; init; } = [];
    public string MainTeacherName { get; init; } = null!;
    public string? ClassroomName { get; init; }
    public bool IsClassStarted { get; init; }
}

public sealed class SuggestClassesResponse
{
    public Guid RegistrationId { get; init; }
    public string ProgramName { get; init; } = null!;
    public string BranchName { get; init; } = null!;
    public string? PreferredSchedule { get; init; }
    public List<SuggestedClassDto> SuggestedClasses { get; init; } = new();
    public List<SuggestedClassDto> AlternativeClasses { get; init; } = new();
    public Guid? SecondaryLevelId { get; init; }
    public string? SecondaryLevelName { get; init; }
    public string? SecondaryLevelSkillFocus { get; init; }
    public List<SuggestedClassDto> SecondarySuggestedClasses { get; init; } = new();
    public List<SuggestedClassDto> SecondaryAlternativeClasses { get; init; } = new();
}
