using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LessonPlanTemplates.ImportLessonPlanTemplateFromWord;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.ImportLessonPlanWords;

public sealed class ImportLessonPlanWordsCommandHandler(
    IDbContext context,
    ISender sender)
    : ICommandHandler<ImportLessonPlanWordsCommand, ImportLessonPlanWordsResponse>
{
    public async Task<Result<ImportLessonPlanWordsResponse>> Handle(
        ImportLessonPlanWordsCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Files.Count == 0)
        {
            return Result.Failure<ImportLessonPlanWordsResponse>(
                SyllabusErrors.InvalidImportFile("No lesson plan Word files were provided."));
        }

        var modules = await context.Modules
            .Where(x => x.LevelId == command.LevelId && x.IsActive)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);

        if (command.ModuleId.HasValue && modules.All(x => x.Id != command.ModuleId.Value))
        {
            return Result.Failure<ImportLessonPlanWordsResponse>(
                LessonPlanTemplateErrors.ModuleNotFound(command.ModuleId.Value));
        }

        var syllabus = await context.Syllabuses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == command.SyllabusId &&
                     x.ProgramId == command.ProgramId &&
                     x.LevelId == command.LevelId &&
                     x.IsActive &&
                     !x.IsDeleted,
                cancellationToken);
        if (syllabus is null)
        {
            return Result.Failure<ImportLessonPlanWordsResponse>(
                LessonPlanTemplateErrors.SyllabusNotFound(command.SyllabusId));
        }

        CurriculumImportConfiguration? importConfiguration = null;
        if (!command.ModuleId.HasValue)
        {
            importConfiguration = await context.CurriculumImportConfigurations
                .AsNoTracking()
                .Include(x => x.ModuleRules)
                .FirstOrDefaultAsync(
                    x => x.ProgramId == command.ProgramId &&
                         x.LevelId == command.LevelId &&
                         x.IsActive,
                    cancellationToken);

            if (importConfiguration is null)
            {
                return Result.Failure<ImportLessonPlanWordsResponse>(
                    SyllabusErrors.ImportConfigurationNotFound(command.ProgramId, command.LevelId));
            }
        }

        var imported = new List<ImportedLessonPlanWordDto>();
        var skipped = new List<string>();

        foreach (var file in command.Files)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!string.Equals(extension, ".docx", StringComparison.OrdinalIgnoreCase))
            {
                skipped.Add($"{file.FileName}: Unsupported file type '{extension}'. Only .docx is supported");
                continue;
            }

            var module = command.ModuleId.HasValue
                ? modules.First(x => x.Id == command.ModuleId.Value)
                : ResolveModule(modules, importConfiguration!, file.FileName);

            if (module is null)
            {
                skipped.Add(file.FileName);
                continue;
            }

            var sessionIndexOverride = command.ModuleId.HasValue
                ? null
                : ResolveSessionIndex(importConfiguration!, module.Id, file.FileName);
            if (!command.ModuleId.HasValue && !sessionIndexOverride.HasValue)
            {
                skipped.Add($"{file.FileName}: Could not resolve session index from import configuration");
                continue;
            }

            var result = await sender.Send(
                new ImportLessonPlanTemplateFromWordCommand
                {
                    SyllabusId = command.SyllabusId,
                    ModuleId = module.Id,
                    SessionIndexOverride = sessionIndexOverride,
                    OverwriteExisting = command.OverwriteExisting,
                    FileName = file.FileName,
                    FileStream = file.FileStream
                },
                cancellationToken);

            if (result.IsFailure)
            {
                skipped.Add($"{file.FileName}: {result.Error.Description}");
                continue;
            }

            imported.Add(new ImportedLessonPlanWordDto
            {
                FileName = file.FileName,
                ModuleId = module.Id,
                LessonPlanTemplateId = result.Value.LessonPlanTemplateId,
                SessionTemplateId = result.Value.SessionTemplateId,
                SessionIndex = result.Value.SessionIndex,
                Created = result.Value.Created,
                Title = result.Value.Title
            });
        }

        return new ImportLessonPlanWordsResponse
        {
            ImportedLessonPlans = imported.Count,
            SkippedFiles = skipped.Count,
            ImportedEntries = imported,
            SkippedEntries = skipped
        };
    }

    private static Domain.Programs.Module? ResolveModule(
        IReadOnlyList<Domain.Programs.Module> modules,
        CurriculumImportConfiguration configuration,
        string fileName)
    {
        return CurriculumImportRuleResolver.Resolve(
            modules,
            configuration.ModuleRules.OrderBy(x => x.OrderIndex).ToList(),
            Path.GetFileNameWithoutExtension(fileName),
            fileName);
    }

    private static int? ResolveSessionIndex(
        CurriculumImportConfiguration configuration,
        Guid moduleId,
        string fileName)
    {
        var rule = configuration.ModuleRules.FirstOrDefault(x => x.ModuleId == moduleId);
        return rule is null
            ? null
            : CurriculumImportRuleResolver.ResolveSessionIndex(
                configuration,
                rule,
                Path.GetFileNameWithoutExtension(fileName),
                fileName);
    }
}
