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

    public static Error Cancelled => Error.Validation(
        "Session.Cancelled",
        "Cancelled sessions cannot be completed");

    public static Error InvalidDuration(int duration) => Error.Validation(
        "Session.InvalidDuration",
        $"Duration phải lớn hơn 0. Giá trị hiện tại: {duration}");

    public static Error InvalidBranch(Guid branchId) => Error.Validation(
        "Session.InvalidBranch",
        $"Branch với ID {branchId} không tồn tại hoặc không active");

    public static Error InvalidRoom(Guid roomId) => Error.Validation(
        "Session.InvalidRoom",
        $"Room với ID {roomId} không tồn tại hoặc không thuộc branch này");

    public static Error InvalidTeacher(Guid teacherId) => Error.Validation(
        "Session.InvalidTeacher",
        $"Main Teacher với ID {teacherId} không tồn tại, không phải Teacher role, hoặc không thuộc branch này");

    public static Error InvalidAssistant(Guid assistantId) => Error.Validation(
        "Session.InvalidAssistant",
        $"Assistant Teacher với ID {assistantId} không tồn tại, không phải Teacher role, hoặc không thuộc branch này");

    public static Error RoomOccupied(string classCode, string className, DateTime plannedDatetime) => Error.Validation(
        "Session.RoomOccupied",
        $"Phòng đã bị chiếm dụng bởi lớp '{classCode} - {className}' vào ngày {plannedDatetime:dd/MM/yyyy HH:mm}");

    public static Error SaveFailed(string details) => Error.Validation(
        "Session.SaveFailed",
        $"Không thể lưu sessions: {details}");

    public static Error UnauthorizedAccess(Guid sessionId) => Error.Validation(
        "Session.UnauthorizedAccess",
        $"Teacher không được phép tạo report cho session với ID '{sessionId}'");
}

