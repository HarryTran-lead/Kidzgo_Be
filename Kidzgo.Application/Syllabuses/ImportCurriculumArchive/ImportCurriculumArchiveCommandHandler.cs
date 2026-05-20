using System.IO.Compression;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
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
    ISender sender)
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
        var docxEntries = archive.Entries
            .Where(x => x.FullName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) &&
                        !Path.GetFileName(x.FullName).StartsWith("~$"))
            .ToList();

        var syllabusEntries = docxEntries
            .Where(x => IsSyllabusEntry(x.FullName))
            .OrderByDescending(x => GetSyllabusEntryPriority(x.FullName))
            .ToList();

        var lessonPlanEntries = docxEntries
            .Where(x => !IsSyllabusEntry(x.FullName))
            .ToList();

        Guid? syllabusId = null;
        var importedLessonPlans = 0;
        var importedEntries = new List<ImportedCurriculumLessonPlanDto>();
        var skipped = new List<string>();
        Error? syllabusImportError = null;

        foreach (var entry in syllabusEntries)
        {
            await using var entryStream = entry.Open();
            await using var memory = new MemoryStream();
            await entryStream.CopyToAsync(memory, cancellationToken);
            memory.Position = 0;

            var syllabusResult = await sender.Send(
                new ImportSyllabusFromWordCommand
                {
                    ProgramId = command.ProgramId,
                    LevelId = command.LevelId,
                    Code = command.Code,
                    Version = command.Version,
                    OverwriteExisting = command.OverwriteExisting,
                    FileName = Path.GetFileName(entry.FullName),
                    FileStream = memory
                },
                cancellationToken);

            if (syllabusResult.IsFailure)
            {
                syllabusImportError = syllabusResult.Error;
                skipped.Add($"{entry.FullName}: {syllabusResult.Error.Description}");
                continue;
            }

            syllabusId = syllabusResult.Value.SyllabusId;
            break;
        }

        if (syllabusId is null)
        {
            if (syllabusImportError is not null)
            {
                return Result.Failure<ImportCurriculumArchiveResponse>(syllabusImportError);
            }

            return Result.Failure<ImportCurriculumArchiveResponse>(
                SyllabusErrors.InvalidImportFile("No syllabus Word document was found in the archive."));
        }

        foreach (var entry in lessonPlanEntries)
        {
            await using var entryStream = entry.Open();
            await using var memory = new MemoryStream();
            await entryStream.CopyToAsync(memory, cancellationToken);
            memory.Position = 0;

            var module = ResolveModuleForEntry(modules, entry.FullName);
            if (module is null)
            {
                module = CurriculumImportRuleResolver.Resolve(
                    modules,
                    importConfiguration.ModuleRules.OrderBy(x => x.OrderIndex).ToList(),
                    Path.GetDirectoryName(entry.FullName),
                    Path.GetFileNameWithoutExtension(entry.FullName),
                    entry.FullName);
            }
            if (module is null)
            {
                skipped.Add(entry.FullName);
                continue;
            }

            var sessionIndexOverride = ResolveSessionIndex(
                importConfiguration,
                module.Id,
                Path.GetDirectoryName(entry.FullName),
                Path.GetFileNameWithoutExtension(entry.FullName),
                entry.FullName);
            if (!sessionIndexOverride.HasValue)
            {
                skipped.Add($"{entry.FullName}: Could not resolve session index from import configuration");
                continue;
            }

            memory.Position = 0;
            var lessonResult = await sender.Send(
                new ImportLessonPlanTemplateFromWordCommand
                {
                    ModuleId = module.Id,
                    SessionIndexOverride = sessionIndexOverride,
                    OverwriteExisting = command.OverwriteExisting,
                    FileName = Path.GetFileName(entry.FullName),
                    FileStream = memory
                },
                cancellationToken);

            if (lessonResult.IsFailure)
            {
                skipped.Add($"{entry.FullName}: {lessonResult.Error.Description}");
                continue;
            }

            importedLessonPlans++;
            importedEntries.Add(new ImportedCurriculumLessonPlanDto
            {
                EntryName = entry.FullName,
                ModuleId = module.Id,
                LessonPlanTemplateId = lessonResult.Value.LessonPlanTemplateId,
                SessionTemplateId = lessonResult.Value.SessionTemplateId,
                SessionIndex = lessonResult.Value.SessionIndex,
                Created = lessonResult.Value.Created,
                Title = lessonResult.Value.Title
            });
        }

        return new ImportCurriculumArchiveResponse
        {
            SyllabusId = syllabusId,
            ImportedLessonPlans = importedLessonPlans,
            SkippedFiles = skipped.Count,
            ImportedEntries = importedEntries,
            SkippedEntries = skipped
        };
    }

    private static bool IsSyllabusEntry(string fullName)
    {
        var normalized = fullName.Replace('\\', '/');
        var fileName = Path.GetFileNameWithoutExtension(normalized);

        return normalized.Contains("PPCT", StringComparison.OrdinalIgnoreCase) &&
               normalized.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) &&
               (fileName.Contains("syllabus", StringComparison.OrdinalIgnoreCase) ||
                fileName.Contains("curriculum", StringComparison.OrdinalIgnoreCase) ||
                fileName.Contains("ppct", StringComparison.OrdinalIgnoreCase));
    }

    private static int GetSyllabusEntryPriority(string fullName)
    {
        var fileName = Path.GetFileNameWithoutExtension(fullName);
        var priority = 0;

        if (fileName.Contains("full", StringComparison.OrdinalIgnoreCase))
        {
            priority += 100;
        }

        if (fileName.Contains("the syllabus", StringComparison.OrdinalIgnoreCase))
        {
            priority += 50;
        }

        if (fileName.Contains("course syllabus", StringComparison.OrdinalIgnoreCase))
        {
            priority += 10;
        }

        if (fileName.Contains("syllabus", StringComparison.OrdinalIgnoreCase))
        {
            priority += 5;
        }

        return priority;
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
}
