using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.CreateSyllabusVersion;

public sealed class CreateSyllabusVersionCommandHandler(IDbContext context)
    : ICommandHandler<CreateSyllabusVersionCommand, CreateSyllabusVersionResponse>
{
    public async Task<Result<CreateSyllabusVersionResponse>> Handle(
        CreateSyllabusVersionCommand command,
        CancellationToken cancellationToken)
    {
        var source = await context.Syllabuses
            .Include(x => x.Units)
            .Include(x => x.Lessons)
            .Include(x => x.Resources)
            .Include(x => x.SessionTemplates)
            .FirstOrDefaultAsync(x => x.Id == command.SourceSyllabusId && !x.IsDeleted, cancellationToken);

        if (source is null)
        {
            return Result.Failure<CreateSyllabusVersionResponse>(SyllabusErrors.NotFound(command.SourceSyllabusId));
        }

        if (command.Version <= 0)
        {
            return Result.Failure<CreateSyllabusVersionResponse>(SyllabusErrors.InvalidVersion(command.Version));
        }

        var version = command.Version;
        var duplicateExists = await context.Syllabuses.AnyAsync(
            x => !x.IsDeleted &&
                 x.ProgramId == source.ProgramId &&
                 x.LevelId == source.LevelId &&
                 x.Code == source.Code &&
                 x.Version == version,
            cancellationToken);

        if (duplicateExists)
        {
            return Result.Failure<CreateSyllabusVersionResponse>(
                SyllabusErrors.DuplicateVersion(source.ProgramId, source.LevelId, source.Code, version));
        }

        var now = VietnamTime.UtcNow();
        var cloned = new Syllabus
        {
            Id = Guid.NewGuid(),
            ProgramId = source.ProgramId,
            LevelId = source.LevelId,
            Code = source.Code,
            Version = version,
            Title = command.Title?.Trim() ?? source.Title,
            Edition = command.Edition ?? source.Edition,
            EffectiveFrom = command.EffectiveFrom,
            EffectiveTo = command.EffectiveTo,
            PacingSchemeJson = source.PacingSchemeJson,
            Overview = source.Overview,
            OverallObjectives = source.OverallObjectives,
            SpecificObjectives = source.SpecificObjectives,
            EthicsAndAttitudes = source.EthicsAndAttitudes,
            BookOverview = source.BookOverview,
            TotalPeriods = source.TotalPeriods,
            MinutesPerPeriod = source.MinutesPerPeriod,
            TotalLessons = source.TotalLessons,
            SourceFileName = source.SourceFileName,
            AttachmentUrl = source.AttachmentUrl,
            RawContentJson = source.RawContentJson,
            DocumentStatus = "Draft",
            SourceType = source.SourceType,
            ParserVersion = source.ParserVersion,
            DocumentVersion = 1,
            SectionsJson = source.SectionsJson,
            WarningsJson = source.WarningsJson,
            ArchiveReason = null,
            IsActive = command.PromoteNow,
            IsDeleted = false,
            CreatedBy = source.CreatedBy,
            CreatedAt = now,
            UpdatedAt = now,
            Units = source.Units.Select(x => new SyllabusUnit
            {
                Id = Guid.NewGuid(),
                SyllabusId = Guid.Empty,
                ModuleId = x.ModuleId,
                Name = x.Name,
                AllocatedPeriods = x.AllocatedPeriods,
                LessonCount = x.LessonCount,
                OrderIndex = x.OrderIndex,
                Notes = x.Notes,
                CreatedAt = now,
                UpdatedAt = now
            }).ToList(),
            Lessons = source.Lessons.Select(x => new SyllabusLesson
            {
                Id = Guid.NewGuid(),
                SyllabusId = Guid.Empty,
                ModuleId = x.ModuleId,
                PeriodFrom = x.PeriodFrom,
                PeriodTo = x.PeriodTo,
                Topic = x.Topic,
                LessonNumber = x.LessonNumber,
                ContentSummary = x.ContentSummary,
                StructureSummary = x.StructureSummary,
                StudentBookPages = x.StudentBookPages,
                TeacherBookPages = x.TeacherBookPages,
                OrderIndex = x.OrderIndex,
                CreatedAt = now,
                UpdatedAt = now
            }).ToList(),
            Resources = source.Resources.Select(x => new SyllabusResource
            {
                Id = Guid.NewGuid(),
                SyllabusId = Guid.Empty,
                DocumentName = x.DocumentName,
                Abbreviation = x.Abbreviation,
                IntendedUsers = x.IntendedUsers,
                Notes = x.Notes,
                OrderIndex = x.OrderIndex,
                CreatedAt = now,
                UpdatedAt = now
            }).ToList(),
            SessionTemplates = source.SessionTemplates.Select(x => new SessionTemplate
            {
                Id = Guid.NewGuid(),
                SyllabusId = Guid.Empty,
                ProgramId = x.ProgramId,
                LevelId = x.LevelId,
                ModuleId = x.ModuleId,
                LessonPlanTemplateId = x.LessonPlanTemplateId,
                SessionIndex = x.SessionIndex,
                SessionIndexInModule = x.SessionIndexInModule,
                LessonNumber = x.LessonNumber,
                Title = x.Title,
                Topic = x.Topic,
                ObjectiveSummary = x.ObjectiveSummary,
                VocabularySummary = x.VocabularySummary,
                GrammarSummary = x.GrammarSummary,
                ContentSummary = x.ContentSummary,
                TeacherNotes = x.TeacherNotes,
                OrderIndex = x.OrderIndex,
                IsActive = x.IsActive,
                CreatedAt = now,
                UpdatedAt = now
            }).ToList()
        };

        foreach (var unit in cloned.Units)
        {
            unit.SyllabusId = cloned.Id;
        }

        foreach (var lesson in cloned.Lessons)
        {
            lesson.SyllabusId = cloned.Id;
        }

        foreach (var resource in cloned.Resources)
        {
            resource.SyllabusId = cloned.Id;
        }

        foreach (var sessionTemplate in cloned.SessionTemplates)
        {
            sessionTemplate.SyllabusId = cloned.Id;
        }

        context.Syllabuses.Add(cloned);
        await context.SaveChangesAsync(cancellationToken);

        if (command.PromoteNow)
        {
            await SyllabusVersionPromotionService.PromoteAsync(context, cloned, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        return new CreateSyllabusVersionResponse
        {
            SyllabusId = cloned.Id,
            SourceSyllabusId = source.Id,
            Code = cloned.Code,
            Version = cloned.Version,
            Title = cloned.Title,
            IsActive = cloned.IsActive
        };
    }
}
