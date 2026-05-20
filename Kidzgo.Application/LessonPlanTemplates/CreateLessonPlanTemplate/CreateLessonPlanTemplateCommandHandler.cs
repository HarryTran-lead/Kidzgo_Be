using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LessonPlanTemplates.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanTemplates.CreateLessonPlanTemplate;

public sealed class CreateLessonPlanTemplateCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateLessonPlanTemplateCommand, CreateLessonPlanTemplateResponse>
{
    public async Task<Result<CreateLessonPlanTemplateResponse>> Handle(
        CreateLessonPlanTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var module = await context.Modules
            .FirstOrDefaultAsync(x => x.Id == command.ModuleId, cancellationToken);
        if (module is null)
        {
            return Result.Failure<CreateLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.ModuleNotFound(command.ModuleId));
        }

        if (command.SessionIndex <= 0)
        {
            return Result.Failure<CreateLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.SessionIndexRequired);
        }

        if (command.SessionIndex > module.PlannedSessionCount)
        {
            return Result.Failure<CreateLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.SessionIndexOutOfRange(command.SessionIndex, module.PlannedSessionCount));
        }

        var duplicateExists = await context.LessonPlanTemplates
            .AnyAsync(
                t => t.ModuleId == command.ModuleId &&
                     t.SessionIndex == command.SessionIndex &&
                     !t.IsDeleted,
                cancellationToken);
        if (duplicateExists)
        {
            return Result.Failure<CreateLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.DuplicateSessionIndex(command.ModuleId, command.SessionIndex));
        }

        var currentUserId = userContext.UserId;
        var now = VietnamTime.UtcNow();
        LessonPlanUnit? lessonPlanUnit = null;
        if (command.LessonPlanUnitId.HasValue)
        {
            lessonPlanUnit = await context.LessonPlanUnits
                .FirstOrDefaultAsync(
                    x => x.Id == command.LessonPlanUnitId.Value && x.IsActive,
                    cancellationToken);

            if (lessonPlanUnit is null)
            {
                return Result.Failure<CreateLessonPlanTemplateResponse>(
                    LessonPlanUnitErrors.NotFound(command.LessonPlanUnitId.Value));
            }

            if (lessonPlanUnit.ModuleId != command.ModuleId)
            {
                return Result.Failure<CreateLessonPlanTemplateResponse>(
                    LessonPlanUnitErrors.LessonMustStayInSameModule);
            }
        }

        var template = new LessonPlanTemplate
        {
            Id = Guid.NewGuid(),
            ModuleId = command.ModuleId,
            LessonPlanUnitId = lessonPlanUnit?.Id,
            OrderIndexInUnit = lessonPlanUnit is null
                ? 0
                : command.OrderIndexInUnit.HasValue
                    ? Math.Max(command.OrderIndexInUnit.Value, 0)
                    : await LessonPlanUnitResolver.GetNextOrderInUnitAsync(
                        context,
                        lessonPlanUnit.Id,
                        cancellationToken),
            Title = command.Title,
            SessionIndex = command.SessionIndex,
            SessionOrder = command.SessionOrder ?? command.SessionIndex,
            SyllabusMetadata = command.SyllabusMetadata,
            SyllabusContent = command.SyllabusContent,
            Objectives = command.Objectives,
            LanguageContent = command.LanguageContent,
            Vocabulary = command.Vocabulary,
            Grammar = command.Grammar,
            TeachingMethodology = command.TeachingMethodology,
            TeacherMaterials = command.TeacherMaterials,
            StudentMaterials = command.StudentMaterials,
            Procedure = command.Procedure,
            Evaluation = command.Evaluation,
            SourceFileName = command.SourceFileName,
            AttachmentUrl = command.Attachment,
            IsActive = true,
            IsDeleted = false,
            CreatedBy = currentUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.LessonPlanTemplates.Add(template);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateLessonPlanTemplateResponse
        {
            Id = template.Id,
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
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }
}
