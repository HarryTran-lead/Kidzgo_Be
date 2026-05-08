using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Media;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Media.Shared;

public static class MediaReadAccessHelper
{
    public static async Task<UserRole?> GetCurrentUserRoleAsync(
        IDbContext context,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        return await context.Users
            .AsNoTracking()
            .Where(user => user.Id == userContext.UserId)
            .Select(user => (UserRole?)user.Role)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<IQueryable<MediaAsset>> ApplyReadAccessFilterAsync(
        IQueryable<MediaAsset> query,
        IDbContext context,
        IUserContext userContext,
        Guid? requestedClassId,
        Guid? requestedStudentProfileId,
        CancellationToken cancellationToken)
    {
        var role = await GetCurrentUserRoleAsync(context, userContext, cancellationToken);
        if (role is null)
        {
            return query.Where(_ => false);
        }

        if (role is not (UserRole.Parent))
        {
            if (requestedStudentProfileId.HasValue)
            {
                query = query.Where(media => media.StudentProfileId == requestedStudentProfileId.Value);
            }

            return query;
        }

        var studentProfileId = await ResolveAccessibleStudentProfileIdAsync(
            context,
            userContext,
            role.Value,
            requestedStudentProfileId,
            cancellationToken);

        if (!studentProfileId.HasValue)
        {
            return query.Where(_ => false);
        }

        query = query.Where(media =>
            media.ApprovalStatus == ApprovalStatus.Approved &&
            media.IsPublished);

        query = role == UserRole.Parent
            ? query.Where(media => media.Visibility != Visibility.PublicParent)
            : query;

        var activeClassIds = context.ClassEnrollments
            .Where(enrollment =>
                enrollment.StudentProfileId == studentProfileId.Value &&
                enrollment.Status == EnrollmentStatus.Active)
            .Select(enrollment => enrollment.ClassId);

        var personalMedia = query.Where(media =>
            media.OwnershipScope == MediaOwnershipScope.Personal &&
            media.StudentProfileId == studentProfileId.Value);

        var classMedia = query.Where(media =>
            media.OwnershipScope == MediaOwnershipScope.Class &&
            media.ClassId.HasValue &&
            activeClassIds.Contains(media.ClassId.Value));

        if (requestedClassId.HasValue)
        {
            return classMedia.Where(media => media.ClassId == requestedClassId.Value);
        }

        if (requestedStudentProfileId.HasValue)
        {
            return personalMedia.Where(media => media.StudentProfileId == requestedStudentProfileId.Value);
        }

        return personalMedia.Union(classMedia);
    }

    public static async Task<bool> CanReadAsync(
        IDbContext context,
        IUserContext userContext,
        MediaAsset media,
        CancellationToken cancellationToken)
    {
        var role = await GetCurrentUserRoleAsync(context, userContext, cancellationToken);
        if (role is null)
        {
            return false;
        }

        if (role is not (UserRole.Parent))
        {
            return true;
        }

        if (role == UserRole.Parent)
        {
            var parentProfileId = await ResolveParentProfileIdAsync(context, userContext, cancellationToken);
            if (!parentProfileId.HasValue)
            {
                return false;
            }

            if (media.ApprovalStatus != ApprovalStatus.Approved || !media.IsPublished)
            {
                return false;
            }

            if (!CanRoleReadVisibility(UserRole.Parent, media.Visibility))
            {
                return false;
            }

            if (media.OwnershipScope == MediaOwnershipScope.Personal && media.StudentProfileId.HasValue)
            {
                return await context.ParentStudentLinks
                    .AsNoTracking()
                    .AnyAsync(link =>
                        link.ParentProfileId == parentProfileId.Value &&
                        link.StudentProfileId == media.StudentProfileId.Value,
                        cancellationToken);
            }

            if (media.ClassId.HasValue && media.OwnershipScope == MediaOwnershipScope.Class)
            {
                var linkedStudentIds = context.ParentStudentLinks
                    .Where(link => link.ParentProfileId == parentProfileId.Value)
                    .Select(link => link.StudentProfileId);

                return await context.ClassEnrollments
                    .AnyAsync(enrollment =>
                        linkedStudentIds.Contains(enrollment.StudentProfileId) &&
                        enrollment.Status == EnrollmentStatus.Active &&
                        enrollment.ClassId == media.ClassId.Value,
                        cancellationToken);
            }

            return false;
        }

        var studentProfileId = await ResolveAccessibleStudentProfileIdAsync(
            context,
            userContext,
            role.Value,
            null,
            cancellationToken);

        if (!studentProfileId.HasValue)
        {
            return false;
        }

        if (media.ApprovalStatus != ApprovalStatus.Approved ||
            !media.IsPublished ||
            !CanRoleReadVisibility(UserRole.Parent, media.Visibility))
        {
            return false;
        }

        if (media.OwnershipScope == MediaOwnershipScope.Personal &&
            media.StudentProfileId == studentProfileId.Value)
        {
            return true;
        }

        if (media.ClassId.HasValue && media.OwnershipScope == MediaOwnershipScope.Class)
        {
            return await context.ClassEnrollments
                .AnyAsync(enrollment =>
                    enrollment.StudentProfileId == studentProfileId.Value &&
                    enrollment.Status == EnrollmentStatus.Active &&
                    enrollment.ClassId == media.ClassId.Value,
                    cancellationToken);
        }

        return false;
    }

    private static bool CanRoleReadVisibility(UserRole role, Visibility visibility)
    {
        if (role == UserRole.Parent)
        {
            return true;
        }

        return visibility is Visibility.ClassOnly or Visibility.Personal;
    }

    public static async Task<Guid?> ResolveAccessibleStudentProfileIdAsync(
        IDbContext context,
        IUserContext userContext,
        UserRole role,
        Guid? requestedStudentProfileId,
        CancellationToken cancellationToken)
    {
        if (role == UserRole.Parent)
        {
            var studentProfileId = userContext.StudentId ?? await context.Profiles
                .AsNoTracking()
                .Where(profile =>
                    profile.UserId == userContext.UserId &&
                    profile.ProfileType == ProfileType.Student &&
                    profile.IsActive &&
                    !profile.IsDeleted)
                .Select(profile => (Guid?)profile.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (!studentProfileId.HasValue)
            {
                return null;
            }

            return requestedStudentProfileId.HasValue && requestedStudentProfileId.Value != studentProfileId.Value
                ? null
                : studentProfileId.Value;
        }

        var parentProfileId = await ResolveParentProfileIdAsync(context, userContext, cancellationToken);

        if (!parentProfileId.HasValue)
        {
            return null;
        }

        if (requestedStudentProfileId.HasValue)
        {
            return await context.ParentStudentLinks
                .AsNoTracking()
                .Where(link =>
                    link.ParentProfileId == parentProfileId.Value &&
                    link.StudentProfileId == requestedStudentProfileId.Value)
                .Select(link => (Guid?)link.StudentProfileId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (userContext.StudentId.HasValue)
        {
            var selectedStudentId = await context.ParentStudentLinks
                .AsNoTracking()
                .Where(link =>
                    link.ParentProfileId == parentProfileId.Value &&
                    link.StudentProfileId == userContext.StudentId.Value)
                .Select(link => (Guid?)link.StudentProfileId)
                .FirstOrDefaultAsync(cancellationToken);

            if (selectedStudentId.HasValue)
            {
                return selectedStudentId.Value;
            }
        }

        return await context.ParentStudentLinks
            .AsNoTracking()
            .Where(link => link.ParentProfileId == parentProfileId.Value)
            .OrderBy(link => link.CreatedAt)
            .Select(link => (Guid?)link.StudentProfileId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static async Task<Guid?> ResolveParentProfileIdAsync(
        IDbContext context,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        return userContext.ParentId ?? await context.Profiles
            .AsNoTracking()
            .Where(profile =>
                profile.UserId == userContext.UserId &&
                profile.ProfileType == ProfileType.Parent &&
                profile.IsActive &&
                !profile.IsDeleted)
            .Select(profile => (Guid?)profile.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
