using System.Text.Json;
using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Application.Syllabuses.Shared;

internal static class SyllabusDocumentMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public static SyllabusDocumentResponse ToResponse(
        Syllabus syllabus,
        IReadOnlyList<SyllabusDocumentSectionDto> sections,
        IReadOnlyList<SyllabusDocumentWarningDto> warnings,
        int totalUnits,
        int totalSessions,
        int totalLessons,
        int totalPeriods)
    {
        return new SyllabusDocumentResponse
        {
            Id = syllabus.Id,
            ProgramId = syllabus.ProgramId,
            LevelId = syllabus.LevelId,
            Code = syllabus.Code,
            Title = syllabus.Title,
            Edition = syllabus.Edition,
            Status = NormalizeStatus(syllabus.DocumentStatus),
            SourceType = NormalizeSourceType(syllabus.SourceType),
            SourceFileName = syllabus.SourceFileName,
            ParserVersion = syllabus.ParserVersion,
            Version = syllabus.DocumentVersion <= 0 ? 1 : syllabus.DocumentVersion,
            Summary = new SyllabusDocumentSummaryDto
            {
                TotalUnits = totalUnits,
                TotalSessions = totalSessions,
                TotalLessons = totalLessons,
                TotalPeriods = totalPeriods,
                MinutesPerPeriod = syllabus.MinutesPerPeriod
            },
            Sections = sections.OrderBy(x => x.OrderIndex).ToList(),
            Warnings = warnings
        };
    }

    public static SyllabusDocumentResponse ToResponseFromSections(
        Syllabus syllabus,
        IReadOnlyList<SyllabusDocumentSectionDto> sections,
        IReadOnlyList<SyllabusDocumentWarningDto> warnings,
        int fallbackTotalUnits = 0,
        int fallbackTotalSessions = 0,
        int fallbackTotalLessons = 0,
        int fallbackTotalPeriods = 0)
    {
        var summary = ComputeSummaryFromSections(sections);
        var totalUnits = summary.TotalUnits > 0 ? summary.TotalUnits : fallbackTotalUnits;
        var totalSessions = summary.TotalSessions > 0 ? summary.TotalSessions : fallbackTotalSessions;
        var totalLessons = summary.TotalLessons > 0 ? summary.TotalLessons : fallbackTotalLessons;
        var totalPeriods = summary.TotalPeriods > 0 ? summary.TotalPeriods : fallbackTotalPeriods;

        return ToResponse(
            syllabus,
            sections,
            warnings,
            totalUnits,
            totalSessions,
            totalLessons,
            totalPeriods);
    }

    public static IReadOnlyList<SyllabusDocumentSectionDto> ReadSections(
        Syllabus syllabus,
        IReadOnlyList<SyllabusUnit>? units = null,
        IReadOnlyList<SyllabusLesson>? lessons = null,
        IReadOnlyList<SyllabusResource>? resources = null)
    {
        if (!string.IsNullOrWhiteSpace(syllabus.SectionsJson))
        {
            var parsed = JsonSerializer.Deserialize<List<SyllabusDocumentSectionDto>>(syllabus.SectionsJson, JsonOptions);
            if (parsed is { Count: > 0 })
            {
                return parsed.OrderBy(x => x.OrderIndex).ToList();
            }
        }

        return BuildFallbackSections(syllabus, units ?? [], lessons ?? [], resources ?? []);
    }

    public static IReadOnlyList<SyllabusDocumentWarningDto> ReadWarnings(Syllabus syllabus)
    {
        if (string.IsNullOrWhiteSpace(syllabus.WarningsJson))
        {
            return [];
        }

        var parsed = JsonSerializer.Deserialize<List<SyllabusDocumentWarningDto>>(syllabus.WarningsJson, JsonOptions);
        return parsed ?? [];
    }

    public static string WriteSections(IReadOnlyList<SyllabusDocumentSectionDto> sections)
    {
        return JsonSerializer.Serialize(sections.OrderBy(x => x.OrderIndex).ToList(), JsonOptions);
    }

    public static string WriteWarnings(IReadOnlyList<SyllabusDocumentWarningDto> warnings)
    {
        return JsonSerializer.Serialize(warnings, JsonOptions);
    }

    public static List<SyllabusDocumentSectionDto> BuildInitialManualSections()
    {
        return [];
    }

    public static (List<SyllabusDocumentSectionDto> Sections, List<SyllabusDocumentWarningDto> Warnings, int TotalPeriods, int TotalLessons)
        BuildFromParsedImport(ParsedSyllabusDocument parsed)
    {
        var sections = new List<SyllabusDocumentSectionDto>();
        var warnings = new List<SyllabusDocumentWarningDto>();
        var orderIndex = 1;

        sections.Add(new SyllabusDocumentSectionDto
        {
            SectionId = Guid.NewGuid(),
            Type = SyllabusDocumentSectionTypes.Heading,
            Title = parsed.Title,
            OrderIndex = orderIndex++,
            Editable = true,
            Content = parsed.Edition
        });

        if (!string.IsNullOrWhiteSpace(parsed.Overview))
        {
            sections.Add(new SyllabusDocumentSectionDto
            {
                SectionId = Guid.NewGuid(),
                Type = SyllabusDocumentSectionTypes.Narrative,
                Title = "Overview",
                OrderIndex = orderIndex++,
                Editable = true,
                Content = parsed.Overview
            });
        }

        AddNarrativeSection(sections, "Overall objectives", parsed.OverallObjectives, ref orderIndex);
        AddNarrativeSection(sections, "Specific objectives", parsed.SpecificObjectives, ref orderIndex);
        AddNarrativeSection(sections, "Ethics and attitudes", parsed.EthicsAndAttitudes, ref orderIndex);
        AddNarrativeSection(sections, "Book overview", parsed.BookOverview, ref orderIndex);

        var lessonTableSection = BuildCurriculumTableSection(parsed.Lessons, orderIndex++);
        sections.Add(lessonTableSection);
        warnings.AddRange(BuildWarningsForTable(lessonTableSection));

        if (parsed.Resources.Count > 0)
        {
            sections.Add(new SyllabusDocumentSectionDto
            {
                SectionId = Guid.NewGuid(),
                Type = SyllabusDocumentSectionTypes.List,
                Title = "Resources",
                OrderIndex = orderIndex,
                Editable = true,
                Items = parsed.Resources
                    .OrderBy(x => x.OrderIndex)
                    .Select(x => string.Join(" - ", new[]
                    {
                        x.DocumentName,
                        x.Abbreviation,
                        x.IntendedUsers
                    }.Where(v => !string.IsNullOrWhiteSpace(v))))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList()
            });
        }

        var totalPeriods = parsed.Lessons
            .Select(x => x.PeriodTo ?? x.PeriodFrom ?? 0)
            .DefaultIfEmpty(0)
            .Max();

        return (sections, warnings, totalPeriods, parsed.Lessons.Count);
    }

    public static string NormalizeStatus(string? status)
    {
        return status switch
        {
            SyllabusDocumentStatuses.Published => SyllabusDocumentStatuses.Published,
            SyllabusDocumentStatuses.Archived => SyllabusDocumentStatuses.Archived,
            _ => SyllabusDocumentStatuses.Draft
        };
    }

    public static string NormalizeSourceType(string? sourceType)
    {
        return sourceType switch
        {
            SyllabusDocumentSourceTypes.Imported => SyllabusDocumentSourceTypes.Imported,
            SyllabusDocumentSourceTypes.Hybrid => SyllabusDocumentSourceTypes.Hybrid,
            _ => SyllabusDocumentSourceTypes.Manual
        };
    }

    public static (int TotalUnits, int TotalSessions, int TotalLessons, int TotalPeriods) ComputeSummaryFromSections(
        IReadOnlyList<SyllabusDocumentSectionDto> sections)
    {
        var rows = sections
            .Where(x => x.Type == SyllabusDocumentSectionTypes.Table && x.Table is not null)
            .SelectMany(x => x.Table!.Rows)
            .ToList();

        var totalPeriods = rows
            .Select(row => row.Cells.FirstOrDefault(x => x.ColumnKey == "periods")?.Value)
            .Select(ParsePeriodEnd)
            .DefaultIfEmpty(0)
            .Max();

        var totalUnits = rows
            .Select(row => row.Cells.FirstOrDefault(x => x.ColumnKey == "topics")?.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        return (totalUnits, rows.Count, rows.Count, totalPeriods);
    }

    private static List<SyllabusDocumentSectionDto> BuildFallbackSections(
        Syllabus syllabus,
        IReadOnlyList<SyllabusUnit> units,
        IReadOnlyList<SyllabusLesson> lessons,
        IReadOnlyList<SyllabusResource> resources)
    {
        var sections = new List<SyllabusDocumentSectionDto>();
        var orderIndex = 1;

        sections.Add(new SyllabusDocumentSectionDto
        {
            SectionId = Guid.NewGuid(),
            Type = SyllabusDocumentSectionTypes.Heading,
            Title = syllabus.Title,
            OrderIndex = orderIndex++,
            Editable = true,
            Content = syllabus.Edition
        });

        AddNarrativeSectionIfPresent(sections, "Overview", syllabus.Overview, ref orderIndex);
        AddNarrativeSectionIfPresent(sections, "Overall objectives", syllabus.OverallObjectives, ref orderIndex);
        AddNarrativeSectionIfPresent(sections, "Specific objectives", syllabus.SpecificObjectives, ref orderIndex);
        AddNarrativeSectionIfPresent(sections, "Ethics and attitudes", syllabus.EthicsAndAttitudes, ref orderIndex);
        AddNarrativeSectionIfPresent(sections, "Book overview", syllabus.BookOverview, ref orderIndex);

        if (lessons.Count > 0)
        {
            var parsedLessons = lessons
                .OrderBy(x => x.OrderIndex)
                .Select(x => new ParsedSyllabusLesson(
                    x.OrderIndex,
                    x.PeriodFrom,
                    x.PeriodTo,
                    x.Topic,
                    x.LessonNumber,
                    x.ContentSummary,
                    x.StructureSummary,
                    null,
                    x.StudentBookPages,
                    x.TeacherBookPages,
                    x.Topic))
                .ToList();

            sections.Add(BuildCurriculumTableSection(parsedLessons, orderIndex++));
        }

        if (resources.Count > 0)
        {
            sections.Add(new SyllabusDocumentSectionDto
            {
                SectionId = Guid.NewGuid(),
                Type = SyllabusDocumentSectionTypes.List,
                Title = "Resources",
                OrderIndex = orderIndex,
                Editable = true,
                Items = resources
                    .OrderBy(x => x.OrderIndex)
                    .Select(x => string.Join(" - ", new[] { x.DocumentName, x.Abbreviation, x.IntendedUsers }
                        .Where(v => !string.IsNullOrWhiteSpace(v))))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList()
            });
        }

        return sections;
    }

    private static void AddNarrativeSection(
        ICollection<SyllabusDocumentSectionDto> sections,
        string title,
        string? content,
        ref int orderIndex)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        sections.Add(new SyllabusDocumentSectionDto
        {
            SectionId = Guid.NewGuid(),
            Type = SyllabusDocumentSectionTypes.Narrative,
            Title = title,
            OrderIndex = orderIndex++,
            Editable = true,
            Content = content
        });
    }

    private static void AddNarrativeSectionIfPresent(
        ICollection<SyllabusDocumentSectionDto> sections,
        string title,
        string? content,
        ref int orderIndex)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        sections.Add(new SyllabusDocumentSectionDto
        {
            SectionId = Guid.NewGuid(),
            Type = SyllabusDocumentSectionTypes.Narrative,
            Title = title,
            OrderIndex = orderIndex++,
            Editable = true,
            Content = content
        });
    }

    private static SyllabusDocumentSectionDto BuildCurriculumTableSection(
        IReadOnlyList<ParsedSyllabusLesson> lessons,
        int orderIndex)
    {
        var columns = new List<SyllabusDocumentTableColumnDto>
        {
            new() { Key = "periods", Label = "Periods", Width = 120, Sticky = false },
            new() { Key = "topics", Label = "Topics", Width = 240, Sticky = true },
            new() { Key = "lessons", Label = "Lessons", Width = 90, Sticky = false },
            new() { Key = "contents", Label = "Contents", Width = 360, Sticky = false },
            new() { Key = "structures", Label = "Structures", Width = 260, Sticky = false },
            new() { Key = "studentsBook", Label = "Students book", Width = 140, Sticky = false },
            new() { Key = "teachersBook", Label = "Teacher's book", Width = 140, Sticky = false }
        };

        var orderedLessons = lessons.OrderBy(x => x.OrderIndex).ToList();
        var rows = new List<SyllabusDocumentTableRowDto>(orderedLessons.Count);
        var topicGroups = orderedLessons
            .Select((lesson, index) => new
            {
                Lesson = lesson,
                Index = index,
                Topic = string.IsNullOrWhiteSpace(lesson.Topic) ? "(Untitled topic)" : lesson.Topic!.Trim()
            })
            .GroupBy(x => x.Topic)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Count = g.Count(),
                    FirstIndex = g.Min(x => x.Index)
                });

        for (var index = 0; index < orderedLessons.Count; index++)
        {
            var lesson = orderedLessons[index];
            var normalizedTopic = string.IsNullOrWhiteSpace(lesson.Topic) ? "(Untitled topic)" : lesson.Topic!.Trim();
            var groupInfo = topicGroups[normalizedTopic];
            var isFirstTopicRow = groupInfo.FirstIndex == index;

            rows.Add(new SyllabusDocumentTableRowDto
            {
                RowId = Guid.NewGuid(),
                OrderIndex = lesson.OrderIndex,
                Group = new SyllabusDocumentTableRowGroupDto
                {
                    BlockLabel = ExtractBlockLabel(normalizedTopic),
                    TopicGroupId = $"topic-{groupInfo.FirstIndex + 1}",
                    TopicRowSpan = groupInfo.Count
                },
                Cells = new List<SyllabusDocumentTableCellDto>
                {
                    new()
                    {
                        ColumnKey = "periods",
                        Value = BuildPeriodValue(lesson.PeriodFrom, lesson.PeriodTo),
                        Align = "center",
                        Bold = true
                    },
                    new()
                    {
                        ColumnKey = "topics",
                        Value = normalizedTopic,
                        RowSpan = isFirstTopicRow ? Math.Max(1, groupInfo.Count) : 1,
                        Align = "left",
                        Bold = true
                    },
                    new()
                    {
                        ColumnKey = "lessons",
                        Value = lesson.LessonNumber?.ToString(),
                        Align = "center"
                    },
                    new()
                    {
                        ColumnKey = "contents",
                        Value = lesson.ContentSummary
                    },
                    new()
                    {
                        ColumnKey = "structures",
                        Value = lesson.StructureSummary
                    },
                    new()
                    {
                        ColumnKey = "studentsBook",
                        Value = lesson.StudentBookPages,
                        Align = "center"
                    },
                    new()
                    {
                        ColumnKey = "teachersBook",
                        Value = lesson.TeacherBookPages,
                        Align = "center"
                    }
                }
            });
        }

        return new SyllabusDocumentSectionDto
        {
            SectionId = Guid.NewGuid(),
            Type = SyllabusDocumentSectionTypes.Table,
            Title = "Curriculum",
            OrderIndex = orderIndex,
            Editable = true,
            Table = new SyllabusDocumentTableDto
            {
                Columns = columns,
                Rows = rows
            }
        };
    }

    private static IEnumerable<SyllabusDocumentWarningDto> BuildWarningsForTable(SyllabusDocumentSectionDto tableSection)
    {
        if (tableSection.Table is null)
        {
            return [];
        }

        var warnings = new List<SyllabusDocumentWarningDto>();
        foreach (var row in tableSection.Table.Rows)
        {
            if (!row.Cells.Any(x => x.ColumnKey == "teachersBook" && !string.IsNullOrWhiteSpace(x.Value)))
            {
                warnings.Add(new SyllabusDocumentWarningDto
                {
                    Code = "MISSING_COLUMN",
                    Severity = "Warning",
                    Message = $"Teachers book column missing in row {row.OrderIndex}",
                    SectionRef = tableSection.SectionId.ToString(),
                    RowRef = row.RowId.ToString(),
                    CellRef = "teachersBook"
                });
            }
        }

        return warnings;
    }

    private static string BuildPeriodValue(int? from, int? to)
    {
        if (!from.HasValue && !to.HasValue)
        {
            return string.Empty;
        }

        if (!from.HasValue || !to.HasValue || from.Value == to.Value)
        {
            return (from ?? to)!.Value.ToString();
        }

        return $"{from}-{to}";
    }

    private static string? ExtractBlockLabel(string? topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            return null;
        }

        if (topic.Contains("starter", StringComparison.OrdinalIgnoreCase))
        {
            return "Starter";
        }

        if (topic.Contains("revision", StringComparison.OrdinalIgnoreCase))
        {
            return "Revision";
        }

        var unitIndex = topic.IndexOf("unit", StringComparison.OrdinalIgnoreCase);
        if (unitIndex >= 0)
        {
            return "Unit";
        }

        return null;
    }

    private static int ParsePeriodEnd(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        var parts = value.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return 0;
        }

        return int.TryParse(parts[^1], out var parsed) ? parsed : 0;
    }
}
