using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Syllabuses.PublishSyllabusDocument;

public sealed class PublishSyllabusDocumentCommandHandler(IDbContext context)
    : ICommandHandler<PublishSyllabusDocumentCommand, SyllabusDocumentResponse>
{
    public async Task<Result<SyllabusDocumentResponse>> Handle(
        PublishSyllabusDocumentCommand command,
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

            var sections = SyllabusDocumentMapper.ReadSections(syllabus).ToList();
            var warnings = SyllabusDocumentMapper.ReadWarnings(syllabus);
            SyllabusDocumentRules.ValidatePublishable(sections);

            syllabus.DocumentStatus = SyllabusDocumentStatuses.Published;
            syllabus.DocumentVersion = Math.Max(1, syllabus.DocumentVersion) + 1;
            syllabus.UpdatedAt = VietnamTime.UtcNow();

            await context.SaveChangesAsync(cancellationToken);

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
