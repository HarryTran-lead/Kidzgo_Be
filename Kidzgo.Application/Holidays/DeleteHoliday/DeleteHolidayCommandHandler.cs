using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Holidays.DeleteHoliday;

public sealed class DeleteHolidayCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteHolidayCommand, DeleteHolidayResponse>
{
    public async Task<Result<DeleteHolidayResponse>> Handle(
        DeleteHolidayCommand command,
        CancellationToken cancellationToken)
    {
        var holiday = await context.Holidays
            .FirstOrDefaultAsync(h => h.Id == command.Id, cancellationToken);

        if (holiday is null)
        {
            return Result.Failure<DeleteHolidayResponse>(HolidayErrors.NotFound(command.Id));
        }

        context.Holidays.Remove(holiday);
        await context.SaveChangesAsync(cancellationToken);

        return new DeleteHolidayResponse
        {
            Id = command.Id,
            Deleted = true
        };
    }
}
