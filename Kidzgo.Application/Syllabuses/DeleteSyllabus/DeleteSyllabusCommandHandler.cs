using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LessonPlanTemplates.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.DeleteSyllabus;

public sealed class DeleteSyllabusCommandHandler(IDbContext context)
    : ICommandHandler<DeleteSyllabusCommand, DeleteSyllabusResponse>
{
    public async Task<Result<DeleteSyllabusResponse>> Handle(
        DeleteSyllabusCommand command,
        CancellationToken cancellationToken)
    {
        var syllabus = await context.Syllabuses
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (syllabus is null)
        {
            return Result.Failure<DeleteSyllabusResponse>(SyllabusErrors.NotFound(command.Id));
        }

        var classCount = await context.Classes
            .CountAsync(x => x.SyllabusId == command.Id, cancellationToken);
        if (classCount > 0)
        {
            return Result.Failure<DeleteSyllabusResponse>(SyllabusErrors.HasClasses(classCount));
        }

        var templateIds = await context.LessonPlanTemplates
            .Where(x => x.SyllabusId == command.Id)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var now = VietnamTime.UtcNow();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var deleteResult = await LessonPlanTemplateHardDeleteHelper.HardDeleteAsync(
            context,
            templateIds,
            now,
            cancellationToken);

        context.Syllabuses.Remove(syllabus);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new DeleteSyllabusResponse
        {
            Id = command.Id,
            DeletedLessonPlanCount = deleteResult.DeletedLessonPlanCount,
            DeletedLessonPlanTemplateCount = deleteResult.DeletedLessonPlanTemplateCount,
            DeletedLessonPlanUnitCount = deleteResult.DeletedLessonPlanUnitCount
        };
    }
}
