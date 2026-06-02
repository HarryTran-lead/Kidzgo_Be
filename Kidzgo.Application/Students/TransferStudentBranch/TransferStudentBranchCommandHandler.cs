using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Students.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Students.TransferStudentBranch;

public sealed class TransferStudentBranchCommandHandler(IDbContext context)
    : ICommandHandler<TransferStudentBranchCommand, StudentBranchStateDto>
{
    public async Task<Result<StudentBranchStateDto>> Handle(
        TransferStudentBranchCommand command,
        CancellationToken cancellationToken)
    {
        var studentResult = await StudentBranchAccessHelper.GetActiveStudentAsync(context, command.StudentProfileId, cancellationToken);
        if (studentResult.IsFailure)
        {
            return Result.Failure<StudentBranchStateDto>(studentResult.Error);
        }

        var fromBranchResult = await StudentBranchAccessHelper.EnsureBranchExistsAsync(context, command.FromBranchId, cancellationToken);
        if (fromBranchResult.IsFailure)
        {
            return Result.Failure<StudentBranchStateDto>(fromBranchResult.Error);
        }

        var toBranchResult = await StudentBranchAccessHelper.EnsureBranchExistsAsync(context, command.ToBranchId, cancellationToken);
        if (toBranchResult.IsFailure)
        {
            return Result.Failure<StudentBranchStateDto>(toBranchResult.Error);
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
                HomeBranchId = command.FromBranchId,
                ActiveBranchId = command.FromBranchId,
                AllowCrossBranchEnrollment = false,
                CreatedAt = now,
                UpdatedAt = now
            };
            context.StudentBranchStates.Add(state);
        }

        if (state.ActiveBranchId != command.FromBranchId)
        {
            return Result.Failure<StudentBranchStateDto>(
                StudentBranchErrors.TransferCurrentBranchMismatch(command.FromBranchId, state.ActiveBranchId));
        }

        var hasExternalOperationalEnrollments = await StudentBranchAccessHelper.HasOperationalEnrollmentsOutsideBranchAsync(
            context,
            command.StudentProfileId,
            command.ToBranchId,
            cancellationToken);

        if (hasExternalOperationalEnrollments)
        {
            if (!command.KeepCurrentClass)
            {
                return Result.Failure<StudentBranchStateDto>(
                    StudentBranchErrors.ActiveEnrollmentsRequireResolution(command.ToBranchId));
            }

            if (!command.AllowCrossBranchEnrollment)
            {
                return Result.Failure<StudentBranchStateDto>(
                    StudentBranchErrors.KeepCurrentClassRequiresCrossBranchPermission);
            }
        }

        state.HomeBranchId = command.ToBranchId;
        state.ActiveBranchId = command.ToBranchId;
        state.AllowCrossBranchEnrollment = command.KeepCurrentClass && command.AllowCrossBranchEnrollment;
        state.LastTransferredAt = VietnamTime.TreatAsVietnamLocal(command.EffectiveDate.ToDateTime(TimeOnly.MinValue));
        state.UpdatedAt = now;

        context.StudentBranchTransfers.Add(new StudentBranchTransfer
        {
            Id = Guid.NewGuid(),
            StudentProfileId = command.StudentProfileId,
            FromBranchId = command.FromBranchId,
            ToBranchId = command.ToBranchId,
            EffectiveDate = command.EffectiveDate,
            Reason = string.IsNullOrWhiteSpace(command.Reason) ? null : command.Reason.Trim(),
            KeepCurrentClass = command.KeepCurrentClass,
            AllowCrossBranchEnrollment = command.KeepCurrentClass && command.AllowCrossBranchEnrollment,
            CreatedAt = now,
            UpdatedAt = now
        });

        await context.SaveChangesAsync(cancellationToken);
        return await StudentBranchReadModelBuilder.BuildAsync(context, command.StudentProfileId, cancellationToken);
    }
}
