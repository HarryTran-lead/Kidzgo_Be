namespace Kidzgo.API.Requests;

public sealed class UpdateSyllabusMetadataRequest
{
    public int ExpectedVersion { get; init; }
    public string? Code { get; init; }
    public string Title { get; init; } = null!;
    public string? Edition { get; init; }
    public int? MinutesPerPeriod { get; init; }
}

public sealed class AddSyllabusSectionRequest
{
    public int ExpectedVersion { get; init; }
    public SyllabusSectionRequest Section { get; init; } = new();
}

public sealed class SyllabusSectionRequest
{
    public string Type { get; init; } = null!;
    public string? Title { get; init; }
    public int OrderIndex { get; init; }
    public string? Content { get; init; }
    public IReadOnlyList<string>? Items { get; init; }
    public SyllabusTableRequest? Table { get; init; }
}

public sealed class UpdateSyllabusSectionRequest
{
    public int ExpectedVersion { get; init; }
    public string? Title { get; init; }
    public string? Content { get; init; }
    public IReadOnlyList<string>? Items { get; init; }
}

public sealed class UpdateSyllabusTableCellRequest
{
    public int ExpectedVersion { get; init; }
    public string? Value { get; init; }
    public int? RowSpan { get; init; }
    public int? ColSpan { get; init; }
    public string? Align { get; init; }
    public bool? Bold { get; init; }
}

public sealed class AddSyllabusTableRowRequest
{
    public int ExpectedVersion { get; init; }
    public int OrderIndex { get; init; }
    public IReadOnlyList<SyllabusTableCellRequest> Cells { get; init; } = [];
}

public sealed class ReorderSyllabusSectionsRequest
{
    public int ExpectedVersion { get; init; }
    public IReadOnlyList<SyllabusSectionOrderRequest> Orders { get; init; } = [];
}

public sealed class SyllabusSectionOrderRequest
{
    public Guid SectionId { get; init; }
    public int OrderIndex { get; init; }
}

public sealed class PublishSyllabusDocumentRequest
{
    public int ExpectedVersion { get; init; }
}

public sealed class ArchiveSyllabusDocumentRequest
{
    public int ExpectedVersion { get; init; }
    public string? Reason { get; init; }
}

public sealed class SyllabusTableRequest
{
    public IReadOnlyList<SyllabusTableColumnRequest> Columns { get; init; } = [];
    public IReadOnlyList<SyllabusTableRowRequest> Rows { get; init; } = [];
}

public sealed class SyllabusTableColumnRequest
{
    public string Key { get; init; } = null!;
    public string Label { get; init; } = null!;
    public int? Width { get; init; }
    public bool Sticky { get; init; }
}

public sealed class SyllabusTableRowRequest
{
    public int OrderIndex { get; init; }
    public SyllabusTableRowGroupRequest? Group { get; init; }
    public IReadOnlyList<SyllabusTableCellRequest> Cells { get; init; } = [];
}

public sealed class SyllabusTableRowGroupRequest
{
    public string? BlockLabel { get; init; }
    public string? TopicGroupId { get; init; }
    public int? TopicRowSpan { get; init; }
}

public sealed class SyllabusTableCellRequest
{
    public string ColumnKey { get; init; } = null!;
    public string? Value { get; init; }
    public int RowSpan { get; init; } = 1;
    public int ColSpan { get; init; } = 1;
    public string Align { get; init; } = "left";
    public bool Bold { get; init; }
}
