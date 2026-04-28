using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Holidays.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Holidays.GetHolidays;

public sealed class GetHolidaysQueryHandler(
    IDbContext context
) : IQueryHandler<GetHolidaysQuery, List<HolidayResponse>>
{
    public async Task<Result<List<HolidayResponse>>> Handle(
        GetHolidaysQuery query,
        CancellationToken cancellationToken)
    {
        var holidaysQuery = context.Holidays
            .AsNoTracking()
            .AsQueryable();

        if (query.IsActive.HasValue)
        {
            holidaysQuery = holidaysQuery.Where(h => h.IsActive == query.IsActive.Value);
        }

        if (query.From.HasValue)
        {
            holidaysQuery = holidaysQuery.Where(h => h.EndDate >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            holidaysQuery = holidaysQuery.Where(h => h.StartDate <= query.To.Value);
        }

        var holidays = await holidaysQuery
            .OrderBy(h => h.StartDate)
            .ThenBy(h => h.Name)
            .ToListAsync(cancellationToken);

        return holidays
            .Select(HolidayMapper.ToResponse)
            .ToList();
    }
}
