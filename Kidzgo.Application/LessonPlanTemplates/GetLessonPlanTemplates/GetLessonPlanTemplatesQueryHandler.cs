using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanTemplates.GetLessonPlanTemplates;

public sealed class GetLessonPlanTemplatesQueryHandler(
    IDbContext context
) : IQueryHandler<GetLessonPlanTemplatesQuery, GetLessonPlanTemplatesResponse>
{
    public async Task<Result<GetLessonPlanTemplatesResponse>> Handle(
        GetLessonPlanTemplatesQuery query,
        CancellationToken cancellationToken)
    {
        var templateQuery = context.LessonPlanTemplates
            .Include(t => t.Module)
                .ThenInclude(t => t.Level)
                    .ThenInclude(t => t.Program)
            .Include(t => t.CreatedByUser)
            .AsQueryable();

        if (!query.IncludeDeleted)
        {
            templateQuery = templateQuery.Where(t => !t.IsDeleted);
        }

        if (query.ModuleId.HasValue)
        {
            templateQuery = templateQuery.Where(t => t.ModuleId == query.ModuleId.Value);
        }

        if (query.SyllabusId.HasValue)
        {
            templateQuery = templateQuery.Where(t => t.SyllabusId == query.SyllabusId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Title))
        {
            var normalizedTitle = query.Title.ToLower();
            templateQuery = templateQuery.Where(t => t.Title != null && t.Title.ToLower() == normalizedTitle);
        }

        if (query.IsActive.HasValue)
        {
            templateQuery = templateQuery.Where(t => t.IsActive == query.IsActive.Value);
        }

        var totalCount = await templateQuery.CountAsync(cancellationToken);

        var templates = await templateQuery
            .OrderBy(t => t.Module.Level.Order)
            .ThenBy(t => t.Module.Order)
            .ThenBy(t => t.SessionOrder)
            .ThenBy(t => t.SessionIndex)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(t => new LessonPlanTemplateDto
            {
                Id = t.Id,
                SyllabusId = t.SyllabusId,
                SyllabusCode = t.Syllabus.Code,
                SyllabusVersion = t.Syllabus.Version,
                SyllabusTitle = t.Syllabus.Title,
                ModuleId = t.ModuleId,
                ModuleCode = t.Module.Code,
                ModuleName = t.Module.Name,
                LessonPlanUnitId = t.LessonPlanUnitId,
                LessonPlanUnitName = t.LessonPlanUnit != null ? t.LessonPlanUnit.Name : null,
                OrderIndexInUnit = t.OrderIndexInUnit,
                LevelId = t.Module.LevelId,
                LevelName = t.Module.Level.Name,
                ProgramId = t.Module.Level.ProgramId,
                ProgramName = t.Module.Level.Program.Name,
                Title = t.Title,
                SessionIndex = t.SessionIndex,
                SessionOrder = t.SessionOrder,
                SyllabusMetadata = t.SyllabusMetadata,
                SyllabusContent = t.SyllabusContent,
                Objectives = t.Objectives,
                LanguageContent = t.LanguageContent,
                Vocabulary = t.Vocabulary,
                Grammar = t.Grammar,
                TeachingMethodology = t.TeachingMethodology,
                TeacherMaterials = t.TeacherMaterials,
                StudentMaterials = t.StudentMaterials,
                Procedure = t.Procedure,
                Evaluation = t.Evaluation,
                SourceFileName = t.SourceFileName,
                Attachment = t.AttachmentUrl,
                IsActive = t.IsActive,
                CreatedBy = t.CreatedBy,
                CreatedByName = t.CreatedByUser != null ? t.CreatedByUser.Name : null,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                UsedCount = t.LessonPlans.Count(lp => !lp.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        var page = new Page<LessonPlanTemplateDto>(
            templates,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetLessonPlanTemplatesResponse
        {
            Templates = page
        };
    }
}
