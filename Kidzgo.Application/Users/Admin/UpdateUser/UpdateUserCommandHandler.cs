using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Profiles.Shared;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.Admin.UpdateUser;

public sealed class UpdateUserCommandHandler(IDbContext context, IUserContext userContext, ISender sender)
    : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    public async Task<Result<UpdateUserResponse>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.Profiles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<UpdateUserResponse>(UserErrors.NotFound(request.UserId));
        }
        
        var targetRole = user.Role;
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            {
                return Result.Failure<UpdateUserResponse>(UserErrors.InvalidRole(request.Role));
            }

            targetRole = role;
        }

        if (!string.IsNullOrWhiteSpace(request.Email) &&
            !string.Equals(request.Email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            bool emailExists = await context.Users.AnyAsync(
                u => u.Id != request.UserId && u.Email.ToLower() == request.Email.ToLower(),
                cancellationToken);

            if (emailExists)
            {
                return Result.Failure<UpdateUserResponse>(UserErrors.EmailNotUnique);
            }
        }

        bool willDeactivate = user.IsActive && request.IsActive == false;
        bool willDelete = !user.IsDeleted && request.isDeleted == true;
        bool willChangeTeacherRole = user.Role == UserRole.Teacher && targetRole != UserRole.Teacher;

        if (willDeactivate || willDelete || willChangeTeacherRole)
        {
            bool hasTeacherAssignments = await context.Classes.AnyAsync(
                                             c => (c.MainTeacherId == user.Id || c.AssistantTeacherId == user.Id) &&
                                                  (c.Status == ClassStatus.Planned ||
                                                   c.Status == ClassStatus.Recruiting ||
                                                   c.Status == ClassStatus.Active ||
                                                   c.Status == ClassStatus.Full),
                                             cancellationToken) ||
                                         await context.Sessions.AnyAsync(
                                             s => s.Status == SessionStatus.Scheduled &&
                                                  s.PlannedDatetime >= VietnamTime.UtcNow() &&
                                                  (s.PlannedTeacherId == user.Id ||
                                                   s.PlannedAssistantId == user.Id ||
                                                   s.ActualTeacherId == user.Id ||
                                                   s.ActualAssistantId == user.Id),
                                             cancellationToken);

            if (hasTeacherAssignments)
            {
                return Result.Failure<UpdateUserResponse>(UserErrors.HasActiveAssignments);
            }
        }

        if (willDeactivate || willDelete)
        {
            foreach (var profile in user.Profiles.Where(p => p.IsActive && !p.IsDeleted))
            {
                var dependencyValidation = await ProfileDependencyGuard.EnsureCanDeactivateOrDeleteAsync(
                    context,
                    profile,
                    cancellationToken);

                if (dependencyValidation.IsFailure)
                {
                    return Result.Failure<UpdateUserResponse>(dependencyValidation.Error);
                }
            }
        }

        user.Role = targetRole;

        if (request.TeacherCompensationType != null)
        {
            if (string.IsNullOrWhiteSpace(request.TeacherCompensationType))
            {
                user.TeacherCompensationType = null;
            }
            else if (Enum.TryParse<TeacherCompensationType>(request.TeacherCompensationType, true, out var teacherCompensationType))
            {
                user.TeacherCompensationType = teacherCompensationType;
            }
        }

        user.Username = request.Username ?? user.Username;
        user.Name = request.Name ?? user.Name;
        user.Email = request.Email ?? user.Email;
        if (request.PhoneNumber != null)
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                user.PhoneNumber = null;
            }
            else
            {
                var phoneLookupCandidates = PhoneNumberNormalizer.GetLookupCandidates(request.PhoneNumber);

                var phoneNumberExists = await context.Users.AnyAsync(
                    u => u.Id != request.UserId &&
                         u.PhoneNumber != null &&
                         phoneLookupCandidates.Contains(
                             u.PhoneNumber
                                 .Replace(" ", "")
                                 .Replace("-", "")
                                 .Replace(".", "")
                                 .Replace("(", "")
                                 .Replace(")", "")
                                 .Replace("+", "")),
                    cancellationToken);

                if (phoneNumberExists)
                {
                    return Result.Failure<UpdateUserResponse>(UserErrors.PhoneNumberNotUnique);
                }

                user.PhoneNumber = PhoneNumberNormalizer.NormalizeVietnamesePhoneNumber(request.PhoneNumber);
            }
        }
        user.IsActive = request.IsActive ?? user.IsActive;
        user.IsDeleted = request.isDeleted ?? user.IsDeleted;
        if (user.Role != UserRole.Teacher)
        {
            user.TeacherCompensationType = null;
        }
        user.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateUserResponse(user));
    }
}
