using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;

namespace Kidzgo.Application.Classes.GetClasses;

public sealed class GetClassesResponse
{
    public Page<ClassDto> Classes { get; init; } = null!;
}

public sealed class ClassDto
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid LevelId { get; init; }
    public string LevelName { get; init; } = null!;
    public Guid? SyllabusId { get; init; }
    public string? SyllabusCode { get; init; }
    public string? SyllabusVersion { get; init; }
    public string? SyllabusTitle { get; init; }
    public Guid StartModuleId { get; init; }
    public int StartSessionIndex { get; init; }
    public string StartModuleName { get; init; } = null!;
    public Guid CurrentModuleId { get; init; }
    public int CurrentSessionIndex { get; init; }
    public Guid? CurrentLessonPlanTemplateId { get; init; }
    public string? CurrentLessonTitle { get; init; }
    public string CurrentModuleName { get; init; } = null!;
    public Guid? SlotTypeId { get; init; }
    public string? SlotTypeCode { get; init; }
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public Guid? MainTeacherId { get; init; }
    public string? MainTeacherName { get; init; }
    public Guid? AssistantTeacherId { get; init; }
    public string? AssistantTeacherName { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? ExpectedEndDate { get; init; }
    public DateOnly? ActualEndDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string Status { get; init; } = null!;
    public int Capacity { get; init; }
    public int CurrentEnrollmentCount { get; init; }
    public List<ScheduleSlot> WeeklyScheduleSlots { get; init; } = [];
    public string? Description { get; init; }
    public string Name => Title;
    public Guid? RoomId { get; init; }
    public string? RoomName { get; init; }
    public string? ScheduleText => SchedulePatternSupport.BuildDisplayText(WeeklyScheduleSlots);
    public int StudentCount => CurrentEnrollmentCount;
    public int TotalSessions { get; init; }
    public int CompletedSessions { get; init; }
    public int TotalCurriculumSessions { get; init; }
    public int CompletedClassSessions { get; init; }
    public int CompletedLessonPlans { get; init; }
    public decimal ProgressPercent => TotalSessions <= 0
        ? 0
        : Math.Round((decimal)CompletedSessions * 100 / TotalSessions, 2);
    public decimal OperationalProgressPercent => TotalCurriculumSessions <= 0
        ? 0
        : Math.Round((decimal)CompletedClassSessions * 100 / TotalCurriculumSessions, 2);
    public decimal CurriculumProgressPercent => TotalCurriculumSessions <= 0
        ? 0
        : Math.Round((decimal)CompletedLessonPlans * 100 / TotalCurriculumSessions, 2);
}

