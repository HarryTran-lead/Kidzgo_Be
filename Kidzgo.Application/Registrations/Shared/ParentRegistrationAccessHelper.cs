using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations.Errors;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.Shared;

internal static class ParentRegistrationAccessHelper
{
    internal static async Task<Result<Guid>> ResolveParentProfileIdAsync(
        IDbContext context,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        var parentProfileId = await context.Profiles
            .AsNoTracking()
            .Where(p =>
                p.UserId == userContext.UserId &&
                p.ProfileType == ProfileType.Parent &&
                p.IsActive &&
                !p.IsDeleted)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return parentProfileId.HasValue
            ? Result.Success(parentProfileId.Value)
            : Result.Failure<Guid>(ProfileErrors.ParentNotFound);
    }

    internal static async Task<Result<Guid>> ResolveTargetStudentIdAsync(
        IDbContext context,
        IUserContext userContext,
        Guid parentProfileId,
        Guid? requestedStudentProfileId,
        CancellationToken cancellationToken)
    {
        var targetStudentId = requestedStudentProfileId
            ?? userContext.StudentId
            ?? await context.ParentStudentLinks
                .AsNoTracking()
                .Where(link => link.ParentProfileId == parentProfileId)
                .Select(link => (Guid?)link.StudentProfileId)
                .FirstOrDefaultAsync(cancellationToken);

        if (!targetStudentId.HasValue)
        {
            return Result.Failure<Guid>(ProfileErrors.StudentNotLinkedToParent);
        }

        var isLinked = await context.ParentStudentLinks
            .AsNoTracking()
            .AnyAsync(link =>
                link.ParentProfileId == parentProfileId &&
                link.StudentProfileId == targetStudentId.Value,
                cancellationToken);

        return isLinked
            ? Result.Success(targetStudentId.Value)
            : Result.Failure<Guid>(ProfileErrors.StudentNotLinkedToParent);
    }

    internal static async Task<Result<ParentRegistrationAccessContext>> EnsureRegistrationAccessAsync(
        IDbContext context,
        IUserContext userContext,
        Guid registrationId,
        CancellationToken cancellationToken)
    {
        var parentProfileIdResult = await ResolveParentProfileIdAsync(context, userContext, cancellationToken);
        if (!parentProfileIdResult.IsSuccess)
        {
            return Result.Failure<ParentRegistrationAccessContext>(parentProfileIdResult.Error);
        }

        var registration = await context.Registrations
            .AsNoTracking()
            .Where(r => r.Id == registrationId)
            .Select(r => new
            {
                r.Id,
                r.StudentProfileId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (registration is null)
        {
            return Result.Failure<ParentRegistrationAccessContext>(RegistrationErrors.NotFound(registrationId));
        }

        var isLinked = await context.ParentStudentLinks
            .AsNoTracking()
            .AnyAsync(link =>
                link.ParentProfileId == parentProfileIdResult.Value &&
                link.StudentProfileId == registration.StudentProfileId,
                cancellationToken);

        if (!isLinked)
        {
            return Result.Failure<ParentRegistrationAccessContext>(RegistrationErrors.NotFound(registrationId));
        }

        return Result.Success(new ParentRegistrationAccessContext(
            registration.Id,
            registration.StudentProfileId,
            parentProfileIdResult.Value));
    }
}

internal sealed record ParentRegistrationAccessContext(
    Guid RegistrationId,
    Guid StudentProfileId,
    Guid ParentProfileId);
