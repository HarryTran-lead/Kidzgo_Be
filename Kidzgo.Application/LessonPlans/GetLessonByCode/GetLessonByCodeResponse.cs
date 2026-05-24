namespace Kidzgo.Application.LessonPlans.GetLessonByCode;

public sealed class GetLessonByCodeResponse
{
    public string CourseCode { get; init; } = null!;
    public string UnitCode { get; init; } = null!;
    public string LessonCode { get; init; } = null!;
    public string Type { get; init; } = null!;
    public string Title { get; init; } = null!;
    public int LessonNo { get; init; }
    public IReadOnlyList<string> Objectives { get; init; } = [];
    public LessonLanguageContentDto LanguageContent { get; init; } = new();
    public LessonMaterialsDto Materials { get; init; } = new();
    public IReadOnlyList<LessonProcedureStageDto> Procedure { get; init; } = [];
    public IReadOnlyList<string> Homework { get; init; } = [];
    public string Evaluation { get; init; } = string.Empty;
    public string? SourceFileUrl { get; init; }
}

public sealed class LessonLanguageContentDto
{
    public IReadOnlyList<string> Vocabulary { get; init; } = [];
    public IReadOnlyList<string> Grammar { get; init; } = [];
}

public sealed class LessonMaterialsDto
{
    public IReadOnlyList<string> Teacher { get; init; } = [];
    public IReadOnlyList<string> Students { get; init; } = [];
}

public sealed class LessonProcedureStageDto
{
    public int StageNo { get; init; }
    public string Stage { get; init; } = null!;
    public IReadOnlyList<string> Details { get; init; } = [];
}
