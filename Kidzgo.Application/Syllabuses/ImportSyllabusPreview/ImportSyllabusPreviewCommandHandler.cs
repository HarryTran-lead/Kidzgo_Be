using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.ImportSyllabusPreview;

public sealed class ImportSyllabusPreviewCommandHandler(IDbContext context)
    : ICommandHandler<ImportSyllabusPreviewCommand, SyllabusImportPreviewResponse>
{
    public async Task<Result<SyllabusImportPreviewResponse>> Handle(
        ImportSyllabusPreviewCommand command,
        CancellationToken cancellationToken)
    {
        var level = await context.Levels
            .AsNoTracking()
            .Where(x => x.Id == command.LevelId && x.IsActive)
            .Select(x => new { x.ProgramId })
            .FirstOrDefaultAsync(cancellationToken);

        if (level is null)
        {
            return Result.Failure<SyllabusImportPreviewResponse>(SyllabusErrors.LevelNotFound(command.LevelId));
        }

        if (level.ProgramId != command.ProgramId)
        {
            return Result.Failure<SyllabusImportPreviewResponse>(
                SyllabusErrors.LevelDoesNotBelongToProgram(command.LevelId, command.ProgramId));
        }

        var parsed = CurriculumWordImportParser.ParseSyllabusFile(command.FileStream, command.FileName);
        if (parsed.IsFailure)
        {
            return Result.Failure<SyllabusImportPreviewResponse>(SyllabusErrors.ImportParseFailed(parsed.Error.Description));
        }

        var (sections, warnings, totalPeriods, totalLessons) = SyllabusDocumentMapper.BuildFromParsedImport(parsed.Value);
        var syllabus = new Syllabus
        {
            Id = Guid.Empty,
            ProgramId = command.ProgramId,
            LevelId = command.LevelId,
            Code = Path.GetFileNameWithoutExtension(command.FileName).ToUpperInvariant(),
            Title = parsed.Value.Title,
            Edition = parsed.Value.Edition,
            DocumentStatus = SyllabusDocumentStatuses.Draft,
            SourceType = SyllabusDocumentSourceTypes.Imported,
            SourceFileName = command.FileName,
            ParserVersion = GetParserVersion(command.FileName),
            DocumentVersion = 1,
            MinutesPerPeriod = 45
        };

        var document = SyllabusDocumentMapper.ToResponse(
            syllabus,
            sections,
            warnings,
            totalUnits: parsed.Value.Units.Count,
            totalSessions: totalLessons,
            totalLessons: totalLessons,
            totalPeriods: totalPeriods);

        return Result.Success(new SyllabusImportPreviewResponse
        {
            Document = document,
            Warnings = warnings
        });
    }

    private static string GetParserVersion(string fileName)
    {
        return string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase)
            ? "pdf-v1"
            : "docx-v1";
    }
}
