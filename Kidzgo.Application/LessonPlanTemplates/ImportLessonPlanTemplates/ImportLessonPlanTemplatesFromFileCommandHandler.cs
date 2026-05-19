using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ExcelDataReader;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;

namespace Kidzgo.Application.LessonPlanTemplates.ImportLessonPlanTemplates;

public sealed class ImportLessonPlanTemplatesFromFileCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<ImportLessonPlanTemplatesFromFileCommand, ImportLessonPlanTemplatesFromFileResponse>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<Result<ImportLessonPlanTemplatesFromFileResponse>> Handle(
        ImportLessonPlanTemplatesFromFileCommand command,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(command.FileName).ToLowerInvariant();
        if (extension is not (".csv" or ".xlsx" or ".xls"))
        {
            return Result.Failure<ImportLessonPlanTemplatesFromFileResponse>(
                LessonPlanTemplateErrors.UnsupportedImportFileType(extension));
        }

        if (!command.ModuleId.HasValue)
        {
            return Result.Failure<ImportLessonPlanTemplatesFromFileResponse>(
                LessonPlanTemplateErrors.ImportFileRequiresModuleId);
        }

        var requestedModule = await context.Modules
            .Where(m => m.Id == command.ModuleId.Value &&
                        m.IsActive)
            .Select(m => new ModuleLookup(m.Id, m.Name, m.LevelId, m.PlannedSessionCount))
            .FirstOrDefaultAsync(cancellationToken);

        if (requestedModule is null)
        {
            return Result.Failure<ImportLessonPlanTemplatesFromFileResponse>(
                LessonPlanTemplateErrors.ModuleNotFound(command.ModuleId));
        }

        var rawSheets = ReadSheets(command.FileStream, command.FileName);
        if (rawSheets.Error != null)
        {
            return Result.Failure<ImportLessonPlanTemplatesFromFileResponse>(rawSheets.Error);
        }

        var parsedSheets = new List<ParsedSyllabusSheet>();
        foreach (var rawSheet in rawSheets.Sheets!)
        {
            if (rawSheet.Rows.Count == 0 || rawSheet.Rows.All(IsEmptyRow))
            {
                continue;
            }

            var parsedSheet = ParseSheet(rawSheet);
            if (parsedSheet.Error != null)
            {
                return Result.Failure<ImportLessonPlanTemplatesFromFileResponse>(parsedSheet.Error);
            }

            parsedSheets.Add(parsedSheet.Value!);
        }

        if (parsedSheets.Count == 0)
        {
            return Result.Failure<ImportLessonPlanTemplatesFromFileResponse>(
                LessonPlanTemplateErrors.InvalidImportFile("No worksheet with syllabus data was found"));
        }

        var currentUserId = userContext.UserId;
        var metadataJson = JsonSerializer.Serialize(
            parsedSheets.SelectMany(x => x.Metadata.Lines).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList(),
            JsonOptions);

        var importedCount = 0;

        foreach (var sheet in parsedSheets)
        {
            foreach (var session in sheet.Sessions)
            {
                if (session.SessionIndex < 1 || session.SessionIndex > requestedModule.TotalSessions)
                {
                    return Result.Failure<ImportLessonPlanTemplatesFromFileResponse>(
                        LessonPlanTemplateErrors.SessionIndexOutOfRange(session.SessionIndex, requestedModule.TotalSessions));
                }

                var template = await context.LessonPlanTemplates
                    .FirstOrDefaultAsync(
                        t => t.ModuleId == requestedModule.Id &&
                             t.SessionIndex == session.SessionIndex,
                        cancellationToken);

                var title = BuildTemplateTitle(requestedModule.Name, session);
                var contentJson = JsonSerializer.Serialize(session, JsonOptions);
                var now = VietnamTime.UtcNow();

                if (template is null)
                {
                    template = new LessonPlanTemplate
                    {
                        Id = Guid.NewGuid(),
                        ModuleId = requestedModule.Id,
                        Title = title,
                        SessionIndex = session.SessionIndex,
                        SyllabusMetadata = metadataJson,
                        SyllabusContent = contentJson,
                        SourceFileName = command.FileName,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = currentUserId,
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    context.LessonPlanTemplates.Add(template);
                    importedCount++;
                    continue;
                }

                if (!command.OverwriteExisting)
                {
                    continue;
                }

                template.Title = title;
                template.SyllabusMetadata = metadataJson;
                template.SyllabusContent = contentJson;
                template.SourceFileName = command.FileName;
                template.IsActive = true;
                template.IsDeleted = false;
                template.UpdatedAt = now;
                importedCount++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return new ImportLessonPlanTemplatesFromFileResponse
        {
            ImportedCount = importedCount,
            Modules =
            [
                new ImportedLessonPlanTemplateModuleDto
                {
                    ModuleId = requestedModule.Id,
                    ModuleName = requestedModule.Name,
                    ImportedSessions = importedCount
                }
            ]
        };
    }

    private static string BuildTemplateTitle(string moduleName, ParsedSyllabusSession session)
    {
        return !string.IsNullOrWhiteSpace(session.Title)
            ? session.Title!
            : $"{moduleName} - Session {session.SessionIndex}";
    }

    private static SheetReadResult ReadSheets(Stream stream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".csv" => ReadCsv(fileName, stream),
            ".xlsx" or ".xls" => ReadExcel(stream),
            _ => SheetReadResult.Failure(LessonPlanTemplateErrors.UnsupportedImportFileType(extension))
        };
    }

    private static SheetReadResult ReadCsv(string fileName, Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        using var parser = new TextFieldParser(reader)
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true
        };
        parser.SetDelimiters(",");

        var rows = new List<string[]>();
        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields();
            if (fields == null)
            {
                continue;
            }

            rows.Add(fields);
        }

        return SheetReadResult.Success(
            [new RawSyllabusSheet(Path.GetFileNameWithoutExtension(fileName), rows)]);
    }

    private static SheetReadResult ReadExcel(Stream stream)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var sheets = new List<RawSyllabusSheet>();
        do
        {
            var rows = new List<string[]>();
            while (reader.Read())
            {
                var fields = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    fields[i] = reader.GetValue(i)?.ToString()?.Trim() ?? string.Empty;
                }

                rows.Add(fields);
            }

            sheets.Add(new RawSyllabusSheet(reader.Name, rows));
        } while (reader.NextResult());

        return SheetReadResult.Success(sheets);
    }

    private static ParsedSheetResult ParseSheet(RawSyllabusSheet sheet)
    {
        var headerRowIndex = FindHeaderRow(sheet.Rows);
        if (headerRowIndex < 0)
        {
            return ParsedSheetResult.Failure(
                LessonPlanTemplateErrors.InvalidImportFile(
                    $"Worksheet '{sheet.Name}' does not contain the required Period/Date/Teacher header"));
        }

        var metadataLines = sheet.Rows
            .Take(headerRowIndex)
            .Select(row => string.Join(" ", row.Where(cell => !string.IsNullOrWhiteSpace(cell))).Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        var dataStartIndex = headerRowIndex + 1;
        if (sheet.Rows.Count > dataStartIndex && IsSubHeaderRow(sheet.Rows[dataStartIndex]))
        {
            dataStartIndex++;
        }

        var sessions = new List<ParsedSyllabusSession>();
        SessionBuilder? currentSession = null;

        for (int rowIndex = dataStartIndex; rowIndex < sheet.Rows.Count; rowIndex++)
        {
            var row = NormalizeMergedRow(EnsureLength(sheet.Rows[rowIndex], 10));
            if (IsEmptyRow(row))
            {
                continue;
            }

            var periodCell = GetCell(row, 0);
            if (TryParseSessionIndex(periodCell, out var sessionIndex))
            {
                if (currentSession != null)
                {
                    sessions.Add(currentSession.Build());
                }

                currentSession = new SessionBuilder(sessionIndex);
                ConsumeSessionRow(currentSession, row);
                continue;
            }

            if (currentSession == null)
            {
                continue;
            }

            ConsumeSessionRow(currentSession, row);
        }

        if (currentSession != null)
        {
            sessions.Add(currentSession.Build());
        }

        if (sessions.Count == 0)
        {
            return ParsedSheetResult.Failure(
                LessonPlanTemplateErrors.InvalidImportFile(
                    $"Worksheet '{sheet.Name}' does not contain any syllabus session rows"));
        }

        return ParsedSheetResult.Success(new ParsedSyllabusSheet(
            sheet.Name,
            new ParsedSyllabusMetadata(
                metadataLines.FirstOrDefault(),
                metadataLines),
            sessions));
    }

    private static void ConsumeSessionRow(SessionBuilder session, string[] row)
    {
        var date = GetCell(row, 1);
        var teacher = GetCell(row, 2);
        var time = GetCell(row, 3);
        var book = GetCell(row, 4);
        var skills = GetCell(row, 5);
        var classwork = GetCell(row, 6);
        var requiredMaterials = GetCell(row, 7);
        var homeworkRequiredMaterials = GetCell(row, 8);
        var extra = GetCell(row, 9);

        if (LooksLikeDate(date) && string.IsNullOrWhiteSpace(session.DateLabel))
        {
            session.DateLabel = date;
        }

        if (!string.IsNullOrWhiteSpace(teacher) && string.IsNullOrWhiteSpace(session.TeacherName))
        {
            session.TeacherName = teacher;
        }

        var startsActivity = !string.IsNullOrWhiteSpace(time) ||
                             !string.IsNullOrWhiteSpace(book) ||
                             !string.IsNullOrWhiteSpace(skills);
        var hasActivityContent = !string.IsNullOrWhiteSpace(classwork) ||
                                 !string.IsNullOrWhiteSpace(requiredMaterials) ||
                                 !string.IsNullOrWhiteSpace(homeworkRequiredMaterials) ||
                                 !string.IsNullOrWhiteSpace(extra);

        if (startsActivity)
        {
            session.StartActivity(time, book, skills, classwork, requiredMaterials, homeworkRequiredMaterials, extra);
            return;
        }

        if (hasActivityContent)
        {
            session.AppendToCurrentActivity(classwork, requiredMaterials, homeworkRequiredMaterials, extra);
            return;
        }

        var note = string.Join(
            " | ",
            new[] { date, teacher }.Where(value => !string.IsNullOrWhiteSpace(value)));

        if (!string.IsNullOrWhiteSpace(note) && !LooksLikeDate(note))
        {
            session.AddNote(note);
        }
    }

    private static int FindHeaderRow(IReadOnlyList<string[]> rows)
    {
        for (int i = 0; i < rows.Count; i++)
        {
            var row = EnsureLength(rows[i], 10);
            if (NormalizeKey(GetCell(row, 0)) == "period" &&
                NormalizeKey(GetCell(row, 1)) == "date" &&
                NormalizeKey(GetCell(row, 2)) == "teacher")
            {
                return i;
            }
        }

        return -1;
    }

    private static bool IsSubHeaderRow(string[] row)
    {
        var normalized = row
            .Select(NormalizeKey)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet();

        return normalized.Contains("classwork") ||
               normalized.Contains("requiredmaterials") ||
               normalized.Contains("extra");
    }

    private static bool LooksLikeDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        return Regex.IsMatch(trimmed, @"^\d{1,2}([/-]\d{1,2})([/-]\d{2,4})?$") ||
               DateTime.TryParse(trimmed, out _) ||
               DateOnly.TryParse(trimmed, out _);
    }

    private static bool LooksLikeDuration(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               Regex.IsMatch(value.Trim(), @"^\d+\s*(mins?|minutes?|hrs?|hours?)$", RegexOptions.IgnoreCase);
    }

    private static bool TryParseSessionIndex(string? value, out int sessionIndex)
    {
        sessionIndex = 0;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        if (int.TryParse(trimmed, out sessionIndex))
        {
            return true;
        }

        if (double.TryParse(trimmed, out var numeric))
        {
            var rounded = (int)Math.Round(numeric);
            if (Math.Abs(numeric - rounded) < 0.0001 && rounded > 0)
            {
                sessionIndex = rounded;
                return true;
            }
        }

        return false;
    }

    private static string[] NormalizeMergedRow(string[] row)
    {
        var normalized = (string[])row.Clone();

        if (LooksLikeDuration(normalized[2]) && string.IsNullOrWhiteSpace(normalized[3]))
        {
            normalized[3] = normalized[2];
            normalized[2] = string.Empty;
        }

        if (LooksLikeDuration(normalized[1]) &&
            string.IsNullOrWhiteSpace(normalized[2]) &&
            string.IsNullOrWhiteSpace(normalized[3]))
        {
            normalized[3] = normalized[1];
            normalized[1] = string.Empty;
        }

        return normalized;
    }

    private static string NormalizeKey(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : Regex.Replace(value.Trim().ToLowerInvariant(), @"[^a-z0-9]+", string.Empty);
    }

    private static string GetCell(string[] row, int index)
    {
        return index >= 0 && index < row.Length
            ? row[index]?.Trim() ?? string.Empty
            : string.Empty;
    }

    private static bool IsEmptyRow(string[] row)
    {
        return row.All(string.IsNullOrWhiteSpace);
    }

    private static string[] EnsureLength(string[] row, int expectedLength)
    {
        if (row.Length >= expectedLength)
        {
            return row;
        }

        var resized = new string[expectedLength];
        Array.Copy(row, resized, row.Length);
        return resized;
    }

    private sealed class SessionBuilder
    {
        private readonly List<ParsedSyllabusActivity> _activities = [];
        private readonly List<string> _notes = [];

        public SessionBuilder(int sessionIndex)
        {
            SessionIndex = sessionIndex;
        }

        public int SessionIndex { get; }
        public string? DateLabel { get; set; }
        public string? TeacherName { get; set; }

        public void StartActivity(
            string? time,
            string? book,
            string? skills,
            string? classwork,
            string? requiredMaterials,
            string? homeworkRequiredMaterials,
            string? extra)
        {
            _activities.Add(new ParsedSyllabusActivity(
                time,
                book,
                skills,
                classwork,
                requiredMaterials,
                homeworkRequiredMaterials,
                extra));
        }

        public void AppendToCurrentActivity(
            string? classwork,
            string? requiredMaterials,
            string? homeworkRequiredMaterials,
            string? extra)
        {
            if (_activities.Count == 0)
            {
                StartActivity(null, null, null, classwork, requiredMaterials, homeworkRequiredMaterials, extra);
                return;
            }

            var current = _activities[^1];
            _activities[^1] = current with
            {
                Classwork = AppendValue(current.Classwork, classwork),
                RequiredMaterials = AppendValue(current.RequiredMaterials, requiredMaterials),
                HomeworkRequiredMaterials = AppendValue(current.HomeworkRequiredMaterials, homeworkRequiredMaterials),
                Extra = AppendValue(current.Extra, extra)
            };
        }

        public void AddNote(string note)
        {
            if (!string.IsNullOrWhiteSpace(note))
            {
                _notes.Add(note);
            }
        }

        public ParsedSyllabusSession Build()
        {
            var title = _notes.FirstOrDefault(note => !LooksLikeDate(note)) ??
                        _activities.Select(a => a.Book).FirstOrDefault(book => !string.IsNullOrWhiteSpace(book)) ??
                        $"Session {SessionIndex}";

            return new ParsedSyllabusSession(
                SessionIndex,
                title,
                DateLabel,
                TeacherName,
                _notes,
                _activities);
        }

        private static string? AppendValue(string? current, string? next)
        {
            if (string.IsNullOrWhiteSpace(next))
            {
                return current;
            }

            if (string.IsNullOrWhiteSpace(current))
            {
                return next.Trim();
            }

            return $"{current}\n{next.Trim()}";
        }
    }

    private sealed record ModuleLookup(Guid Id, string Name, Guid LevelId, int TotalSessions);
    private sealed record RawSyllabusSheet(string Name, List<string[]> Rows);
    private sealed record ParsedSyllabusSheet(string SheetName, ParsedSyllabusMetadata Metadata, List<ParsedSyllabusSession> Sessions);
    private sealed record ParsedSyllabusMetadata(string? Title, List<string> Lines);
    private sealed record ParsedSyllabusSession(
        int SessionIndex,
        string? Title,
        string? DateLabel,
        string? TeacherName,
        List<string> Notes,
        List<ParsedSyllabusActivity> Activities);
    private sealed record ParsedSyllabusActivity(
        string? Time,
        string? Book,
        string? Skills,
        string? Classwork,
        string? RequiredMaterials,
        string? HomeworkRequiredMaterials,
        string? Extra);

    private sealed class SheetReadResult
    {
        private SheetReadResult(List<RawSyllabusSheet>? sheets, Error? error)
        {
            Sheets = sheets;
            Error = error;
        }

        public List<RawSyllabusSheet>? Sheets { get; }
        public Error? Error { get; }

        public static SheetReadResult Success(List<RawSyllabusSheet> sheets) => new(sheets, null);
        public static SheetReadResult Failure(Error error) => new(null, error);
    }

    private sealed class ParsedSheetResult
    {
        private ParsedSheetResult(ParsedSyllabusSheet? value, Error? error)
        {
            Value = value;
            Error = error;
        }

        public ParsedSyllabusSheet? Value { get; }
        public Error? Error { get; }

        public static ParsedSheetResult Success(ParsedSyllabusSheet value) => new(value, null);
        public static ParsedSheetResult Failure(Error error) => new(null, error);
    }
}
