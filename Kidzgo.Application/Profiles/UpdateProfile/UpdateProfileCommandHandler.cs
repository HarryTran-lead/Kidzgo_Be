using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Profiles.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.UpdateProfile;

public sealed class UpdateProfileCommandHandler(IDbContext context)
    : ICommandHandler<UpdateProfileCommand, UpdateProfileResponse>
{
    public async Task<Result<UpdateProfileResponse>> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (profile is null)
        {
            return Result.Failure<UpdateProfileResponse>(ProfileErrors.NotFound(command.Id));
        }

        if (!string.IsNullOrWhiteSpace(command.DisplayName))
        {
            profile.DisplayName = command.DisplayName.Trim();
        }

        if (command.IsActive.HasValue)
        {
            if (profile.IsActive && !command.IsActive.Value)
            {
                var dependencyValidation = await ProfileDependencyGuard.EnsureCanDeactivateOrDeleteAsync(
                    context,
                    profile,
                    cancellationToken);

                if (dependencyValidation.IsFailure)
                {
                    return Result.Failure<UpdateProfileResponse>(dependencyValidation.Error);
                }
            }

            profile.IsActive = command.IsActive.Value;
        }

        profile.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateProfileResponse
        {
            Id = profile.Id,
            UserId = profile.UserId,
            ProfileType = profile.ProfileType,
            DisplayName = profile.DisplayName,
            IsActive = profile.IsActive,
            IsDeleted = profile.IsDeleted,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }
}

