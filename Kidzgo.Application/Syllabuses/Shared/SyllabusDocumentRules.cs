using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.Shared;

internal static class SyllabusDocumentRules
{
    public static async Task<Result<Syllabus>> GetTrackedSyllabusAsync(
        IDbContext context,
        Guid syllabusId,
        CancellationToken cancellationToken)
    {
        var syllabus = await context.Syllabuses
            .FirstOrDefaultAsync(x => x.Id == syllabusId && !x.IsDeleted, cancellationToken);

        return syllabus is null
            ? Result.Failure<Syllabus>(SyllabusErrors.NotFound(syllabusId))
            : Result.Success(syllabus);
    }

    public static void EnsureExpectedVersion(Syllabus syllabus, int expectedVersion)
    {
        var currentVersion = syllabus.DocumentVersion <= 0 ? 1 : syllabus.DocumentVersion;
        if (currentVersion != expectedVersion)
        {
            throw new SyllabusDocumentRuleException(SyllabusErrors.VersionConflict(expectedVersion, currentVersion));
        }
    }

    public static void EnsureDraftEditable(Syllabus syllabus)
    {
        if (SyllabusDocumentMapper.NormalizeStatus(syllabus.DocumentStatus) != SyllabusDocumentStatuses.Draft)
        {
            throw new SyllabusDocumentRuleException(SyllabusErrors.PublishedReadOnly);
        }
    }

    public static async Task EnsureUniqueActiveCodeAsync(
        IDbContext context,
        Guid programId,
        Guid levelId,
        string code,
        Guid? ignoreId,
        CancellationToken cancellationToken)
    {
        var exists = await context.Syllabuses.AnyAsync(
            x => x.ProgramId == programId &&
                 x.LevelId == levelId &&
                 x.Code == code &&
                 !x.IsDeleted &&
                 x.Id != ignoreId &&
                 x.DocumentStatus != SyllabusDocumentStatuses.Archived,
            cancellationToken);

        if (exists)
        {
            throw new SyllabusDocumentRuleException(SyllabusErrors.DuplicateCode(programId, levelId, code));
        }
    }

    public static void MarkDocumentChanged(
        Syllabus syllabus,
        IReadOnlyList<SyllabusDocumentSectionDto> sections,
        IReadOnlyList<SyllabusDocumentWarningDto> warnings,
        bool setHybridSourceType = true)
    {
        syllabus.SectionsJson = SyllabusDocumentMapper.WriteSections(sections);
        syllabus.WarningsJson = SyllabusDocumentMapper.WriteWarnings(warnings);
        syllabus.DocumentVersion = Math.Max(1, syllabus.DocumentVersion) + 1;
        syllabus.UpdatedAt = VietnamTime.UtcNow();
        if (setHybridSourceType &&
            SyllabusDocumentMapper.NormalizeSourceType(syllabus.SourceType) == SyllabusDocumentSourceTypes.Imported)
        {
            syllabus.SourceType = SyllabusDocumentSourceTypes.Hybrid;
        }
    }

    public static void ValidateTableLayout(SyllabusDocumentSectionDto section)
    {
        if (section.Type != SyllabusDocumentSectionTypes.Table || section.Table is null)
        {
            return;
        }

        var columnKeys = section.Table.Columns.Select(x => x.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var orderIndexes = new HashSet<int>();
        foreach (var row in section.Table.Rows)
        {
            if (!orderIndexes.Add(row.OrderIndex))
            {
                throw new SyllabusDocumentRuleException(
                    SyllabusErrors.InvalidTableLayout($"Duplicate row orderIndex '{row.OrderIndex}' in section '{section.SectionId}'."));
            }

            foreach (var cell in row.Cells)
            {
                if (!columnKeys.Contains(cell.ColumnKey))
                {
                    throw new SyllabusDocumentRuleException(
                        SyllabusErrors.InvalidTableLayout($"Unknown column key '{cell.ColumnKey}' in section '{section.SectionId}'."));
                }

                if (cell.RowSpan <= 0 || cell.ColSpan <= 0)
                {
                    throw new SyllabusDocumentRuleException(
                        SyllabusErrors.InvalidTableLayout("rowSpan and colSpan must be greater than 0."));
                }
            }
        }
    }

    public static void ValidatePublishable(IReadOnlyList<SyllabusDocumentSectionDto> sections)
    {
        var hasValidTable = sections.Any(x =>
            x.Type == SyllabusDocumentSectionTypes.Table &&
            x.Table is not null &&
            x.Table.Rows.Count > 0 &&
            x.Table.Columns.Count > 0);

        if (!hasValidTable)
        {
            throw new SyllabusDocumentRuleException(
                SyllabusErrors.PublishValidationFailed("At least one valid curriculum table is required before publishing."));
        }
    }
}

internal sealed class SyllabusDocumentRuleException(Error error) : Exception(error.Description)
{
    public Error Error { get; } = error;
}
