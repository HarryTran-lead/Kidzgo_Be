using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.GetSyllabusById;

public sealed class GetSyllabusByIdQueryHandler(IDbContext context)
    : IQueryHandler<GetSyllabusByIdQuery, GetSyllabusByIdResponse>
{
    public async Task<Result<GetSyllabusByIdResponse>> Handle(GetSyllabusByIdQuery query, CancellationToken cancellationToken)
    {
        var syllabus = await context.Syllabuses
            .AsNoTracking()
            .Where(x => x.Id == query.Id && !x.IsDeleted)
            .Select(x => new
            {
                Id = x.Id,
                ProgramId = x.ProgramId,
                ProgramName = x.Program.Name,
                LevelId = x.LevelId,
                LevelName = x.Level.Name,
                Code = x.Code,
                Version = x.Version,
                Title = x.Title,
                Edition = x.Edition,
                EffectiveFrom = x.EffectiveFrom,
                EffectiveTo = x.EffectiveTo,
                PacingSchemeJson = x.PacingSchemeJson,
                Overview = x.Overview,
                OverallObjectives = x.OverallObjectives,
                SpecificObjectives = x.SpecificObjectives,
                EthicsAndAttitudes = x.EthicsAndAttitudes,
                BookOverview = x.BookOverview,
                TotalPeriods = x.TotalPeriods,
                MinutesPerPeriod = x.MinutesPerPeriod,
                TotalLessons = x.TotalLessons,
                SourceFileName = x.SourceFileName,
                AttachmentUrl = x.AttachmentUrl,
                RawContentJson = x.RawContentJson,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (syllabus is null)
        {
            return Result.Failure<GetSyllabusByIdResponse>(SyllabusErrors.NotFound(query.Id));
        }

        var units = await context.SyllabusUnits
            .AsNoTracking()
            .Where(u => u.SyllabusId == query.Id)
            .OrderBy(u => u.OrderIndex)
            .Select(u => new SyllabusUnitDetailDto
            {
                Id = u.Id,
                ModuleId = u.ModuleId,
                ModuleName = u.Module != null ? u.Module.Name : null,
                Name = u.Name,
                AllocatedPeriods = u.AllocatedPeriods,
                LessonCount = u.LessonCount,
                OrderIndex = u.OrderIndex,
                Notes = u.Notes
            })
            .ToListAsync(cancellationToken);

        var lessons = await context.SyllabusLessons
            .AsNoTracking()
            .Where(l => l.SyllabusId == query.Id)
            .OrderBy(l => l.OrderIndex)
            .Select(l => new SyllabusLessonDetailDto
            {
                Id = l.Id,
                ModuleId = l.ModuleId,
                ModuleName = l.Module != null ? l.Module.Name : null,
                PeriodFrom = l.PeriodFrom,
                PeriodTo = l.PeriodTo,
                Topic = l.Topic,
                LessonNumber = l.LessonNumber,
                ContentSummary = l.ContentSummary,
                StructureSummary = l.StructureSummary,
                StudentBookPages = l.StudentBookPages,
                TeacherBookPages = l.TeacherBookPages,
                OrderIndex = l.OrderIndex
            })
            .ToListAsync(cancellationToken);

        var resources = await context.SyllabusResources
            .AsNoTracking()
            .Where(r => r.SyllabusId == query.Id)
            .OrderBy(r => r.OrderIndex)
            .Select(r => new SyllabusResourceDetailDto
            {
                Id = r.Id,
                DocumentName = r.DocumentName,
                Abbreviation = r.Abbreviation,
                IntendedUsers = r.IntendedUsers,
                Notes = r.Notes,
                OrderIndex = r.OrderIndex
            })
            .ToListAsync(cancellationToken);

        var sessionTemplates = await context.SessionTemplates
            .AsNoTracking()
            .Where(s => s.SyllabusId == query.Id)
            .OrderBy(s => s.OrderIndex)
            .Select(s => new SyllabusSessionTemplateDetailDto
            {
                Id = s.Id,
                ModuleId = s.ModuleId,
                ModuleName = s.Module != null ? s.Module.Name : null,
                LessonPlanTemplateId = s.LessonPlanTemplateId ?? (s.LessonPlanTemplate != null ? s.LessonPlanTemplate.Id : null),
                LessonPlanTemplateTitle = s.LessonPlanTemplate != null ? s.LessonPlanTemplate.Title : null,
                LessonPlanTemplateSourceFileName = s.LessonPlanTemplate != null ? s.LessonPlanTemplate.SourceFileName : null,
                SessionIndex = s.SessionIndex,
                SessionIndexInModule = s.SessionIndexInModule,
                LessonNumber = s.LessonNumber,
                Title = s.Title,
                Topic = s.Topic,
                ObjectiveSummary = s.ObjectiveSummary,
                VocabularySummary = s.VocabularySummary,
                GrammarSummary = s.GrammarSummary,
                OrderIndex = s.OrderIndex
            })
            .ToListAsync(cancellationToken);

        var lessonPlanTemplateSummaries = await context.Modules
            .AsNoTracking()
            .Where(m => m.LevelId == syllabus.LevelId && m.IsActive)
            .OrderBy(m => m.Order)
            .Select(m => new SyllabusModuleLessonPlanSummaryDto
            {
                ModuleId = m.Id,
                ModuleCode = m.Code,
                ModuleName = m.Name,
                ModuleOrder = m.Order,
                PlannedSessionCount = m.PlannedSessionCount,
                SyllabusSessionTemplateCount = m.SessionTemplates.Count(s => s.SyllabusId == query.Id && s.IsActive),
                ImportedLessonPlanTemplateCount = m.LessonPlanTemplates.Count(t => t.IsActive && !t.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        return new GetSyllabusByIdResponse
        {
            Id = syllabus.Id,
            ProgramId = syllabus.ProgramId,
            ProgramName = syllabus.ProgramName,
            LevelId = syllabus.LevelId,
            LevelName = syllabus.LevelName,
            Code = syllabus.Code,
            Version = syllabus.Version,
            Title = syllabus.Title,
            Edition = syllabus.Edition,
            EffectiveFrom = syllabus.EffectiveFrom,
            EffectiveTo = syllabus.EffectiveTo,
            PacingSchemeJson = syllabus.PacingSchemeJson,
            Overview = syllabus.Overview,
            OverallObjectives = syllabus.OverallObjectives,
            SpecificObjectives = syllabus.SpecificObjectives,
            EthicsAndAttitudes = syllabus.EthicsAndAttitudes,
            BookOverview = syllabus.BookOverview,
            TotalPeriods = syllabus.TotalPeriods,
            MinutesPerPeriod = syllabus.MinutesPerPeriod,
            TotalLessons = syllabus.TotalLessons,
            SourceFileName = syllabus.SourceFileName,
            AttachmentUrl = syllabus.AttachmentUrl,
            RawContentJson = syllabus.RawContentJson,
            IsActive = syllabus.IsActive,
            Units = units,
            Lessons = lessons,
            Resources = resources,
            SessionTemplates = sessionTemplates,
            ImportedLessonPlanTemplateCount = lessonPlanTemplateSummaries.Sum(x => x.ImportedLessonPlanTemplateCount),
            LessonPlanTemplateSummaries = lessonPlanTemplateSummaries
        };
    }
}
