using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Branches.DeleteBranchSyllabusAssignment;

public sealed class DeleteBranchSyllabusAssignmentCommandHandler(IDbContext context)
    : ICommandHandler<DeleteBranchSyllabusAssignmentCommand, DeleteBranchSyllabusAssignmentResponse>
{
    public async Task<Result<DeleteBranchSyllabusAssignmentResponse>> Handle(
        DeleteBranchSyllabusAssignmentCommand command,
        CancellationToken cancellationToken)
    {
        var assignment = await context.CurriculumAssignments
            .FirstOrDefaultAsync(
                x => x.Id == command.AssignmentId && x.BranchId == command.BranchId,
                cancellationToken);

        if (assignment is null)
        {
            return Result.Failure<DeleteBranchSyllabusAssignmentResponse>(
                CurriculumAssignmentErrors.NotFound(command.AssignmentId, command.BranchId));
        }

        var hasOperationalClasses = await context.Classes.AnyAsync(
            x => x.BranchId == command.BranchId &&
                 x.SyllabusId == assignment.SyllabusId &&
                 x.Status != ClassStatus.Closed &&
                 x.Status != ClassStatus.Completed &&
                 x.Status != ClassStatus.Cancelled,
            cancellationToken);

        if (hasOperationalClasses)
        {
            return Result.Failure<DeleteBranchSyllabusAssignmentResponse>(
                CurriculumAssignmentErrors.HasOperationalClasses(command.AssignmentId, assignment.SyllabusId));
        }

        context.CurriculumAssignments.Remove(assignment);
        await context.SaveChangesAsync(cancellationToken);

        return new DeleteBranchSyllabusAssignmentResponse
        {
            BranchId = command.BranchId,
            AssignmentId = command.AssignmentId,
            SyllabusId = assignment.SyllabusId
        };
    }
}
