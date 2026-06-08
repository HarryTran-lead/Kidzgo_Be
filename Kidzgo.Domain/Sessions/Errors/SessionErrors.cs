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

    public static Error CannotChangeCancelledOrCompleted(Guid sessionId) => Error.Validation(
        "Session.CannotChangeCancelledOrCompleted",
        $"Session with Id = '{sessionId}' cannot be changed because it is cancelled or completed");

    public static Error CannotChangePastSession(Guid sessionId) => Error.Validation(
        "Session.CannotChangePastSession",
        $"Session with Id = '{sessionId}' cannot be changed because it has already ended");

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

    public static Error InvalidTeacherRole(string? role) => Error.Validation(
        "Session.InvalidTeacherRole",
        $"Invalid teacher role: '{role}'. Valid values: MainTeacher, Assistant");

    public static Error InvalidParticipationType(string? participationType) => Error.Validation(
        "Session.InvalidParticipationType",
        $"Invalid participation type: '{participationType}'. Valid values: {string.Join(", ", ParticipationTypeRules.SelectableValues)}");

    public static Error InvalidSectionType(string? sectionType) => Error.Validation(
        "Session.InvalidSectionType",
        $"Invalid section type: '{sectionType}'. Valid values: {string.Join(", ", Enum.GetNames<SectionType>())}");

    public static Error TeacherCanOnlyChangeSectionTypeOnSessionDate(Guid sessionId, DateOnly sessionDate, DateOnly today) => Error.Validation(
        "Session.TeacherCanOnlyChangeSectionTypeOnSessionDate",
        $"Teacher can only change section type on the session date. SessionId='{sessionId}', sessionDate='{sessionDate:yyyy-MM-dd}', today='{today:yyyy-MM-dd}'");

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

    public static Error TeachingLogAlreadyExists(Guid sessionId) => Error.Conflict(
        "Session.TeachingLogAlreadyExists",
        $"Teaching log already exists for session '{sessionId}'.");

    public static Error TeachingLogNotFound(Guid sessionId) => Error.NotFound(
        "Session.TeachingLogNotFound",
        $"Teaching log not found for session '{sessionId}'.");

    public static Error TeachingLogLocked(Guid sessionId) => Error.Conflict(
        "Session.TeachingLogLocked",
        $"Teaching log for session '{sessionId}' is locked and cannot be updated.");

    public static Error MissingLessonTemplateForTeachingLog(Guid sessionId) => Error.Validation(
        "Session.MissingLessonTemplateForTeachingLog",
        $"Session '{sessionId}' does not have a planned lesson plan template.");

    public static Error LessonPlanTemplateMissing(
        Guid sessionId,
        Guid classId,
        Guid? moduleId) => Error.NotFound(
        "Session.LessonPlanTemplateMissing",
        $"Lesson plan template linkage is missing for session '{sessionId}' in class '{classId}' and module '{moduleId}'.");

    public static Error LessonPlanTemplateInconsistent(
        Guid sessionId,
        Guid classId,
        Guid? moduleId,
        IReadOnlyCollection<Guid> templateIds) => Error.Conflict(
        "Session.LessonPlanTemplateInconsistent",
        $"Lesson plan template linkage is inconsistent for session '{sessionId}' in class '{classId}' and module '{moduleId}'. Candidates: {string.Join(", ", templateIds)}");

    public static Error CurriculumMappingMissing(
        Guid sessionId,
        Guid classId,
        Guid moduleId,
        int sessionIndexInModule) => Error.NotFound(
        "Session.CurriculumMappingMissing",
        $"Curriculum mapping is missing for session '{sessionId}' in class '{classId}', module '{moduleId}', sessionIndexInModule '{sessionIndexInModule}'.");

    public static Error LessonPlanDocumentNotFound(
        Guid sessionId,
        Guid classId,
        Guid templateId) => Error.NotFound(
        "Session.LessonPlanDocumentNotFound",
        $"Lesson plan document '{templateId}' was not found for session '{sessionId}' in class '{classId}'.");

    public static Error InvalidTeachingProgressStatus(string? status) => Error.Validation(
        "Session.InvalidTeachingProgressStatus",
        $"Invalid teaching progress status '{status}'. Valid values: completed, partial, not_started, skipped.");

    public static Error SkippedRequiresReason => Error.Validation(
        "Session.SkippedRequiresReason",
        "Skipped lesson requires a teacher note or reason.");
}
