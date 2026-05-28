using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanTemplates.GetLessonPlanTemplateById;

public sealed class GetLessonPlanTemplateByIdQueryHandler(
    IDbContext context,
    IUserContext userContext
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
            .Include(t => t.Syllabus)
            .Include(t => t.LessonPlanUnit)
            .Include(t => t.CreatedByUser)
            .FirstOrDefaultAsync(t => t.Id == query.Id && !t.IsDeleted, cancellationToken);

        if (template is null)
        {
            return Result.Failure<GetLessonPlanTemplateByIdResponse>(
                LessonPlanTemplateErrors.NotFound(query.Id));
        }

        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<GetLessonPlanTemplateByIdResponse>(LessonPlanTemplateErrors.Unauthorized);
        }

        if (currentUser.Role == UserRole.Teacher)
        {
            var canAccessTemplate = await context.Sessions
                .AnyAsync(s =>
                        (s.PlannedTeacherId == currentUser.Id ||
                         s.ActualTeacherId == currentUser.Id ||
                         s.Class.MainTeacherId == currentUser.Id ||
                         s.Class.AssistantTeacherId == currentUser.Id) &&
                        (s.LessonPlanTemplateId == template.Id ||
                         (s.LessonPlan != null && s.LessonPlan.TemplateId == template.Id) ||
                         (s.TeachingLog != null &&
                          (s.TeachingLog.PlannedLessonPlanTemplateId == template.Id ||
                           s.TeachingLog.ActualLessonPlanTemplateId == template.Id)) ||
                         s.SessionLessons.Any(sl => sl.LessonPlanTemplateId == template.Id) ||
                         (s.ModuleId == template.ModuleId &&
                          s.SessionIndexInModule == template.SessionIndex &&
                          ((s.Class.SyllabusId.HasValue && s.Class.SyllabusId.Value == template.SyllabusId) ||
                           (s.LessonPlanTemplate != null && s.LessonPlanTemplate.SyllabusId == template.SyllabusId) ||
                           (s.LessonPlan != null &&
                            s.LessonPlan.Template != null &&
                            s.LessonPlan.Template.SyllabusId == template.SyllabusId) ||
                           (s.TeachingLog != null &&
                            ((s.TeachingLog.PlannedLessonPlanTemplate != null &&
                              s.TeachingLog.PlannedLessonPlanTemplate.SyllabusId == template.SyllabusId) ||
                             (s.TeachingLog.ActualLessonPlanTemplate != null &&
                              s.TeachingLog.ActualLessonPlanTemplate.SyllabusId == template.SyllabusId))) ||
                           s.SessionLessons.Any(sl =>
                               sl.LessonPlanTemplate != null &&
                               sl.LessonPlanTemplate.SyllabusId == template.SyllabusId)))),
                    cancellationToken);

            if (!canAccessTemplate)
            {
                return Result.Failure<GetLessonPlanTemplateByIdResponse>(LessonPlanTemplateErrors.Unauthorized);
            }
        }

        var usedCount = await context.LessonPlans
            .CountAsync(lp => lp.TemplateId == template.Id && !lp.IsDeleted, cancellationToken);

        return new GetLessonPlanTemplateByIdResponse
        {
            Id = template.Id,
            SyllabusId = template.SyllabusId,
            SyllabusCode = template.Syllabus.Code,
            SyllabusVersion = template.Syllabus.Version,
            SyllabusTitle = template.Syllabus.Title,
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
