using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Classes.CreateClass;

namespace Kidzgo.Application.Classes.OpenClassFromWaitingList;

public sealed class OpenClassFromWaitingListCommand : ICommand<OpenClassFromWaitingListResponse>
{
    public CreateClassCommand CreateClass { get; init; } = new();
    public string Track { get; init; } = "primary";
}
