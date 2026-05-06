using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.ProgramProgressions.Errors;

public static class ProgramProgressionErrors
{
    public static Error RuleNotFound(Guid id) => Error.NotFound(
        "ProgramProgression.RuleNotFound",
        $"Program progression rule with ID '{id}' was not found.");

    public static Error AssessmentNotFound(Guid id) => Error.NotFound(
        "ProgramProgression.AssessmentNotFound",
        $"Program progression assessment with ID '{id}' was not found.");

    public static Error ScheduleNotFound(Guid id) => Error.NotFound(
        "ProgramProgression.ScheduleNotFound",
        $"Program progression schedule with ID '{id}' was not found.");

    public static Error ScheduleParticipantNotFound(Guid id) => Error.NotFound(
        "ProgramProgression.ScheduleParticipantNotFound",
        $"Program progression schedule participant with ID '{id}' was not found.");

    public static Error ActiveRuleAlreadyExists(Guid sourceProgramId) => Error.Conflict(
        "ProgramProgression.ActiveRuleAlreadyExists",
        $"An active progression rule already exists for source program '{sourceProgramId}'.");

    public static Error NoActiveRuleForProgram(Guid sourceProgramId) => Error.Validation(
        "ProgramProgression.NoActiveRuleForProgram",
        $"No active progression rule was found for source program '{sourceProgramId}'.");

    public static Error InvalidRuleConfiguration(string description) => Error.Validation(
        "ProgramProgression.InvalidRuleConfiguration",
        description);

    public static Error AssessmentAlreadyApproved(Guid assessmentId) => Error.Validation(
        "ProgramProgression.AssessmentAlreadyApproved",
        $"Assessment '{assessmentId}' has already been approved.");

    public static Error AssessmentNotEligible(Guid assessmentId) => Error.Validation(
        "ProgramProgression.AssessmentNotEligible",
        $"Assessment '{assessmentId}' is not eligible for approval.");

    public static Error RegistrationNotFound(Guid registrationId) => Error.NotFound(
        "ProgramProgression.RegistrationNotFound",
        $"Registration '{registrationId}' was not found.");

    public static Error InvalidRegistrationStatus(string status) => Error.Validation(
        "ProgramProgression.InvalidRegistrationStatus",
        $"Cannot create or approve a progression assessment for registration status '{status}'.");

    public static Error SourceRegistrationRequired => Error.Validation(
        "ProgramProgression.SourceRegistrationRequired",
        "Either SourceRegistrationId or ScheduleParticipantId must be provided.");

    public static Error SourceProgramMismatch(Guid registrationId, Guid programId) => Error.Validation(
        "ProgramProgression.SourceProgramMismatch",
        $"Registration '{registrationId}' does not match source program '{programId}'.");

    public static Error TargetProgramMissing(Guid ruleId) => Error.Validation(
        "ProgramProgression.TargetProgramMissing",
        $"Rule '{ruleId}' does not define a target program.");

    public static Error SourceClassNotFound(Guid classId) => Error.NotFound(
        "ProgramProgression.SourceClassNotFound",
        $"Source class '{classId}' was not found.");

    public static Error AssignedTeacherRequired => Error.Validation(
        "ProgramProgression.AssignedTeacherRequired",
        "An assigned teacher is required for the progression schedule.");

    public static Error InvalidScheduleDuration => Error.Validation(
        "ProgramProgression.InvalidScheduleDuration",
        "Progression schedule duration must be greater than zero.");

    public static Error AssignedTeacherMustTeachClass(Guid teacherUserId, Guid classId) => Error.Validation(
        "ProgramProgression.AssignedTeacherMustTeachClass",
        $"Teacher '{teacherUserId}' is not assigned to class '{classId}'.");

    public static Error AssignedTeacherUnavailable(Guid teacherUserId) => Error.Conflict(
        "ProgramProgression.AssignedTeacherUnavailable",
        $"Teacher '{teacherUserId}' is not available for the selected progression schedule.");

    public static Error RoomUnavailable(Guid roomId) => Error.Conflict(
        "ProgramProgression.RoomUnavailable",
        $"Room '{roomId}' is not available for the selected progression schedule.");

    public static Error RoomBranchMismatch(Guid roomId, Guid branchId) => Error.Validation(
        "ProgramProgression.RoomBranchMismatch",
        $"Room '{roomId}' does not belong to branch '{branchId}'.");

    public static Error ScheduleHasNoEligibleStudents(Guid classId) => Error.Validation(
        "ProgramProgression.ScheduleHasNoEligibleStudents",
        $"Class '{classId}' does not have eligible students for a progression schedule.");

    public static Error StudentNotInSourceClass(Guid studentProfileId, Guid classId) => Error.Validation(
        "ProgramProgression.StudentNotInSourceClass",
        $"Student '{studentProfileId}' is not eligible in source class '{classId}'.");

    public static Error ActiveScheduleAlreadyExists(Guid sourceRegistrationId) => Error.Conflict(
        "ProgramProgression.ActiveScheduleAlreadyExists",
        $"Registration '{sourceRegistrationId}' already has a scheduled progression assessment.");

    public static Error ScheduleAlreadyProcessing(Guid scheduleId) => Error.Validation(
        "ProgramProgression.ScheduleAlreadyProcessing",
        $"Schedule '{scheduleId}' can only be changed before any participant has been processed.");

    public static Error ScheduleParticipantCannotBeMarkedNoShow(Guid participantId, string status) => Error.Validation(
        "ProgramProgression.ScheduleParticipantCannotBeMarkedNoShow",
        $"Schedule participant '{participantId}' cannot be marked as no-show from status '{status}'.");

    public static Error AssessmentAlreadyLinkedToScheduleParticipant(Guid participantId) => Error.Conflict(
        "ProgramProgression.AssessmentAlreadyLinkedToScheduleParticipant",
        $"Schedule participant '{participantId}' already has an assessment.");

    public static Error ScheduleParticipantInvalidStatus(Guid participantId, string status) => Error.Validation(
        "ProgramProgression.ScheduleParticipantInvalidStatus",
        $"Schedule participant '{participantId}' is in status '{status}' and cannot receive an assessment result.");

    public static Error TeacherCannotManageAssessment(Guid teacherUserId, Guid sourceClassId) => Error.Unauthorized(
        "ProgramProgression.TeacherCannotManageAssessment",
        $"Teacher '{teacherUserId}' is not allowed to manage progression assessments for class '{sourceClassId}'.");

    public static Error TeacherNotAssignedToSchedule(Guid teacherUserId, Guid scheduleId) => Error.Unauthorized(
        "ProgramProgression.TeacherNotAssignedToSchedule",
        $"Teacher '{teacherUserId}' is not assigned to schedule '{scheduleId}'.");
}
