using System.IO.Compression;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LessonPlanTemplates.ImportLessonPlanTemplateFromWord;
using Kidzgo.Application.Syllabuses.ImportSyllabusFromWord;
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

        using var archive = new ZipArchive(command.FileStream, ZipArchiveMode.Read, leaveOpen: true);
        Guid? syllabusId = null;
        var importedLessonPlans = 0;
        var skipped = new List<string>();
        Error? syllabusImportError = null;
        var sawSyllabusCandidate = false;

        foreach (var entry in archive.Entries.Where(x =>
                     x.FullName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) &&
                     !Path.GetFileName(x.FullName).StartsWith("~$")))
        {
            await using var entryStream = entry.Open();
            await using var memory = new MemoryStream();
            await entryStream.CopyToAsync(memory, cancellationToken);
            memory.Position = 0;

            if (IsSyllabusEntry(entry.FullName))
            {
                sawSyllabusCandidate = true;
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
                continue;
            }

            var module = ResolveModuleForEntry(modules, entry.FullName);
            if (module is null)
            {
                skipped.Add(entry.FullName);
                continue;
            }

            memory.Position = 0;
            var lessonResult = await sender.Send(
                new ImportLessonPlanTemplateFromWordCommand
                {
                    ModuleId = module.Id,
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
        }

        if (syllabusId is null && sawSyllabusCandidate && syllabusImportError is not null)
        {
            return Result.Failure<ImportCurriculumArchiveResponse>(syllabusImportError);
        }

        return new ImportCurriculumArchiveResponse
        {
            SyllabusId = syllabusId,
            ImportedLessonPlans = importedLessonPlans,
            SkippedFiles = skipped.Count,
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

    private static Domain.Programs.Module? ResolveModuleForEntry(
        IReadOnlyList<Domain.Programs.Module> modules,
        string fullName)
    {
        var normalized = fullName.Replace('\\', '/');
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var folderName = segments.Length > 1 ? segments[^2] : normalized;

        var exact = modules.FirstOrDefault(x =>
            x.Name.Contains(folderName, StringComparison.OrdinalIgnoreCase) ||
            x.Code.Contains(folderName, StringComparison.OrdinalIgnoreCase));
        if (exact is not null)
        {
            return exact;
        }

        var match = System.Text.RegularExpressions.Regex.Match(folderName, @"(UNIT|REVISION)\s*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return null;
        }

        var number = int.Parse(match.Groups[2].Value);
        var isRevision = match.Groups[1].Value.Equals("REVISION", StringComparison.OrdinalIgnoreCase);

        return modules.FirstOrDefault(x =>
            x.Order == number ||
            x.Code.Contains(number.ToString(), StringComparison.OrdinalIgnoreCase) ||
            x.Name.Contains(number.ToString(), StringComparison.OrdinalIgnoreCase) ||
            (isRevision && x.Type == Domain.Programs.ModuleType.Revision && x.Order == number));
    }
}
