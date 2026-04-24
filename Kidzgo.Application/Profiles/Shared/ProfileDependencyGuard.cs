using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.Shared;

internal static class ProfileDependencyGuard
{
    public static async Task<Result> EnsureCanDeactivateOrDeleteAsync(
        IDbContext context,
        Profile profile,
        CancellationToken cancellationToken)
    {
        if (profile.ProfileType == ProfileType.Student)
        {
            bool hasActiveEnrollments = await context.ClassEnrollments.AnyAsync(
                e => e.StudentProfileId == profile.Id &&
                     (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Paused),
                cancellationToken);

            if (hasActiveEnrollments)
            {
                return Result.Failure(ProfileErrors.HasActiveEnrollments);
            }

            bool hasFutureSessions = await context.StudentSessionAssignments.AnyAsync(
                a => a.StudentProfileId == profile.Id &&
                     a.Status == StudentSessionAssignmentStatus.Assigned &&
                     a.Session.Status == SessionStatus.Scheduled &&
                     a.Session.PlannedDatetime >= VietnamTime.UtcNow(),
                cancellationToken);

            if (hasFutureSessions)
            {
                return Result.Failure(ProfileErrors.HasFutureSessions);
            }
        }

        if (profile.ProfileType == ProfileType.Parent)
        {
            bool hasActiveStudentLinks = await context.ParentStudentLinks.AnyAsync(
                l => l.ParentProfileId == profile.Id &&
                     l.StudentProfile.IsActive &&
                     !l.StudentProfile.IsDeleted,
                cancellationToken);

            if (hasActiveStudentLinks)
            {
                return Result.Failure(ProfileErrors.HasActiveStudentLinks);
            }
        }

        return Result.Success();
    }
}
