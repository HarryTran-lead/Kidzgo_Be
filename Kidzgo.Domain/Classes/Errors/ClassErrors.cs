using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Classes.Errors;

public static class ClassErrors
{
    public static Error NotFound(Guid? classId) => Error.NotFound(
        "Class.NotFound",
        $"Class with Id = '{classId}' was not found");

    public static readonly Error BranchNotFound = Error.NotFound(
        "Class.BranchNotFound",
        "Branch not found or inactive");

    public static readonly Error ProgramNotFound = Error.NotFound(
        "Class.ProgramNotFound",
        "Program not found, deleted, or inactive");

    public static readonly Error ProgramNotAvailableInBranch = Error.Validation(
        "Class.ProgramNotAvailableInBranch",
        "Program is not assigned to the selected branch");

    public static readonly Error CodeExists = Error.Conflict(
        "Class.CodeExists",
        "Class code already exists");

    public static readonly Error MainTeacherNotFound = Error.NotFound(
        "Class.MainTeacherNotFound",
        "Main teacher not found or is not a teacher");

    public static readonly Error MainTeacherBranchMismatch = Error.Conflict(
        "Class.MainTeacherBranchMismatch",
        "Main teacher must belong to the same branch as the class");

    public static readonly Error AssistantTeacherNotFound = Error.NotFound(
        "Class.AssistantTeacherNotFound",
        "Assistant teacher not found or is not a teacher");

    public static readonly Error AssistantTeacherBranchMismatch = Error.Conflict(
        "Class.AssistantTeacherBranchMismatch",
        "Assistant teacher must belong to the same branch as the class");

    public static readonly Error HasActiveEnrollments = Error.Conflict(
        "Class.HasActiveEnrollments",
        "Cannot change class while it has active or paused enrollments");

    public static readonly Error HasFutureSessions = Error.Conflict(
        "Class.HasFutureSessions",
        "Cannot change class while it has future sessions");

    public static readonly Error HasOperationalDependencies = Error.Conflict(
        "Class.HasOperationalDependencies",
        "Cannot change branch or program while class has enrollments or sessions");

    public static readonly Error CapacityBelowActiveEnrollments = Error.Conflict(
        "Class.CapacityBelowActiveEnrollments",
        "Capacity cannot be lower than the number of active enrollments");

    public static readonly Error RoomNotFound = Error.NotFound(
        "Class.RoomNotFound",
        "Room not found or inactive");

    public static readonly Error RoomBranchMismatch = Error.Conflict(
        "Class.RoomBranchMismatch",
        "Room must belong to the same branch as the class");

    public static readonly Error TeacherAndAssistantMustDiffer = Error.Validation(
        "Class.TeacherAndAssistantMustDiffer",
        "Main teacher and assistant teacher must be different users");

    public static readonly Error InvalidActiveDependencies = Error.Conflict(
        "Class.InvalidActiveDependencies",
        "Class cannot be activated while branch, program, room, or teachers are inactive");

    public static readonly Error CannotCloseWithActiveEnrollments = Error.Conflict(
        "Class.CannotCloseWithActiveEnrollments",
        "Cannot close, suspend, complete, or cancel class with active or paused enrollments");

    public static readonly Error CannotCloseWithFutureSessions = Error.Conflict(
        "Class.CannotCloseWithFutureSessions",
        "Cannot close, suspend, complete, or cancel class with future sessions");

    public static readonly Error StatusUnchanged = Error.Validation(
        "Class.StatusUnchanged",
        "Class status is already set to the requested status");

    public static readonly Error InvalidStatusTransition = Error.Validation(
        "Class.InvalidStatusTransition",
        "Cannot change status from Closed to Planned");

    public static readonly Error SupplementaryProgramRequired = Error.Validation(
        "Class.SupplementaryProgramRequired",
        "Schedule segments are only supported for supplementary programs");

    public static Error InvalidScheduleSegmentEffectiveDate(string message) => Error.Validation(
        "Class.ScheduleSegmentInvalidEffectiveDate",
        message);

    public static Error ScheduleSegmentAlreadyExists(DateOnly effectiveFrom) => Error.Conflict(
        "Class.ScheduleSegmentAlreadyExists",
        $"A class schedule segment already starts on {effectiveFrom:dd/MM/yyyy}");

    public static Error FutureScheduleSegmentExists(DateOnly effectiveFrom) => Error.Conflict(
        "Class.FutureScheduleSegmentExists",
        $"Cannot add a class schedule segment from {effectiveFrom:dd/MM/yyyy} because a future schedule segment already exists");

    // Session conflict errors
    public static Error RoomConflict(string classCode, string classTitle, DateTime conflictDatetime) => Error.Conflict(
        "Class.RoomConflict",
        $"Room is already booked by class '{classCode} - {classTitle}' at {conflictDatetime:dd/MM/yyyy HH:mm}");

    public static Error TeacherConflict(string classCode, string classTitle, DateTime conflictDatetime, string? roomName = null) => Error.Conflict(
        "Class.TeacherConflict",
        roomName != null
            ? $"Teacher is already assigned to class '{classCode} - {classTitle}' at {conflictDatetime:dd/MM/yyyy HH:mm} in room '{roomName}'"
            : $"Teacher is already assigned to class '{classCode} - {classTitle}' at {conflictDatetime:dd/MM/yyyy HH:mm}");

    public static Error AssistantConflict(string classCode, string classTitle, DateTime conflictDatetime) => Error.Conflict(
        "Class.AssistantConflict",
        $"Assistant teacher is already assigned to class '{classCode} - {classTitle}' at {conflictDatetime:dd/MM/yyyy HH:mm}");
}

public static class ScheduleErrors
{
    public static Error Empty => Error.Validation(
        "SchedulePattern.Empty",
        "Schedule pattern cannot be empty");

    public static Error Invalid(string message) => Error.Validation(
        "SchedulePattern.Invalid",
        $"Invalid schedule pattern: {message}");
}
