using System.Text.Json;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.ImportSyllabusFromWord;

public sealed class ImportSyllabusFromWordCommandHandler(IDbContext context)
    : ICommandHandler<ImportSyllabusFromWordCommand, ImportSyllabusFromWordResponse>
{
    public async Task<Result<ImportSyllabusFromWordResponse>> Handle(ImportSyllabusFromWordCommand command, CancellationToken cancellationToken)
    {
        var level = await context.Levels
            .Where(x => x.Id == command.LevelId && x.IsActive)
            .Select(x => new { x.Id, x.ProgramId })
            .FirstOrDefaultAsync(cancellationToken);

        if (level is null)
        {
            return Result.Failure<ImportSyllabusFromWordResponse>(SyllabusErrors.LevelNotFound(command.LevelId));
        }

        if (level.ProgramId != command.ProgramId)
        {
            return Result.Failure<ImportSyllabusFromWordResponse>(
                SyllabusErrors.LevelDoesNotBelongToProgram(command.LevelId, command.ProgramId));
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
            return Result.Failure<ImportSyllabusFromWordResponse>(
                SyllabusErrors.ImportConfigurationNotFound(command.ProgramId, command.LevelId));
        }

        var parsed = CurriculumWordImportParser.ParseSyllabusDocx(command.FileStream, command.FileName);
        if (parsed.IsFailure)
        {
            return Result.Failure<ImportSyllabusFromWordResponse>(parsed.Error);
        }

        if (parsed.Value.Lessons.Count == 0)
        {
            return Result.Failure<ImportSyllabusFromWordResponse>(
                SyllabusErrors.InvalidImportFile("No syllabus lessons were found in the imported Word document."));
        }

        var syllabus = await context.Syllabuses
            .FirstOrDefaultAsync(
                x => x.ProgramId == command.ProgramId &&
                     x.LevelId == command.LevelId &&
                     x.Code == command.Code &&
                     x.Version == command.Version &&
                     !x.IsDeleted,
                cancellationToken);

        if (syllabus is not null && !command.OverwriteExisting)
        {
            return Result.Failure<ImportSyllabusFromWordResponse>(
                SyllabusErrors.DuplicateVersion(command.ProgramId, command.LevelId, command.Code, command.Version));
        }

        var now = VietnamTime.UtcNow();
        var importDocument = SyllabusDocumentMapper.BuildFromParsedImport(parsed.Value);
        if (syllabus is null)
        {
            syllabus = new Syllabus
            {
                Id = Guid.NewGuid(),
                ProgramId = command.ProgramId,
                LevelId = command.LevelId,
                Code = command.Code.Trim(),
                Version = command.Version.Trim(),
                CreatedAt = now
            };
            context.Syllabuses.Add(syllabus);
        }
        else
        {
            await context.SessionTemplates
                .Where(x => x.SyllabusId == syllabus.Id)
                .ExecuteDeleteAsync(cancellationToken);

            await context.SyllabusUnits
                .Where(x => x.SyllabusId == syllabus.Id)
                .ExecuteDeleteAsync(cancellationToken);

            await context.SyllabusLessons
                .Where(x => x.SyllabusId == syllabus.Id)
                .ExecuteDeleteAsync(cancellationToken);

            await context.SyllabusResources
                .Where(x => x.SyllabusId == syllabus.Id)
                .ExecuteDeleteAsync(cancellationToken);
        }

        syllabus.Title = FitLegacyShortText(parsed.Value.Title);
        syllabus.Edition = FitMax(parsed.Value.Edition, 100);
        syllabus.Overview = parsed.Value.Overview;
        syllabus.TotalLessons = parsed.Value.Lessons.Count;
        syllabus.TotalPeriods = parsed.Value.Lessons
            .Select(x => x.PeriodTo ?? x.PeriodFrom ?? 0)
            .DefaultIfEmpty(0)
            .Max();
        syllabus.SourceFileName = FitLegacyShortText(command.FileName);
        syllabus.RawContentJson = JsonSerializer.Serialize(parsed.Value);
        syllabus.DocumentStatus = SyllabusDocumentStatuses.Draft;
        syllabus.SourceType = SyllabusDocumentSourceTypes.Imported;
        syllabus.ParserVersion = "docx-v1";
        syllabus.DocumentVersion = Math.Max(1, syllabus.DocumentVersion);
        syllabus.SectionsJson = SyllabusDocumentMapper.WriteSections(importDocument.Sections);
        syllabus.WarningsJson = SyllabusDocumentMapper.WriteWarnings(importDocument.Warnings);
        syllabus.IsActive = true;
        syllabus.IsDeleted = false;
        syllabus.UpdatedAt = now;
        EntityStringLengthTrimmer.TrimToModelLimits(context, syllabus);

        var unitEntities = parsed.Value.Units.Select(unit => new SyllabusUnit
        {
            Id = Guid.NewGuid(),
            SyllabusId = syllabus.Id,
            ModuleId = ResolveModuleId(modules, importConfiguration.ModuleRules, unit.ModuleHint),
            Name = FitLegacyShortText(unit.Name),
            AllocatedPeriods = unit.AllocatedPeriods,
            LessonCount = unit.LessonCount,
            OrderIndex = unit.OrderIndex,
            Notes = FitLegacyShortText(unit.Notes),
            CreatedAt = now,
            UpdatedAt = now
        }).ToList();

        var lessonEntities = parsed.Value.Lessons.Select(lesson => new SyllabusLesson
        {
            Id = Guid.NewGuid(),
            SyllabusId = syllabus.Id,
            ModuleId = ResolveModuleId(modules, importConfiguration.ModuleRules, lesson.ModuleHint),
            PeriodFrom = lesson.PeriodFrom,
            PeriodTo = lesson.PeriodTo,
            Topic = FitLegacyShortText(lesson.Topic),
            LessonNumber = lesson.LessonNumber,
            ContentSummary = FitLegacyShortText(lesson.ContentSummary),
            StructureSummary = FitLegacyShortText(lesson.StructureSummary),
            StudentBookPages = FitMax(lesson.StudentBookPages, 100),
            TeacherBookPages = FitMax(lesson.TeacherBookPages, 100),
            OrderIndex = lesson.OrderIndex,
            CreatedAt = now,
            UpdatedAt = now
        }).ToList();

        var resourceEntities = parsed.Value.Resources.Select(resource => new SyllabusResource
        {
            Id = Guid.NewGuid(),
            SyllabusId = syllabus.Id,
            DocumentName = FitLegacyShortText(resource.DocumentName),
            Abbreviation = FitMax(resource.Abbreviation, 50),
            IntendedUsers = FitLegacyShortText(resource.IntendedUsers),
            Notes = FitLegacyShortText(resource.Notes),
            OrderIndex = resource.OrderIndex,
            CreatedAt = now,
            UpdatedAt = now
        }).ToList();

        var sessionTemplates = parsed.Value.Lessons.Select(lesson => new SessionTemplate
        {
            Id = Guid.NewGuid(),
            SyllabusId = syllabus.Id,
            ProgramId = command.ProgramId,
            LevelId = command.LevelId,
            ModuleId = ResolveModuleId(modules, importConfiguration.ModuleRules, lesson.ModuleHint),
            SessionIndex = lesson.OrderIndex,
            SessionIndexInModule = lesson.LessonNumber,
            LessonNumber = lesson.LessonNumber,
            Title = FitLegacyShortText(lesson.Topic),
            Topic = FitLegacyShortText(lesson.Topic),
            ObjectiveSummary = FitLegacyShortText(lesson.ContentSummary),
            VocabularySummary = FitLegacyShortText(lesson.Components),
            GrammarSummary = FitLegacyShortText(lesson.StructureSummary),
            ContentSummary = FitLegacyShortText(lesson.ContentSummary),
            OrderIndex = lesson.OrderIndex,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        }).ToList();

        EntityStringLengthTrimmer.TrimToModelLimits(context, unitEntities);
        EntityStringLengthTrimmer.TrimToModelLimits(context, lessonEntities);
        EntityStringLengthTrimmer.TrimToModelLimits(context, resourceEntities);
        EntityStringLengthTrimmer.TrimToModelLimits(context, sessionTemplates);

        context.SyllabusUnits.AddRange(unitEntities);
        context.SyllabusLessons.AddRange(lessonEntities);
        context.SyllabusResources.AddRange(resourceEntities);
        context.SessionTemplates.AddRange(sessionTemplates);

        await context.SaveChangesAsync(cancellationToken);

        return new ImportSyllabusFromWordResponse
        {
            SyllabusId = syllabus.Id,
            ImportedUnits = unitEntities.Count,
            ImportedLessons = lessonEntities.Count,
            ImportedResources = resourceEntities.Count,
            ImportedSessionTemplates = sessionTemplates.Count
        };
    }

    private static Guid? ResolveModuleId(
        IReadOnlyList<Domain.Programs.Module> modules,
        IEnumerable<Domain.LessonPlans.CurriculumImportModuleRule> rules,
        string? hint)
    {
        return CurriculumImportRuleResolver.Resolve(
            modules,
            rules.OrderBy(x => x.OrderIndex).ToList(),
            hint)?.Id;
    }

    private static string? FitLegacyShortText(string? value)
    {
        return FitMax(value, 100);
    }

    private static string? FitMax(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        var normalized = System.Text.RegularExpressions.Regex.Replace(value, @"\s+", " ").Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength].Trim();
    }
}
