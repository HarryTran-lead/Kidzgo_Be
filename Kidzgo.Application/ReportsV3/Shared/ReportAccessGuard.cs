using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.Shared;

internal sealed class ReportAccessGuard(
    IDbContext context,
    IUserContext userContext)
{
    public async Task<Result<User>> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Id == userContext.UserId, cancellationToken);

        return user is null
            ? Result.Failure<User>(Error.NotFound("Report.UserNotFound", "Current user was not found."))
            : Result.Success(user);
    }

    public async Task<HashSet<Guid>> GetTeacherClassIdsAsync(Guid teacherUserId, CancellationToken cancellationToken)
    {
        var classIds = await context.Classes
            .Where(c => c.MainTeacherId == teacherUserId || c.AssistantTeacherId == teacherUserId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        return classIds.ToHashSet();
    }

    public async Task<HashSet<Guid>> GetParentStudentIdsAsync(Guid parentUserId, CancellationToken cancellationToken)
    {
        var parentProfileId = await context.Profiles
            .Where(p =>
                p.UserId == parentUserId &&
                p.ProfileType == ProfileType.Parent &&
                p.IsActive &&
                !p.IsDeleted)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!parentProfileId.HasValue)
        {
            return [];
        }

        var studentIds = await context.ParentStudentLinks
            .Where(link => link.ParentProfileId == parentProfileId.Value)
            .Select(link => link.StudentProfileId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return studentIds.ToHashSet();
    }
}
