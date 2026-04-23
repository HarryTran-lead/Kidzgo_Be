using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Homework.Errors;

public static class HomeworkErrors
{
    public static Error NotFound(Guid? homeworkId) => Error.NotFound(
        "Homework.NotFound",
        $"Homework assignment with Id = '{homeworkId}' was not found");

    public static readonly Error ClassNotFound = Error.NotFound(
        "Homework.ClassNotFound",
        "Class not found or inactive");

    public static Error SessionNotFound(Guid? sessionId) => Error.NotFound(
        "Homework.SessionNotFound",
        $"Session with Id = '{sessionId}' was not found or does not belong to the class");

    public static Error MissionNotFound(Guid? missionId) => Error.NotFound(
        "Homework.MissionNotFound",
        $"Mission with Id = '{missionId}' was not found or inactive");

    public static readonly Error InvalidDueDate = Error.Validation(
        "Homework.InvalidDueDate",
        "Due date must be in the future");

    public static readonly Error CannotUpdate = Error.Validation(
        "Homework.CannotUpdate",
        "Cannot update homework assignment that has submitted or graded submissions");

    public static Error Unauthorized(Guid? homeworkId) => Error.Validation(
        "Homework.Unauthorized",
        $"You do not have permission to access homework assignment with Id = '{homeworkId}'");

    public static Error ClassHasNoActiveStudents(Guid? classId) => Error.Validation(
        "Homework.ClassHasNoActiveStudents",
        $"Class with Id = '{classId}' has no active enrolled students");

    // Homework Submission errors
    public static Error SubmissionNotFound(Guid? homeworkStudentId) => Error.NotFound(
        "HomeworkSubmission.NotFound",
        $"Homework submission with Id = '{homeworkStudentId}' was not found");

    public static readonly Error SubmissionInvalidScore = Error.Validation(
        "HomeworkSubmission.InvalidScore",
        "Score cannot be negative");

    public static Error SubmissionScoreExceedsMax(decimal maxScore) => Error.Validation(
        "HomeworkSubmission.ScoreExceedsMax",
        $"Score cannot exceed maximum score of {maxScore}");

    public static readonly Error SubmissionInvalidStatus = Error.Validation(
        "HomeworkSubmission.InvalidStatus",
        "Status must be either LATE or MISSING");

    public static Error SubmissionInvalidStatusTransition(string currentStatus, string targetStatus) => Error.Validation(
        "HomeworkSubmission.InvalidStatusTransition",
        $"Cannot change status from {currentStatus} to {targetStatus}");

    public static readonly Error SubmissionUnauthorized = Error.Validation(
        "HomeworkSubmission.Unauthorized",
        "You do not have permission to access this homework submission");

    public static readonly Error SubmissionAlreadySubmitted = Error.Validation(
        "HomeworkSubmission.AlreadySubmitted",
        "This homework has already been submitted");

    public static Error SubmissionAttemptLimitReached(int maxAttempts) => Error.Validation(
        "HomeworkSubmission.AttemptLimitReached",
        $"This homework can only be submitted {maxAttempts} time(s)");

    public static readonly Error SubmissionAlreadyAutoGraded = Error.Validation(
        "HomeworkSubmission.AlreadyAutoGraded",
        "This homework was automatically graded 0 because it was not submitted before the deadline");

    public static Error SubmissionInvalidData(string submissionType) => Error.Validation(
        "HomeworkSubmission.InvalidData",
        $"Submission data is required for {submissionType} submission type");

    public static readonly Error InvalidTitle = Error.Validation(
        "Homework.InvalidTitle",
        "Title cannot be empty or whitespace");

    public static readonly Error InvalidMaxScore = Error.Validation(
        "Homework.InvalidMaxScore",
        "MaxScore must be greater than 0");

    public static readonly Error InvalidRewardStars = Error.Validation(
        "Homework.InvalidRewardStars",
        "RewardStars must be greater than or equal to 0");

    public static readonly Error InvalidTimeLimitMinutes = Error.Validation(
        "Homework.InvalidTimeLimitMinutes",
        "TimeLimitMinutes must be greater than 0");

    public static readonly Error InvalidMaxAttempts = Error.Validation(
        "Homework.InvalidMaxAttempts",
        "MaxAttempts must be greater than 0");

    public static readonly Error SubmissionCannotSubmitMissing = Error.Validation(
        "HomeworkSubmission.CannotSubmitMissing",
        "Cannot submit homework with MISSING status");

    public static readonly Error SubmissionTimeExpired = Error.Validation(
        "HomeworkSubmission.TimeExpired",
        "Time limit has expired for this homework");

    public static readonly Error SubmissionNotSubmitted = Error.Validation(
        "HomeworkSubmission.NotSubmitted",
        "Can only grade homework that has been submitted");

    public static readonly Error AiHintNotEnabled = Error.Validation(
        "Homework.AiHintNotEnabled",
        "AI hint is not enabled for this homework");

    public static readonly Error AiRecommendNotEnabled = Error.Validation(
        "Homework.AiRecommendNotEnabled",
        "AI recommendation is not enabled for this homework");

    public static readonly Error AiSpeakingNotAvailable = Error.Validation(
        "Homework.AiSpeakingNotAvailable",
        "AI speaking analysis is not available for this homework");

    public static readonly Error AiSpeakingPracticeFileRequired = Error.Validation(
        "Homework.AiSpeakingPracticeFileRequired",
        "An audio or video file is required for instant AI speaking analysis");

    public static readonly Error AiCreatorTopicRequired = Error.Validation(
        "Homework.AiCreatorTopicRequired",
        "Topic is required for AI question generation");

    public static Error AiCreatorQuestionCountInvalid(int min, int max) => Error.Validation(
        "Homework.AiCreatorQuestionCountInvalid",
        $"Question count must be between {min} and {max}");

    public static readonly Error AiCreatorInvalidPoints = Error.Validation(
        "Homework.AiCreatorInvalidPoints",
        "Points per question must be greater than 0");

    public static readonly Error AiCreatorBusy = Error.Failure(
        "Homework.AiCreatorBusy",
        "Hiện tại AI đang bận, vui lòng thử lại sau ít phút.");

    public static readonly Error AiCreatorUnavailable = Error.Failure(
        "Homework.AiCreatorUnavailable",
        "Hiện tại hệ thống AI đang tạm gián đoạn, vui lòng thử lại sau.");

    // Input validation errors
    public static readonly Error InvalidSubmissionType = Error.Validation(
        "Homework.InvalidSubmissionType",
        "Invalid submission type");

    public static readonly Error InvalidStatusForMarking = Error.Validation(
        "Homework.InvalidStatusForMarking",
        "Status must be either 'LATE' or 'MISSING'");

    // Multiple Choice Homework errors
    public static readonly Error NoQuestionsProvided = Error.Validation(
        "Homework.NoQuestionsProvided",
        "At least one question is required for multiple choice homework");

    public static Error InvalidQuestionText(int questionNumber) => Error.Validation(
        "Homework.InvalidQuestionText",
        $"Question {questionNumber} text cannot be empty");

    public static Error InsufficientOptions(int questionNumber) => Error.Validation(
        "Homework.InsufficientOptions",
        $"Question {questionNumber} must have at least 2 options for multiple choice");

    public static Error InvalidCorrectAnswer(int questionNumber) => Error.Validation(
        "Homework.InvalidCorrectAnswer",
        $"Question {questionNumber} has invalid correct answer index");

    public static Error InvalidPoints(int questionNumber) => Error.Validation(
        "Homework.InvalidPoints",
        $"Question {questionNumber} points must be greater than 0");

    public static Error ProgramNotFound(Guid? programId) => Error.NotFound(
        "Homework.ProgramNotFound",
        $"Program with Id = '{programId}' was not found");

    public static Error QuestionBankItemNotFound(Guid? questionBankItemId) => Error.NotFound(
        "Homework.QuestionBankItemNotFound",
        $"Question bank item with Id = '{questionBankItemId}' was not found");

    public static readonly Error InvalidQuestionDistribution = Error.Validation(
        "Homework.InvalidQuestionDistribution",
        "Question distribution must have at least one level with count > 0");

    public static Error InsufficientQuestionsInBank(QuestionLevel level, int required, int available) => Error.Validation(
        "Homework.InsufficientQuestionsInBank",
        $"Not enough questions in bank for level {level}. Required {required}, available {available}");

    public static Error UnsupportedQuestionBankFileType(string? extension) => Error.Validation(
        "Homework.UnsupportedQuestionBankFileType",
        $"Unsupported question bank file type: {extension}");

    public static Error InvalidQuestionBankFile(string message) => Error.Validation(
        "Homework.InvalidQuestionBankFile",
        message);

    public static Error InvalidQuestionBankRow(int rowNumber, string message) => Error.Validation(
        "Homework.InvalidQuestionBankRow",
        $"Row {rowNumber}: {message}");

    public static readonly Error CannotSubmitMultipleChoice = Error.Validation(
        "HomeworkSubmission.CannotSubmitMultipleChoice",
        "Wrong submission endpoint for this homework type");

    public static readonly Error NoAnswersProvided = Error.Validation(
        "HomeworkSubmission.NoAnswersProvided",
        "At least one answer must be provided");

    public static Error QuestionNotFound(Guid questionNumber) => Error.NotFound(
        "HomeworkSubmission.QuestionNotFound",
        $"Question {questionNumber}was not found");

    public static Error SubmissionAttemptNotFound(Guid homeworkStudentId, int attemptNumber) => Error.NotFound(
        "HomeworkSubmission.AttemptNotFound",
        $"Attempt {attemptNumber} for homework submission '{homeworkStudentId}' was not found");
}


