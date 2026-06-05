using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Classes.Errors;

public static class EnrollmentErrors
{
    public static Error NotFound(Guid? enrollmentId) => Error.NotFound(
        "Enrollment.NotFound",
        $"Enrollment with Id = '{enrollmentId}' was not found");

    public static readonly Error StudentNotFound = Error.NotFound(
        "Enrollment.StudentNotFound",
        "Student profile not found or is not a student");

    public static readonly Error ClassNotFound = Error.NotFound(
        "Enrollment.ClassNotFound",
        "Class not found");

    public static readonly Error ClassNotAvailable = Error.Conflict(
        "Enrollment.ClassNotAvailable",
        "Class is not available for enrollment");

    public static readonly Error AlreadyEnrolled = Error.Conflict(
        "Enrollment.AlreadyEnrolled",
        "Student is already enrolled in this class");

    public static readonly Error ClassFull = Error.Conflict(
        "Enrollment.ClassFull",
        "Class has reached its capacity");

    public static Error StudentScheduleConflict(
        string? classCode,
        string? classTitle,
        DateTime conflictDatetime,
        int minimumGapMinutes) => Error.Conflict(
        "Enrollment.StudentScheduleConflict",
        BuildStudentScheduleConflictMessage(classCode, classTitle, conflictDatetime, minimumGapMinutes));

    public static readonly Error TuitionPlanNotFound = Error.NotFound(
        "Enrollment.TuitionPlanNotFound",
        "Tuition plan not found");

    public static readonly Error TuitionPlanNotAvailable = Error.Conflict(
        "Enrollment.TuitionPlanNotAvailable",
        "Tuition plan is not available");

    public static readonly Error TuitionPlanProgramMismatch = Error.Conflict(
        "Enrollment.TuitionPlanProgramMismatch",
        "Tuition plan must belong to the same program as the class");

    public static readonly Error TuitionPlanLevelMismatch = Error.Conflict(
        "Enrollment.TuitionPlanLevelMismatch",
        "Tuition plan must belong to the same level as the class");

    public static readonly Error TuitionPlanModuleMismatch = Error.Conflict(
        "Enrollment.TuitionPlanModuleMismatch",
        "Module-specific tuition plan must match the class start module");

    public static readonly Error ModuleBasedTuitionPlanRequiresUpcomingClass = Error.Conflict(
        "Enrollment.ModuleBasedTuitionPlanRequiresUpcomingClass",
        "Module-based tuition plans can only be assigned to classes that are planned or recruiting.");

    public static readonly Error TuitionPlanBranchMismatch = Error.Conflict(
        "Enrollment.TuitionPlanBranchMismatch",
        "Tuition plan is not available for the class branch");

    public static Error TuitionPlanIncompatibleWithClassSlotType(Guid? learningTicketTypeId, Guid? slotTypeId) => Error.Conflict(
        "Enrollment.TuitionPlanIncompatibleWithClassSlotType",
        $"Tuition plan ticket type '{learningTicketTypeId}' is incompatible with class slot type '{slotTypeId}'.");

    public static readonly Error AlreadyActive = Error.Conflict(
        "Enrollment.AlreadyActive",
        "Enrollment is already active");

    public static readonly Error CannotReactivateDropped = Error.Conflict(
        "Enrollment.CannotReactivateDropped",
        "Cannot reactivate a dropped enrollment");

    public static readonly Error CannotReactivateCompleted = Error.Conflict(
        "Enrollment.CannotReactivateCompleted",
        "Cannot reactivate a completed enrollment manually");

    public static readonly Error InvalidStatus = Error.Conflict(
        "Enrollment.InvalidStatus",
        "Only active enrollments can be paused");

    public static readonly Error AlreadyDropped = Error.Conflict(
        "Enrollment.AlreadyDropped",
        "Enrollment is already dropped");

    public static readonly Error SupplementaryProgramRequired = Error.Validation(
        "Enrollment.SupplementaryProgramRequired",
        "Schedule segments are only supported for supplementary programs");

    public static Error InvalidScheduleSegmentEffectiveDate(string message) => Error.Validation(
        "Enrollment.ScheduleSegmentInvalidEffectiveDate",
        message);

    public static Error ScheduleSegmentAlreadyExists(DateOnly effectiveFrom) => Error.Conflict(
        "Enrollment.ScheduleSegmentAlreadyExists",
        $"An enrollment schedule segment already starts on {effectiveFrom:dd/MM/yyyy}");

    public static Error FutureScheduleSegmentExists(DateOnly effectiveFrom) => Error.Conflict(
        "Enrollment.FutureScheduleSegmentExists",
        $"Cannot add an enrollment schedule segment from {effectiveFrom:dd/MM/yyyy} because a future schedule segment already exists");

    private static string BuildStudentScheduleConflictMessage(
        string? classCode,
        string? classTitle,
        DateTime conflictDatetime,
        int minimumGapMinutes)
    {
        var classLabel = !string.IsNullOrWhiteSpace(classCode) && !string.IsNullOrWhiteSpace(classTitle)
            ? $"class '{classCode} - {classTitle}'"
            : !string.IsNullOrWhiteSpace(classCode)
                ? $"class '{classCode}'"
                : !string.IsNullOrWhiteSpace(classTitle)
                    ? $"class '{classTitle}'"
                    : "another assigned session";

        return $"Student already has a scheduled session in {classLabel} at {conflictDatetime:dd/MM/yyyy HH:mm}. Sessions must be at least {minimumGapMinutes} minutes apart.";
    }
}

