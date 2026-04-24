using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Schools.Errors;

public static class BranchErrors
{
    public static Error NotFound(Guid? branchId) => Error.NotFound(
        "Branch.NotFound",
        $"The branch with the Id = '{branchId}' was not found");
    
    public static readonly Error CodeNotUnique = Error.Conflict(
        "Branch.CodeNotUnique",
        "The provided branch code is not unique");
    
    public static readonly Error NameNotUnique = Error.Conflict(
        "Branch.NameNotUnique",
        "The provided branch name is not unique");

    public static readonly Error HasActiveDependencies = Error.Conflict(
        "Branch.HasActiveDependencies",
        "Cannot deactivate branch while it has active classes, students, staff, or rooms");
}

