using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.ResyncFutureLessons;

public sealed class ResyncFutureLessonsCommandHandler(
    IDbContext context,
    ClassProgressionService classProgressionService
) : ICommandHandler<ResyncFutureLessonsCommand, ResyncFutureLessonsResponse>
{
    public async Task<Result<ResyncFutureLessonsResponse>> Handle(
        ResyncFutureLessonsCommand command,
        CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .FirstOrDefaultAsync(x => x.Id == command.ClassId, cancellationToken);
        if (classEntity is null)
        {
            return Result.Failure<ResyncFutureLessonsResponse>(ClassErrors.NotFound(command.ClassId));
        }

        var syncResult = await classProgressionService.RecalculateAndResyncAsync(
            command.ClassId,
            resyncFutureSessions: true,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new ResyncFutureLessonsResponse
        {
            ClassId = classEntity.Id,
            UpdatedSessionCount = syncResult?.UpdatedFutureSessionCount ?? 0,
            CurrentModuleId = classEntity.CurrentModuleId,
            CurrentSessionIndex = classEntity.CurrentSessionIndex,
            CurrentLessonPlanTemplateId = classEntity.CurrentLessonPlanTemplateId
        };
    }
}
