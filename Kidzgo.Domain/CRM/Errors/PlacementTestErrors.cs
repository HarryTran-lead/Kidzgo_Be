using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;

namespace Kidzgo.Domain.CRM.Errors;

public static class PlacementTestErrors
{
    public static Error NotFound(Guid? placementTestId) => Error.NotFound(
        "PlacementTest.NotFound",
        $"The placement test with the Id = '{placementTestId}' was not found");

    public static Error LeadNotFound(Guid? leadId) => Error.NotFound(
        "PlacementTest.LeadNotFound",
        $"The lead with the Id = '{leadId}' was not found");

    public static Error StudentProfileNotFound(Guid? profileId) => Error.NotFound(
        "PlacementTest.StudentProfileNotFound",
        $"The student profile with the Id = '{profileId}' was not found");

    public static Error ClassNotFound(Guid? classId) => Error.NotFound(
        "PlacementTest.ClassNotFound",
        $"The class with the Id = '{classId}' was not found");

    public static Error InvigilatorNotFound(Guid? userId) => Error.NotFound(
        "PlacementTest.InvigilatorNotFound",
        $"The invigilator user with the Id = '{userId}' was not found");

    public static Error InvigilatorInvalidRole(Guid? userId) => Error.Validation(
        "PlacementTest.InvigilatorInvalidRole",
        $"The user with Id = '{userId}' cannot be assigned as placement test invigilator");

    public static Error InvigilatorUnavailable(Guid? userId) => Error.Validation(
        "PlacementTest.InvigilatorUnavailable",
        $"The invigilator user with Id = '{userId}' is not available at the selected time");

    public static readonly Error InvigilatorRequired = Error.Validation(
        "PlacementTest.InvigilatorRequired",
        "InvigilatorUserId is required for a scheduled placement test");

    public static readonly Error RoomRequired = Error.Validation(
        "PlacementTest.RoomRequired",
        "RoomId is required for a scheduled placement test");

    public static Error RoomNotFound(Guid? roomId) => Error.NotFound(
        "PlacementTest.RoomNotFound",
        $"The active room with Id = '{roomId}' was not found");

    public static Error RoomBranchMismatch(Guid? roomId, Guid? branchId) => Error.Validation(
        "PlacementTest.RoomBranchMismatch",
        $"The room with Id = '{roomId}' does not belong to branch Id = '{branchId}'");

    public static Error RoomUnavailable(Guid? roomId) => Error.Validation(
        "PlacementTest.RoomUnavailable",
        $"The room with Id = '{roomId}' is not available at the selected time");

    public static readonly Error InvalidDuration = Error.Validation(
        "PlacementTest.InvalidDuration",
        "DurationMinutes must be greater than 0");

    public static readonly Error InvalidStatusTransition = Error.Validation(
        "PlacementTest.InvalidStatusTransition",
        "Invalid status transition. Cannot change from current status to target status");

    public static readonly Error CannotUpdateCompletedTest = Error.Validation(
        "PlacementTest.CannotUpdateCompletedTest",
        "Cannot update a placement test that has been completed");

    public static readonly Error CannotCancelCompletedTest = Error.Validation(
        "PlacementTest.CannotCancelCompletedTest",
        "Cannot cancel a placement test that has been completed");

    public static readonly Error CannotMarkNoShowCompletedTest = Error.Validation(
        "PlacementTest.CannotMarkNoShowCompletedTest",
        "Cannot mark a completed placement test as NoShow");

    public static readonly Error LeadAlreadyEnrolled = Error.Conflict(
        "PlacementTest.LeadAlreadyEnrolled",
        "The lead has already been converted to enrollment");

    public static Error ActivePlacementTestAlreadyExistsForChild(Guid leadChildId) => Error.Conflict(
        "PlacementTest.ActivePlacementTestAlreadyExistsForChild",
        $"This child already has a scheduled placement test. It's not possible to schedule another placement test while one is still on hold. LeadChildId = '{leadChildId}'");

    public static Error ActivePlacementTestAlreadyExistsForStudent(Guid studentProfileId) => Error.Conflict(
        "PlacementTest.ActivePlacementTestAlreadyExistsForStudent",
        $"The student profile with Id = '{studentProfileId}' already has a scheduled placement test.");

    public static Error StudentProfileAlreadyAssigned(Guid? profileId, Guid leadChildId) => Error.Conflict(
        "PlacementTest.StudentProfileAlreadyAssigned",
        $"The student profile with Id = '{profileId}' is already assigned to another child (LeadChildId = '{leadChildId}'). One profile can only be assigned to one child.");

    public static Error NoActiveRegistrationForRetake(Guid studentProfileId) => Error.Validation(
        "PlacementTest.NoActiveRegistrationForRetake",
        $"Student has no active registration to retake placement test");

    public static Error RetakeAlreadyScheduled(Guid studentProfileId) => Error.Conflict(
        "PlacementTest.RetakeAlreadyScheduled",
        $"Student already has a scheduled or completed retake placement test");

    public static Error ProgramNotResolved(Guid placementTestId) => Error.Validation(
        "PlacementTest.ProgramNotResolved",
        $"Cannot resolve program for placement test '{placementTestId}'. Provide ProgramId explicitly or set ProgramRecommendation/Class first.");

    public static readonly Error InvalidQuestionMatrixDistribution = Error.Validation(
        "PlacementTest.InvalidQuestionMatrixDistribution",
        "Question distribution must have at least one level with count > 0");

    public static Error MatrixTotalMismatch(int expectedTotal, int distributedTotal) => Error.Validation(
        "PlacementTest.MatrixTotalMismatch",
        $"TotalQuestions = {expectedTotal} does not match distributed total = {distributedTotal}");

    public static Error InsufficientQuestionsInBank(QuestionLevel level, int required, int available) => Error.Validation(
        "PlacementTest.InsufficientQuestionsInBank",
        $"Not enough question bank items for level '{level}'. Required {required}, available {available}");
}

