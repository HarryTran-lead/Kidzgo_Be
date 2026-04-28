using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Holidays.Shared;

namespace Kidzgo.Application.Holidays.GetHolidayById;

public sealed class GetHolidayByIdQuery : IQuery<HolidayResponse>
{
    public Guid Id { get; set; }
}
