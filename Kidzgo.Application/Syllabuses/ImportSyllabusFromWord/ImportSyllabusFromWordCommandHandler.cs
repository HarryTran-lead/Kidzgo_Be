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
            .Include(x => x.Units)
            .Include(x => x.Lessons)
            .Include(x => x.Resources)
            .Include(x => x.SessionTemplates)
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
            context.SyllabusUnits.RemoveRange(syllabus.Units);
            context.SyllabusLessons.RemoveRange(syllabus.Lessons);
            context.SyllabusResources.RemoveRange(syllabus.Resources);
            context.SessionTemplates.RemoveRange(syllabus.SessionTemplates);
        }

        syllabus.Title = parsed.Value.Title;
        syllabus.Edition = parsed.Value.Edition;
        syllabus.Overview = parsed.Value.Overview;
        syllabus.TotalLessons = parsed.Value.Lessons.Count;
        syllabus.TotalPeriods = parsed.Value.Lessons
            .Select(x => x.PeriodTo ?? x.PeriodFrom ?? 0)
            .DefaultIfEmpty(0)
            .Max();
        syllabus.SourceFileName = command.FileName;
        syllabus.RawContentJson = JsonSerializer.Serialize(parsed.Value);
        syllabus.IsActive = true;
        syllabus.IsDeleted = false;
        syllabus.UpdatedAt = now;

        var unitEntities = parsed.Value.Units.Select(unit => new SyllabusUnit
        {
            Id = Guid.NewGuid(),
            SyllabusId = syllabus.Id,
            ModuleId = ResolveModuleId(modules, unit.ModuleHint),
            Name = unit.Name,
            AllocatedPeriods = unit.AllocatedPeriods,
            LessonCount = unit.LessonCount,
            OrderIndex = unit.OrderIndex,
            Notes = unit.Notes,
            CreatedAt = now,
            UpdatedAt = now
        }).ToList();

        var lessonEntities = parsed.Value.Lessons.Select(lesson => new SyllabusLesson
        {
            Id = Guid.NewGuid(),
            SyllabusId = syllabus.Id,
            ModuleId = ResolveModuleId(modules, lesson.ModuleHint),
            PeriodFrom = lesson.PeriodFrom,
            PeriodTo = lesson.PeriodTo,
            Topic = lesson.Topic,
            LessonNumber = lesson.LessonNumber,
            ContentSummary = lesson.ContentSummary,
            StructureSummary = lesson.StructureSummary,
            StudentBookPages = lesson.StudentBookPages,
            TeacherBookPages = lesson.TeacherBookPages,
            OrderIndex = lesson.OrderIndex,
            CreatedAt = now,
            UpdatedAt = now
        }).ToList();

        var resourceEntities = parsed.Value.Resources.Select(resource => new SyllabusResource
        {
            Id = Guid.NewGuid(),
            SyllabusId = syllabus.Id,
            DocumentName = resource.DocumentName,
            Abbreviation = resource.Abbreviation,
            IntendedUsers = resource.IntendedUsers,
            Notes = resource.Notes,
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
            ModuleId = ResolveModuleId(modules, lesson.ModuleHint),
            SessionIndex = lesson.OrderIndex,
            SessionIndexInModule = lesson.LessonNumber,
            LessonNumber = lesson.LessonNumber,
            Title = lesson.Topic,
            Topic = lesson.Topic,
            ObjectiveSummary = lesson.ContentSummary,
            VocabularySummary = lesson.Components,
            GrammarSummary = lesson.StructureSummary,
            ContentSummary = lesson.ContentSummary,
            OrderIndex = lesson.OrderIndex,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        }).ToList();

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

    private static Guid? ResolveModuleId(IEnumerable<Domain.Programs.Module> modules, string? hint)
    {
        if (string.IsNullOrWhiteSpace(hint))
        {
            return null;
        }

        var normalizedHint = hint.Trim();
        var module = modules.FirstOrDefault(x =>
            x.Name.Contains(normalizedHint, StringComparison.OrdinalIgnoreCase) ||
            x.Code.Contains(normalizedHint, StringComparison.OrdinalIgnoreCase));

        if (module is not null)
        {
            return module.Id;
        }

        var numberMatch = System.Text.RegularExpressions.Regex.Match(normalizedHint, @"\d+");
        if (!numberMatch.Success)
        {
            return null;
        }

        var number = int.Parse(numberMatch.Value);
        return modules.FirstOrDefault(x =>
            x.Order == number ||
            x.Code.Contains(numberMatch.Value, StringComparison.OrdinalIgnoreCase) ||
            x.Name.Contains(numberMatch.Value, StringComparison.OrdinalIgnoreCase))?.Id;
    }
}
