using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ExcelDataReader;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using UglyToad.PdfPig;

namespace Kidzgo.Application.Syllabuses.Shared;

internal static class CurriculumWordImportParser
{
    public static Result<ParsedSyllabusDocument> ParseSyllabusFile(Stream stream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".docx" => ParseSyllabusDocx(stream, fileName),
            ".pdf" => ParseSyllabusPdf(stream, fileName),
            ".xlsx" or ".xls" => ParseSyllabusExcel(stream, fileName),
            _ => Result.Failure<ParsedSyllabusDocument>(
                SyllabusErrors.UnsupportedImportFileType(extension))
        };
    }

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
        var narrative = ParseSyllabusNarrativeSections(paragraphTexts, title, edition);

        var tables = body.Elements<Table>().ToList();
        var resources = ParseResources(tables);
        var lessons = ParseLessons(tables);
        var units = BuildUnits(lessons, tables);

        return Result.Success(new ParsedSyllabusDocument(
            title.Trim(),
            edition?.Trim(),
            narrative.Overview,
            narrative.OverallObjectives,
            narrative.SpecificObjectives,
            narrative.EthicsAndAttitudes,
            narrative.BookOverview,
            narrative.MinutesPerPeriod,
            units,
            lessons,
            resources,
            rawText));
    }

    public static Result<ParsedSyllabusDocument> ParseSyllabusPdf(Stream stream, string fileName)
    {
        if (!string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<ParsedSyllabusDocument>(
                SyllabusErrors.UnsupportedImportFileType(Path.GetExtension(fileName)));
        }

        using var document = PdfDocument.Open(stream);
        var rawText = string.Join("\n", document.GetPages().Select(p => p.Text));
        var lines = rawText
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => Regex.Replace(x, @"\s+", " ").Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (lines.Count == 0)
        {
            return Result.Failure<ParsedSyllabusDocument>(
                SyllabusErrors.InvalidImportFile("The PDF document is empty."));
        }

        var title = lines.FirstOrDefault(x =>
                        x.Contains("syllabus", StringComparison.OrdinalIgnoreCase) ||
                        x.Contains("curriculum", StringComparison.OrdinalIgnoreCase))
                    ?? Path.GetFileNameWithoutExtension(fileName);
        var edition = lines.FirstOrDefault(x => x.Contains("edition", StringComparison.OrdinalIgnoreCase));
        var overview = string.Join(
            "\n",
            lines.TakeWhile(x => !LooksLikePdfTableStart(x))
                .Take(20));

        var lessons = ParseLessonsFromPdfLines(lines);
        if (lessons.Count == 0)
        {
            return Result.Failure<ParsedSyllabusDocument>(
                SyllabusErrors.ImportParseFailed(
                    "Could not detect curriculum rows from PDF. Export to Word or provide a PDF with selectable text table layout."));
        }

        var units = BuildUnitsFromLessons(lessons);
        var resources = ParseResourcesFromPdfLines(lines);

        return Result.Success(new ParsedSyllabusDocument(
            title.Trim(),
            edition?.Trim(),
            string.IsNullOrWhiteSpace(overview) ? null : overview.Trim(),
            null,
            null,
            null,
            null,
            null,
            units,
            lessons,
            resources,
            rawText));
    }

    public static Result<ParsedSyllabusDocument> ParseSyllabusExcel(Stream stream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension is not (".xlsx" or ".xls"))
        {
            return Result.Failure<ParsedSyllabusDocument>(
                SyllabusErrors.UnsupportedImportFileType(Path.GetExtension(fileName)));
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using var reader = ExcelReaderFactory.CreateReader(stream);
        var sheets = new Dictionary<string, List<string[]>>(StringComparer.OrdinalIgnoreCase);

        do
        {
            var rows = new List<string[]>();
            while (reader.Read())
            {
                var values = new string[reader.FieldCount];
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    values[i] = reader.GetValue(i)?.ToString()?.Trim() ?? string.Empty;
                }

                rows.Add(values);
            }

            sheets[reader.Name] = rows;
        } while (reader.NextResult());

        if (!sheets.TryGetValue("Import_Syllabus", out var importRows) || importRows.Count < 2)
        {
            return Result.Failure<ParsedSyllabusDocument>(
                SyllabusErrors.InvalidImportFile("Worksheet 'Import_Syllabus' was not found or is empty."));
        }

        var importHeader = importRows[0].Select(Normalize).ToArray();
        var lessons = ParseLessonsFromImportSheet(importRows.Skip(1).ToList(), importHeader);
        if (lessons.Count == 0)
        {
            return Result.Failure<ParsedSyllabusDocument>(
                SyllabusErrors.InvalidImportFile("No syllabus lessons were found in worksheet 'Import_Syllabus'."));
        }

        var units = ParseUnitsFromSummarySheet(sheets, lessons);
        var resources = ParseResourcesFromMaterialsSheet(sheets);
        var readmeLines = ParseReadmeLines(sheets);
        var title = ResolveExcelTitle(readmeLines, fileName);
        var overview = readmeLines.Count == 0 ? null : string.Join("\n", readmeLines);
        var minutesPerPeriod = ParseMinutesPerPeriod(readmeLines);
        var rawText = string.Join(
            "\n",
            importRows.Select(row => string.Join(" | ", row.Where(cell => !string.IsNullOrWhiteSpace(cell)))));

        return Result.Success(new ParsedSyllabusDocument(
            title,
            null,
            overview,
            null,
            null,
            null,
            null,
            minutesPerPeriod,
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

        var unitLine = blocks.Select(x => x.Text).FirstOrDefault(x =>
            Regex.IsMatch(x, @"\bUNIT\s+(?:\d+|STARTER)\b", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(x, @"\bREVISION\s*0*\d+\b", RegexOptions.IgnoreCase));
        var lessonLine = blocks.Select(x => x.Text).FirstOrDefault(x => Regex.IsMatch(x, @"\bLesson\s*0*\d+\b", RegexOptions.IgnoreCase));
        var unitMatch = unitLine is null
            ? null
            : Regex.Match(unitLine, @"(UNIT\s+(?:\d+|STARTER)|REVISION\s*0*\d+)\s*:?\s*(.*)$", RegexOptions.IgnoreCase);
        var lessonMatch = lessonLine is null ? null : Regex.Match(lessonLine, @"Lesson\s*0*(\d+)", RegexOptions.IgnoreCase);

        var sections = ParseSections(blocks);
        var lessonNumber = lessonMatch?.Success == true ? int.Parse(lessonMatch.Groups[1].Value) : (int?)null;
        var unitTitle = unitMatch?.Success == true
            ? $"{unitMatch.Groups[1].Value.Trim()}{(string.IsNullOrWhiteSpace(unitMatch.Groups[2].Value) ? string.Empty : $": {unitMatch.Groups[2].Value.Trim()}")}"
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

            var documentIndex = FindHeaderIndex(header, "document");
            var abbreviationIndex = FindHeaderIndex(header, "abbreviation");
            var userIndex = FindHeaderIndex(header, "user", "teacher", "pupil");
            var notesIndex = FindHeaderIndex(header, "note");
            var startRowIndex = 1;

            if (rows.Count > 2 && LooksLikeSupplementalResourceHeaderRow(rows[1]))
            {
                startRowIndex = 2;
            }

            return rows.Skip(startRowIndex)
                .Where(row => row.Any(x => !string.IsNullOrWhiteSpace(x)))
                .Select((row, index) =>
                {
                    var documentName = GetByIndex(row, documentIndex);
                    var abbreviation = GetByIndex(row, abbreviationIndex);
                    var notes = ResolveResourceNotes(row, notesIndex);
                    var intendedUsers = ResolveResourceUsers(row, userIndex, notesIndex);

                    return new ParsedSyllabusResource(
                        index + 1,
                        documentName,
                        abbreviation,
                        intendedUsers,
                        notes);
                })
                .Where(resource =>
                    !string.IsNullOrWhiteSpace(resource.DocumentName) ||
                    !string.IsNullOrWhiteSpace(resource.Abbreviation) ||
                    !string.IsNullOrWhiteSpace(resource.IntendedUsers) ||
                    !string.IsNullOrWhiteSpace(resource.Notes))
                .ToList();
        }

        return [];
    }

    private static List<ParsedSyllabusLesson> ParseLessons(IEnumerable<Table> tables)
    {
        List<ParsedSyllabusLesson> bestMatch = [];

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

            if (parsed.Count > bestMatch.Count)
            {
                bestMatch = parsed;
            }
        }

        return bestMatch;
    }

    private static List<ParsedSyllabusLesson> TryParseLessonsFromTable(IReadOnlyList<string[]> rows)
    {
        var courseSyllabusLessons = TryParseCourseSyllabusRows(rows);
        if (courseSyllabusLessons.Count > 0)
        {
            return courseSyllabusLessons;
        }

        var headerSearchLimit = Math.Min(5, rows.Count - 1);
        for (var headerRowIndex = 0; headerRowIndex < headerSearchLimit; headerRowIndex++)
        {
            var header = rows[headerRowIndex].Select(Normalize).ToArray();
            var supplementalHeader = headerRowIndex + 1 < rows.Count && LooksLikeSupplementalLessonHeaderRow(rows[headerRowIndex + 1])
                ? rows[headerRowIndex + 1].Select(Normalize).ToArray()
                : null;
            var layout = BuildLessonTableLayout(header, supplementalHeader);
            if (!layout.IsRecognized)
            {
                continue;
            }

            var headerRows = supplementalHeader is null ? 1 : 2;
            var lessons = ParseLessonRows(rows.Skip(headerRowIndex + headerRows), layout);
            if (lessons.Count > 0)
            {
                return lessons;
            }
        }

        var heuristicLayout = BuildHeuristicLessonTableLayout(rows);
        return heuristicLayout.IsRecognized ? ParseLessonRows(rows, heuristicLayout) : [];
    }

    private static List<ParsedSyllabusLesson> TryParseCourseSyllabusRows(IReadOnlyList<string[]> rows)
    {
        if (rows.Count < 2)
        {
            return [];
        }

        var header = rows[0].Select(Normalize).ToArray();
        var dayIndex = FindHeaderIndex(header, "day");
        var lessonIndex = FindHeaderIndex(header, "lesson");
        var vocabularyIndex = FindHeaderIndex(header, "vocabulary");
        var languageIndex = FindHeaderIndex(header, "languageinuse");
        var grammarIndex = FindHeaderIndex(header, "grammar");
        var activitiesIndex = FindHeaderIndex(header, "languageactivities");

        if (dayIndex < 0 ||
            lessonIndex < 0 ||
            vocabularyIndex < 0 ||
            languageIndex < 0 ||
            activitiesIndex < 0)
        {
            return [];
        }

        var lessons = new List<ParsedSyllabusLesson>();
        foreach (var row in rows.Skip(1))
        {
            var day = GetByIndex(row, dayIndex);
            if (string.IsNullOrWhiteSpace(day))
            {
                continue;
            }

            var topic = ExtractTopicFromCourseDay(day);
            if (topic is null)
            {
                continue;
            }

            var orderIndex = ParseFirstInt(day) ?? lessons.Count + 1;
            var lessonNumber = ParseFirstInt(GetByIndex(row, lessonIndex));
            var content = GetByIndex(row, languageIndex);
            var vocabulary = GetByIndex(row, vocabularyIndex);
            var grammar = GetByIndex(row, grammarIndex);
            var activities = GetByIndex(row, activitiesIndex);

            lessons.Add(new ParsedSyllabusLesson(
                lessons.Count + 1,
                orderIndex,
                orderIndex,
                topic,
                lessonNumber,
                content,
                grammar,
                string.IsNullOrWhiteSpace(vocabulary) ? activities : vocabulary,
                null,
                null,
                topic));
        }

        return lessons;
    }

    private static string? ExtractTopicFromCourseDay(string value)
    {
        var topic = Regex.Replace(value, @"^\s*\d+\s*[\.)-]?\s*", string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(topic))
        {
            return null;
        }

        return Regex.IsMatch(topic, @"\b(Unit|Revision|Hello)\b", RegexOptions.IgnoreCase)
            ? topic
            : null;
    }

    private static LessonTableLayout BuildLessonTableLayout(
        IReadOnlyList<string> header,
        IReadOnlyList<string>? supplementalHeader = null)
    {
        var periodIndex = FindHeaderIndex(header, "period", "week", "session");
        var topicIndex = FindHeaderIndex(header, "topic", "theme", "unit", "revision");
        var lessonIndex = FindHeaderIndex(header, "lesson");
        var contentIndex = FindHeaderIndex(header, "content", "aim", "objective", "outcome", "classwork");
        var structureIndex = FindHeaderIndex(header, "structure", "languagefocus", "grammar");
        var componentIndex = FindHeaderIndex(header, "component", "skill", "competenc");
        var studentBookIndex = FindHeaderIndex(header, "studentbook", "sb", "book");
        var teacherBookIndex = FindHeaderIndex(header, "teacherbook", "tb");

        if (supplementalHeader is not null)
        {
            var nestedStudentBookIndex = FindHeaderIndex(
                supplementalHeader,
                "studentbook",
                "studentsbook",
                "student'sbook",
                "students'book",
                "sb");
            var nestedTeacherBookIndex = FindHeaderIndex(
                supplementalHeader,
                "teacherbook",
                "teachersbook",
                "teacher'sbook",
                "teachers'book",
                "tb");

            if (nestedStudentBookIndex >= 0 || nestedTeacherBookIndex >= 0)
            {
                studentBookIndex = nestedStudentBookIndex >= 0 ? nestedStudentBookIndex : studentBookIndex;
                teacherBookIndex = nestedTeacherBookIndex >= 0 ? nestedTeacherBookIndex : teacherBookIndex;

                if (componentIndex == studentBookIndex || componentIndex == teacherBookIndex)
                {
                    componentIndex = -1;
                }
            }
        }

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

    private static List<ParsedSyllabusUnit> BuildUnits(List<ParsedSyllabusLesson> lessons, List<Table> tables)
    {
        var definitions = ParseUnitDefinitions(tables);
        return BuildUnits(lessons, definitions);
    }

    private static List<ParsedSyllabusUnit> BuildUnitsFromLessons(List<ParsedSyllabusLesson> lessons)
    {
        return BuildUnits(lessons, Array.Empty<UnitDefinition>());
    }

    private static List<ParsedSyllabusUnit> BuildUnits(
        List<ParsedSyllabusLesson> lessons,
        IReadOnlyList<UnitDefinition> definitions)
    {
        var lessonGroups = lessons
            .Select(lesson => new
            {
                Lesson = lesson,
                Key = ExtractUnitKey(lesson.ModuleHint ?? lesson.Topic)
            })
            .Where(x => x.Key is not null)
            .GroupBy(x => x.Key!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Lesson).ToList(),
                StringComparer.OrdinalIgnoreCase);

        var orderedKeys = definitions
            .OrderBy(x => x.OrderIndex)
            .Select(x => x.Key)
            .Concat(lessonGroups.Keys.Where(key => definitions.All(x => !x.Key.Equals(key, StringComparison.OrdinalIgnoreCase))))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var units = new List<ParsedSyllabusUnit>();
        foreach (var key in orderedKeys)
        {
            lessonGroups.TryGetValue(key, out var groupedLessons);
            var definition = definitions.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            var representativeLesson = groupedLessons?
                .OrderBy(x => x.OrderIndex)
                .FirstOrDefault();

            var periodCountFromLessons = groupedLessons?
                .Sum(x => ((x.PeriodTo ?? x.PeriodFrom ?? 0) - (x.PeriodFrom ?? x.PeriodTo ?? 0)) + 1);

            var name = representativeLesson?.Topic;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = definition?.DisplayName ?? BuildDisplayNameFromKey(key);
            }

            var moduleHint = definition?.ModuleHint ?? representativeLesson?.ModuleHint ?? representativeLesson?.Topic ?? name;
            var lessonCount = groupedLessons?.Count;
            var allocatedPeriods = definition?.AllocatedPeriods ?? (periodCountFromLessons > 0 ? periodCountFromLessons : null);

            units.Add(new ParsedSyllabusUnit(
                name!,
                units.Count + 1,
                allocatedPeriods,
                lessonCount > 0 ? lessonCount : null,
                null,
                moduleHint));
        }

        return units;
    }

    private static List<ParsedSyllabusLesson> ParseLessonsFromPdfLines(IReadOnlyList<string> lines)
    {
        var headerIndex = lines
            .Select((line, index) => new { line, index })
            .FirstOrDefault(x => LooksLikePdfTableStart(x.line))?.index ?? -1;

        if (headerIndex < 0)
        {
            return [];
        }

        string? currentTopic = null;
        var lessons = new List<ParsedSyllabusLesson>();

        foreach (var rawLine in lines.Skip(headerIndex + 1))
        {
            if (LooksLikeResourceHeader(rawLine))
            {
                break;
            }

            var columns = SplitPdfColumns(rawLine);
            var lesson = TryParsePdfLessonRow(columns, ref currentTopic, lessons.Count + 1)
                         ?? TryParsePdfLessonRow([rawLine], ref currentTopic, lessons.Count + 1);
            if (lesson is not null)
            {
                lessons.Add(lesson);
            }
        }

        return lessons;
    }

    private static ParsedSyllabusLesson? TryParsePdfLessonRow(
        IReadOnlyList<string> columns,
        ref string? currentTopic,
        int orderIndex)
    {
        if (columns.Count == 0)
        {
            return null;
        }

        var periods = columns[0];
        var (periodFrom, periodTo) = ParsePeriodRange(periods);
        if (!periodFrom.HasValue && !periodTo.HasValue)
        {
            var regexMatch = Regex.Match(
                string.Join(" | ", columns),
                @"^(?<period>\d+(?:\s*-\s*\d+)?)\s+(?<topic>.+?)(?:\s+\|\s+|\s{2,})(?<lesson>\d+)(?:\s+\|\s+|\s{2,})(?<content>.*)$",
                RegexOptions.IgnoreCase);

            if (!regexMatch.Success)
            {
                return null;
            }

            periods = regexMatch.Groups["period"].Value;
            (periodFrom, periodTo) = ParsePeriodRange(periods);
            columns = [
                periods,
                regexMatch.Groups["topic"].Value,
                regexMatch.Groups["lesson"].Value,
                regexMatch.Groups["content"].Value
            ];
        }

        if (!periodFrom.HasValue && !periodTo.HasValue)
        {
            return null;
        }

        var topic = columns.Count > 1 ? columns[1] : null;
        if (!string.IsNullOrWhiteSpace(topic))
        {
            currentTopic = topic.Trim();
        }

        var lessonNumber = columns.Count > 2 ? ParseFirstInt(columns[2]) : null;
        var content = columns.Count > 3 ? columns[3] : null;
        var structures = columns.Count > 4 ? columns[4] : null;
        var studentBook = columns.Count > 5 ? columns[5] : null;
        var teacherBook = columns.Count > 6 ? columns[6] : null;

        if (string.IsNullOrWhiteSpace(currentTopic) && !lessonNumber.HasValue && string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        return new ParsedSyllabusLesson(
            orderIndex,
            periodFrom,
            periodTo,
            currentTopic,
            lessonNumber,
            content,
            structures,
            null,
            studentBook,
            teacherBook,
            currentTopic);
    }

    private static List<string> SplitPdfColumns(string line)
    {
        if (line.Contains('|'))
        {
            return line.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        if (line.Contains('\t'))
        {
            return line.Split('\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        var multiSpaceParts = Regex.Split(line, @"\s{2,}")
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        return multiSpaceParts.Count > 1 ? multiSpaceParts : [line.Trim()];
    }

    private static List<ParsedSyllabusResource> ParseResourcesFromPdfLines(IReadOnlyList<string> lines)
    {
        var headerIndex = lines
            .Select((line, index) => new { line, index })
            .FirstOrDefault(x => LooksLikeResourceHeader(x.line))?.index ?? -1;

        if (headerIndex < 0)
        {
            return [];
        }

        var resources = new List<ParsedSyllabusResource>();
        foreach (var line in lines.Skip(headerIndex + 1))
        {
            var columns = SplitPdfColumns(line);
            if (columns.Count == 0 || columns.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            resources.Add(new ParsedSyllabusResource(
                resources.Count + 1,
                columns.ElementAtOrDefault(0),
                columns.ElementAtOrDefault(1),
                columns.ElementAtOrDefault(2),
                columns.ElementAtOrDefault(3)));
        }

        return resources;
    }

    private static List<ParsedSyllabusLesson> ParseLessonsFromImportSheet(
        IReadOnlyList<string[]> rows,
        IReadOnlyList<string> header)
    {
        var unitCodeIndex = FindHeaderIndex(header, "unitcode");
        var unitTitleIndex = FindHeaderIndex(header, "unittitle");
        var topicIndex = FindHeaderIndex(header, "topic");
        var lessonNoIndex = FindHeaderIndex(header, "lessonno");
        var periodStartIndex = FindHeaderIndex(header, "periodstart");
        var periodEndIndex = FindHeaderIndex(header, "periodend");
        var periodRangeIndex = FindHeaderIndex(header, "periodrange");
        var contentsIndex = FindHeaderIndex(header, "contentsobjectives");
        var structuresIndex = FindHeaderIndex(header, "structuresgrammar");
        var studentBookIndex = FindHeaderIndex(header, "studentbookpages");
        var teacherBookIndex = FindHeaderIndex(header, "teacherbookpages");

        var lessons = new List<ParsedSyllabusLesson>();
        foreach (var row in rows)
        {
            if (row.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var periodStart = ParseFirstInt(GetByIndex(row, periodStartIndex));
            var periodEnd = ParseFirstInt(GetByIndex(row, periodEndIndex));
            if (!periodStart.HasValue && !periodEnd.HasValue)
            {
                (periodStart, periodEnd) = ParsePeriodRange(GetByIndex(row, periodRangeIndex));
            }

            if (!periodStart.HasValue && !periodEnd.HasValue)
            {
                continue;
            }

            var unitCode = GetByIndex(row, unitCodeIndex);
            var unitTitle = GetByIndex(row, unitTitleIndex);
            var topic = GetByIndex(row, topicIndex);
            var normalizedTopic = FirstNonEmpty(
                NormalizeExcelTitleValue(topic),
                NormalizeExcelTitleValue(unitTitle),
                NormalizeExcelTitleValue(unitCode));

            lessons.Add(new ParsedSyllabusLesson(
                lessons.Count + 1,
                periodStart,
                periodEnd,
                normalizedTopic,
                ParseFirstInt(GetByIndex(row, lessonNoIndex)),
                NormalizeExcelCell(GetByIndex(row, contentsIndex)),
                NormalizeExcelCell(GetByIndex(row, structuresIndex)),
                null,
                NormalizeExcelCell(GetByIndex(row, studentBookIndex)),
                NormalizeExcelCell(GetByIndex(row, teacherBookIndex)),
                FirstNonEmpty(unitTitle, unitCode, normalizedTopic)));
        }

        return lessons;
    }

    private static List<ParsedSyllabusUnit> ParseUnitsFromSummarySheet(
        IReadOnlyDictionary<string, List<string[]>> sheets,
        List<ParsedSyllabusLesson> lessons)
    {
        if (!sheets.TryGetValue("Units_Summary", out var summaryRows) || summaryRows.Count < 2)
        {
            return BuildUnitsFromLessons(lessons);
        }

        var header = summaryRows[0].Select(Normalize).ToArray();
        var unitCodeIndex = FindHeaderIndex(header, "unitcode");
        var unitLabelIndex = FindHeaderIndex(header, "unitlabel");
        var unitTitleIndex = FindHeaderIndex(header, "unittitle");
        var plannedPeriodsIndex = FindHeaderIndex(header, "plannedperiods");

        var units = new List<ParsedSyllabusUnit>();
        foreach (var row in summaryRows.Skip(1))
        {
            if (row.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var unitCode = GetByIndex(row, unitCodeIndex);
            var unitLabel = GetByIndex(row, unitLabelIndex);
            var unitTitle = GetByIndex(row, unitTitleIndex);
            var lessonCount = lessons.Count(lesson =>
                string.Equals(
                    Normalize(GetUnitCodeFromHint(lesson.ModuleHint ?? lesson.Topic)),
                    Normalize(unitCode),
                    StringComparison.OrdinalIgnoreCase));

            units.Add(new ParsedSyllabusUnit(
                NormalizeExcelTitleValue(FirstNonEmpty(unitTitle, unitLabel, unitCode)) ?? unitCode,
                units.Count + 1,
                ParseFirstInt(GetByIndex(row, plannedPeriodsIndex)),
                lessonCount > 0 ? lessonCount : null,
                null,
                FirstNonEmpty(unitTitle, unitLabel, unitCode)));
        }

        return units.Count > 0 ? units : BuildUnitsFromLessons(lessons);
    }

    private static List<ParsedSyllabusResource> ParseResourcesFromMaterialsSheet(
        IReadOnlyDictionary<string, List<string[]>> sheets)
    {
        if (!sheets.TryGetValue("Materials", out var materialRows) || materialRows.Count < 2)
        {
            return [];
        }

        var header = materialRows[0].Select(Normalize).ToArray();
        var documentIndex = FindHeaderIndex(header, "document");
        var abbreviationIndex = FindHeaderIndex(header, "abbreviation");
        var teacherIndex = FindHeaderIndex(header, "teacheruser");
        var pupilIndex = FindHeaderIndex(header, "pupiluser");
        var notesIndex = FindHeaderIndex(header, "notes");

        return materialRows
            .Skip(1)
            .Where(row => row.Any(cell => !string.IsNullOrWhiteSpace(cell)))
            .Select((row, index) => new ParsedSyllabusResource(
                index + 1,
                GetByIndex(row, documentIndex),
                GetByIndex(row, abbreviationIndex),
                string.Join(", ", new[]
                {
                    GetByIndex(row, teacherIndex),
                    GetByIndex(row, pupilIndex)
                }.Where(value => !string.IsNullOrWhiteSpace(value))),
                GetByIndex(row, notesIndex)))
            .Where(resource =>
                !string.IsNullOrWhiteSpace(resource.DocumentName) ||
                !string.IsNullOrWhiteSpace(resource.Abbreviation) ||
                !string.IsNullOrWhiteSpace(resource.IntendedUsers) ||
                !string.IsNullOrWhiteSpace(resource.Notes))
            .ToList();
    }

    private static List<string> ParseReadmeLines(IReadOnlyDictionary<string, List<string[]>> sheets)
    {
        if (!sheets.TryGetValue("README", out var readmeRows))
        {
            return [];
        }

        return readmeRows
            .Select(row => string.Join(": ", row.Where(cell => !string.IsNullOrWhiteSpace(cell))))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Take(12)
            .ToList();
    }

    private static string ResolveExcelTitle(IReadOnlyList<string> readmeLines, string fileName)
    {
        var sourceLine = readmeLines.FirstOrDefault(line =>
            line.StartsWith("Source DOCX", StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(sourceLine))
        {
            var sourceValue = sourceLine.Split(':', 2).ElementAtOrDefault(1)?.Trim();
            if (!string.IsNullOrWhiteSpace(sourceValue))
            {
                return Path.GetFileNameWithoutExtension(sourceValue);
            }
        }

        var firstLine = readmeLines.FirstOrDefault();
        return !string.IsNullOrWhiteSpace(firstLine)
            ? firstLine
            : Path.GetFileNameWithoutExtension(fileName);
    }

    private static string? NormalizeExcelCell(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value
            .Replace("\r", string.Empty)
            .Replace("; ", "\n")
            .Replace(";", "\n");

        return Regex.Replace(normalized, @"[ \t]+", " ").Trim();
    }

    private static string? NormalizeExcelTitleValue(string? value)
    {
        var normalized = NormalizeExcelCell(value);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return normalized.Replace("\n", " ").Trim();
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
    }

    private static string? GetUnitCodeFromHint(string? hint)
    {
        if (string.IsNullOrWhiteSpace(hint))
        {
            return null;
        }

        var normalized = hint.Trim();
        var revisionMatch = Regex.Match(normalized, @"REVISION\s*0*(\d+)", RegexOptions.IgnoreCase);
        if (revisionMatch.Success)
        {
            return $"revision-{int.Parse(revisionMatch.Groups[1].Value)}";
        }

        if (normalized.Contains("starter", StringComparison.OrdinalIgnoreCase))
        {
            return "starter";
        }

        var unitMatch = Regex.Match(normalized, @"UNIT\s*0*(\d+)", RegexOptions.IgnoreCase);
        return unitMatch.Success ? $"unit-{int.Parse(unitMatch.Groups[1].Value):00}" : normalized;
    }

    private static List<UnitDefinition> ParseUnitDefinitions(IEnumerable<Table> tables)
    {
        var definitions = new List<UnitDefinition>();
        var orderIndex = 1;

        foreach (var table in tables)
        {
            var rows = ReadTable(table);
            if (rows.Count < 2)
            {
                continue;
            }

            if (!string.Equals(Normalize(GetByIndex(rows[0], 0)), "unit", StringComparison.Ordinal) ||
                !string.Equals(Normalize(GetByIndex(rows[1], 0)), "periods", StringComparison.Ordinal))
            {
                continue;
            }

            var columnCount = Math.Min(rows[0].Length, rows[1].Length);
            for (var columnIndex = 1; columnIndex < columnCount; columnIndex++)
            {
                var label = GetByIndex(rows[0], columnIndex);
                var key = ExtractUnitKey(label);
                if (key is null || definitions.Any(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                definitions.Add(new UnitDefinition(
                    key,
                    label,
                    ParseFirstInt(GetByIndex(rows[1], columnIndex)),
                    orderIndex++,
                    label));
            }
        }

        return definitions;
    }

    private static string? ExtractUnitKey(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var revisionMatch = Regex.Match(text, @"\bREVISION\s*0*(\d+)\b", RegexOptions.IgnoreCase);
        if (revisionMatch.Success)
        {
            return $"REVISION:{int.Parse(revisionMatch.Groups[1].Value)}";
        }

        var unitMatch = Regex.Match(text, @"\bUNIT\s*0*(\d+)\b", RegexOptions.IgnoreCase);
        if (unitMatch.Success)
        {
            return $"UNIT:{int.Parse(unitMatch.Groups[1].Value)}";
        }

        if (text.Contains("starter", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("unit hello", StringComparison.OrdinalIgnoreCase) ||
            (text.Contains("hello", StringComparison.OrdinalIgnoreCase) &&
             text.Contains("unit", StringComparison.OrdinalIgnoreCase)))
        {
            return "STARTER";
        }

        return null;
    }

    private static string BuildDisplayNameFromKey(string key)
    {
        if (string.Equals(key, "STARTER", StringComparison.OrdinalIgnoreCase))
        {
            return "Unit Starter";
        }

        if (key.StartsWith("UNIT:", StringComparison.OrdinalIgnoreCase))
        {
            return $"Unit {key["UNIT:".Length..]}";
        }

        if (key.StartsWith("REVISION:", StringComparison.OrdinalIgnoreCase))
        {
            return $"Revision {key["REVISION:".Length..]}";
        }

        return key;
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

    private static bool LooksLikePdfTableStart(string value)
    {
        var normalized = Normalize(value);
        return normalized.Contains("period") &&
               (normalized.Contains("topic") || normalized.Contains("lesson") || normalized.Contains("content"));
    }

    private static bool LooksLikeResourceHeader(string value)
    {
        var normalized = Normalize(value);
        return normalized.Contains("document") && normalized.Contains("abbreviation");
    }

    private static SyllabusNarrativeSections ParseSyllabusNarrativeSections(
        IReadOnlyList<string> paragraphTexts,
        string title,
        string? edition)
    {
        var overview = new StringBuilder();
        var overallObjectives = new StringBuilder();
        var specificObjectives = new StringBuilder();
        var ethicsAndAttitudes = new StringBuilder();
        var bookOverview = new StringBuilder();
        string? currentSection = null;

        foreach (var paragraph in paragraphTexts)
        {
            var trimmed = paragraph.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) ||
                trimmed.Equals(title, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(edition) &&
                 trimmed.Equals(edition, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var normalized = Normalize(trimmed);
            if (normalized.StartsWith("ii", StringComparison.Ordinal) &&
                normalized.Contains("courseobjectives", StringComparison.Ordinal))
            {
                currentSection = null;
                continue;
            }

            if (normalized.StartsWith("iii", StringComparison.Ordinal) &&
                normalized.Contains("thesyllabusofgetreadyforstarters", StringComparison.Ordinal))
            {
                currentSection = null;
                continue;
            }

            if (normalized.Contains("whatisgetreadyforstarters"))
            {
                currentSection = "overview";
                continue;
            }

            if (normalized.Contains("overallobjectives"))
            {
                currentSection = "overallObjectives";
                continue;
            }

            if (normalized.Contains("specificobjectives"))
            {
                currentSection = "specificObjectives";
                continue;
            }

            if (normalized.Contains("ethicsandattitudes"))
            {
                currentSection = "ethicsAndAttitudes";
                continue;
            }

            if (normalized.Contains("overallinformationaboutthisbook") ||
                normalized.Contains("bookoverview"))
            {
                currentSection = "bookOverview";
                continue;
            }

            if (normalized.Contains("totaldurationoflearningprogram"))
            {
                currentSection = "bookOverview";
                continue;
            }

            if (currentSection == "specificObjectives" &&
                (normalized.Contains("listeningspeakingskills") ||
                 normalized.Contains("readingskill") ||
                 normalized.Contains("writingskill")))
            {
                AppendLine(specificObjectives, trimmed);
                continue;
            }

            switch (currentSection)
            {
                case "overview":
                    AppendLine(overview, trimmed);
                    break;
                case "overallObjectives":
                    AppendLine(overallObjectives, trimmed);
                    break;
                case "specificObjectives":
                    AppendLine(specificObjectives, trimmed);
                    break;
                case "ethicsAndAttitudes":
                    AppendLine(ethicsAndAttitudes, trimmed);
                    break;
                case "bookOverview":
                    AppendLine(bookOverview, trimmed);
                    break;
            }
        }

        return new SyllabusNarrativeSections(
            NullIfWhiteSpace(overview),
            NullIfWhiteSpace(overallObjectives),
            NullIfWhiteSpace(specificObjectives),
            NullIfWhiteSpace(ethicsAndAttitudes),
            NullIfWhiteSpace(bookOverview),
            ParseMinutesPerPeriod(paragraphTexts));
    }

    private static bool LooksLikeSupplementalLessonHeaderRow(IReadOnlyList<string> row)
    {
        var normalized = row.Select(Normalize).ToArray();
        return normalized.Any(x => x.Contains("studentbook") || x.Contains("studentsbook")) ||
               normalized.Any(x => x.Contains("teacherbook") || x.Contains("teachersbook"));
    }

    private static bool LooksLikeSupplementalResourceHeaderRow(IReadOnlyList<string> row)
    {
        var normalized = row.Select(Normalize).ToArray();
        return normalized.Any(x => x == "teachers" || x == "pupils" || x == "teacher" || x == "pupil");
    }

    private static string? ResolveResourceUsers(string[] row, int userIndex, int notesIndex)
    {
        if (userIndex < 0 || userIndex >= row.Length)
        {
            return null;
        }

        var lastUserIndex = notesIndex >= 0
            ? Math.Min(notesIndex, row.Length - 1) - 1
            : row.Length - 1;

        if (lastUserIndex < userIndex)
        {
            lastUserIndex = userIndex;
        }

        var users = row
            .Skip(userIndex)
            .Take(lastUserIndex - userIndex + 1)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        return users.Count == 0 ? null : string.Join(", ", users);
    }

    private static string? ResolveResourceNotes(string[] row, int notesIndex)
    {
        if (notesIndex < 0)
        {
            return row.Length > 0 ? NullIfWhiteSpace(row[^1]) : null;
        }

        if (row.Length > notesIndex + 1)
        {
            return NullIfWhiteSpace(row[^1]);
        }

        return NullIfWhiteSpace(GetByIndex(row, notesIndex));
    }

    private static void AppendLine(StringBuilder builder, string value)
    {
        if (builder.Length > 0)
        {
            builder.AppendLine();
        }

        builder.Append(value);
    }

    private static string? NullIfWhiteSpace(StringBuilder builder)
    {
        return NullIfWhiteSpace(builder.ToString());
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static int? ParseMinutesPerPeriod(IReadOnlyList<string> paragraphTexts)
    {
        foreach (var paragraph in paragraphTexts)
        {
            var match = Regex.Match(
                paragraph,
                @"(?<periods>\d+)\s*periods?\s*x\s*(?<minutes>\d+)\s*minutes?",
                RegexOptions.IgnoreCase);

            if (match.Success && int.TryParse(match.Groups["minutes"].Value, out var minutes))
            {
                return minutes;
            }
        }

        return null;
    }

    private sealed record DocumentBlock(bool IsTable, string Text);

    private sealed record SyllabusNarrativeSections(
        string? Overview,
        string? OverallObjectives,
        string? SpecificObjectives,
        string? EthicsAndAttitudes,
        string? BookOverview,
        int? MinutesPerPeriod);

    private sealed record UnitDefinition(
        string Key,
        string DisplayName,
        int? AllocatedPeriods,
        int OrderIndex,
        string? ModuleHint);

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
