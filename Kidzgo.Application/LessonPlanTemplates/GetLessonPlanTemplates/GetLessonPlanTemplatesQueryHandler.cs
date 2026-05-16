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
            .Include(t => t.Program)
            .Include(t => t.Module)
            .Include(t => t.CreatedByUser)
            .AsQueryable();

        // Filter by IsDeleted
        if (!query.IncludeDeleted)
        {
            templateQuery = templateQuery.Where(t => !t.IsDeleted);
        }

        // Filter by program
        if (query.ProgramId.HasValue)
        {
            templateQuery = templateQuery.Where(t => t.ProgramId == query.ProgramId.Value);
        }

        // Filter by level
        if (!string.IsNullOrWhiteSpace(query.Level))
        {
            templateQuery = templateQuery.Where(t => t.Level == query.Level);
        }
        
        if (!string.IsNullOrWhiteSpace(query.Title))
        {
            var normalizedTitle = query.Title.ToLower();
            templateQuery = templateQuery.Where(t => t.Title != null && t.Title.ToLower() == normalizedTitle);
        }

        // Filter by IsActive
        if (query.IsActive.HasValue)
        {
            templateQuery = templateQuery.Where(t => t.IsActive == query.IsActive.Value);
        }

        // Get total count
        int totalCount = await templateQuery.CountAsync(cancellationToken);

        // Apply pagination and select
        var templates = await templateQuery
            .OrderBy(t => t.ProgramId)
            .ThenBy(t => t.SessionIndex)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(t => new LessonPlanTemplateDto
            {
                Id = t.Id,
                ProgramId = t.ProgramId,
                ModuleId = t.ModuleId,
                ModuleCode = t.Module != null ? t.Module.Code : null,
                ModuleName = t.Module != null ? t.Module.Name : null,
                ProgramName = t.Program != null ? t.Program.Name : null,
                Level = t.Level,
                Title = t.Title,
                SessionIndex = t.SessionIndex,
                SessionOrder = t.SessionOrder,
                SyllabusMetadata = t.SyllabusMetadata,
                SyllabusContent = t.SyllabusContent,
                SourceFileName = t.SourceFileName,
                Attachment = t.AttachmentUrl,
                IsActive = t.IsActive,
                CreatedBy = t.CreatedBy,
                CreatedByName = t.CreatedByUser != null ? t.CreatedByUser.Name : null,
                CreatedAt = t.CreatedAt,
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

