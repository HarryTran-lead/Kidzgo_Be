using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Students.Shared;

internal static class StudentBranchAccessHelper
{
    internal sealed class StudentBranchValidationResult
    {
        public required StudentBranchState State { get; init; }
        public required bool IsCrossBranch { get; init; }
    }

    public static async Task<Result<StudentBranchValidationResult>> ValidateBranchAccessAsync(
        IDbContext context,
        Guid studentProfileId,
        Guid targetBranchId,
        bool allowCrossBranchEnrollment,
        CancellationToken cancellationToken)
    {
        var state = await context.StudentBranchStates
            .FirstOrDefaultAsync(x => x.StudentProfileId == studentProfileId, cancellationToken);

        var now = VietnamTime.UtcNow();
        if (state is null)
        {
            state = new StudentBranchState
            {
                Id = Guid.NewGuid(),
                StudentProfileId = studentProfileId,
                HomeBranchId = targetBranchId,
                ActiveBranchId = targetBranchId,
                AllowCrossBranchEnrollment = allowCrossBranchEnrollment,
                LastTransferredAt = null,
                CreatedAt = now,
                UpdatedAt = now
            };

            context.StudentBranchStates.Add(state);

            return Result.Success(new StudentBranchValidationResult
            {
                State = state,
                IsCrossBranch = false
            });
        }

        var isCrossBranch = state.ActiveBranchId != targetBranchId;
        if (isCrossBranch && !(allowCrossBranchEnrollment || state.AllowCrossBranchEnrollment))
        {
            return Result.Failure<StudentBranchValidationResult>(
                StudentBranchErrors.CrossBranchEnrollmentNotAllowed(
                    studentProfileId,
                    state.ActiveBranchId,
                    targetBranchId));
        }

        if (allowCrossBranchEnrollment && !state.AllowCrossBranchEnrollment)
        {
            state.AllowCrossBranchEnrollment = true;
            state.UpdatedAt = now;
        }

        return Result.Success(new StudentBranchValidationResult
        {
            State = state,
            IsCrossBranch = isCrossBranch
        });
    }

    public static async Task<Result<Profile>> GetActiveStudentAsync(
        IDbContext context,
        Guid studentProfileId,
        CancellationToken cancellationToken)
    {
        var student = await context.Profiles
            .FirstOrDefaultAsync(
                x => x.Id == studentProfileId &&
                     x.ProfileType == ProfileType.Student &&
                     x.IsActive &&
                     !x.IsDeleted,
                cancellationToken);

        return student is null
            ? Result.Failure<Profile>(StudentBranchErrors.StudentNotFound(studentProfileId))
            : Result.Success(student);
    }

    public static async Task<Result> EnsureBranchExistsAsync(
        IDbContext context,
        Guid branchId,
        CancellationToken cancellationToken)
    {
        var exists = await context.Branches.AnyAsync(x => x.Id == branchId && x.IsActive, cancellationToken);
        return exists
            ? Result.Success()
            : Result.Failure(StudentBranchErrors.BranchNotFound(branchId));
    }

    public static async Task<bool> HasOperationalEnrollmentsOutsideBranchAsync(
        IDbContext context,
        Guid studentProfileId,
        Guid targetBranchId,
        CancellationToken cancellationToken)
    {
        return await context.ClassEnrollments.AnyAsync(
            x => x.StudentProfileId == studentProfileId &&
                 (x.Status == Domain.Classes.EnrollmentStatus.Active || x.Status == Domain.Classes.EnrollmentStatus.Paused) &&
                 x.Class.BranchId != targetBranchId &&
                 x.Class.Status != Domain.Classes.ClassStatus.Closed &&
                 x.Class.Status != Domain.Classes.ClassStatus.Completed &&
                 x.Class.Status != Domain.Classes.ClassStatus.Cancelled,
            cancellationToken);
    }
}
