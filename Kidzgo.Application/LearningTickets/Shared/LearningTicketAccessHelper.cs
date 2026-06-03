using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LearningTickets.Shared;

internal static class LearningTicketAccessHelper
{
    internal static async Task<Result<Guid>> ResolveReadableStudentProfileIdAsync(
        IDbContext context,
        IUserContext userContext,
        Guid requestedStudentProfileId,
        CancellationToken cancellationToken)
    {
        var role = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == userContext.UserId)
            .Select(user => (UserRole?)user.Role)
            .FirstOrDefaultAsync(cancellationToken);

        if (!role.HasValue)
        {
            return Result.Failure<Guid>(
                Error.NotFound("LearningTicket.UserNotFound", "Current user was not found"));
        }

        if (role.Value != UserRole.Parent)
        {
            return Result.Success(requestedStudentProfileId);
        }

        var parentProfileId = userContext.ParentId ?? await context.Profiles
            .AsNoTracking()
            .Where(profile =>
                profile.UserId == userContext.UserId &&
                profile.ProfileType == ProfileType.Parent &&
                profile.IsActive &&
                !profile.IsDeleted)
            .Select(profile => (Guid?)profile.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!parentProfileId.HasValue)
        {
            return Result.Failure<Guid>(
                Error.NotFound("ParentProfile", "Parent profile not found"));
        }

        var isLinked = await context.ParentStudentLinks
            .AsNoTracking()
            .AnyAsync(link =>
                link.ParentProfileId == parentProfileId.Value &&
                link.StudentProfileId == requestedStudentProfileId,
                cancellationToken);

        return isLinked
            ? Result.Success(requestedStudentProfileId)
            : Result.Failure<Guid>(
                Error.NotFound("StudentProfile", "Student profile not linked to current parent"));
    }
}
