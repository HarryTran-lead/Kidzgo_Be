using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;

namespace Kidzgo.Application.Syllabuses.UpdateSyllabusTableCell;

public sealed class UpdateSyllabusTableCellCommandHandler(IDbContext context)
    : ICommandHandler<UpdateSyllabusTableCellCommand, SyllabusDocumentResponse>
{
    public async Task<Result<SyllabusDocumentResponse>> Handle(
        UpdateSyllabusTableCellCommand command,
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
            var sectionIndex = sections.FindIndex(x => x.SectionId == command.SectionId);
            if (sectionIndex < 0)
            {
                return Result.Failure<SyllabusDocumentResponse>(SyllabusErrors.SectionNotFound(command.SectionId));
            }

            var section = sections[sectionIndex];
            if (section.Type != SyllabusDocumentSectionTypes.Table || section.Table is null)
            {
                return Result.Failure<SyllabusDocumentResponse>(
                    SyllabusErrors.InvalidTableLayout("Target section is not a table section."));
            }

            var rows = section.Table.Rows.ToList();
            var rowIndex = rows.FindIndex(x => x.RowId == command.RowId);
            if (rowIndex < 0)
            {
                return Result.Failure<SyllabusDocumentResponse>(SyllabusErrors.RowNotFound(command.RowId));
            }

            var row = rows[rowIndex];
            var cells = row.Cells.ToList();
            var cellIndex = cells.FindIndex(x => string.Equals(x.ColumnKey, command.ColumnKey, StringComparison.OrdinalIgnoreCase));
            if (cellIndex < 0)
            {
                return Result.Failure<SyllabusDocumentResponse>(SyllabusErrors.CellNotFound(command.ColumnKey));
            }

            var currentCell = cells[cellIndex];
            cells[cellIndex] = new SyllabusDocumentTableCellDto
            {
                ColumnKey = currentCell.ColumnKey,
                Value = command.Value ?? currentCell.Value,
                RowSpan = command.RowSpan ?? currentCell.RowSpan,
                ColSpan = command.ColSpan ?? currentCell.ColSpan,
                Align = command.Align ?? currentCell.Align,
                Bold = command.Bold ?? currentCell.Bold
            };

            rows[rowIndex] = new SyllabusDocumentTableRowDto
            {
                RowId = row.RowId,
                OrderIndex = row.OrderIndex,
                Group = row.Group,
                Cells = cells
            };

            var updatedSection = new SyllabusDocumentSectionDto
            {
                SectionId = section.SectionId,
                Type = section.Type,
                Title = section.Title,
                OrderIndex = section.OrderIndex,
                Editable = section.Editable,
                Content = section.Content,
                Items = section.Items,
                Table = new SyllabusDocumentTableDto
                {
                    Columns = section.Table.Columns,
                    Rows = rows
                }
            };

            SyllabusDocumentRules.ValidateTableLayout(updatedSection);
            sections[sectionIndex] = updatedSection;
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
