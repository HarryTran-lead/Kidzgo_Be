using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.GetSyllabusDocument;
using Kidzgo.Application.Syllabuses.ImportSyllabusFromWord;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.ImportSyllabusCommit;

public sealed class ImportSyllabusCommitCommandHandler(
    IDbContext context,
    ISender sender)
    : ICommandHandler<ImportSyllabusCommitCommand, SyllabusImportCommitResponse>
{
    public async Task<Result<SyllabusImportCommitResponse>> Handle(
        ImportSyllabusCommitCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            await SyllabusDocumentRules.EnsureUniqueActiveCodeAsync(
                context,
                command.ProgramId,
                command.LevelId,
                command.Code.Trim(),
                ignoreId: null,
                cancellationToken);
        }
        catch (SyllabusDocumentRuleException ex)
        {
            return Result.Failure<SyllabusImportCommitResponse>(ex.Error);
        }

        var version = $"doc-{VietnamTime.UtcNow():yyyyMMddHHmmssfff}";
        var importResult = await sender.Send(new ImportSyllabusFromWordCommand
        {
            BranchId = command.BranchId,
            ProgramId = command.ProgramId,
            LevelId = command.LevelId,
            Code = command.Code.Trim(),
            Version = version,
            OverwriteExisting = false,
            FileName = command.FileName,
            FileStream = command.FileStream
        }, cancellationToken);

        if (importResult.IsFailure)
        {
            return Result.Failure<SyllabusImportCommitResponse>(importResult.Error);
        }

        var syllabus = await context.Syllabuses
            .FirstOrDefaultAsync(x => x.Id == importResult.Value.SyllabusId, cancellationToken);

        if (syllabus is null)
        {
            return Result.Failure<SyllabusImportCommitResponse>(
                Kidzgo.Domain.LessonPlans.Errors.SyllabusErrors.NotFound(importResult.Value.SyllabusId));
        }

        if (!string.IsNullOrWhiteSpace(command.Title))
        {
            syllabus.Title = command.Title.Trim();
        }

        if (command.Edition != null)
        {
            syllabus.Edition = string.IsNullOrWhiteSpace(command.Edition) ? null : command.Edition.Trim();
        }

        syllabus.DocumentStatus = command.AsDraft ? SyllabusDocumentStatuses.Draft : SyllabusDocumentStatuses.Published;
        syllabus.SourceType = SyllabusDocumentSourceTypes.Imported;
        syllabus.ParserVersion ??= SyllabusImportFileMetadata.ResolveParserVersion(command.FileName);
        syllabus.UpdatedAt = VietnamTime.UtcNow();
        await context.SaveChangesAsync(cancellationToken);

        var documentResult = await sender.Send(new GetSyllabusDocumentQuery { Id = syllabus.Id }, cancellationToken);
        return documentResult.IsFailure
            ? Result.Failure<SyllabusImportCommitResponse>(documentResult.Error)
            : Result.Success(new SyllabusImportCommitResponse
            {
                Document = documentResult.Value
            });
    }
}
