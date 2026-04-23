using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Users.Errors;

public static class UserErrors
{
    public static readonly Error EmailNotUnique = Error.Conflict(
        "Users.EmailNotUnique",
        "The provided email is not unique");
    
    public static readonly Error AdminAlreadyExists = Error.Conflict(
        code: "User.AdminAlreadyExists",
        description: "Only one Admin is allowed in the system.");
    
    public static Error NotFound(Guid? userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found");

    public static readonly Error NotFoundByEmail = Error.NotFound(
        "Users.NotFoundByEmail",
        "The user with the specified email was not found");

    public static readonly Error NotFoundByPhoneNumber = Error.NotFound(
        "Users.NotFoundByPhoneNumber",
        "The user with the specified phone number was not found");

    public static readonly Error WrongPassword = Error.Conflict(
        "Users.WrongPassword",
        "The passsword for this account is wrong");
    
    public static readonly Error InActive = Error.Conflict(
        "Users.InActive",
        "The user is inactive");

    public static readonly Error InvalidRefreshToken = Error.Conflict(
        "Users.RefreshTokenInvalid",
        "The refresh token is invalid");
    
    public static readonly Error InvalidCurrentPassword = Error.Conflict(
        "Users.InvalidCurrentPassword",
        "The current password provided is incorrect.");

    public static readonly Error SamePassword = Error.Conflict(
        "SamePassword",
        "New password cannot be the same as the current password.");
    public static readonly Error InvalidGoogleToken = Error.Problem(
        "Users.InvalidGoogleToken",
        "Google token is invalid");

    public static readonly Error IsNotVerified = Error.Conflict(
        "NotVerified",
        "Account is not verified");
    
    public static readonly Error IsNotMember = Error.Conflict(
        "NotMember", "User is not member");
    
    public static readonly Error MemberCannotUpdateOthers = Error.Problem(
        "User.MemberCannotUpdateOthers",
        "You are only allowed to update your own donor information.");
    
    public static Error InvalidRole(string? role) => Error.Validation(
        "Users.InvalidRole",
        $"Invalid role value: '{role}'. Valid values: Admin, ManagementStaff, AccountantStaff, Teacher, Student, Parent");
    
    public static readonly Error RoleRequired = Error.Validation(
        "Users.RoleRequired",
        "Role is required. Valid values: Admin, ManagementStaff, AccountantStaff, Teacher, Student, Parent");
    
    public static Error EmailAlreadyExists(string email) => Error.Conflict(
        "User.EmailAlreadyExists",
        $"Email '{email}' is already in use.");

    public static Error InvalidRoleForBranchAssignment => Error.Validation(
        "User.InvalidRole",
        "Only ManagementStaff, AccountantStaff, Teacher, and Parent can be assigned to a branch");

    public static readonly Error BranchRequiredForRole = Error.Validation(
        "User.BranchRequired",
        "BranchId is required for ManagementStaff, AccountantStaff, Teacher, and Parent accounts");

    public static readonly Error PhoneNumberNotUnique = Error.Conflict(
        "Users.PhoneNumberNotUnique",
        "The provided phone number is already in use");

    public static readonly Error HasActiveAssignments = Error.Conflict(
        "Users.HasActiveAssignments",
        "Cannot deactivate, delete, or change role while the user has active assignments");

    public static readonly Error BranchInactive = Error.Conflict(
        "Users.BranchInactive",
        "User cannot be assigned to an inactive branch");
}
