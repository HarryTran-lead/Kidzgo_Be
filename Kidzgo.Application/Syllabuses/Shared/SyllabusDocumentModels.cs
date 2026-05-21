using System.Text.Json.Serialization;

namespace Kidzgo.Application.Syllabuses.Shared;

public static class SyllabusDocumentStatuses
{
    public const string Draft = "Draft";
    public const string Published = "Published";
    public const string Archived = "Archived";
}

public static class SyllabusDocumentSourceTypes
{
    public const string Manual = "Manual";
    public const string Imported = "Imported";
    public const string Hybrid = "Hybrid";
}

public static class SyllabusDocumentSectionTypes
{
    public const string Heading = "heading";
    public const string Narrative = "narrative";
    public const string List = "list";
    public const string Table = "table";
}

public sealed class SyllabusDocumentResponse
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string? Edition { get; init; }
    public string Status { get; init; } = null!;
    public string SourceType { get; init; } = null!;
    public string? SourceFileName { get; init; }
    public string? ParserVersion { get; init; }
    public int Version { get; init; }
    public SyllabusDocumentSummaryDto Summary { get; init; } = new();
    public IReadOnlyList<SyllabusDocumentSectionDto> Sections { get; init; } = [];
    public IReadOnlyList<SyllabusDocumentWarningDto> Warnings { get; init; } = [];
}

public sealed class SyllabusDocumentSummaryDto
{
    public int TotalUnits { get; init; }
    public int TotalSessions { get; init; }
    public int TotalLessons { get; init; }
    public int TotalPeriods { get; init; }
    public int? MinutesPerPeriod { get; init; }
}

public sealed class SyllabusDocumentWarningDto
{
    public string Code { get; init; } = null!;
    public string Severity { get; init; } = "Warning";
    public string Message { get; init; } = null!;
    public string? SectionRef { get; init; }
    public string? RowRef { get; init; }
    public string? CellRef { get; init; }
}

public sealed class SyllabusDocumentSectionDto
{
    public Guid SectionId { get; init; }
    public string Type { get; init; } = null!;
    public string? Title { get; init; }
    public int OrderIndex { get; init; }
    public bool Editable { get; init; } = true;
    public string? Content { get; init; }
    public IReadOnlyList<string>? Items { get; init; }
    public SyllabusDocumentTableDto? Table { get; init; }
}

public sealed class SyllabusDocumentTableDto
{
    public IReadOnlyList<SyllabusDocumentTableColumnDto> Columns { get; init; } = [];
    public IReadOnlyList<SyllabusDocumentTableRowDto> Rows { get; init; } = [];
}

public sealed class SyllabusDocumentTableColumnDto
{
    public string Key { get; init; } = null!;
    public string Label { get; init; } = null!;
    public int? Width { get; init; }
    public bool Sticky { get; init; }
}

public sealed class SyllabusDocumentTableRowDto
{
    public Guid RowId { get; init; }
    public int OrderIndex { get; init; }
    public SyllabusDocumentTableRowGroupDto? Group { get; init; }
    public IReadOnlyList<SyllabusDocumentTableCellDto> Cells { get; init; } = [];
}

public sealed class SyllabusDocumentTableRowGroupDto
{
    public string? BlockLabel { get; init; }
    public string? TopicGroupId { get; init; }
    public int? TopicRowSpan { get; init; }
}

public sealed class SyllabusDocumentTableCellDto
{
    public string ColumnKey { get; init; } = null!;
    public string? Value { get; init; }
    public int RowSpan { get; init; } = 1;
    public int ColSpan { get; init; } = 1;
    public string Align { get; init; } = "left";
    public bool Bold { get; init; }
}

public sealed class SyllabusImportPreviewResponse
{
    [JsonPropertyName("document")]
    public SyllabusDocumentResponse Document { get; init; } = new();

    [JsonPropertyName("warnings")]
    public IReadOnlyList<SyllabusDocumentWarningDto> Warnings { get; init; } = [];
}
