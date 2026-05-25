using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanTemplates.UpdateLessonPlanTemplate;

public sealed class UpdateLessonPlanTemplateCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<UpdateLessonPlanTemplateCommand, UpdateLessonPlanTemplateResponse>
{
    public async Task<Result<UpdateLessonPlanTemplateResponse>> Handle(
        UpdateLessonPlanTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (currentUser is null || currentUser.Role == UserRole.Teacher)
        {
            return Result.Failure<UpdateLessonPlanTemplateResponse>(LessonPlanTemplateErrors.Unauthorized);
        }

        var template = await context.LessonPlanTemplates
            .FirstOrDefaultAsync(t => t.Id == command.Id && !t.IsDeleted, cancellationToken);
        if (template is null)
        {
            return Result.Failure<UpdateLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.NotFound(command.Id));
        }

        var targetModuleId = command.ModuleId ?? template.ModuleId;
        var module = await context.Modules
            .Select(x => new { x.Id, x.LevelId, x.PlannedSessionCount })
            .FirstOrDefaultAsync(x => x.Id == targetModuleId, cancellationToken);
        if (module is null)
        {
            return Result.Failure<UpdateLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.ModuleNotFound(targetModuleId));
        }

        var targetSyllabusId = command.SyllabusId ?? template.SyllabusId;
        var syllabus = await context.Syllabuses
            .AsNoTracking()
            .Select(x => new { x.Id, x.LevelId, x.IsActive, x.IsDeleted })
            .FirstOrDefaultAsync(x => x.Id == targetSyllabusId, cancellationToken);
        if (syllabus is null || syllabus.IsDeleted || !syllabus.IsActive)
        {
            return Result.Failure<UpdateLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.SyllabusNotFound(targetSyllabusId));
        }

        if (syllabus.LevelId != module.LevelId)
        {
            return Result.Failure<UpdateLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.SyllabusModuleMismatch(targetSyllabusId, targetModuleId));
        }

        var targetSessionIndex = command.SessionIndex ?? template.SessionIndex;
        if (targetSessionIndex <= 0)
        {
            return Result.Failure<UpdateLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.SessionIndexRequired);
        }

        if (targetSessionIndex > module.PlannedSessionCount)
        {
            return Result.Failure<UpdateLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.SessionIndexOutOfRange(targetSessionIndex, module.PlannedSessionCount));
        }

        var duplicateExists = await context.LessonPlanTemplates
            .AnyAsync(
                t => t.ModuleId == targetModuleId &&
                     t.SyllabusId == targetSyllabusId &&
                     t.SessionIndex == targetSessionIndex &&
                     t.Id != command.Id &&
                     !t.IsDeleted,
                cancellationToken);
        if (duplicateExists)
        {
            return Result.Failure<UpdateLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.DuplicateSessionIndex(targetModuleId, targetSessionIndex));
        }

        var shouldClearUnitForModuleMove = command.ModuleId.HasValue && !command.LessonPlanUnitId.HasValue;
        if (command.LessonPlanUnitId.HasValue)
        {
            var unit = await context.LessonPlanUnits
                .FirstOrDefaultAsync(
                    x => x.Id == command.LessonPlanUnitId.Value && x.IsActive,
                    cancellationToken);

            if (unit is null)
            {
                return Result.Failure<UpdateLessonPlanTemplateResponse>(
                    LessonPlanUnitErrors.NotFound(command.LessonPlanUnitId.Value));
            }

            if (unit.ModuleId != targetModuleId)
            {
                return Result.Failure<UpdateLessonPlanTemplateResponse>(
                    LessonPlanUnitErrors.LessonMustStayInSameModule);
            }

            template.LessonPlanUnitId = unit.Id;
            template.OrderIndexInUnit = command.OrderIndexInUnit.HasValue
                ? Math.Max(command.OrderIndexInUnit.Value, 0)
                : template.OrderIndexInUnit;
        }
        else if (shouldClearUnitForModuleMove)
        {
            template.LessonPlanUnitId = null;
            template.OrderIndexInUnit = 0;
        }
        else if (command.OrderIndexInUnit.HasValue && template.LessonPlanUnitId.HasValue)
        {
            template.OrderIndexInUnit = Math.Max(command.OrderIndexInUnit.Value, 0);
        }

        if (command.ModuleId.HasValue)
        {
            template.ModuleId = command.ModuleId.Value;
        }

        if (command.SyllabusId.HasValue)
        {
            template.SyllabusId = command.SyllabusId.Value;
        }

        if (command.Title != null)
        {
            template.Title = command.Title;
        }

        if (command.SessionIndex.HasValue)
        {
            template.SessionIndex = command.SessionIndex.Value;
        }

        if (command.SessionOrder.HasValue)
        {
            template.SessionOrder = command.SessionOrder.Value;
        }

        if (command.SyllabusMetadata != null)
        {
            template.SyllabusMetadata = command.SyllabusMetadata;
        }

        if (command.SyllabusContent != null)
        {
            template.SyllabusContent = command.SyllabusContent;
        }

        if (command.Objectives != null)
        {
            template.Objectives = command.Objectives;
        }

        if (command.LanguageContent != null)
        {
            template.LanguageContent = command.LanguageContent;
        }

        if (command.Vocabulary != null)
        {
            template.Vocabulary = command.Vocabulary;
        }

        if (command.Grammar != null)
        {
            template.Grammar = command.Grammar;
        }

        if (command.TeachingMethodology != null)
        {
            template.TeachingMethodology = command.TeachingMethodology;
        }

        if (command.TeacherMaterials != null)
        {
            template.TeacherMaterials = command.TeacherMaterials;
        }

        if (command.StudentMaterials != null)
        {
            template.StudentMaterials = command.StudentMaterials;
        }

        if (command.Procedure != null)
        {
            template.Procedure = command.Procedure;
        }

        if (command.Evaluation != null)
        {
            template.Evaluation = command.Evaluation;
        }

        if (command.SourceFileName != null)
        {
            template.SourceFileName = command.SourceFileName;
        }

        if (command.Attachment != null)
        {
            template.AttachmentUrl = command.Attachment;
        }

        if (command.IsActive.HasValue)
        {
            template.IsActive = command.IsActive.Value;
        }

        template.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateLessonPlanTemplateResponse
        {
            Id = template.Id,
            SyllabusId = template.SyllabusId,
            ModuleId = template.ModuleId,
            LessonPlanUnitId = template.LessonPlanUnitId,
            OrderIndexInUnit = template.OrderIndexInUnit,
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
            UpdatedAt = template.UpdatedAt
        };
    }
}
