namespace Kidzgo.API.Requests;

public sealed class AssignClassRequest
{
    /// <summary>
    /// Class ID to assign. Required for immediate/retake, optional for wait.
    /// </summary>
    public Guid? ClassId { get; set; }

    /// <summary>
    /// Entry type: "immediate" | "wait" | "retake"
    /// - immediate: Vao hoc ngay, tham gia cac buoi con lai
    /// - wait: Cho lop moi, chua xep lop
    /// - retake: Thi lai / cho xep lop sau placement test retake
    /// </summary>
    public string EntryType { get; set; } = "immediate";

    /// <summary>
    /// Track to assign: "primary" | "secondary"
    /// </summary>
    public string Track { get; set; } = "primary";

    /// <summary>
    /// Optional first date the student will attend this class.
    /// If provided, the date must match an available class session and assignments before this date are skipped.
    /// </summary>
    public DateOnly? FirstStudyDate { get; set; }

    /// <summary>
    /// Optional subset of the class weekly schedule for this student, using RRULE format.
    /// If omitted, the student attends all sessions of the class.
    /// Examples:
    /// - If class runs Wednesday 08:30: FREQ=WEEKLY;BYDAY=WE;BYHOUR=8;BYMINUTE=30
    /// - If class runs Tuesday 18:00: FREQ=WEEKLY;BYDAY=TU;BYHOUR=18;BYMINUTE=0
    /// - If class runs Tuesday and Thursday 18:00: FREQ=WEEKLY;BYDAY=TU,TH;BYHOUR=18;BYMINUTE=0
    /// Note: the selection pattern must match the class slot time, not only the weekday.
    /// The pattern must be a subset of the class weekly schedule slots.
    /// </summary>
    public string? SessionSelectionPattern { get; set; }
}
