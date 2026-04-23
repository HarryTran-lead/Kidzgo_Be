using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Exams.Errors;

public static class ExamErrors
{
    public static Error NotFound(Guid? examId) => Error.NotFound(
        "Exam.NotFound",
        $"Exam with Id = '{examId}' was not found");

    public static readonly Error ClassNotFound = Error.NotFound(
        "Exam.ClassNotFound",
        "Class not found or inactive");

    public static readonly Error ExamResultNotFound = Error.NotFound(
        "ExamResult.NotFound",
        "Exam result not found");

    public static readonly Error StudentProfileNotFound = Error.NotFound(
        "ExamResult.StudentProfileNotFound",
        "Student profile not found or inactive");

    public static readonly Error ExamResultAlreadyExists = Error.Conflict(
        "ExamResult.AlreadyExists",
        "Exam result already exists for this student");

    public static readonly Error UserNotFound = Error.NotFound(
        "Exam.UserNotFound",
        "User not found");

    public static readonly Error HasSubmissions = Error.Conflict(
        "Exam.HasSubmissions",
        "Cannot delete exam with submissions");

    public static readonly Error HasResults = Error.Conflict(
        "Exam.HasResults",
        "Cannot delete exam with results");
}

