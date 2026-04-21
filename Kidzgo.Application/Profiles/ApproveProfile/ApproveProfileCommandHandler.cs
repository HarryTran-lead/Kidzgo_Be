using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.ApproveProfile;

public sealed class ApproveProfileCommandHandler(IDbContext context, IPublisher publisher)
    : ICommandHandler<ApproveProfileCommand, ApproveProfileResponse>
{
    public async Task<Result<ApproveProfileResponse>> Handle(ApproveProfileCommand command, CancellationToken cancellationToken)
    {
        var result = new ApproveProfileResponse();

        if (command.Id == null || !command.Id.Any())
        {
            return Result.Success(result);
        }

        var ids = command.Id.Distinct().ToList();
        var profiles = await context.Profiles
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .Select(p => new
            {
                p.Id,
                p.IsApproved,
                p.UserId,
                p.ProfileType,
                p.DisplayName,
                p.Name,
                p.Gender,
                p.DateOfBirth,
                p.CreatedAt,
                UserName = p.User.Name,
                UserEmail = p.User.Email,
                UserPhoneNumber = p.User.PhoneNumber
            })
            .ToListAsync(cancellationToken);

        var foundIds = profiles.Select(p => p.Id).ToHashSet();

        result.NotFound = ids
            .Where(id => !foundIds.Contains(id))
            .ToList();

        result.AlreadyApproved = profiles
            .Where(p => p.IsApproved)
            .Select(p => p.Id)
            .ToList();

        var idsToApprove = profiles
            .Where(p => !p.IsApproved)
            .Select(p => p.Id)
            .ToList();

        if (!idsToApprove.Any())
        {
            return Result.Success(result);
        }

        var now = VietnamTime.UtcNow();

        var updatedCount = await context.Profiles
            .Where(p => idsToApprove.Contains(p.Id))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.IsApproved, true)
                .SetProperty(p => p.UpdatedAt, now),
                cancellationToken);

        result.ApprovedCount = updatedCount;

        const string defaultPassword = "123456";
        const string defaultPin = "1234";

        var emailEvents = profiles
            .Where(profile => idsToApprove.Contains(profile.Id))
            .GroupBy(profile => profile.UserId)
            .Select(group =>
            {
                var firstProfile = group
                    .OrderBy(profile => profile.CreatedAt)
                    .First();

                var recipientName = group
                    .Select(profile => profile.DisplayName)
                    .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name))
                    ?? firstProfile.UserName
                    ?? string.Empty;

                var emailProfiles = group
                    .OrderBy(profile => profile.CreatedAt)
                    .Select(profile => new ProfileCreatedEmailProfile(
                        profile.Id,
                        VietnameseEnumText.ForProfileType(profile.ProfileType),
                        profile.DisplayName,
                        profile.Name ?? string.Empty,
                        VietnameseEnumText.ForGender(profile.Gender),
                        profile.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty
                    ))
                    .ToList();

                return new ProfileCreatedDomainEvent(
                    group.Key,
                    recipientName,
                    firstProfile.UserEmail ?? string.Empty,
                    firstProfile.UserPhoneNumber ?? string.Empty,
                    defaultPassword,
                    defaultPin,
                    emailProfiles
                );
            })
            .ToList();

        foreach (var emailEvent in emailEvents)
        {
            await publisher.Publish(emailEvent, cancellationToken);
        }

        return Result.Success(result);
    }
}
