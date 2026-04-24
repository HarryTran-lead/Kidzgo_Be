using Kidzgo.Domain.Classes;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Services;

namespace Kidzgo.Application.Registrations;

public sealed class RegistrationActualStudyScheduleDto
{
    public string Track { get; init; } = null!;
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = null!;
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public bool UsesClassDefaultSchedule { get; init; }
    public List<ScheduleSlot> ClassWeeklyScheduleSlots { get; init; } = new();
    public List<WeeklyPatternEntry>? WeeklyPattern { get; init; }
    public List<WeeklyPatternEntry>? EffectiveWeeklyPattern { get; init; }
    public List<string> StudyDayCodes { get; init; } = new();
    public List<string> StudyDays { get; init; } = new();
    public string? StudyDaySummary { get; init; }
    public List<RegistrationActualStudyScheduleSegmentDto> ScheduleSegments { get; init; } = new();
}

public sealed class RegistrationActualStudyScheduleSegmentDto
{
    public Guid Id { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public List<WeeklyPatternEntry>? WeeklyPattern { get; init; }
    public List<WeeklyPatternEntry>? EffectiveWeeklyPattern { get; init; }
    public List<string> StudyDayCodes { get; init; } = new();
    public List<string> StudyDays { get; init; } = new();
    public string? StudyDaySummary { get; init; }
}

internal static class RegistrationActualStudyScheduleMapper
{
    private static readonly string[] DayOrder = ["MO", "TU", "WE", "TH", "FR", "SA", "SU"];

    private static readonly IReadOnlyDictionary<string, string> LegacyDayLabels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["MO"] = "Thứ 2",
            ["TU"] = "Thứ 3",
            ["WE"] = "Thứ 4",
            ["TH"] = "Thứ 5",
            ["FR"] = "Thứ 6",
            ["SA"] = "Thứ 7",
            ["SU"] = "Chủ nhật"
        };

    private static readonly IReadOnlyDictionary<string, string> DayLabels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["MO"] = "Thu 2",
            ["TU"] = "Thu 3",
            ["WE"] = "Thu 4",
            ["TH"] = "Thu 5",
            ["FR"] = "Thu 6",
            ["SA"] = "Thu 7",
            ["SU"] = "Chu nhat"
        };

    public static List<RegistrationActualStudyScheduleDto> Map(IEnumerable<ClassEnrollment> enrollments)
    {
        return enrollments
            .Where(e => e.Class != null && e.Class.Program != null)
            .GroupBy(e => e.Track)
            .Select(group => group
                .OrderByDescending(e => e.UpdatedAt)
                .ThenByDescending(e => e.CreatedAt)
                .First())
            .OrderBy(e => e.Track)
            .Select(MapSingle)
            .ToList();
    }

    private static RegistrationActualStudyScheduleDto MapSingle(ClassEnrollment enrollment)
    {
        var classEntity = enrollment.Class;
        var currentClassWeeklyScheduleJson = ResolveClassWeeklyScheduleJson(
            classEntity,
            VietnamTime.TodayDateOnly());
        var usesClassDefaultSchedule = string.IsNullOrWhiteSpace(enrollment.SessionSelectionPattern);
        var effectiveSchedulePattern = usesClassDefaultSchedule
            ? currentClassWeeklyScheduleJson
            : enrollment.SessionSelectionPattern;
        var studyDayCodes = ExtractOrderedDayCodes(effectiveSchedulePattern);
        var studyDays = studyDayCodes
            .Select(code => DayLabels.TryGetValue(code, out var label) ? label : code)
            .ToList();

        return new RegistrationActualStudyScheduleDto
        {
            Track = RegistrationTrackHelper.ToTrackName(enrollment.Track),
            ClassId = classEntity.Id,
            ClassName = classEntity.Title,
            ProgramId = classEntity.ProgramId,
            ProgramName = classEntity.Program.Name,
            UsesClassDefaultSchedule = usesClassDefaultSchedule,
            ClassWeeklyScheduleSlots = ParseWeeklyScheduleSlots(currentClassWeeklyScheduleJson),
            WeeklyPattern = ParseWeeklyPattern(enrollment.SessionSelectionPattern),
            EffectiveWeeklyPattern = ParseWeeklyPattern(effectiveSchedulePattern),
            StudyDayCodes = studyDayCodes,
            StudyDays = studyDays,
            StudyDaySummary = studyDays.Count > 0 ? string.Join(", ", studyDays) : null,
            ScheduleSegments = enrollment.ScheduleSegments
                .OrderBy(segment => segment.EffectiveFrom)
                .Select(segment => MapSegment(
                    segment,
                    ResolveClassWeeklyScheduleJson(classEntity, segment.EffectiveFrom)))
                .ToList()
        };
    }

    private static RegistrationActualStudyScheduleSegmentDto MapSegment(
        ClassEnrollmentScheduleSegment segment,
        string? classSchedulePattern)
    {
        var effectiveSchedulePattern = string.IsNullOrWhiteSpace(segment.SessionSelectionPattern)
            ? classSchedulePattern
            : segment.SessionSelectionPattern;
        var studyDayCodes = ExtractOrderedDayCodes(effectiveSchedulePattern);
        var studyDays = studyDayCodes
            .Select(code => DayLabels.TryGetValue(code, out var label) ? label : code)
            .ToList();

        return new RegistrationActualStudyScheduleSegmentDto
        {
            Id = segment.Id,
            EffectiveFrom = segment.EffectiveFrom,
            EffectiveTo = segment.EffectiveTo,
            WeeklyPattern = ParseWeeklyPattern(segment.SessionSelectionPattern),
            EffectiveWeeklyPattern = ParseWeeklyPattern(effectiveSchedulePattern),
            StudyDayCodes = studyDayCodes,
            StudyDays = studyDays,
            StudyDaySummary = studyDays.Count > 0 ? string.Join(", ", studyDays) : null
        };
    }

    private static List<string> ExtractOrderedDayCodes(string? schedulePattern)
    {
        var dayCodes = SchedulePatternSupport.ExtractOrderedDayCodes(schedulePattern);
        return dayCodes.Count == 0
            ? new List<string>()
            : dayCodes.ToList();
    }

    private static List<ScheduleSlot> ParseWeeklyScheduleSlots(string? weeklyScheduleJson)
    {
        if (string.IsNullOrWhiteSpace(weeklyScheduleJson))
        {
            return new List<ScheduleSlot>();
        }

        var parseResult = SchedulePatternSupport.ParseScheduleSlots(weeklyScheduleJson);
        return parseResult.IsSuccess ? parseResult.Value : new List<ScheduleSlot>();
    }

    private static List<WeeklyPatternEntry>? ParseWeeklyPattern(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var parseResult = SchedulePatternSupport.ParseWeeklyPattern(value);
        return parseResult.IsSuccess ? parseResult.Value : null;
    }

    private static string? ResolveClassWeeklyScheduleJson(Class classEntity, DateOnly referenceDate)
    {
        return SchedulePatternSupport.ResolveEffectiveWeeklyScheduleJson(
            classEntity.WeeklyScheduleJson,
            classEntity.ScheduleSegments.Select(segment => new WeeklyScheduleSegmentWindow(
                segment.EffectiveFrom,
                segment.EffectiveTo,
                segment.WeeklyScheduleJson)),
            referenceDate);
    }
}
