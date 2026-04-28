using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Holidays.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Holidays.GetHolidayById;

public sealed class GetHolidayByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetHolidayByIdQuery, HolidayResponse>
{
    public async Task<Result<HolidayResponse>> Handle(
        GetHolidayByIdQuery query,
        CancellationToken cancellationToken)
    {
        var holiday = await context.Holidays
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == query.Id, cancellationToken);

        if (holiday is null)
        {
            return Result.Failure<HolidayResponse>(HolidayErrors.NotFound(query.Id));
        }

        return HolidayMapper.ToResponse(holiday);
    }
}
