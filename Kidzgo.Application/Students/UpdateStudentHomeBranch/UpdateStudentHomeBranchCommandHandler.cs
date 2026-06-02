using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Students.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Students.UpdateStudentHomeBranch;

public sealed class UpdateStudentHomeBranchCommandHandler(IDbContext context)
    : ICommandHandler<UpdateStudentHomeBranchCommand, StudentBranchStateDto>
{
    public async Task<Result<StudentBranchStateDto>> Handle(
        UpdateStudentHomeBranchCommand command,
        CancellationToken cancellationToken)
    {
        var studentResult = await StudentBranchAccessHelper.GetActiveStudentAsync(context, command.StudentProfileId, cancellationToken);
        if (studentResult.IsFailure)
        {
            return Result.Failure<StudentBranchStateDto>(studentResult.Error);
        }

        var branchResult = await StudentBranchAccessHelper.EnsureBranchExistsAsync(context, command.BranchId, cancellationToken);
        if (branchResult.IsFailure)
        {
            return Result.Failure<StudentBranchStateDto>(branchResult.Error);
        }

        var now = VietnamTime.UtcNow();
        var state = await context.StudentBranchStates
            .FirstOrDefaultAsync(x => x.StudentProfileId == command.StudentProfileId, cancellationToken);

        if (state is null)
        {
            state = new StudentBranchState
            {
                Id = Guid.NewGuid(),
                StudentProfileId = command.StudentProfileId,
                HomeBranchId = command.BranchId,
                ActiveBranchId = command.BranchId,
                AllowCrossBranchEnrollment = false,
                CreatedAt = now,
                UpdatedAt = now
            };
            context.StudentBranchStates.Add(state);
        }
        else
        {
            state.HomeBranchId = command.BranchId;
            state.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);
        return await StudentBranchReadModelBuilder.BuildAsync(context, command.StudentProfileId, cancellationToken);
    }
}
