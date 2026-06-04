using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LessonPlanTemplates.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanTemplates.HardDeleteLessonPlanTemplate;

public sealed class HardDeleteLessonPlanTemplateCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<HardDeleteLessonPlanTemplateCommand, HardDeleteLessonPlanTemplateResponse>
{
    public async Task<Result<HardDeleteLessonPlanTemplateResponse>> Handle(
        HardDeleteLessonPlanTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (currentUser is null || currentUser.Role == UserRole.Teacher)
        {
            return Result.Failure<HardDeleteLessonPlanTemplateResponse>(LessonPlanTemplateErrors.Unauthorized);
        }

        var template = await context.LessonPlanTemplates
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (template is null)
        {
            return Result.Failure<HardDeleteLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.NotFound(command.Id));
        }

        var now = VietnamTime.UtcNow();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var deleteResult = await LessonPlanTemplateHardDeleteHelper.HardDeleteAsync(
            context,
            [command.Id],
            now,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new HardDeleteLessonPlanTemplateResponse
        {
            Id = command.Id,
            DeletedLessonPlanCount = deleteResult.DeletedLessonPlanCount,
            DeletedLessonPlanUnitCount = deleteResult.DeletedLessonPlanUnitCount
        };
    }
}
