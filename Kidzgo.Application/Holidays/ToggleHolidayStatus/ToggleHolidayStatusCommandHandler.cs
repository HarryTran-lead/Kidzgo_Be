using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Holidays.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Holidays.ToggleHolidayStatus;

public sealed class ToggleHolidayStatusCommandHandler(
    IDbContext context
) : ICommandHandler<ToggleHolidayStatusCommand, HolidayResponse>
{
    public async Task<Result<HolidayResponse>> Handle(
        ToggleHolidayStatusCommand command,
        CancellationToken cancellationToken)
    {
        var holiday = await context.Holidays
            .FirstOrDefaultAsync(h => h.Id == command.Id, cancellationToken);

        if (holiday is null)
        {
            return Result.Failure<HolidayResponse>(HolidayErrors.NotFound(command.Id));
        }

        holiday.IsActive = !holiday.IsActive;
        holiday.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return HolidayMapper.ToResponse(holiday);
    }
}
