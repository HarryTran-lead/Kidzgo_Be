using System.Text;
using System.Text.Json;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ExcelDataReader;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Application.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using UglyToad.PdfPig;

namespace Kidzgo.Application.QuestionBank.ImportQuestionBank;

public sealed class ImportQuestionBankFromFileCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<ImportQuestionBankFromFileCommand, ImportQuestionBankFromFileResponse>
{
    public async Task<Result<ImportQuestionBankFromFileResponse>> Handle(
        ImportQuestionBankFromFileCommand command,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(command.FileName).ToLowerInvariant();
        var programExists = await context.Programs
            .AnyAsync(p => p.Id == command.ProgramId, cancellationToken);

        if (!programExists)
        {
            return Result.Failure<ImportQuestionBankFromFileResponse>(
                HomeworkErrors.ProgramNotFound(command.ProgramId));
        }

        var table = ReadTable(command.FileStream, command.FileName);
        if (table.Error != null)
        {
            return Result.Failure<ImportQuestionBankFromFileResponse>(table.Error);
        }

        var headers = table.Headers!;
        var rows = table.Rows!;

        var headerIndex = BuildHeaderIndex(headers);
        var questionTextIdx = FindHeaderIndex(headerIndex, "questiontext", "question_text", "question");
        var optionsIdx = FindHeaderIndex(headerIndex, "options", "choices");
        var correctAnswerIdx = FindHeaderIndex(headerIndex, "correctanswer", "correct_answer", "answer", "correct");
        var levelIdx = FindHeaderIndex(headerIndex, "level", "difficulty");
        var pointsIdx = FindHeaderIndex(headerIndex, "points", "score");
        var explanationIdx = FindHeaderIndex(headerIndex, "explanation", "explain");
        var questionTypeIdx = FindHeaderIndex(headerIndex, "questiontype", "question_type", "type");
        var topicIdx = FindHeaderIndex(headerIndex, "topic", "subject");
        var skillIdx = FindHeaderIndex(headerIndex, "skill", "skills");
        var grammarTagsIdx = FindHeaderIndex(headerIndex, "grammartags", "grammar_tags", "grammar");
        var vocabularyTagsIdx = FindHeaderIndex(headerIndex, "vocabularytags", "vocabulary_tags", "vocabulary", "vocabtags", "vocab_tags");

        if (questionTextIdx < 0 || optionsIdx < 0 || correctAnswerIdx < 0 || levelIdx < 0)
        {
            return Result.Failure<ImportQuestionBankFromFileResponse>(
                HomeworkErrors.InvalidQuestionBankFile(
                    "Required columns: QuestionText, Options, CorrectAnswer, Level"));
        }

        var items = new List<QuestionBankItem>();
        var createdBy = userContext.UserId;
        var now = VietnamTime.UtcNow();

        foreach (var row in rows)
        {
            var rowNumber = row.RowNumber;
            var fields = row.Fields;
            if (fields.Length == 0 || fields.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var questionText = GetField(fields, questionTextIdx);
            var optionsRaw = GetField(fields, optionsIdx);
            var correctAnswerRaw = GetField(fields, correctAnswerIdx);
            var levelRaw = GetField(fields, levelIdx);
            var pointsRaw = pointsIdx >= 0 ? GetField(fields, pointsIdx) : null;
            var explanation = explanationIdx >= 0 ? GetField(fields, explanationIdx) : null;
            var questionTypeRaw = questionTypeIdx >= 0 ? GetField(fields, questionTypeIdx) : null;
            var topic = topicIdx >= 0 ? GetField(fields, topicIdx) : null;
            var skill = skillIdx >= 0 ? GetField(fields, skillIdx) : null;
            var grammarTags = grammarTagsIdx >= 0
                ? StringListJson.ParseTags(GetField(fields, grammarTagsIdx))
                : new List<string>();
            var vocabularyTags = vocabularyTagsIdx >= 0
                ? StringListJson.ParseTags(GetField(fields, vocabularyTagsIdx))
                : new List<string>();

            if (string.IsNullOrWhiteSpace(questionText))
            {
                return Result.Failure<ImportQuestionBankFromFileResponse>(
                    HomeworkErrors.InvalidQuestionBankRow(rowNumber, "QuestionText is required"));
            }

            if (!Enum.TryParse<QuestionLevel>(levelRaw, ignoreCase: true, out var level))
            {
                return Result.Failure<ImportQuestionBankFromFileResponse>(
                    HomeworkErrors.InvalidQuestionBankRow(rowNumber, "Invalid Level"));
            }

            HomeworkQuestionType questionType = HomeworkQuestionType.MultipleChoice;
            if (!string.IsNullOrWhiteSpace(questionTypeRaw))
            {
                if (!Enum.TryParse<HomeworkQuestionType>(questionTypeRaw, ignoreCase: true, out questionType))
                {
                    return Result.Failure<ImportQuestionBankFromFileResponse>(
                        HomeworkErrors.InvalidQuestionBankRow(rowNumber, "Invalid QuestionType"));
                }
            }

            var points = 1;
            if (!string.IsNullOrWhiteSpace(pointsRaw) &&
                (!int.TryParse(pointsRaw, out points) || points <= 0))
            {
                return Result.Failure<ImportQuestionBankFromFileResponse>(
                    HomeworkErrors.InvalidQuestionBankRow(rowNumber, "Invalid Points"));
            }

            List<string> options = new();
            string? correctAnswer = null;

            if (questionType == HomeworkQuestionType.MultipleChoice)
            {
                if (string.IsNullOrWhiteSpace(optionsRaw))
                {
                    return Result.Failure<ImportQuestionBankFromFileResponse>(
                        HomeworkErrors.InvalidQuestionBankRow(rowNumber, "Options are required"));
                }

                options = optionsRaw.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

                if (options.Count < 2)
                {
                    return Result.Failure<ImportQuestionBankFromFileResponse>(
                        HomeworkErrors.InvalidQuestionBankRow(rowNumber, "Options must have at least 2 values"));
                }

                if (string.IsNullOrWhiteSpace(correctAnswerRaw))
                {
                    return Result.Failure<ImportQuestionBankFromFileResponse>(
                        HomeworkErrors.InvalidQuestionBankRow(rowNumber, "CorrectAnswer is required"));
                }

                var normalizedCorrectAnswer = QuizOptionUtils.NormalizeCorrectAnswerForStorage(options, correctAnswerRaw);
                if (string.IsNullOrWhiteSpace(normalizedCorrectAnswer))
                {
                    return Result.Failure<ImportQuestionBankFromFileResponse>(
                        HomeworkErrors.InvalidQuestionBankRow(rowNumber, "Invalid CorrectAnswer"));
                }

                correctAnswer = normalizedCorrectAnswer;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(correctAnswerRaw))
                {
                    return Result.Failure<ImportQuestionBankFromFileResponse>(
                        HomeworkErrors.InvalidQuestionBankRow(rowNumber, "CorrectAnswer is required"));
                }

                correctAnswer = correctAnswerRaw.Trim();
            }

            items.Add(new QuestionBankItem
            {
                Id = Guid.NewGuid(),
                ProgramId = command.ProgramId,
                QuestionText = questionText,
                QuestionType = questionType,
                Options = questionType == HomeworkQuestionType.MultipleChoice
                    ? JsonSerializer.Serialize(options)
                    : null,
                CorrectAnswer = correctAnswer,
                Points = points,
                Explanation = string.IsNullOrWhiteSpace(explanation) ? null : explanation,
                Topic = string.IsNullOrWhiteSpace(topic) ? null : topic,
                Skill = string.IsNullOrWhiteSpace(skill) ? null : skill,
                GrammarTags = StringListJson.Serialize(grammarTags),
                VocabularyTags = StringListJson.Serialize(vocabularyTags),
                Level = level,
                CreatedBy = createdBy,
                CreatedAt = now
            });
        }

        if (items.Count == 0)
        {
            return Result.Failure<ImportQuestionBankFromFileResponse>(
                HomeworkErrors.InvalidQuestionBankFile("No valid rows found"));
        }

        context.QuestionBankItems.AddRange(items);
        await context.SaveChangesAsync(cancellationToken);

        return new ImportQuestionBankFromFileResponse
        {
            ImportedCount = items.Count
        };
    }

    private static TableResult ReadTable(Stream stream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".csv" => ReadCsv(stream),
            ".xlsx" or ".xls" => ReadExcel(stream),
            ".docx" => ReadDocx(stream),
            ".pdf" => ReadPdf(stream),
            _ => TableResult.Failure(HomeworkErrors.UnsupportedQuestionBankFileType(extension))
        };
    }

    private static TableResult ReadCsv(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        using var parser = new TextFieldParser(reader)
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true
        };
        parser.SetDelimiters(",");

        if (parser.EndOfData)
        {
            return TableResult.Failure(HomeworkErrors.InvalidQuestionBankFile("Empty CSV file"));
        }

        var headers = parser.ReadFields();
        if (headers == null || headers.Length == 0)
        {
            return TableResult.Failure(HomeworkErrors.InvalidQuestionBankFile("Missing header row"));
        }

        var rows = new List<RowData>();
        var rowNumber = 1;
        while (!parser.EndOfData)
        {
            rowNumber++;
            var fields = parser.ReadFields();
            if (fields == null)
            {
                continue;
            }
            rows.Add(new RowData(rowNumber, fields));
        }

        return TableResult.Success(headers, rows);
    }

    private static TableResult ReadExcel(Stream stream)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        try
        {
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var rows = new List<string[]>();

            do
            {
                while (reader.Read())
                {
                    var fields = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        fields[i] = reader.GetValue(i)?.ToString() ?? string.Empty;
                    }
                    rows.Add(fields);
                }

                break;
            } while (reader.NextResult());

            return BuildTableFromRows(rows, "Empty Excel file");
        }
        catch (HeaderException)
        {
            return TableResult.Failure(
                HomeworkErrors.InvalidQuestionBankFile(
                    "Invalid Excel file. Please upload a valid .xls or .xlsx file."));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return TableResult.Failure(
                HomeworkErrors.InvalidQuestionBankFile(
                    $"Unable to read Excel file: {ex.Message}"));
        }
    }

    private static TableResult ReadDocx(Stream stream)
    {
        using var doc = WordprocessingDocument.Open(stream, false);
        var table = doc.MainDocumentPart?.Document.Body?
            .Elements<Table>()
            .FirstOrDefault();

        if (table == null)
        {
            return TableResult.Failure(HomeworkErrors.InvalidQuestionBankFile("No table found in Word document"));
        }

        var rows = new List<string[]>();
        foreach (var row in table.Elements<TableRow>())
        {
            var cells = row.Elements<TableCell>()
                .Select(ExtractCellText)
                .ToArray();
            rows.Add(cells);
        }

        return BuildTableFromRows(rows, "Empty Word table");
    }

    private static TableResult ReadPdf(Stream stream)
    {
        using var document = PdfDocument.Open(stream);
        var text = string.Join("\n", document.GetPages().Select(p => p.Text));

        var lines = text.Split('\n')
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        if (lines.Count == 0)
        {
            return TableResult.Failure(HomeworkErrors.InvalidQuestionBankFile("Empty PDF file"));
        }

        var headerLine = lines[0];
        var delimiter = DetectDelimiter(headerLine);
        if (delimiter == '\0')
        {
            return TableResult.Failure(HomeworkErrors.InvalidQuestionBankFile(
                "PDF must have a header row with delimiter ',' or '|' or tab"));
        }

        var rows = new List<string[]>();
        foreach (var line in lines)
        {
            var fields = line.Split(delimiter).Select(s => s.Trim()).ToArray();
            rows.Add(fields);
        }

        return BuildTableFromRows(rows, "Empty PDF table");
    }

    private static TableResult BuildTableFromRows(List<string[]> rows, string emptyMessage)
    {
        if (rows.Count == 0)
        {
            return TableResult.Failure(HomeworkErrors.InvalidQuestionBankFile(emptyMessage));
        }

        var headers = rows[0].Select(h => h?.Trim() ?? string.Empty).ToArray();
        if (headers.Length == 0 || headers.All(string.IsNullOrWhiteSpace))
        {
            return TableResult.Failure(HomeworkErrors.InvalidQuestionBankFile("Missing header row"));
        }

        var dataRows = new List<RowData>();
        for (int i = 1; i < rows.Count; i++)
        {
            dataRows.Add(new RowData(i + 1, rows[i]));
        }

        return TableResult.Success(headers, dataRows);
    }

    private static char DetectDelimiter(string headerLine)
    {
        if (headerLine.Contains('|'))
        {
            return '|';
        }
        if (headerLine.Contains(','))
        {
            return ',';
        }
        if (headerLine.Contains('\t'))
        {
            return '\t';
        }
        return '\0';
    }

    private static string ExtractCellText(TableCell cell)
    {
        var texts = cell.Descendants<Text>().Select(t => t.Text);
        return string.Join("", texts).Trim();
    }

    private static Dictionary<string, int> BuildHeaderIndex(string[] headers)
    {
        var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Length; i++)
        {
            var key = headers[i].Trim();
            if (!dict.ContainsKey(key))
            {
                dict[key] = i;
            }
        }
        return dict;
    }

    private static int FindHeaderIndex(Dictionary<string, int> headerIndex, params string[] names)
    {
        foreach (var name in names)
        {
            if (headerIndex.TryGetValue(name, out var idx))
            {
                return idx;
            }
        }
        return -1;
    }

    private static string? GetField(string[] fields, int index)
    {
        if (index < 0 || index >= fields.Length)
        {
            return null;
        }
        return fields[index]?.Trim();
    }

    private sealed record RowData(int RowNumber, string[] Fields);

    private sealed class TableResult
    {
        private TableResult(string[]? headers, List<RowData>? rows, Error? error)
        {
            Headers = headers;
            Rows = rows;
            Error = error;
        }

        public string[]? Headers { get; }
        public List<RowData>? Rows { get; }
        public Error? Error { get; }

        public static TableResult Success(string[] headers, List<RowData> rows) => new(headers, rows, null);
        public static TableResult Failure(Error error) => new(null, null, error);
    }
}
