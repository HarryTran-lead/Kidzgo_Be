using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
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
            .Include(t => t.Module)
                .ThenInclude(t => t.Level)
                    .ThenInclude(t => t.Program)
            .Include(t => t.LessonPlanUnit)
            .Include(t => t.CreatedByUser)
            .FirstOrDefaultAsync(t => t.Id == query.Id && !t.IsDeleted, cancellationToken);

        if (template is null)
        {
            return Result.Failure<GetLessonPlanTemplateByIdResponse>(
                LessonPlanTemplateErrors.NotFound(query.Id));
        }

        var usedCount = await context.LessonPlans
            .CountAsync(lp => lp.TemplateId == template.Id && !lp.IsDeleted, cancellationToken);

        return new GetLessonPlanTemplateByIdResponse
        {
            Id = template.Id,
            ModuleId = template.ModuleId,
            ModuleCode = template.Module.Code,
            ModuleName = template.Module.Name,
            LessonPlanUnitId = template.LessonPlanUnitId,
            LessonPlanUnitName = template.LessonPlanUnit?.Name,
            OrderIndexInUnit = template.OrderIndexInUnit,
            LevelId = template.Module.LevelId,
            LevelName = template.Module.Level.Name,
            ProgramId = template.Module.Level.ProgramId,
            ProgramName = template.Module.Level.Program.Name,
            Title = template.Title,
            SessionIndex = template.SessionIndex,
            SessionOrder = template.SessionOrder,
            SyllabusMetadata = template.SyllabusMetadata,
            SyllabusContent = template.SyllabusContent,
            Objectives = template.Objectives,
            LanguageContent = template.LanguageContent,
            Vocabulary = template.Vocabulary,
            Grammar = template.Grammar,
            TeachingMethodology = template.TeachingMethodology,
            TeacherMaterials = template.TeacherMaterials,
            StudentMaterials = template.StudentMaterials,
            Procedure = template.Procedure,
            Evaluation = template.Evaluation,
            SourceFileName = template.SourceFileName,
            Attachment = template.AttachmentUrl,
            IsActive = template.IsActive,
            CreatedBy = template.CreatedBy,
            CreatedByName = template.CreatedByUser?.Name,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            UsedCount = usedCount
        };
    }
}
