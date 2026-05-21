using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;

namespace Kidzgo.Application.Syllabuses.ReorderSyllabusSections;

public sealed class ReorderSyllabusSectionsCommandHandler(IDbContext context)
    : ICommandHandler<ReorderSyllabusSectionsCommand, SyllabusDocumentResponse>
{
    public async Task<Result<SyllabusDocumentResponse>> Handle(
        ReorderSyllabusSectionsCommand command,
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
            var orderMap = command.Orders.ToDictionary(x => x.SectionId, x => x.OrderIndex);
            if (orderMap.Count != command.Orders.Count)
            {
                return Result.Failure<SyllabusDocumentResponse>(
                    SyllabusErrors.InvalidTableLayout("Duplicate sectionId in reorder payload."));
            }

            var usedOrders = new HashSet<int>();
            var updated = new List<SyllabusDocumentSectionDto>(sections.Count);
            foreach (var section in sections)
            {
                if (!orderMap.TryGetValue(section.SectionId, out var orderIndex))
                {
                    return Result.Failure<SyllabusDocumentResponse>(SyllabusErrors.SectionNotFound(section.SectionId));
                }

                if (!usedOrders.Add(orderIndex))
                {
                    return Result.Failure<SyllabusDocumentResponse>(
                        SyllabusErrors.InvalidTableLayout($"Duplicate section orderIndex '{orderIndex}'."));
                }

                updated.Add(new SyllabusDocumentSectionDto
                {
                    SectionId = section.SectionId,
                    Type = section.Type,
                    Title = section.Title,
                    OrderIndex = orderIndex,
                    Editable = section.Editable,
                    Content = section.Content,
                    Items = section.Items,
                    Table = section.Table
                });
            }

            var warnings = SyllabusDocumentMapper.ReadWarnings(syllabus);
            SyllabusDocumentRules.MarkDocumentChanged(syllabus, updated, warnings);
            await context.SaveChangesAsync(cancellationToken);

            return SyllabusDocumentMapper.ToResponseFromSections(
                syllabus,
                updated,
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
