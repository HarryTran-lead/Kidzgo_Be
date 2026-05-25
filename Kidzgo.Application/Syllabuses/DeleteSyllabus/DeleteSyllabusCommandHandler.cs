using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
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

        var templateIdsQuery = context.LessonPlanTemplates
            .Where(x => x.SyllabusId == command.Id)
            .Select(x => x.Id);

        var lessonPlanCount = await context.LessonPlans
            .CountAsync(
                x => x.TemplateId.HasValue && templateIdsQuery.Contains(x.TemplateId.Value),
                cancellationToken);
        if (lessonPlanCount > 0)
        {
            return Result.Failure<DeleteSyllabusResponse>(SyllabusErrors.HasLessonPlans(lessonPlanCount));
        }

        var templateCount = await context.LessonPlanTemplates
            .CountAsync(x => x.SyllabusId == command.Id, cancellationToken);

        var candidateUnitIds = await context.LessonPlanTemplates
            .Where(x => x.SyllabusId == command.Id && x.LessonPlanUnitId.HasValue)
            .Select(x => x.LessonPlanUnitId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var orphanUnits = candidateUnitIds.Count == 0
            ? []
            : await context.LessonPlanUnits
                .Where(x => candidateUnitIds.Contains(x.Id))
                .Where(x => !context.LessonPlanTemplates.Any(
                    t => t.LessonPlanUnitId == x.Id && t.SyllabusId != command.Id))
                .ToListAsync(cancellationToken);

        context.LessonPlanUnits.RemoveRange(orphanUnits);
        context.Syllabuses.Remove(syllabus);
        await context.SaveChangesAsync(cancellationToken);

        return new DeleteSyllabusResponse
        {
            Id = command.Id,
            DeletedLessonPlanTemplateCount = templateCount,
            DeletedLessonPlanUnitCount = orphanUnits.Count
        };
    }
}
