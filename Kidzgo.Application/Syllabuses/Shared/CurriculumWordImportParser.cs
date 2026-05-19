using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;

namespace Kidzgo.Application.Syllabuses.Shared;

internal static class CurriculumWordImportParser
{
    public static Result<ParsedSyllabusDocument> ParseSyllabusDocx(Stream stream, string fileName)
    {
        if (!string.Equals(Path.GetExtension(fileName), ".docx", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<ParsedSyllabusDocument>(
                SyllabusErrors.UnsupportedImportFileType(Path.GetExtension(fileName)));
        }

        using var document = WordprocessingDocument.Open(stream, false);
        var body = document.MainDocumentPart?.Document.Body;
        if (body is null)
        {
            return Result.Failure<ParsedSyllabusDocument>(
                SyllabusErrors.InvalidImportFile("The Word document body is empty."));
        }

        var paragraphTexts = body.Descendants<Paragraph>()
            .Select(ExtractParagraphText)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        var rawText = string.Join("\n", paragraphTexts);
        var title = paragraphTexts.FirstOrDefault(x => x.Contains("syllabus", StringComparison.OrdinalIgnoreCase))
                    ?? Path.GetFileNameWithoutExtension(fileName);
        var edition = paragraphTexts.FirstOrDefault(x => x.Contains("edition", StringComparison.OrdinalIgnoreCase));
        var overview = string.Join(
            "\n",
            paragraphTexts.TakeWhile(x => !LooksLikeTableHeader(x))
                .Take(20));

        var tables = body.Elements<Table>().ToList();
        var resources = ParseResources(tables);
        var lessons = ParseLessons(tables);
        var units = BuildUnits(lessons, paragraphTexts);

        return Result.Success(new ParsedSyllabusDocument(
            title.Trim(),
            edition?.Trim(),
            string.IsNullOrWhiteSpace(overview) ? null : overview.Trim(),
            units,
            lessons,
            resources,
            rawText));
    }

    public static Result<ParsedLessonPlanDocument> ParseLessonPlanDocx(Stream stream, string fileName)
    {
        if (!string.Equals(Path.GetExtension(fileName), ".docx", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<ParsedLessonPlanDocument>(
                SyllabusErrors.UnsupportedImportFileType(Path.GetExtension(fileName)));
        }

        using var document = WordprocessingDocument.Open(stream, false);
        var body = document.MainDocumentPart?.Document.Body;
        if (body is null)
        {
            return Result.Failure<ParsedLessonPlanDocument>(
                SyllabusErrors.InvalidImportFile("The Word document body is empty."));
        }

        var blocks = ExtractBlocks(body);
        var rawText = string.Join("\n", blocks.Select(x => x.Text).Where(x => !string.IsNullOrWhiteSpace(x)));

        var unitLine = blocks.Select(x => x.Text).FirstOrDefault(x => Regex.IsMatch(x, @"\bUNIT\s+\d+\b", RegexOptions.IgnoreCase));
        var lessonLine = blocks.Select(x => x.Text).FirstOrDefault(x => Regex.IsMatch(x, @"\bLesson\s*0*\d+\b", RegexOptions.IgnoreCase));
        var unitMatch = unitLine is null ? null : Regex.Match(unitLine, @"UNIT\s+(\d+)\s*:?\s*(.*)$", RegexOptions.IgnoreCase);
        var lessonMatch = lessonLine is null ? null : Regex.Match(lessonLine, @"Lesson\s*0*(\d+)", RegexOptions.IgnoreCase);

        var sections = ParseSections(blocks);
        var lessonNumber = lessonMatch?.Success == true ? int.Parse(lessonMatch.Groups[1].Value) : (int?)null;
        var unitTitle = unitMatch?.Success == true
            ? $"UNIT {unitMatch.Groups[1].Value}{(string.IsNullOrWhiteSpace(unitMatch.Groups[2].Value) ? string.Empty : $": {unitMatch.Groups[2].Value.Trim()}")}"
            : null;

        var title = !string.IsNullOrWhiteSpace(unitTitle) && lessonNumber.HasValue
            ? $"{unitTitle} - Lesson {lessonNumber.Value}"
            : Path.GetFileNameWithoutExtension(fileName);

        return Result.Success(new ParsedLessonPlanDocument(
            unitTitle,
            unitTitle,
            lessonNumber,
            title,
            GetSection(sections, "objectives"),
            GetSection(sections, "languagecontent"),
            GetSection(sections, "vocabulary"),
            GetSection(sections, "grammar"),
            GetSection(sections, "teachingmethodology"),
            GetSection(sections, "materialsforteacher", "teachermaterials"),
            GetSection(sections, "materialsforstudents", "studentmaterials"),
            GetSection(sections, "procedure", "procedures"),
            GetSection(sections, "evaluation"),
            GetSection(sections, "homework"),
            rawText));
    }

    private static List<ParsedSyllabusResource> ParseResources(IEnumerable<Table> tables)
    {
        foreach (var table in tables)
        {
            var rows = ReadTable(table);
            if (rows.Count < 2)
            {
                continue;
            }

            var header = rows[0].Select(Normalize).ToArray();
            if (!header.Any(x => x.Contains("document")) || !header.Any(x => x.Contains("abbreviation")))
            {
                continue;
            }

            return rows.Skip(1)
                .Where(row => row.Any(x => !string.IsNullOrWhiteSpace(x)))
                .Select((row, index) => new ParsedSyllabusResource(
                    index + 1,
                    GetByIndex(row, 0),
                    GetByIndex(row, 1),
                    GetByIndex(row, 2),
                    GetByIndex(row, 3)))
                .ToList();
        }

        return [];
    }

    private static List<ParsedSyllabusLesson> ParseLessons(IEnumerable<Table> tables)
    {
        foreach (var table in tables)
        {
            var rows = ReadTable(table);
            if (rows.Count < 2)
            {
                continue;
            }

            var parsed = TryParseLessonsFromTable(rows);
            if (parsed.Count == 0)
            {
                continue;
            }

            return parsed;
        }

        return [];
    }

    private static List<ParsedSyllabusLesson> TryParseLessonsFromTable(IReadOnlyList<string[]> rows)
    {
        var headerSearchLimit = Math.Min(5, rows.Count - 1);
        for (var headerRowIndex = 0; headerRowIndex < headerSearchLimit; headerRowIndex++)
        {
            var header = rows[headerRowIndex].Select(Normalize).ToArray();
            var layout = BuildLessonTableLayout(header);
            if (!layout.IsRecognized)
            {
                continue;
            }

            var lessons = ParseLessonRows(rows.Skip(headerRowIndex + 1), layout);
            if (lessons.Count > 0)
            {
                return lessons;
            }
        }

        var heuristicLayout = BuildHeuristicLessonTableLayout(rows);
        return heuristicLayout.IsRecognized ? ParseLessonRows(rows, heuristicLayout) : [];
    }

    private static LessonTableLayout BuildLessonTableLayout(IReadOnlyList<string> header)
    {
        var periodIndex = FindHeaderIndex(header, "period", "week", "session");
        var topicIndex = FindHeaderIndex(header, "topic", "theme", "unit", "revision");
        var lessonIndex = FindHeaderIndex(header, "lesson");
        var contentIndex = FindHeaderIndex(header, "content", "aim", "objective", "outcome", "classwork");
        var structureIndex = FindHeaderIndex(header, "structure", "languagefocus", "grammar");
        var componentIndex = FindHeaderIndex(header, "component", "skill", "competenc");
        var studentBookIndex = FindHeaderIndex(header, "studentbook", "sb", "book");
        var teacherBookIndex = FindHeaderIndex(header, "teacherbook", "tb");

        return new LessonTableLayout(
            periodIndex,
            topicIndex,
            lessonIndex,
            contentIndex,
            structureIndex,
            componentIndex,
            studentBookIndex,
            teacherBookIndex,
            periodIndex >= 0 && (topicIndex >= 0 || lessonIndex >= 0 || contentIndex >= 0));
    }

    private static LessonTableLayout BuildHeuristicLessonTableLayout(IReadOnlyList<string[]> rows)
    {
        var maxColumns = rows.Max(r => r.Length);
        if (maxColumns == 0)
        {
            return LessonTableLayout.Unrecognized;
        }

        var candidateRows = rows
            .Where(r => r.Any(x => !string.IsNullOrWhiteSpace(x)))
            .ToList();

        if (candidateRows.Count == 0)
        {
            return LessonTableLayout.Unrecognized;
        }

        var periodIndex = -1;
        var bestPeriodScore = 0;
        for (var i = 0; i < maxColumns; i++)
        {
            var score = candidateRows.Count(r => ParsePeriodRange(GetByIndex(r, i)).From.HasValue);
            if (score > bestPeriodScore)
            {
                bestPeriodScore = score;
                periodIndex = i;
            }
        }

        if (periodIndex < 0 || bestPeriodScore < 2)
        {
            return LessonTableLayout.Unrecognized;
        }

        var topicIndex = -1;
        var lessonIndex = -1;
        var contentIndex = -1;
        for (var i = 0; i < maxColumns; i++)
        {
            if (i == periodIndex)
            {
                continue;
            }

            var values = candidateRows
                .Select(r => GetByIndex(r, i))
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();

            if (values.Count == 0)
            {
                continue;
            }

            if (topicIndex < 0 && values.Any(v => Regex.IsMatch(v, @"\b(Unit|Revision)\b", RegexOptions.IgnoreCase)))
            {
                topicIndex = i;
                continue;
            }

            if (lessonIndex < 0 && values.Count(v => Regex.IsMatch(v, @"\bLesson\b|\d+", RegexOptions.IgnoreCase)) >= Math.Max(2, values.Count / 3))
            {
                lessonIndex = i;
                continue;
            }

            if (contentIndex < 0)
            {
                contentIndex = i;
            }
        }

        if (topicIndex < 0)
        {
            topicIndex = periodIndex + 1 < maxColumns ? periodIndex + 1 : -1;
        }

        if (lessonIndex < 0)
        {
            lessonIndex = topicIndex + 1 < maxColumns ? topicIndex + 1 : -1;
        }

        if (contentIndex < 0)
        {
            contentIndex = lessonIndex + 1 < maxColumns ? lessonIndex + 1 : -1;
        }

        return new LessonTableLayout(
            periodIndex,
            topicIndex,
            lessonIndex,
            contentIndex,
            StructureIndex: contentIndex + 1,
            ComponentIndex: contentIndex + 2,
            StudentBookIndex: contentIndex + 3,
            TeacherBookIndex: contentIndex + 4,
            IsRecognized: topicIndex >= 0 || lessonIndex >= 0 || contentIndex >= 0);
    }

    private static List<ParsedSyllabusLesson> ParseLessonRows(IEnumerable<string[]> rows, LessonTableLayout layout)
    {
        string? currentTopic = null;
        var lessons = new List<ParsedSyllabusLesson>();

        foreach (var row in rows)
        {
            if (row.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var periods = GetByIndex(row, layout.PeriodIndex, 0);
            var topic = GetByIndex(row, layout.TopicIndex, 1);
            var lesson = GetByIndex(row, layout.LessonIndex, 2);
            var content = GetByIndex(row, layout.ContentIndex, 3);
            var structures = GetByIndex(row, layout.StructureIndex, 4);
            var components = GetByIndex(row, layout.ComponentIndex, 5);
            var studentBook = GetByIndex(row, layout.StudentBookIndex, 6);
            var teacherBook = GetByIndex(row, layout.TeacherBookIndex, 7);

            var (periodFrom, periodTo) = ParsePeriodRange(periods);
            if (!periodFrom.HasValue && !periodTo.HasValue)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(topic))
            {
                currentTopic = topic;
            }

            if (string.IsNullOrWhiteSpace(currentTopic) && string.IsNullOrWhiteSpace(lesson) && string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            var lessonNumber = ParseFirstInt(lesson);
            lessons.Add(new ParsedSyllabusLesson(
                lessons.Count + 1,
                periodFrom,
                periodTo,
                currentTopic,
                lessonNumber,
                content,
                structures,
                components,
                studentBook,
                teacherBook,
                currentTopic));
        }

        return lessons;
    }

    private static List<ParsedSyllabusUnit> BuildUnits(List<ParsedSyllabusLesson> lessons, List<string> paragraphTexts)
    {
        var candidates = lessons
            .Select(x => x.Topic)
            .Concat(paragraphTexts.Where(x => Regex.IsMatch(x, @"\b(Unit|Revision)\b", RegexOptions.IgnoreCase)))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var units = new List<ParsedSyllabusUnit>();
        foreach (var candidate in candidates)
        {
            if (!Regex.IsMatch(candidate, @"\b(Unit|Revision)\b", RegexOptions.IgnoreCase))
            {
                continue;
            }

            var lessonCount = lessons.Count(x => string.Equals(x.Topic?.Trim(), candidate, StringComparison.OrdinalIgnoreCase));
            var periodCount = lessons
                .Where(x => string.Equals(x.Topic?.Trim(), candidate, StringComparison.OrdinalIgnoreCase))
                .Sum(x => ((x.PeriodTo ?? x.PeriodFrom ?? 0) - (x.PeriodFrom ?? x.PeriodTo ?? 0)) + 1);

            units.Add(new ParsedSyllabusUnit(
                candidate,
                units.Count + 1,
                periodCount <= 0 ? null : periodCount,
                lessonCount <= 0 ? null : lessonCount,
                null,
                candidate));
        }

        return units;
    }

    private static List<DocumentBlock> ExtractBlocks(Body body)
    {
        var blocks = new List<DocumentBlock>();
        foreach (var child in body.ChildElements)
        {
            switch (child)
            {
                case Paragraph paragraph:
                    var paragraphText = ExtractParagraphText(paragraph);
                    if (!string.IsNullOrWhiteSpace(paragraphText))
                    {
                        blocks.Add(new DocumentBlock(false, paragraphText.Trim()));
                    }
                    break;
                case Table table:
                    var rows = ReadTable(table)
                        .Where(x => x.Any(cell => !string.IsNullOrWhiteSpace(cell)))
                        .Select(row => string.Join(" | ", row.Where(cell => !string.IsNullOrWhiteSpace(cell))));
                    var tableText = string.Join("\n", rows);
                    if (!string.IsNullOrWhiteSpace(tableText))
                    {
                        blocks.Add(new DocumentBlock(true, tableText.Trim()));
                    }
                    break;
            }
        }

        return blocks;
    }

    private static Dictionary<string, StringBuilder> ParseSections(List<DocumentBlock> blocks)
    {
        var sections = new Dictionary<string, StringBuilder>(StringComparer.OrdinalIgnoreCase);
        string? currentSection = null;

        foreach (var block in blocks)
        {
            var normalized = NormalizeHeading(block.Text);
            if (TryMapSection(normalized, out var sectionKey))
            {
                currentSection = sectionKey;
                if (!sections.TryGetValue(sectionKey, out _))
                {
                    sections[sectionKey] = new StringBuilder();
                }

                var remainder = ExtractRemainder(block.Text);
                if (!string.IsNullOrWhiteSpace(remainder))
                {
                    sections[sectionKey].AppendLine(remainder.Trim());
                }

                continue;
            }

            if (currentSection != null)
            {
                sections[currentSection].AppendLine(block.Text.Trim());
            }
        }

        return sections;
    }

    private static bool TryMapSection(string normalized, out string sectionKey)
    {
        var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["objectives"] = "objectives",
            ["languagecontent"] = "languagecontent",
            ["vocabulary"] = "vocabulary",
            ["grammar"] = "grammar",
            ["teachingmethodology"] = "teachingmethodology",
            ["materialsforteacher"] = "materialsforteacher",
            ["materialsforthestudents"] = "materialsforstudents",
            ["materialsforstudents"] = "materialsforstudents",
            ["studentmaterials"] = "materialsforstudents",
            ["teachermaterials"] = "materialsforteacher",
            ["procedure"] = "procedure",
            ["procedures"] = "procedure",
            ["evaluation"] = "evaluation",
            ["homework"] = "homework"
        };

        foreach (var alias in aliases)
        {
            if (normalized.StartsWith(alias.Key, StringComparison.OrdinalIgnoreCase))
            {
                sectionKey = alias.Value;
                return true;
            }
        }

        sectionKey = string.Empty;
        return false;
    }

    private static string? GetSection(Dictionary<string, StringBuilder> sections, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (sections.TryGetValue(key, out var builder))
            {
                var value = builder.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        return null;
    }

    private static string ExtractParagraphText(Paragraph paragraph)
    {
        return string.Concat(paragraph.Descendants<Text>().Select(x => x.Text));
    }

    private static List<string[]> ReadTable(Table table)
    {
        return table.Elements<TableRow>()
            .Select(row => row.Elements<TableCell>()
                .Select(cell => string.Join(" ", cell.Descendants<Text>().Select(t => t.Text)).Trim())
                .ToArray())
            .ToList();
    }

    private static (int? From, int? To) ParsePeriodRange(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (null, null);
        }

        var numbers = Regex.Matches(value, @"\d+")
            .Select(x => int.Parse(x.Value))
            .ToList();

        if (numbers.Count == 0)
        {
            return (null, null);
        }

        if (numbers.Count == 1)
        {
            return (numbers[0], numbers[0]);
        }

        return (numbers[0], numbers[1]);
    }

    private static int? ParseFirstInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var match = Regex.Match(value, @"\d+");
        return match.Success ? int.Parse(match.Value) : null;
    }

    private static string GetByIndex(string[] row, int index)
    {
        return index >= 0 && index < row.Length ? row[index]?.Trim() ?? string.Empty : string.Empty;
    }

    private static string GetByIndex(string[] row, int index, int fallbackIndex)
    {
        var value = GetByIndex(row, index);
        return string.IsNullOrWhiteSpace(value) ? GetByIndex(row, fallbackIndex) : value;
    }

    private static int FindHeaderIndex(IReadOnlyList<string> header, params string[] aliases)
    {
        for (var i = 0; i < header.Count; i++)
        {
            if (aliases.Any(alias => header[i].Contains(alias, StringComparison.OrdinalIgnoreCase)))
            {
                return i;
            }
        }

        return -1;
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : Regex.Replace(value.Trim().ToLowerInvariant(), @"[^a-z0-9]+", string.Empty);
    }

    private static string NormalizeHeading(string value) => Normalize(value);

    private static string ExtractRemainder(string value)
    {
        var index = value.IndexOf(':');
        return index >= 0 ? value[(index + 1)..] : string.Empty;
    }

    private static bool LooksLikeTableHeader(string value)
    {
        var normalized = Normalize(value);
        return normalized.Contains("period") ||
               normalized.Contains("topic") ||
               normalized.Contains("document");
    }

    private sealed record DocumentBlock(bool IsTable, string Text);

    private readonly record struct LessonTableLayout(
        int PeriodIndex,
        int TopicIndex,
        int LessonIndex,
        int ContentIndex,
        int StructureIndex,
        int ComponentIndex,
        int StudentBookIndex,
        int TeacherBookIndex,
        bool IsRecognized)
    {
        public static LessonTableLayout Unrecognized => new(-1, -1, -1, -1, -1, -1, -1, -1, false);
    }
}
