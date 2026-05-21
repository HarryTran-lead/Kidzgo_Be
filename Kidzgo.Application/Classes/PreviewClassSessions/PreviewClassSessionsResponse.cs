namespace Kidzgo.Application.Classes.PreviewClassSessions;

public sealed class PreviewClassSessionsResponse
{
    public DateOnly? ExpectedEndDate { get; init; }
    public List<PreviewClassSessionItem> Sessions { get; init; } = [];
    public List<string> Warnings { get; init; } = [];
}

public sealed class PreviewClassSessionItem
{
    public int ClassSessionNo { get; init; }
    public DateOnly Date { get; init; }
    public string ModuleName { get; init; } = null!;
    public string? UnitName { get; init; }
    public string? LessonTitle { get; init; }
    public int CurriculumSessionIndex { get; init; }
}
