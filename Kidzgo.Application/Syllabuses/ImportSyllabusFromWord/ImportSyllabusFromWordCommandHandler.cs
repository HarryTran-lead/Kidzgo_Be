using System.Text.Json;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LessonPlanTemplates.Shared;
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
        if (command.Version <= 0)
        {
            return Result.Failure<ImportSyllabusFromWordResponse>(SyllabusErrors.InvalidVersion(command.Version));
        }

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

        if (command.BranchId.HasValue)
        {
            var branchExists = await context.Branches
                .AnyAsync(x => x.Id == command.BranchId.Value && x.IsActive, cancellationToken);
            if (!branchExists)
            {
                return Result.Failure<ImportSyllabusFromWordResponse>(Error.NotFound(
                    "Syllabus.BranchNotFound",
                    $"Branch with Id = '{command.BranchId.Value}' was not found or inactive."));
            }

            var programAssignedToBranch = await context.BranchPrograms
                .AnyAsync(
                    x => x.BranchId == command.BranchId.Value &&
                         x.ProgramId == command.ProgramId &&
                         x.IsActive,
                    cancellationToken);
            if (!programAssignedToBranch)
            {
                return Result.Failure<ImportSyllabusFromWordResponse>(Error.Validation(
                    "Syllabus.ProgramNotAvailableInBranch",
                    "Program is not assigned to the selected branch."));
            }
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

        var parsed = CurriculumWordImportParser.ParseSyllabusFile(command.FileStream, command.FileName);
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
                Version = command.Version,
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
        syllabus.OverallObjectives = parsed.Value.OverallObjectives;
        syllabus.SpecificObjectives = parsed.Value.SpecificObjectives;
        syllabus.EthicsAndAttitudes = parsed.Value.EthicsAndAttitudes;
        syllabus.BookOverview = parsed.Value.BookOverview;
        syllabus.TotalLessons = parsed.Value.Lessons.Count;
        syllabus.TotalPeriods = parsed.Value.Lessons
            .Select(x => x.PeriodTo ?? x.PeriodFrom ?? 0)
            .DefaultIfEmpty(0)
            .Max();
        syllabus.MinutesPerPeriod = parsed.Value.MinutesPerPeriod ?? syllabus.MinutesPerPeriod;
        syllabus.SourceFileName = FitLegacyShortText(command.FileName);
        syllabus.RawContentJson = JsonSerializer.Serialize(parsed.Value);
        syllabus.DocumentStatus = SyllabusDocumentStatuses.Draft;
        syllabus.SourceType = SyllabusDocumentSourceTypes.Imported;
        syllabus.ParserVersion = SyllabusImportFileMetadata.ResolveParserVersion(command.FileName);
        syllabus.DocumentVersion = Math.Max(1, syllabus.DocumentVersion);
        syllabus.SectionsJson = SyllabusDocumentMapper.WriteSections(importDocument.Sections);
        syllabus.WarningsJson = SyllabusDocumentMapper.WriteWarnings(importDocument.Warnings);
        syllabus.IsActive = true;
        syllabus.IsDeleted = false;
        syllabus.UpdatedAt = now;
        EntityStringLengthTrimmer.TrimToModelLimits(context, syllabus);

        var syllabusUnitSessionLookup = SyllabusUnitSessionIndexResolver.BuildLookup(
            parsed.Value.Units
                .Select(unit => new
                {
                    ModuleId = ResolveModuleId(modules, importConfiguration.ModuleRules, unit.ModuleHint),
                    Identity = LessonPlanUnitNameNormalizer.ExtractUnitIdentity(unit.Name, unit.ModuleHint),
                    Unit = unit
                })
                .Where(x => x.ModuleId.HasValue && x.Identity is not null)
                .Select(x => new OrderedSyllabusUnitSession(
                    x.ModuleId!.Value,
                    x.Identity!.NormalizedKey,
                    x.Unit.OrderIndex,
                    x.Unit.LessonCount)));

        var resolvedLessonImports = ResolveLessonImports(
            parsed.Value.Lessons,
            modules,
            importConfiguration.ModuleRules,
            syllabusUnitSessionLookup);

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

        var lessonEntities = resolvedLessonImports.Select(lesson => new SyllabusLesson
        {
            Id = Guid.NewGuid(),
            SyllabusId = syllabus.Id,
            ModuleId = lesson.ModuleId,
            PeriodFrom = lesson.Lesson.PeriodFrom,
            PeriodTo = lesson.Lesson.PeriodTo,
            Topic = FitLegacyShortText(lesson.Lesson.Topic),
            LessonNumber = lesson.Lesson.LessonNumber,
            ContentSummary = FitLegacyShortText(lesson.Lesson.ContentSummary),
            StructureSummary = FitLegacyShortText(lesson.Lesson.StructureSummary),
            StudentBookPages = FitMax(lesson.Lesson.StudentBookPages, 100),
            TeacherBookPages = FitMax(lesson.Lesson.TeacherBookPages, 100),
            OrderIndex = lesson.Lesson.OrderIndex,
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

        var sessionTemplates = resolvedLessonImports.Select(lesson => new SessionTemplate
        {
            Id = Guid.NewGuid(),
            SyllabusId = syllabus.Id,
            ProgramId = command.ProgramId,
            LevelId = command.LevelId,
            ModuleId = lesson.ModuleId,
            SessionIndex = lesson.Lesson.OrderIndex,
            SessionIndexInModule = lesson.SessionIndexInModule,
            LessonNumber = lesson.Lesson.LessonNumber,
            Title = FitLegacyShortText(lesson.Lesson.Topic),
            Topic = FitLegacyShortText(lesson.Lesson.Topic),
            ObjectiveSummary = FitLegacyShortText(lesson.Lesson.ContentSummary),
            VocabularySummary = FitLegacyShortText(lesson.Lesson.Components),
            GrammarSummary = FitLegacyShortText(lesson.Lesson.StructureSummary),
            ContentSummary = FitLegacyShortText(lesson.Lesson.ContentSummary),
            OrderIndex = lesson.Lesson.OrderIndex,
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

        if (command.BranchId.HasValue)
        {
            await UpsertCurriculumAssignmentAsync(
                command.BranchId.Value,
                command.ProgramId,
                command.LevelId,
                syllabus.Id,
                now,
                cancellationToken);
        }

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

    private async Task UpsertCurriculumAssignmentAsync(
        Guid branchId,
        Guid programId,
        Guid levelId,
        Guid syllabusId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var assignment = await context.CurriculumAssignments
            .FirstOrDefaultAsync(
                x => x.BranchId == branchId &&
                     x.ProgramId == programId &&
                     x.LevelId == levelId &&
                     x.SyllabusId == syllabusId,
                cancellationToken);

        if (assignment is null)
        {
            assignment = new CurriculumAssignment
            {
                Id = Guid.NewGuid(),
                BranchId = branchId,
                ProgramId = programId,
                LevelId = levelId,
                SyllabusId = syllabusId,
                EffectiveFrom = null,
                EffectiveTo = null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            context.CurriculumAssignments.Add(assignment);
        }
        else
        {
            assignment.BranchId = branchId;
            assignment.ProgramId = programId;
            assignment.LevelId = levelId;
            assignment.SyllabusId = syllabusId;
            assignment.EffectiveFrom = null;
            assignment.EffectiveTo = null;
            assignment.IsActive = true;
            assignment.UpdatedAt = now;
        }

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

    private static IReadOnlyList<ResolvedParsedLessonImport> ResolveLessonImports(
        IReadOnlyList<ParsedSyllabusLesson> lessons,
        IReadOnlyList<Domain.Programs.Module> modules,
        IEnumerable<Domain.LessonPlans.CurriculumImportModuleRule> rules,
        IReadOnlyDictionary<Guid, IReadOnlyList<OrderedSyllabusUnitSession>> syllabusUnitSessionLookup)
    {
        var orderedRules = rules.OrderBy(x => x.OrderIndex).ToList();
        var resolved = new List<ResolvedParsedLessonImport>(lessons.Count);
        var moduleSessionCounters = new Dictionary<Guid, int>();

        foreach (var lesson in lessons.OrderBy(x => x.OrderIndex))
        {
            var moduleId = ResolveModuleId(modules, orderedRules, lesson.ModuleHint);
            int? sessionIndexInModule = null;

            if (moduleId.HasValue)
            {
                var unitIdentity = LessonPlanUnitNameNormalizer.ExtractUnitIdentity(lesson.ModuleHint, lesson.Topic);
                if (unitIdentity is not null && lesson.LessonNumber.HasValue)
                {
                    sessionIndexInModule = SyllabusUnitSessionIndexResolver.ResolveSessionIndex(
                        syllabusUnitSessionLookup,
                        moduleId.Value,
                        unitIdentity.NormalizedKey,
                        lesson.LessonNumber.Value);
                }

                var nextSequentialIndex = moduleSessionCounters.GetValueOrDefault(moduleId.Value) + 1;
                if (!sessionIndexInModule.HasValue)
                {
                    sessionIndexInModule = nextSequentialIndex;
                }

                moduleSessionCounters[moduleId.Value] = Math.Max(nextSequentialIndex, sessionIndexInModule.Value);
            }

            resolved.Add(new ResolvedParsedLessonImport(lesson, moduleId, sessionIndexInModule));
        }

        return resolved;
    }

    private sealed record ResolvedParsedLessonImport(
        ParsedSyllabusLesson Lesson,
        Guid? ModuleId,
        int? SessionIndexInModule);
}
