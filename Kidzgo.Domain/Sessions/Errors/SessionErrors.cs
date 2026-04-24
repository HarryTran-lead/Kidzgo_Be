using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Sessions.Errors;

public static class SessionErrors
{
    public static Error NotFound(Guid? sessionId) => Error.NotFound(
        "Session.NotFound",
        $"Session with Id = '{sessionId}' was not found");

    public static Error InvalidStatus => Error.Validation(
        "Session.InvalidStatus",
        "Only sessions with Scheduled status can be updated");

    public static Error InvalidClassStatus => Error.Validation(
        "Session.InvalidClassStatus",
        "Sessions can only be created for Planned, Recruiting, or Active classes");

    public static Error MissingSchedulePattern(Guid classId) => Error.Validation(
        "Session.MissingSchedulePattern",
        $"Class '{classId}' does not have a schedule pattern");

    public static Error MissingClassEndDate(Guid classId) => Error.Validation(
        "Session.MissingClassEndDate",
        $"Class '{classId}' must have an end date before generating sessions from schedule pattern");

    public static Error AlreadyCancelled => Error.Validation(
        "Session.AlreadyCancelled",
        "Session is already cancelled");

    public static Error AlreadyCompleted => Error.Validation(
        "Session.AlreadyCompleted",
        "Completed sessions cannot be cancelled");

    public static Error HasAttendance => Error.Conflict(
        "Session.HasAttendance",
        "Session cannot be cancelled because attendance has already been recorded");

    public static Error HasReports => Error.Conflict(
        "Session.HasReports",
        "Session cannot be cancelled because reports have already been created");

    public static Error Cancelled => Error.Validation(
        "Session.Cancelled",
        "Cancelled sessions cannot be completed");

    public static Error InvalidDuration(int duration) => Error.Validation(
        "Session.InvalidDuration",
        $"Duration must be greater than 0. Current value: {duration}");

    public static Error InvalidBranch(Guid branchId) => Error.Validation(
        "Session.InvalidBranch",
        $"Branch with ID {branchId} does not exist or is inactive");

    public static Error InvalidRoom(Guid roomId) => Error.Validation(
        "Session.InvalidRoom",
        $"Room with ID {roomId} does not exist, is inactive, or does not belong to this branch");

    public static Error InvalidTeacher(Guid teacherId) => Error.Validation(
        "Session.InvalidTeacher",
        $"Main teacher with ID {teacherId} does not exist, is inactive, is not a teacher, or does not belong to this branch");

    public static Error InvalidAssistant(Guid assistantId) => Error.Validation(
        "Session.InvalidAssistant",
        $"Assistant teacher with ID {assistantId} does not exist, is inactive, is not a teacher, or does not belong to this branch");

    public static Error TeacherAndAssistantMustDiffer => Error.Validation(
        "Session.TeacherAndAssistantMustDiffer",
        "Main teacher and assistant teacher must be different users");

    public static Error RoomOccupied(string classCode, string className, DateTime plannedDatetime) => Error.Conflict(
        "Session.RoomOccupied",
        $"Room is already occupied by class '{classCode} - {className}' at {plannedDatetime:dd/MM/yyyy HH:mm}");

    public static Error TeacherOccupied(string classCode, string className, DateTime plannedDatetime) => Error.Conflict(
        "Session.TeacherOccupied",
        $"Teacher is already assigned to class '{classCode} - {className}' at {plannedDatetime:dd/MM/yyyy HH:mm}");

    public static Error AssistantOccupied(string classCode, string className, DateTime plannedDatetime) => Error.Conflict(
        "Session.AssistantOccupied",
        $"Assistant teacher is already assigned to class '{classCode} - {className}' at {plannedDatetime:dd/MM/yyyy HH:mm}");

    public static Error SaveFailed(string details) => Error.Validation(
        "Session.SaveFailed",
        $"Cannot save sessions: {details}");

    public static Error UnauthorizedAccess(Guid sessionId) => Error.Validation(
        "Session.UnauthorizedAccess",
        $"Teacher is not allowed to create a report for session with ID '{sessionId}'");
}
