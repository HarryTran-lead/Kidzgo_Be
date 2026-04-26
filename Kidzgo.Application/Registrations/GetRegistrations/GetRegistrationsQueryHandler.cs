using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.GetRegistrations;

public sealed class GetRegistrationsQueryHandler(
    IDbContext context
) : IQueryHandler<GetRegistrationsQuery, GetRegistrationsResponse>
{
    public async Task<Result<GetRegistrationsResponse>> Handle(
        GetRegistrationsQuery query,
        CancellationToken cancellationToken)
    {
        var filteredQuery = RegistrationReadModelQueryHelper.ApplyFilters(
            RegistrationReadModelQueryHelper.CreateBaseQuery(context),
            query.StudentProfileId,
            query.BranchId,
            query.ProgramId,
            query.Status,
            query.ClassId);

        // Get total count
        var totalCount = await filteredQuery.CountAsync(cancellationToken);

        // Apply pagination
        var items = await RegistrationReadModelQueryHelper
            .SelectListDto(
                filteredQuery
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize))
            .ToListAsync(cancellationToken);

        var page = new Page<RegistrationDto>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetRegistrationsResponse
        {
            Page = page
        };
    }
}
