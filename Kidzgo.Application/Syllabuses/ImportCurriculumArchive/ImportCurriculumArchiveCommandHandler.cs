using System.IO.Compression;
using System.Text.RegularExpressions;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Application.Syllabuses.GetSyllabusDocument;
using Kidzgo.Application.LessonPlanTemplates.ImportLessonPlanTemplateFromWord;
using Kidzgo.Application.Syllabuses.ImportSyllabusFromWord;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.ImportCurriculumArchive;

public sealed class ImportCurriculumArchiveCommandHandler(
    IDbContext context,
    ISender sender,
    IFileStorageService fileStorage)
    : ICommandHandler<ImportCurriculumArchiveCommand, ImportCurriculumArchiveResponse>
{
    public async Task<Result<ImportCurriculumArchiveResponse>> Handle(
        ImportCurriculumArchiveCommand command,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(command.FileName).ToLowerInvariant();
        if (extension != ".zip")
        {
            return Result.Failure<ImportCurriculumArchiveResponse>(
                SyllabusErrors.UnsupportedImportFileType(extension));
        }

        var modules = await context.Modules
            .Where(x => x.LevelId == command.LevelId && x.IsActive)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);

        var importConfiguration = await context.CurriculumImportConfigurations
            .AsNoTracking()
            .Include(x => x.ModuleRules)
            .FirstOrDefaultAsync(
                x => x.ProgramId == command.ProgramId &&
                     x.LevelId == command.LevelId &&
                     x.IsActive,
                cancellationToken);
        if (importConfiguration is null)
        {
            return Result.Failure<ImportCurriculumArchiveResponse>(
                SyllabusErrors.ImportConfigurationNotFound(command.ProgramId, command.LevelId));
        }

        using var archive = new ZipArchive(command.FileStream, ZipArchiveMode.Read, leaveOpen: true);
        var importEntries = archive.Entries
            .Where(x => CurriculumArchiveImportEntryRules.IsSupportedImportEntry(x.FullName))
            .ToList();

        var syllabusEntries = importEntries
            .Where(x => CurriculumArchiveImportEntryRules.IsSyllabusEntry(x.FullName))
            .OrderByDescending(x => CurriculumArchiveImportEntryRules.GetSyllabusEntryPriority(x.FullName))
            .ToList();

        var lessonPlanEntries = importEntries
            .Where(x => !CurriculumArchiveImportEntryRules.IsSyllabusEntry(x.FullName) &&
                        x.FullName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            .ToList();

        ApplyArchiveLessonPlanCounts(importConfiguration, lessonPlanEntries);

        Guid? syllabusId = null;
        var importedLessonPlans = 0;
        var importedEntries = new List<ImportedCurriculumArchiveEntryDto>();
        var skipped = new List<string>();
        var skippedItems = new List<SkippedCurriculumArchiveEntryDto>();
        Error? syllabusImportError = null;
        ZipArchiveEntry? selectedSyllabusEntry = null;
        string? selectedSyllabusParserVersion = null;
        string? selectedSyllabusNormalizedEntryName = null;

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        foreach (var entry in syllabusEntries)
        {
            await using var entryStream = entry.Open();
            await using var buffer = new MemoryStream();
            await entryStream.CopyToAsync(buffer, cancellationToken);
            var entryBytes = buffer.ToArray();

            await using var importStream = new MemoryStream(entryBytes, writable: false);
            var syllabusResult = await sender.Send(
                new ImportSyllabusFromWordCommand
                {
                    BranchId = command.BranchId,
                    ProgramId = command.ProgramId,
                    LevelId = command.LevelId,
                    Code = command.Code,
                    Version = command.Version,
                    OverwriteExisting = command.OverwriteExisting,
                    FileName = Path.GetFileName(entry.FullName),
                    FileStream = importStream
                },
                cancellationToken);

            if (syllabusResult.IsFailure)
            {
                syllabusImportError = syllabusResult.Error;
                AddSkippedEntry(
                    skipped,
                    skippedItems,
                    entry.FullName,
                    syllabusResult.Error.Description,
                    SyllabusImportFileMetadata.ResolveParserVersion(entry.FullName));
                continue;
            }

            syllabusId = syllabusResult.Value.SyllabusId;
            selectedSyllabusEntry = entry;
            selectedSyllabusParserVersion = SyllabusImportFileMetadata.ResolveParserVersion(entry.FullName);
            selectedSyllabusNormalizedEntryName = CurriculumArchiveImportEntryRules.NormalizeArchivePath(entry.FullName);

            var validationResult = await ValidateImportedSyllabusAsync(
                syllabusResult.Value.SyllabusId,
                syllabusResult.Value.ImportedLessons,
                entry.FullName,
                cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<ImportCurriculumArchiveResponse>(validationResult.Error);
            }

            await using var uploadStream = new MemoryStream(entryBytes, writable: false);
            await AttachOriginalSyllabusFileAsync(
                fileStorage,
                syllabusResult.Value.SyllabusId,
                entry,
                uploadStream,
                cancellationToken);
            importedEntries.Add(new ImportedCurriculumArchiveEntryDto
            {
                EntryName = entry.FullName,
                NormalizedEntryName = CurriculumArchiveImportEntryRules.NormalizeArchivePath(entry.FullName),
                FileName = Path.GetFileName(entry.FullName),
                SourceFolder = ResolveSourceFolder(entry.FullName),
                SourceType = CurriculumArchiveImportEntryRules.ResolveSourceType(entry.FullName),
                ParserVersion = selectedSyllabusParserVersion,
                IsPrimarySyllabusSource = true,
                Created = true,
                Title = Path.GetFileNameWithoutExtension(entry.FullName)
            });
            break;
        }

        if (syllabusId is null)
        {
            if (syllabusImportError is not null)
            {
                return Result.Failure<ImportCurriculumArchiveResponse>(syllabusImportError);
            }

            return Result.Failure<ImportCurriculumArchiveResponse>(
                SyllabusErrors.InvalidImportFile("No syllabus Excel or Word document was found in the archive."));
        }

        foreach (var entry in syllabusEntries)
        {
            if (selectedSyllabusEntry is not null &&
                !string.Equals(entry.FullName, selectedSyllabusEntry.FullName, StringComparison.OrdinalIgnoreCase) &&
                skippedItems.All(x => !string.Equals(x.EntryName, entry.FullName, StringComparison.OrdinalIgnoreCase)))
            {
                AddSkippedEntry(
                    skipped,
                    skippedItems,
                    entry.FullName,
                    "Skipped because another syllabus source was selected with higher priority.",
                    SyllabusImportFileMetadata.ResolveParserVersion(entry.FullName));
            }
        }

        var resolvedLessonEntries = ResolveLessonPlanEntries(
            modules,
            importConfiguration,
            lessonPlanEntries,
            skipped,
            skippedItems);

        foreach (var resolvedEntry in resolvedLessonEntries)
        {
            await using var entryStream = resolvedEntry.Entry.Open();
            await using var buffer = new MemoryStream();
            await entryStream.CopyToAsync(buffer, cancellationToken);
            var entryBytes = buffer.ToArray();

            await using var importStream = new MemoryStream(entryBytes, writable: false);
            var lessonResult = await sender.Send(
                new ImportLessonPlanTemplateFromWordCommand
                {
                    SyllabusId = syllabusId.Value,
                    ModuleId = resolvedEntry.Module.Id,
                    SessionIndexOverride = resolvedEntry.SessionIndex,
                    OverwriteExisting = command.OverwriteExisting,
                    FileName = resolvedEntry.FileName,
                    FileStream = importStream
                },
                cancellationToken);

            if (lessonResult.IsFailure)
            {
                AddSkippedEntry(
                    skipped,
                    skippedItems,
                    resolvedEntry.Entry.FullName,
                    lessonResult.Error.Description,
                    resolvedEntry.ParserVersion);
                continue;
            }

            await using var uploadStream = new MemoryStream(entryBytes, writable: false);
            await AttachOriginalLessonPlanFileAsync(
                fileStorage,
                lessonResult.Value.LessonPlanTemplateId,
                resolvedEntry.Entry,
                uploadStream,
                cancellationToken);
            importedLessonPlans++;
            importedEntries.Add(new ImportedCurriculumArchiveEntryDto
            {
                EntryName = resolvedEntry.Entry.FullName,
                NormalizedEntryName = resolvedEntry.NormalizedEntryName,
                FileName = resolvedEntry.FileName,
                SourceFolder = ResolveSourceFolder(resolvedEntry.Entry.FullName),
                SourceType = resolvedEntry.SourceType,
                ParserVersion = resolvedEntry.ParserVersion,
                ModuleId = resolvedEntry.Module.Id,
                ModuleName = resolvedEntry.Module.Name,
                LessonPlanTemplateId = lessonResult.Value.LessonPlanTemplateId,
                SessionTemplateId = lessonResult.Value.SessionTemplateId,
                SessionIndex = lessonResult.Value.SessionIndex,
                SessionOrder = lessonResult.Value.SessionIndex,
                Created = lessonResult.Value.Created,
                Title = lessonResult.Value.Title
            });
        }

        await transaction.CommitAsync(cancellationToken);

        return new ImportCurriculumArchiveResponse
        {
            ArchiveFileName = command.FileName,
            ArchiveParserVersion = "zip-v1",
            SyllabusId = syllabusId,
            SelectedSyllabusEntryName = selectedSyllabusEntry?.FullName,
            SelectedSyllabusNormalizedEntryName = selectedSyllabusNormalizedEntryName,
            SelectedSyllabusFileName = selectedSyllabusEntry is null ? null : Path.GetFileName(selectedSyllabusEntry.FullName),
            SelectedSyllabusSourceType = selectedSyllabusEntry is null ? null : CurriculumArchiveImportEntryRules.ResolveSourceType(selectedSyllabusEntry.FullName),
            SelectedSyllabusParserVersion = selectedSyllabusParserVersion,
            ImportedLessonPlans = importedLessonPlans,
            SkippedFiles = skipped.Count,
            ImportedEntries = importedEntries,
            SkippedEntries = skipped,
            SkippedItems = skippedItems
        };
    }

    private static Domain.Programs.Module? ResolveModuleForEntry(
        IReadOnlyList<Domain.Programs.Module> modules,
        string fullName)
    {
        var normalized = fullName.Replace('\\', '/');
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var folderName = segments.Length > 1 ? segments[^2] : normalized;
        var fileName = Path.GetFileNameWithoutExtension(normalized);
        return modules.FirstOrDefault(x =>
            x.Name.Contains(folderName, StringComparison.OrdinalIgnoreCase) ||
            x.Code.Contains(folderName, StringComparison.OrdinalIgnoreCase) ||
            x.Name.Contains(fileName, StringComparison.OrdinalIgnoreCase) ||
            x.Code.Contains(fileName, StringComparison.OrdinalIgnoreCase));
    }

    private static int? ResolveSessionIndex(
        Domain.LessonPlans.CurriculumImportConfiguration configuration,
        Guid moduleId,
        params string?[] hints)
    {
        var rule = configuration.ModuleRules.FirstOrDefault(x => x.ModuleId == moduleId);
        return rule is null
            ? null
            : CurriculumImportRuleResolver.ResolveSessionIndex(configuration, rule, hints);
    }

    private static IReadOnlyList<ResolvedLessonPlanArchiveEntry> ResolveLessonPlanEntries(
        IReadOnlyList<Domain.Programs.Module> modules,
        Domain.LessonPlans.CurriculumImportConfiguration configuration,
        IReadOnlyList<ZipArchiveEntry> lessonPlanEntries,
        ICollection<string> skipped,
        ICollection<SkippedCurriculumArchiveEntryDto> skippedItems)
    {
        var orderedRules = configuration.ModuleRules.OrderBy(x => x.OrderIndex).ToList();
        var candidates = new List<ResolvedLessonPlanArchiveEntry>();

        foreach (var entry in lessonPlanEntries)
        {
            var module = ResolveModuleForEntry(modules, entry.FullName) ??
                         CurriculumImportRuleResolver.Resolve(
                             modules,
                             orderedRules,
                             Path.GetDirectoryName(entry.FullName),
                             Path.GetFileNameWithoutExtension(entry.FullName),
                             entry.FullName);
            if (module is null)
            {
                AddSkippedEntry(
                    skipped,
                    skippedItems,
                    entry.FullName,
                    "Could not map entry to a module.",
                    SyllabusImportFileMetadata.ResolveParserVersion(entry.FullName));
                continue;
            }

            var sessionIndexOverride = ResolveSessionIndex(
                configuration,
                module.Id,
                Path.GetFileNameWithoutExtension(entry.FullName),
                entry.FullName,
                Path.GetDirectoryName(entry.FullName));
            if (!sessionIndexOverride.HasValue)
            {
                AddSkippedEntry(
                    skipped,
                    skippedItems,
                    entry.FullName,
                    "Could not resolve session index from import configuration",
                    SyllabusImportFileMetadata.ResolveParserVersion(entry.FullName));
                continue;
            }

            candidates.Add(new ResolvedLessonPlanArchiveEntry(
                entry,
                module,
                sessionIndexOverride.Value,
                CurriculumArchiveImportEntryRules.ResolveSourceType(entry.FullName),
                SyllabusImportFileMetadata.ResolveParserVersion(entry.FullName)));
        }

        var selectedEntries = new List<ResolvedLessonPlanArchiveEntry>();
        foreach (var group in candidates.GroupBy(x => new { x.Module.Id, x.SessionIndex }))
        {
            var preferred = group
                .OrderByDescending(x => CurriculumArchiveImportEntryRules.GetLessonEntryPriority(x.Entry.FullName))
                .ThenBy(x => x.NormalizedEntryName.Length)
                .ThenBy(x => x.FileName.Length)
                .ThenBy(x => x.Entry.FullName, StringComparer.OrdinalIgnoreCase)
                .First();

            selectedEntries.Add(preferred);

            foreach (var duplicate in group.Where(x =>
                         !string.Equals(x.Entry.FullName, preferred.Entry.FullName, StringComparison.OrdinalIgnoreCase)))
            {
                AddSkippedEntry(
                    skipped,
                    skippedItems,
                    duplicate.Entry.FullName,
                    $"Skipped duplicate archive entry for module '{preferred.Module.Name}' session {preferred.SessionIndex}. Preferred entry: {preferred.Entry.FullName}",
                    duplicate.ParserVersion);
            }
        }

        return selectedEntries
            .OrderBy(x => x.Module.Order)
            .ThenBy(x => x.SessionIndex)
            .ToList();
    }

    private static void ApplyArchiveLessonPlanCounts(
        Domain.LessonPlans.CurriculumImportConfiguration configuration,
        IEnumerable<ZipArchiveEntry> lessonPlanEntries)
    {
        var starterLessonCount = 0;
        var regularUnitLessonCount = 0;
        var revisionLessonCount = 0;
        var revisionFileCounts = new Dictionary<int, int>();

        foreach (var entry in lessonPlanEntries)
        {
            var text = CurriculumArchiveImportEntryRules.NormalizeArchivePath(entry.FullName);
            var lessonIndex = ExtractLessonIndex(text) ?? 1;

            if (Regex.IsMatch(text, @"\bUNIT\s*STARTER\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                starterLessonCount = Math.Max(starterLessonCount, lessonIndex);
                continue;
            }

            if (Regex.IsMatch(text, @"\bUNIT\s*0*\d+\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                regularUnitLessonCount = Math.Max(regularUnitLessonCount, lessonIndex);
                continue;
            }

            var revisionMatch = Regex.Match(
                text,
                @"\bREVISION\s*0*(\d+)\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (revisionMatch.Success && int.TryParse(revisionMatch.Groups[1].Value, out var revisionNumber))
            {
                revisionFileCounts[revisionNumber] = Math.Max(
                    revisionFileCounts.GetValueOrDefault(revisionNumber),
                    lessonIndex);
            }
        }

        if (starterLessonCount > configuration.StarterUnitLessonPlanCount)
        {
            configuration.StarterUnitLessonPlanCount = starterLessonCount;
        }

        if (regularUnitLessonCount > configuration.RegularUnitLessonPlanCount)
        {
            configuration.RegularUnitLessonPlanCount = regularUnitLessonCount;
        }

        if (revisionFileCounts.Count > 0)
        {
            revisionLessonCount = revisionFileCounts.Values.Max();
        }

        if (revisionLessonCount > configuration.RevisionLessonPlanCount)
        {
            configuration.RevisionLessonPlanCount = revisionLessonCount;
        }
    }

    private static int? ExtractLessonIndex(string text)
    {
        var match = Regex.Match(
            text,
            @"\bLESSON\s*0*(\d+)\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        return match.Success && int.TryParse(match.Groups[1].Value, out var lessonIndex)
            ? lessonIndex
            : null;
    }

    private static void AddSkippedEntry(
        ICollection<string> skipped,
        ICollection<SkippedCurriculumArchiveEntryDto> skippedItems,
        string entryName,
        string reason,
        string? parserVersion = null)
    {
        skipped.Add($"{entryName}: {reason}");
        skippedItems.Add(new SkippedCurriculumArchiveEntryDto
        {
            EntryName = entryName,
            NormalizedEntryName = CurriculumArchiveImportEntryRules.NormalizeArchivePath(entryName),
            FileName = Path.GetFileName(entryName),
            SourceFolder = ResolveSourceFolder(entryName),
            SourceType = CurriculumArchiveImportEntryRules.ResolveSourceType(entryName),
            ParserVersion = parserVersion,
            Reason = reason
        });
    }

    private static string? ResolveSourceFolder(string fullName)
    {
        var normalized = fullName.Replace('\\', '/');
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
        {
            return null;
        }

        return segments[^2];
    }

    private async Task AttachOriginalSyllabusFileAsync(
        IFileStorageService fileStorage,
        Guid syllabusId,
        ZipArchiveEntry entry,
        MemoryStream fileStream,
        CancellationToken cancellationToken)
    {
        var syllabus = await context.Syllabuses.FirstOrDefaultAsync(x => x.Id == syllabusId, cancellationToken);
        if (syllabus is null)
        {
            return;
        }

        var extension = Path.GetExtension(entry.FullName).ToLowerInvariant();
        var resourceType = extension is ".xlsx" or ".xls" ? "excel" : "document";
        var folder = $"curriculum/{syllabus.ProgramId}/{syllabus.LevelId}/syllabuses";
        var url = await fileStorage.UploadFileAsync(
            fileStream,
            Path.GetFileName(entry.FullName),
            folder,
            resourceType,
            cancellationToken);

        syllabus.AttachmentUrl = url;
        syllabus.SourceFileName = Path.GetFileName(entry.FullName);
        syllabus.UpdatedAt = VietnamTime.UtcNow();
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task AttachOriginalLessonPlanFileAsync(
        IFileStorageService fileStorage,
        Guid templateId,
        ZipArchiveEntry entry,
        MemoryStream fileStream,
        CancellationToken cancellationToken)
    {
        var template = await context.LessonPlanTemplates.FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken);
        if (template is null)
        {
            return;
        }

        var folder = $"curriculum/{template.ModuleId}/lesson-plans";
        var url = await fileStorage.UploadFileAsync(
            fileStream,
            Path.GetFileName(entry.FullName),
            folder,
            "document",
            cancellationToken);

        template.AttachmentUrl = url;
        template.AttachmentMimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        template.AttachmentFileSize = fileStream.Length;
        template.AttachmentOriginalFileName = Path.GetFileName(entry.FullName);
        template.SourceFileName = Path.GetFileName(entry.FullName);
        template.UpdatedAt = VietnamTime.UtcNow();
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Result> ValidateImportedSyllabusAsync(
        Guid syllabusId,
        int expectedLessonCount,
        string sourceEntryName,
        CancellationToken cancellationToken)
    {
        if (expectedLessonCount <= 0)
        {
            return Result.Success();
        }

        var documentResult = await sender.Send(new GetSyllabusDocumentQuery { Id = syllabusId }, cancellationToken);
        if (documentResult.IsFailure)
        {
            return Result.Failure(documentResult.Error);
        }

        var curriculumSection = documentResult.Value.Sections
            .FirstOrDefault(x =>
                x.Type == SyllabusDocumentSectionTypes.Table &&
                x.Table is not null &&
                string.Equals(x.Title, "Curriculum", StringComparison.OrdinalIgnoreCase))
            ?? documentResult.Value.Sections.FirstOrDefault(x => x.Type == SyllabusDocumentSectionTypes.Table && x.Table is not null);
        var rowCount = curriculumSection?.Table?.Rows.Count ?? 0;

        if (rowCount < expectedLessonCount)
        {
            return Result.Failure(SyllabusErrors.InvalidImportFile(
                $"Imported curriculum rows mismatch for '{sourceEntryName}'. Expected {expectedLessonCount} rows from source, but syllabus document only contains {rowCount} rows. The archive import was aborted."));
        }

        var summaryLessons = documentResult.Value.Summary.TotalLessons;
        if (summaryLessons != rowCount)
        {
            return Result.Failure(SyllabusErrors.InvalidImportFile(
                $"Imported syllabus summary mismatch for '{sourceEntryName}'. Curriculum rows = {rowCount}, summary lessons = {summaryLessons}. The archive import was aborted."));
        }

        return Result.Success();
    }

    private sealed record ResolvedLessonPlanArchiveEntry(
        ZipArchiveEntry Entry,
        Domain.Programs.Module Module,
        int SessionIndex,
        string SourceType,
        string ParserVersion)
    {
        public string NormalizedEntryName => CurriculumArchiveImportEntryRules.NormalizeArchivePath(Entry.FullName);
        public string FileName => Path.GetFileName(Entry.FullName);
    }
}
