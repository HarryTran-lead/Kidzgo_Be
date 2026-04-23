using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.DeleteHomeworkAssignment;

public sealed class DeleteHomeworkAssignmentCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteHomeworkAssignmentCommand>
{
    public async Task<Result> Handle(
        DeleteHomeworkAssignmentCommand command,
        CancellationToken cancellationToken)
    {
        var homework = await context.HomeworkAssignments
            .FirstOrDefaultAsync(h => h.Id == command.Id, cancellationToken);

        if (homework is null)
        {
            return Result.Failure(HomeworkErrors.NotFound(command.Id));
        }

        bool hasStudentWork = await context.HomeworkStudents.AnyAsync(
            hs => hs.AssignmentId == homework.Id &&
                  (hs.Status == HomeworkStatus.Submitted ||
                   hs.Status == HomeworkStatus.Graded ||
                   hs.Status == HomeworkStatus.Late ||
                   hs.Status == HomeworkStatus.Missing ||
                   hs.StartedAt.HasValue ||
                   hs.SubmittedAt.HasValue ||
                   hs.GradedAt.HasValue ||
                   hs.SubmissionAttempts.Any()),
            cancellationToken);

        if (hasStudentWork)
        {
            return Result.Failure(HomeworkErrors.CannotDeleteWithStudentWork);
        }

        context.HomeworkAssignments.Remove(homework);
        
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

