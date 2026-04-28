using Kidzgo.Domain.Schools;

namespace Kidzgo.Application.Holidays.Shared;

internal static class HolidayMapper
{
    public static HolidayResponse ToResponse(Holiday holiday)
        => new()
        {
            Id = holiday.Id,
            Name = holiday.Name,
            StartDate = holiday.StartDate,
            EndDate = holiday.EndDate,
            Description = holiday.Description,
            IsActive = holiday.IsActive,
            CreatedAt = holiday.CreatedAt,
            UpdatedAt = holiday.UpdatedAt
        };
}
