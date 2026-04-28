using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Holidays.Shared;

namespace Kidzgo.Application.Holidays.CreateHoliday;

public sealed class CreateHolidayCommand : ICommand<HolidayResponse>
{
    public string Name { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
