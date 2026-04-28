using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Holidays.Shared;

namespace Kidzgo.Application.Holidays.GetHolidays;

public sealed class GetHolidaysQuery : IQuery<List<HolidayResponse>>
{
    public bool? IsActive { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}
