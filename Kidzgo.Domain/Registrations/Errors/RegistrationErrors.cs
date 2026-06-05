using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Registrations.Errors;

public static class RegistrationErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Registration.NotFound",
        $"Registration with ID {id} not found");

    public static Error AlreadyExists(Guid studentId, Guid programId) => Error.Conflict(
        "Registration.AlreadyExists",
        $"Student already has an active registration for this program");

    public static Error InvalidStatus(string currentStatus, string action) => Error.Validation(
        "Registration.InvalidStatus",
        $"Cannot perform action '{action}' on registration with status '{currentStatus}'");

    public static Error ClassNotFound(Guid classId) => Error.NotFound(
        "Registration.ClassNotFound",
        $"Class with ID {classId} not found");

    public static Error ClassFull(Guid classId) => Error.Validation(
        "Registration.ClassFull",
        $"Class with ID {classId} is already full");

    public static Error ClassNotMatchingProgram(Guid classId, Guid programId) => Error.Validation(
        "Registration.ClassNotMatchingProgram",
        $"Class does not match the registered program");

    public static Error ClassNotMatchingLevel(Guid classId, Guid levelId) => Error.Validation(
        "Registration.ClassNotMatchingLevel",
        $"Class with ID {classId} does not belong to level {levelId}");

    public static Error TuitionPlanLevelMismatch(Guid tuitionPlanId, Guid classId) => Error.Validation(
        "Registration.TuitionPlanLevelMismatch",
        $"Tuition plan '{tuitionPlanId}' does not match class '{classId}' level.");

    public static Error TuitionPlanModuleMismatch(Guid tuitionPlanId, Guid classId) => Error.Validation(
        "Registration.TuitionPlanModuleMismatch",
        $"Tuition plan '{tuitionPlanId}' does not match class '{classId}' start module.");

    public static Error ModuleBasedTuitionPlanRequiresUpcomingClass(Guid tuitionPlanId) => Error.Validation(
        "Registration.ModuleBasedTuitionPlanRequiresUpcomingClass",
        $"Tuition plan '{tuitionPlanId}' can only be assigned to classes that are planned or recruiting.");

    public static Error StudentNotFound(Guid studentProfileId) => Error.NotFound(
        "Registration.StudentNotFound",
        $"Student profile with ID {studentProfileId} not found");

    public static Error ProgramNotFound(Guid programId) => Error.NotFound(
        "Registration.ProgramNotFound",
        $"Program with ID {programId} not found");

    public static Error ProgramNotAvailableInBranch(Guid programId, Guid branchId) => Error.Validation(
        "Registration.ProgramNotAvailableInBranch",
        $"Program with ID {programId} is not assigned to branch {branchId}");

    public static Error SecondarySupplementaryRequiresSeparateRegistration(Guid programId) => Error.Validation(
        "Registration.SecondarySupplementaryRequiresSeparateRegistration",
        $"Supplementary program with ID {programId} must be created as a separate registration because it uses a separate tuition plan");

    public static Error TuitionPlanNotFound(Guid tuitionPlanId) => Error.NotFound(
        "Registration.TuitionPlanNotFound",
        $"Tuition plan with ID {tuitionPlanId} not found");

    public static Error BranchNotFound(Guid branchId) => Error.NotFound(
        "Registration.BranchNotFound",
        $"Branch with ID {branchId} not found");

    public static Error ClassNotMatchingBranch(Guid classId, Guid branchId) => Error.Validation(
        "Registration.ClassNotMatchingBranch",
        $"Class with ID {classId} does not belong to branch {branchId}");

    public static Error CannotTransferToSameClass() => Error.Validation(
        "Registration.CannotTransferToSameClass",
        "Cannot transfer to the same class");

    public static Error CannotTransferToSameBranch() => Error.Validation(
        "Registration.CannotTransferToSameBranch",
        "Cannot transfer registration to the same branch");

    public static Error CannotTransferBranchWithSecondaryClass() => Error.Validation(
        "Registration.CannotTransferBranchWithSecondaryClass",
        "Cannot transfer branch while the registration still has a secondary class assigned.");

    public static Error CannotCancelWhenStudying() => Error.Validation(
        "Registration.CannotCancelWhenStudying",
        "Cannot cancel an active registration. Please drop from class first.");

    public static Error NoActiveRegistrationForUpgrade(Guid studentProfileId) => Error.Validation(
        "Registration.NoActiveRegistrationForUpgrade",
        $"Student has no active registration to upgrade");

    public static Error InvalidUpgradeTuitionPlan() => Error.Validation(
        "Registration.InvalidUpgradeTuitionPlan",
        "New tuition plan must be different from current plan");

    public static Error InvalidEntryType(string? entryType) => Error.Validation(
        "Registration.InvalidEntryType",
        $"Invalid entry type: {entryType}. Allowed values are immediate, wait, retake.");

    public static Error AlreadyPaused() => Error.Validation(
        "Registration.AlreadyPaused",
        "Registration is already paused");

    public static Error NotPaused() => Error.Validation(
        "Registration.NotPaused",
        "Registration is not paused");

    public static Error InvalidEnrollmentConfirmationPdfFormType(string? formType) => Error.Validation(
        "Registration.InvalidEnrollmentConfirmationPdfFormType",
        $"Invalid enrollment confirmation PDF form type: {formType}. Allowed values are auto, new, newStudent, continuing, continuingStudent.");

    public static Error TicketTypeIncompatibleWithClassSlotType(Guid? learningTicketTypeId, Guid? slotTypeId) => Error.Conflict(
        "Registration.TicketTypeIncompatibleWithClassSlotType",
        $"Registration ticket type '{learningTicketTypeId}' is incompatible with class slot type '{slotTypeId}'.");
}
