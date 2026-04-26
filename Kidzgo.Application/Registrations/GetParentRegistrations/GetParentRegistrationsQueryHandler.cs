using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Registrations.GetRegistrations;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.GetParentRegistrations;

public sealed class GetParentRegistrationsQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetParentRegistrationsQuery, GetRegistrationsResponse>
{
    public async Task<Result<GetRegistrationsResponse>> Handle(
        GetParentRegistrationsQuery query,
        CancellationToken cancellationToken)
    {
        var parentProfileIdResult = await ParentRegistrationAccessHelper.ResolveParentProfileIdAsync(
            context,
            userContext,
            cancellationToken);

        if (!parentProfileIdResult.IsSuccess)
        {
            return Result.Failure<GetRegistrationsResponse>(parentProfileIdResult.Error);
        }

        var targetStudentIdResult = await ParentRegistrationAccessHelper.ResolveTargetStudentIdAsync(
            context,
            userContext,
            parentProfileIdResult.Value,
            query.StudentProfileId,
            cancellationToken);

        if (!targetStudentIdResult.IsSuccess)
        {
            return Result.Failure<GetRegistrationsResponse>(targetStudentIdResult.Error);
        }

        var filteredQuery = RegistrationReadModelQueryHelper.ApplyFilters(
            RegistrationReadModelQueryHelper.CreateBaseQuery(context),
            targetStudentIdResult.Value,
            query.BranchId,
            query.ProgramId,
            query.Status,
            query.ClassId);

        var totalCount = await filteredQuery.CountAsync(cancellationToken);

        var items = await RegistrationReadModelQueryHelper
            .SelectListDto(
                filteredQuery
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize))
            .ToListAsync(cancellationToken);

        return Result.Success(new GetRegistrationsResponse
        {
            Page = new Page<RegistrationDto>(items, totalCount, query.PageNumber, query.PageSize)
        });
    }
}
