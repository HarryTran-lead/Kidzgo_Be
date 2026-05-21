using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.UpdateSyllabusDocumentMetadata;

public sealed class UpdateSyllabusDocumentMetadataCommandHandler(IDbContext context)
    : ICommandHandler<UpdateSyllabusDocumentMetadataCommand, SyllabusDocumentResponse>
{
    public async Task<Result<SyllabusDocumentResponse>> Handle(
        UpdateSyllabusDocumentMetadataCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var syllabusResult = await SyllabusDocumentRules.GetTrackedSyllabusAsync(context, command.Id, cancellationToken);
            if (syllabusResult.IsFailure)
            {
                return Result.Failure<SyllabusDocumentResponse>(syllabusResult.Error);
            }

            var syllabus = syllabusResult.Value;
            SyllabusDocumentRules.EnsureExpectedVersion(syllabus, command.ExpectedVersion);
            SyllabusDocumentRules.EnsureDraftEditable(syllabus);
            await SyllabusDocumentRules.EnsureUniqueActiveCodeAsync(
                context,
                syllabus.ProgramId,
                syllabus.LevelId,
                command.Code.Trim(),
                syllabus.Id,
                cancellationToken);

            syllabus.Code = command.Code.Trim();
            syllabus.Title = command.Title.Trim();
            syllabus.Edition = string.IsNullOrWhiteSpace(command.Edition) ? null : command.Edition.Trim();
            syllabus.MinutesPerPeriod = command.MinutesPerPeriod;
            syllabus.DocumentVersion = Math.Max(1, syllabus.DocumentVersion) + 1;
            syllabus.UpdatedAt = VietnamTime.UtcNow();

            await context.SaveChangesAsync(cancellationToken);

            var sections = SyllabusDocumentMapper.ReadSections(syllabus);
            var warnings = SyllabusDocumentMapper.ReadWarnings(syllabus);
            return SyllabusDocumentMapper.ToResponseFromSections(
                syllabus,
                sections,
                warnings,
                fallbackTotalLessons: syllabus.TotalLessons ?? 0,
                fallbackTotalPeriods: syllabus.TotalPeriods ?? 0);
        }
        catch (SyllabusDocumentRuleException ex)
        {
            return Result.Failure<SyllabusDocumentResponse>(ex.Error);
        }
    }
}
