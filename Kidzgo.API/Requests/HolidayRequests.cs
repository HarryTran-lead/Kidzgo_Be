namespace Kidzgo.API.Requests;

public sealed class CreateHolidayRequest
{
    public string Name { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateHolidayRequest
{
    public string Name { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
