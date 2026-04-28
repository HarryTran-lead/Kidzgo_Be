using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.RegistrationDiscountCampaigns.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.RegistrationDiscountCampaigns.GetRegistrationDiscountCampaigns;

public sealed class GetRegistrationDiscountCampaignsQueryHandler(
    IDbContext context
) : IQueryHandler<GetRegistrationDiscountCampaignsQuery, GetRegistrationDiscountCampaignsResponse>
{
    public async Task<Result<GetRegistrationDiscountCampaignsResponse>> Handle(
        GetRegistrationDiscountCampaignsQuery query,
        CancellationToken cancellationToken)
    {
        var campaignsQuery = context.RegistrationDiscountCampaigns
            .AsNoTracking()
            .AsQueryable();

        if (query.BranchId.HasValue)
        {
            campaignsQuery = campaignsQuery.Where(x => x.BranchId == query.BranchId.Value);
        }

        if (query.ProgramId.HasValue)
        {
            campaignsQuery = campaignsQuery.Where(x => x.ProgramId == query.ProgramId.Value);
        }

        if (query.TuitionPlanId.HasValue)
        {
            campaignsQuery = campaignsQuery.Where(x => x.TuitionPlanId == query.TuitionPlanId.Value);
        }

        if (query.IsActive.HasValue)
        {
            campaignsQuery = campaignsQuery.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.Trim().ToLower();
            campaignsQuery = campaignsQuery.Where(x =>
                x.Name.ToLower().Contains(searchTerm) ||
                (x.Code != null && x.Code.ToLower().Contains(searchTerm)));
        }

        campaignsQuery = campaignsQuery
            .OrderByDescending(x => x.Priority)
            .ThenByDescending(x => x.StartDate)
            .ThenByDescending(x => x.CreatedAt);

        var totalCount = await campaignsQuery.CountAsync(cancellationToken);
        var today = VietnamTime.ToVietnamDateOnly(VietnamTime.UtcNow());

        var items = await RegistrationDiscountCampaignReadModelProjector
            .Project(
                campaignsQuery
                    .ApplyPagination(query.PageNumber, query.PageSize),
                today)
            .ToListAsync(cancellationToken);

        return new GetRegistrationDiscountCampaignsResponse
        {
            Campaigns = new Page<RegistrationDiscountCampaignModel>(
                items,
                totalCount,
                query.PageNumber,
                query.PageSize)
        };
    }
}
