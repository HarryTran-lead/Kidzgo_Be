using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Attendance.GetSessionAttendance;

public sealed class GetSessionAttendanceResponse
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public string StudentName { get; set; } = null!;
    public string? StudentAvatarUrl { get; set; }
    public Guid? RegistrationId { get; set; }
    public string? Track { get; set; }
    public bool IsMakeup { get; set; }
    public string AttendanceStatus { get; set; } = null!;
    public string? AbsenceType { get; set; }
    public bool HasMakeupCredit { get; set; }
    public bool HasApprovedLeave { get; set; }
    public string? Note { get; set; }
    public DateTime? MarkedAt { get; set; }
}

/// <summary>
/// Response wrapper for GET session attendance with summary
/// </summary>
public sealed class GetSessionAttendanceListResponse
{
    public Guid SessionId { get; set; }
    public string? SessionName { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public AttendanceSummary Summary { get; set; } = new();
    public List<GetSessionAttendanceResponse> Attendances { get; set; } = new();
}

public sealed class AttendanceSummary
{
    public int TotalStudents { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int MakeupCount { get; set; }
    public int NotMarkedCount { get; set; }
}

