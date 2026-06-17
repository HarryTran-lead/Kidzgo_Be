using Kidzgo.Application.Classes.CreateClass;

namespace Kidzgo.Application.Classes.OpenClassFromWaitingList;

public sealed class OpenClassFromWaitingListResponse
{
    public CreateClassResponse CreatedClass { get; init; } = null!;
    public string Track { get; init; } = null!;
    public int WaitingCount { get; init; }
    public int AssignedCount { get; init; }
    public int SkippedCount { get; init; }
    public List<Guid> AssignedRegistrationIds { get; init; } = [];
}
