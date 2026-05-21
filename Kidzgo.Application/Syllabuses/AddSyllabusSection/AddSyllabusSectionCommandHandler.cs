using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Syllabuses.AddSyllabusSection;

public sealed class AddSyllabusSectionCommandHandler(IDbContext context)
    : ICommandHandler<AddSyllabusSectionCommand, SyllabusDocumentResponse>
{
    public async Task<Result<SyllabusDocumentResponse>> Handle(
        AddSyllabusSectionCommand command,
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
            if (sections.Any(x => x.OrderIndex == command.Section.OrderIndex))
            {
                return Result.Failure<SyllabusDocumentResponse>(
                    Kidzgo.Domain.LessonPlans.Errors.SyllabusErrors.InvalidTableLayout(
                        $"Section orderIndex '{command.Section.OrderIndex}' already exists."));
            }

            var section = new SyllabusDocumentSectionDto
            {
                SectionId = command.Section.SectionId == Guid.Empty ? Guid.NewGuid() : command.Section.SectionId,
                Type = command.Section.Type,
                Title = command.Section.Title,
                OrderIndex = command.Section.OrderIndex,
                Editable = true,
                Content = command.Section.Content,
                Items = command.Section.Items,
                Table = command.Section.Table
            };

            SyllabusDocumentRules.ValidateTableLayout(section);
            sections.Add(section);
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
