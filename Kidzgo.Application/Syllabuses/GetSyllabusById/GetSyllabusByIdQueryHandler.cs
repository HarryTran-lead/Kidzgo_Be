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
            .Where(x => x.Id == query.Id && !x.IsDeleted)
            .Select(x => new GetSyllabusByIdResponse
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
                IsActive = x.IsActive,
                Units = x.Units.OrderBy(u => u.OrderIndex).Select(u => new SyllabusUnitDetailDto
                {
                    Id = u.Id,
                    ModuleId = u.ModuleId,
                    ModuleName = u.Module != null ? u.Module.Name : null,
                    Name = u.Name,
                    AllocatedPeriods = u.AllocatedPeriods,
                    LessonCount = u.LessonCount,
                    OrderIndex = u.OrderIndex,
                    Notes = u.Notes
                }).ToList(),
                Lessons = x.Lessons.OrderBy(l => l.OrderIndex).Select(l => new SyllabusLessonDetailDto
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
                }).ToList(),
                Resources = x.Resources.OrderBy(r => r.OrderIndex).Select(r => new SyllabusResourceDetailDto
                {
                    Id = r.Id,
                    DocumentName = r.DocumentName,
                    Abbreviation = r.Abbreviation,
                    IntendedUsers = r.IntendedUsers,
                    Notes = r.Notes,
                    OrderIndex = r.OrderIndex
                }).ToList(),
                SessionTemplates = x.SessionTemplates.OrderBy(s => s.OrderIndex).Select(s => new SyllabusSessionTemplateDetailDto
                {
                    Id = s.Id,
                    ModuleId = s.ModuleId,
                    ModuleName = s.Module != null ? s.Module.Name : null,
                    LessonPlanTemplateId = s.LessonPlanTemplateId,
                    SessionIndex = s.SessionIndex,
                    SessionIndexInModule = s.SessionIndexInModule,
                    LessonNumber = s.LessonNumber,
                    Title = s.Title,
                    Topic = s.Topic,
                    ObjectiveSummary = s.ObjectiveSummary,
                    VocabularySummary = s.VocabularySummary,
                    GrammarSummary = s.GrammarSummary,
                    OrderIndex = s.OrderIndex
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (syllabus is null)
        {
            return Result.Failure<GetSyllabusByIdResponse>(SyllabusErrors.NotFound(query.Id));
        }

        return syllabus;
    }
}
