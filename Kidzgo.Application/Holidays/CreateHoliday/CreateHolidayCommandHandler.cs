using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Holidays.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;

namespace Kidzgo.Application.Holidays.CreateHoliday;

public sealed class CreateHolidayCommandHandler(
    IDbContext context
) : ICommandHandler<CreateHolidayCommand, HolidayResponse>
{
    public async Task<Result<HolidayResponse>> Handle(
        CreateHolidayCommand command,
        CancellationToken cancellationToken)
    {
        var validation = HolidayValidationHelper.Validate(
            command.Name,
            command.StartDate,
            command.EndDate);
        if (validation.IsFailure)
        {
            return Result.Failure<HolidayResponse>(validation.Error);
        }

        var now = VietnamTime.UtcNow();
        var holiday = new Holiday
        {
            Id = Guid.NewGuid(),
            Name = command.Name.Trim(),
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Description = command.Description,
            IsActive = command.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Holidays.Add(holiday);
        await context.SaveChangesAsync(cancellationToken);

        return HolidayMapper.ToResponse(holiday);
    }
}
