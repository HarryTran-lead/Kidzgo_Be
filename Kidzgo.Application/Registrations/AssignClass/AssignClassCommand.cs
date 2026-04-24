using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.Application.Registrations.AssignClass;

public sealed class AssignClassCommand : ICommand<AssignClassResponse>
{
    public Guid RegistrationId { get; init; }

    /// <summary>
    /// Class ID to assign. Required for immediate/retake, optional for wait.
    /// </summary>
    public Guid? ClassId { get; init; }

    /// <summary>
    /// Entry type: "immediate" | "wait" | "retake"
    /// - immediate: Vao hoc ngay, tham gia cac buoi con lai
    /// - wait: Cho lop moi, chua xep lop
    /// - retake: Thi lai / cho xep lop sau placement test retake
    /// </summary>
    public string EntryType { get; init; } = "immediate";

    /// <summary>
    /// Track to assign: "primary" | "secondary"
    /// </summary>
    public string Track { get; init; } = "primary";

    /// <summary>
    /// Optional first date the student will attend this class.
    /// </summary>
    public DateOnly? FirstStudyDate { get; init; }

    /// <summary>
    /// Optional subset of the class schedule for this student.
    /// Group days with the same startTime into one entry.
    /// </summary>
    public IReadOnlyCollection<WeeklyPatternEntry>? WeeklyPattern { get; init; }
}
