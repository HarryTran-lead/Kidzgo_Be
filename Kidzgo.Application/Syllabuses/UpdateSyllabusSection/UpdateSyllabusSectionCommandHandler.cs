using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;

namespace Kidzgo.Application.Syllabuses.UpdateSyllabusSection;

public sealed class UpdateSyllabusSectionCommandHandler(IDbContext context)
    : ICommandHandler<UpdateSyllabusSectionCommand, SyllabusDocumentResponse>
{
    public async Task<Result<SyllabusDocumentResponse>> Handle(
        UpdateSyllabusSectionCommand command,
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
            var index = sections.FindIndex(x => x.SectionId == command.SectionId);
            if (index < 0)
            {
                return Result.Failure<SyllabusDocumentResponse>(SyllabusErrors.SectionNotFound(command.SectionId));
            }

            var current = sections[index];
            if (current.Type == SyllabusDocumentSectionTypes.Table)
            {
                return Result.Failure<SyllabusDocumentResponse>(
                    SyllabusErrors.InvalidTableLayout("Use row or cell endpoints to edit table sections."));
            }

            sections[index] = new SyllabusDocumentSectionDto
            {
                SectionId = current.SectionId,
                Type = current.Type,
                Title = command.Title ?? current.Title,
                OrderIndex = current.OrderIndex,
                Editable = current.Editable,
                Content = command.Content ?? current.Content,
                Items = command.Items ?? current.Items,
                Table = current.Table
            };

            var warnings = SyllabusDocumentMapper.ReadWarnings(syllabus);
            SyllabusDocumentRules.MarkDocumentChanged(syllabus, sections, warnings);
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
