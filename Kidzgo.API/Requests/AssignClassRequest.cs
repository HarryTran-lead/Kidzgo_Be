using Kidzgo.Application.Abstraction.Services;

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
    /// Optional subset of the class weekly schedule for this student.
    /// If omitted, the student attends all sessions of the class.
    /// Group days with the same startTime into one entry, and split different times into separate entries.
    /// </summary>
    public List<WeeklyPatternEntry>? WeeklyPattern { get; set; }
}
