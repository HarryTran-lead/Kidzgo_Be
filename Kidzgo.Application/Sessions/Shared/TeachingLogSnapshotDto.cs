namespace Kidzgo.Application.Sessions.Shared;

public sealed class TeachingLogSnapshotDto
{
    public Guid TeachingLogId { get; init; }
    public Guid? SessionId { get; init; }
    public string? TeachingLogStatus { get; init; }
    public string? ProgressStatus { get; init; }
    public string? ActualTeachingType { get; init; }
    public string? ActualContent { get; init; }
    public string? ActualHomework { get; init; }
    public string? TeacherNote { get; init; }
    public string? TeacherNotes => TeacherNote;
    public Guid? SubmittedBy { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
