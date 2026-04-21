using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Users.Events;

public sealed record ProfileCreatedEmailProfile(
    Guid ProfileId,
    string ProfileType,
    string DisplayName,
    string FullName,
    string Gender,
    string Birthday
);

public sealed record ProfileCreatedDomainEvent(
    Guid UserId,
    string RecipientName,
    string Email,
    string Phone,
    string Password,
    string Pin,
    IReadOnlyCollection<ProfileCreatedEmailProfile> Profiles
) : IDomainEvent;

