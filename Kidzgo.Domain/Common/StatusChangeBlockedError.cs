namespace Kidzgo.Domain.Common;

public sealed record StatusChangeBlockedError(
    string Entity,
    Guid EntityId,
    IReadOnlyCollection<string> Reasons,
    IReadOnlyDictionary<string, int> Counts)
    : Error(
        "STATUS_CHANGE_BLOCKED",
        "Cannot deactivate because the entity is currently in use.",
        ErrorType.Conflict);
