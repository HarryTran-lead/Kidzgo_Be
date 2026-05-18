using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanTemplates.GetLessonPlanTemplateById;

public sealed class GetLessonPlanTemplateByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetLessonPlanTemplateByIdQuery, GetLessonPlanTemplateByIdResponse>
{
    public async Task<Result<GetLessonPlanTemplateByIdResponse>> Handle(
        GetLessonPlanTemplateByIdQuery query,
        CancellationToken cancellationToken)
    {
        var template = await context.LessonPlanTemplates
            .Include(t => t.Program)
            .Include(t => t.Module)
            .Include(t => t.CreatedByUser)
            .FirstOrDefaultAsync(t => t.Id == query.Id && !t.IsDeleted, cancellationToken);

        if (template is null)
        {
            return Result.Failure<GetLessonPlanTemplateByIdResponse>(
                LessonPlanTemplateErrors.NotFound(query.Id));
        }

        // Count how many lesson plans are using this template
        var usedCount = await context.LessonPlans
            .CountAsync(lp => lp.TemplateId == template.Id && !lp.IsDeleted, cancellationToken);

        return new GetLessonPlanTemplateByIdResponse
        {
            Id = template.Id,
            ProgramId = template.ProgramId,
            ModuleId = template.ModuleId,
            ModuleCode = template.Module?.Code,
            ModuleName = template.Module?.Name,
            ProgramName = template.Program?.Name,
            Level = template.Level,
            Title = template.Title,
            SessionIndex = template.SessionIndex,
            SessionOrder = template.SessionOrder,
            SyllabusMetadata = template.SyllabusMetadata,
            SyllabusContent = template.SyllabusContent,
            SourceFileName = template.SourceFileName,
            Attachment = template.AttachmentUrl,
            IsActive = template.IsActive,
            CreatedBy = template.CreatedBy,
            CreatedByName = template.CreatedByUser?.Name,
            CreatedAt = template.CreatedAt,
            UsedCount = usedCount
        };
    }
}
