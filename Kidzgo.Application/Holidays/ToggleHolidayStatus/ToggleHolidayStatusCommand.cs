using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Holidays.Shared;

namespace Kidzgo.Application.Holidays.ToggleHolidayStatus;

public sealed class ToggleHolidayStatusCommand : ICommand<HolidayResponse>
{
    public Guid Id { get; set; }
}
