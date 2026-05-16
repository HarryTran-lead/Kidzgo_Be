using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
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

        if (currentUser is null)
        {
            return Result.Failure<UpdateLessonPlanTemplateResponse>(LessonPlanTemplateErrors.Unauthorized);
        }

        if (currentUser.Role == UserRole.Teacher)
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

        if (command.ModuleId.HasValue)
        {
            var moduleExists = await context.Modules
                .Include(x => x.Level)
                .AnyAsync(x => x.Id == command.ModuleId.Value && x.Level.ProgramId == template.ProgramId, cancellationToken);
            if (!moduleExists)
            {
                return Result.Failure<UpdateLessonPlanTemplateResponse>(
                    Error.Validation("LessonPlanTemplate.ModuleInvalid", "Module does not belong to the selected program."));
            }
        }

        // Validate session index if provided
        if (command.SessionIndex.HasValue)
        {
            if (command.SessionIndex.Value <= 0)
            {
                return Result.Failure<UpdateLessonPlanTemplateResponse>(
                    LessonPlanTemplateErrors.SessionIndexRequired);
            }

            // Check for duplicate session index in the same program
            var duplicateExists = await context.LessonPlanTemplates
                .AnyAsync(t => t.ProgramId == template.ProgramId && 
                              t.SessionIndex == command.SessionIndex.Value && 
                              t.Id != command.Id && 
                              !t.IsDeleted, 
                       cancellationToken);

            if (duplicateExists)
            {
                return Result.Failure<UpdateLessonPlanTemplateResponse>(
                    LessonPlanTemplateErrors.DuplicateSessionIndex(template.ProgramId, command.SessionIndex.Value));
            }
        }

        // Update fields
        if (command.ModuleId.HasValue)
        {
            template.ModuleId = command.ModuleId;
        }

        if (command.Level != null)
        {
            template.Level = command.Level;
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

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateLessonPlanTemplateResponse
        {
            Id = template.Id,
            ProgramId = template.ProgramId,
            ModuleId = template.ModuleId,
            Title = template.Title,
            Level = template.Level,
            SessionIndex = template.SessionIndex,
            SessionOrder = template.SessionOrder,
            SyllabusMetadata = template.SyllabusMetadata,
            SyllabusContent = template.SyllabusContent,
            SourceFileName = template.SourceFileName,
            Attachment = template.AttachmentUrl,
            IsActive = template.IsActive
        };
    }
}

