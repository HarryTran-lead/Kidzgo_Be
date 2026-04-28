using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Holidays.DeleteHoliday;

public sealed class DeleteHolidayCommand : ICommand<DeleteHolidayResponse>
{
    public Guid Id { get; set; }
}
