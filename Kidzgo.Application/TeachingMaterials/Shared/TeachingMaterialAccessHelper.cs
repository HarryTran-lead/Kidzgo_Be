using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.TeachingMaterials;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.Shared;

internal static class TeachingMaterialAccessHelper
{
    public static async Task<IQueryable<TeachingMaterial>> ApplyReadAccessFilterAsync(
        IQueryable<TeachingMaterial> query,
        IDbContext context,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (user is null)
        {
            return query.Where(_ => false);
        }

        var studentProfileId = await ResolveStudentProfileIdAsync(context, userContext, user.Role, cancellationToken);
        if (!studentProfileId.HasValue)
        {
            return user.Role == UserRole.Parent ? query.Where(_ => false) : query;
        }

        var enrolledProgramIds = context.ClassEnrollments
            .Where(enrollment =>
                enrollment.StudentProfileId == studentProfileId.Value &&
                enrollment.Status == EnrollmentStatus.Active)
            .Select(enrollment => enrollment.Class.ProgramId);

        return query.Where(material => enrolledProgramIds.Contains(material.ProgramId));
    }

    public static async Task<bool> CanReadAsync(
        IDbContext context,
        IUserContext userContext,
        TeachingMaterial material,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (user is null)
        {
            return false;
        }

        var studentProfileId = await ResolveStudentProfileIdAsync(context, userContext, user.Role, cancellationToken);
        if (!studentProfileId.HasValue)
        {
            return user.Role != UserRole.Parent;
        }

        return await context.ClassEnrollments
            .AnyAsync(enrollment =>
                enrollment.StudentProfileId == studentProfileId.Value &&
                enrollment.Status == EnrollmentStatus.Active &&
                enrollment.Class.ProgramId == material.ProgramId,
                cancellationToken);
    }

    public static async Task<UserRole?> GetCurrentUserRoleAsync(
        IDbContext context,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        return await context.Users
            .AsNoTracking()
            .Where(u => u.Id == userContext.UserId)
            .Select(u => (UserRole?)u.Role)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static async Task<Guid?> ResolveStudentProfileIdAsync(
        IDbContext context,
        IUserContext userContext,
        UserRole role,
        CancellationToken cancellationToken)
    {
        if (userContext.StudentId.HasValue)
        {
            return userContext.StudentId.Value;
        }

        if (role != UserRole.Parent)
        {
            return null;
        }

        return await context.Profiles
            .AsNoTracking()
            .Where(profile =>
                profile.UserId == userContext.UserId &&
                profile.ProfileType == ProfileType.Student &&
                profile.IsActive &&
                !profile.IsDeleted)
            .Select(profile => (Guid?)profile.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
