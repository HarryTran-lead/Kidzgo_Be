using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Sessions.Errors;

public static class AttendanceErrors
{
    public static Error NotFound(Guid? id) => Error.NotFound(
        "Attendance.NotFound",
        $"The attendance record with Id = '{id}' was not found.");

    public static Error NotFoundForSessionStudent(Guid sessionId, Guid studentProfileId) => Error.NotFound(
        "Attendance.NotFound",
        $"Attendance not found for session '{sessionId}' and student '{studentProfileId}'.");

    public static Error UpdateWindowClosed(Guid sessionId) => Error.Validation(
        "Attendance.UpdateWindowClosed",
        $"Attendance for session '{sessionId}' can only be updated within 24 hours after it ends.");

    public static Error FutureSessionNotAllowed(Guid sessionId) => Error.Validation(
        "Attendance.FutureSessionNotAllowed",
        $"Teachers cannot mark attendance for future session '{sessionId}'.");

    public static Error ApprovedLeaveLocked(Guid sessionId, Guid studentProfileId) => Error.Validation(
        "Attendance.ApprovedLeaveLocked",
        $"Attendance for student '{studentProfileId}' in session '{sessionId}' is locked because the leave request was approved.");
}

