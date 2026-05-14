using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Attendance.UpdateAttendance;

public sealed class UpdateAttendanceResponse
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public Guid StudentProfileId { get; init; }
    public string AttendanceStatus { get; init; } = null!;
    public string? AbsenceType { get; init; }
    public string? Note { get; init; }
    public bool TicketConsumed { get; init; }
    public int ConsumedQuantity { get; init; }
    public bool AdvanceLessonProgression { get; init; }
    public int? TicketBalance { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

