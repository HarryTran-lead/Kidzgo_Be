using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LessonPlanTemplates.Shared;
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

        var syllabusUnits = await context.SyllabusUnits
            .AsNoTracking()
            .Where(x => x.SyllabusId == command.SyllabusId && x.ModuleId.HasValue)
            .OrderBy(x => x.ModuleId)
            .ThenBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken);
        var syllabusUnitSessionLookup = SyllabusUnitSessionIndexResolver.BuildLookupFromSyllabusUnits(syllabusUnits);

        var importConfiguration = await context.CurriculumImportConfigurations
            .AsNoTracking()
            .Include(x => x.ModuleRules)
            .FirstOrDefaultAsync(
                x => x.ProgramId == command.ProgramId &&
                     x.LevelId == command.LevelId &&
                     x.IsActive,
                cancellationToken);

        if (!command.ModuleId.HasValue && importConfiguration is null)
        {
            return Result.Failure<ImportLessonPlanWordsResponse>(
                SyllabusErrors.ImportConfigurationNotFound(command.ProgramId, command.LevelId));
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

            var unitIdentity = LessonPlanUnitNameNormalizer.ExtractUnitIdentity(file.FileName);
            var lessonPlanUnitNameOverride = unitIdentity?.CanonicalDisplayName;
            var lessonNumberOverride = LessonPlanUnitNameNormalizer.ExtractLessonNumber(file.FileName);
            var sessionIndexOverride = unitIdentity is not null && lessonNumberOverride.HasValue
                ? SyllabusUnitSessionIndexResolver.ResolveSessionIndex(
                    syllabusUnitSessionLookup,
                    module.Id,
                    unitIdentity.NormalizedKey,
                    lessonNumberOverride.Value)
                : null;

            if (!sessionIndexOverride.HasValue)
            {
                sessionIndexOverride = command.ModuleId.HasValue
                    ? ResolveModuleScopedSessionIndex(importConfiguration, module.Id, file.FileName)
                    : ResolveSessionIndex(importConfiguration!, module.Id, file.FileName);
            }

            if (!command.ModuleId.HasValue && !sessionIndexOverride.HasValue)
            {
                skipped.Add($"{file.FileName}: Could not resolve session index from syllabus units or import configuration");
                continue;
            }

            var result = await sender.Send(
                new ImportLessonPlanTemplateFromWordCommand
                {
                    SyllabusId = command.SyllabusId,
                    ModuleId = module.Id,
                    LessonPlanUnitNameOverride = lessonPlanUnitNameOverride,
                    LessonNumberOverride = lessonNumberOverride,
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

    private static int? ResolveModuleScopedSessionIndex(
        CurriculumImportConfiguration? configuration,
        Guid moduleId,
        string fileName)
    {
        if (configuration is null)
        {
            return null;
        }

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
