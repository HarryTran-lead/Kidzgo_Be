using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Holidays.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Holidays.UpdateHoliday;

public sealed class UpdateHolidayCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateHolidayCommand, HolidayResponse>
{
    public async Task<Result<HolidayResponse>> Handle(
        UpdateHolidayCommand command,
        CancellationToken cancellationToken)
    {
        var holiday = await context.Holidays
            .FirstOrDefaultAsync(h => h.Id == command.Id, cancellationToken);

        if (holiday is null)
        {
            return Result.Failure<HolidayResponse>(HolidayErrors.NotFound(command.Id));
        }

        var validation = HolidayValidationHelper.Validate(
            command.Name,
            command.StartDate,
            command.EndDate);
        if (validation.IsFailure)
        {
            return Result.Failure<HolidayResponse>(validation.Error);
        }

        holiday.Name = command.Name.Trim();
        holiday.StartDate = command.StartDate;
        holiday.EndDate = command.EndDate;
        holiday.Description = command.Description;
        holiday.IsActive = command.IsActive;
        holiday.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return HolidayMapper.ToResponse(holiday);
    }
}
