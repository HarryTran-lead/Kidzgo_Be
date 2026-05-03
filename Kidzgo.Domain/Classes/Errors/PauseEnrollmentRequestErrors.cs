using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Classes.Errors;

public static class PauseEnrollmentRequestErrors
{
    public static Error NotFound(Guid requestId) => Error.NotFound(
        "PauseEnrollmentRequest.NotFound",
        $"Pause enrollment request with Id = '{requestId}' was not found");

    public static Error StudentNotFound(Guid studentProfileId) => Error.NotFound(
        "PauseEnrollmentRequest.StudentNotFound",
        $"Student profile with Id = '{studentProfileId}' was not found");

    public static Error ClassNotFound(Guid classId) => Error.NotFound(
        "PauseEnrollmentRequest.ClassNotFound",
        $"Class with Id = '{classId}' was not found");

    public static Error NotEnrolled(Guid classId, Guid studentProfileId) => Error.Conflict(
        "PauseEnrollmentRequest.NotEnrolled",
        $"Student '{studentProfileId}' is not enrolled in class '{classId}'");

    public static readonly Error EnrollmentNotActive = Error.Conflict(
        "PauseEnrollmentRequest.EnrollmentNotActive",
        "Only active enrollments can be paused");

    public static readonly Error DuplicateActiveRequest = Error.Conflict(
        "PauseEnrollmentRequest.DuplicateActiveRequest",
        "A pending or approved pause request already exists for this student in the selected date range");

    public static readonly Error NoEnrollmentsInRange = Error.Conflict(
        "PauseEnrollmentRequest.NoEnrollmentsInRange",
        "Student has no active enrollments with sessions in the pause range");

    public static Error ClassNotInPauseRange(Guid classId, DateOnly pauseFrom, DateOnly pauseTo) => Error.Validation(
        "PauseEnrollmentRequest.ClassNotInPauseRange",
        $"Class '{classId}' does not have assigned study sessions in the pause range '{pauseFrom}' - '{pauseTo}'");

    public static readonly Error AlreadyApproved = Error.Conflict(
        "PauseEnrollmentRequest.AlreadyApproved",
        "Pause enrollment request is already approved");

    public static readonly Error AlreadyRejected = Error.Conflict(
        "PauseEnrollmentRequest.AlreadyRejected",
        "Pause enrollment request is already rejected");

    public static readonly Error AlreadyCancelled = Error.Conflict(
        "PauseEnrollmentRequest.AlreadyCancelled",
        "Pause enrollment request is already cancelled");

    public static Error CancelWindowExpired(DateOnly pauseFrom) => Error.Conflict(
        "PauseEnrollmentRequest.CancelWindowExpired",
        $"Cannot cancel after pause start date '{pauseFrom}'");

    public static readonly Error OutcomeNotAllowed = Error.Conflict(
        "PauseEnrollmentRequest.OutcomeNotAllowed",
        "Outcome can only be set for approved requests");

    public static readonly Error OutcomeAlreadyCompleted = Error.Conflict(
        "PauseEnrollmentRequest.OutcomeAlreadyCompleted",
        "Pause enrollment outcome has already been completed");

    public static readonly Error OutcomeMustBeReassignEquivalentClass = Error.Validation(
        "PauseEnrollmentRequest.OutcomeMustBeReassignEquivalentClass",
        "Pause enrollment outcome must be ReassignEquivalentClass before reassigning class");

    public static readonly Error NoPausedEnrollmentToReassign = Error.Conflict(
        "PauseEnrollmentRequest.NoPausedEnrollmentToReassign",
        "No paused enrollment from this pause request can be reassigned");

    public static Error EffectiveDateBeforePauseEnd(DateOnly pauseTo) => Error.Validation(
        "PauseEnrollmentRequest.EffectiveDateBeforePauseEnd",
        $"Effective date must be after pause end date '{pauseTo}'");

    public static readonly Error RegistrationStudentMismatch = Error.Validation(
        "PauseEnrollmentRequest.RegistrationStudentMismatch",
        "Registration does not belong to the paused student");
}
