using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Syllabuses.GetSyllabusById;

public sealed class GetSyllabusByIdQuery : IQuery<GetSyllabusByIdResponse>
{
    public Guid Id { get; init; }
}

public sealed class GetSyllabusByIdResponse
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid LevelId { get; init; }
    public string LevelName { get; init; } = null!;
    public string Code { get; init; } = null!;
    public string Version { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string? Edition { get; init; }
    public DateTime? EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public string? PacingSchemeJson { get; init; }
    public string? Overview { get; init; }
    public string? OverallObjectives { get; init; }
    public string? SpecificObjectives { get; init; }
    public string? EthicsAndAttitudes { get; init; }
    public string? BookOverview { get; init; }
    public int? TotalPeriods { get; init; }
    public int? MinutesPerPeriod { get; init; }
    public int? TotalLessons { get; init; }
    public string? SourceFileName { get; init; }
    public string? AttachmentUrl { get; init; }
    public string? RawContentJson { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<SyllabusUnitDetailDto> Units { get; init; } = [];
    public IReadOnlyList<SyllabusLessonDetailDto> Lessons { get; init; } = [];
    public IReadOnlyList<SyllabusResourceDetailDto> Resources { get; init; } = [];
    public IReadOnlyList<SyllabusSessionTemplateDetailDto> SessionTemplates { get; init; } = [];
}

public sealed class SyllabusUnitDetailDto
{
    public Guid Id { get; init; }
    public Guid? ModuleId { get; init; }
    public string? ModuleName { get; init; }
    public string Name { get; init; } = null!;
    public int? AllocatedPeriods { get; init; }
    public int? LessonCount { get; init; }
    public int OrderIndex { get; init; }
    public string? Notes { get; init; }
}

public sealed class SyllabusLessonDetailDto
{
    public Guid Id { get; init; }
    public Guid? ModuleId { get; init; }
    public string? ModuleName { get; init; }
    public int? PeriodFrom { get; init; }
    public int? PeriodTo { get; init; }
    public string? Topic { get; init; }
    public int? LessonNumber { get; init; }
    public string? ContentSummary { get; init; }
    public string? StructureSummary { get; init; }
    public string? StudentBookPages { get; init; }
    public string? TeacherBookPages { get; init; }
    public int OrderIndex { get; init; }
}

public sealed class SyllabusResourceDetailDto
{
    public Guid Id { get; init; }
    public string? DocumentName { get; init; }
    public string? Abbreviation { get; init; }
    public string? IntendedUsers { get; init; }
    public string? Notes { get; init; }
    public int OrderIndex { get; init; }
}

public sealed class SyllabusSessionTemplateDetailDto
{
    public Guid Id { get; init; }
    public Guid? ModuleId { get; init; }
    public string? ModuleName { get; init; }
    public Guid? LessonPlanTemplateId { get; init; }
    public int SessionIndex { get; init; }
    public int? SessionIndexInModule { get; init; }
    public int? LessonNumber { get; init; }
    public string? Title { get; init; }
    public string? Topic { get; init; }
    public string? ObjectiveSummary { get; init; }
    public string? VocabularySummary { get; init; }
    public string? GrammarSummary { get; init; }
    public int OrderIndex { get; init; }
}
