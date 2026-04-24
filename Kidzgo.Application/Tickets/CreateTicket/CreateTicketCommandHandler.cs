using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Tickets.Notifications;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Tickets;
using Kidzgo.Domain.Tickets.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Tickets.CreateTicket;

public sealed class CreateTicketCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ITemplateRenderer templateRenderer
) : ICommandHandler<CreateTicketCommand, CreateTicketResponse>
{
    public async Task<Result<CreateTicketResponse>> Handle(CreateTicketCommand command, CancellationToken cancellationToken)
    {
        var openedByUserId = userContext.UserId;

        // Check if user exists
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == openedByUserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<CreateTicketResponse>(TicketErrors.UserNotFound);
        }

        // Check if branch exists
        var branch = await context.Branches
            .FirstOrDefaultAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (branch is null)
        {
            return Result.Failure<CreateTicketResponse>(TicketErrors.BranchNotFound);
        }

        // Check if profile exists (if provided)
        if (command.OpenedByProfileId.HasValue)
        {
            var profile = await context.Profiles
                .FirstOrDefaultAsync(p => p.Id == command.OpenedByProfileId.Value && p.UserId == openedByUserId, cancellationToken);

            if (profile is null)
            {
                return Result.Failure<CreateTicketResponse>(TicketErrors.ProfileNotFound);
            }
        }

        // Validate for DirectToTeacher ticket
        Guid? assignedToUserId = null;
        var ticketType = command.Type;

        if (command.Type == TicketType.DirectToTeacher)
        {
            // ClassId is required for DirectToTeacher
            if (!command.ClassId.HasValue)
            {
                return Result.Failure<CreateTicketResponse>(TicketErrors.ClassIdRequiredForDirectToTeacher);
            }

            // AssignedToUserId is required for DirectToTeacher
            if (!command.AssignedToUserId.HasValue)
            {
                return Result.Failure<CreateTicketResponse>(TicketErrors.AssignedUserRequiredForDirectToTeacher);
            }

            // Validate: Assigned user must be the MainTeacher of the class
            var classEntity = await context.Classes
                .FirstOrDefaultAsync(c => c.Id == command.ClassId.Value, cancellationToken);

            if (classEntity is null)
            {
                return Result.Failure<CreateTicketResponse>(TicketErrors.ClassNotFound);
            }

            if (classEntity.MainTeacherId != command.AssignedToUserId.Value)
            {
                return Result.Failure<CreateTicketResponse>(TicketErrors.MustBeMainTeacherOfClass);
            }

            // Validate: Student must be enrolled in this class
            if (command.OpenedByProfileId.HasValue)
            {
                var isEnrolled = await context.ClassEnrollments
                    .AnyAsync(ce => ce.ClassId == command.ClassId.Value 
                        && ce.StudentProfileId == command.OpenedByProfileId.Value
                        && ce.Status == Domain.Classes.EnrollmentStatus.Active, cancellationToken);

                if (!isEnrolled)
                {
                    return Result.Failure<CreateTicketResponse>(TicketErrors.NotEnrolledInClass);
                }
            }

            assignedToUserId = command.AssignedToUserId;
        }

        // Check if class exists (if provided) - for General tickets
        if (command.ClassId.HasValue && command.Type == TicketType.General)
        {
            var classExists = await context.Classes
                .AnyAsync(c => c.Id == command.ClassId.Value, cancellationToken);

            if (!classExists)
            {
                return Result.Failure<CreateTicketResponse>(TicketErrors.ClassNotFound);
            }
        }

        var now = VietnamTime.UtcNow();
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            OpenedByUserId = openedByUserId,
            OpenedByProfileId = command.OpenedByProfileId,
            BranchId = command.BranchId,
            ClassId = command.ClassId,
            Category = command.Category,
            Subject = command.Subject,
            Message = command.Message,
            Status = TicketStatus.Open,
            Type = ticketType,
            AssignedToUserId = assignedToUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Tickets.Add(ticket);
        await context.SaveChangesAsync(cancellationToken);

        if (ticket.AssignedToUserId.HasValue)
        {
            var assignedUser = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == ticket.AssignedToUserId.Value, cancellationToken);

            if (assignedUser is not null)
            {
                await TicketNotificationHelper.NotifyAssignedSupportAsync(
                    context,
                    templateRenderer,
                    ticket,
                    assignedUser.Id,
                    assignedUser.Role.ToString(),
                    user.Role.ToString(),
                    user.Name ?? "System",
                    cancellationToken);
            }
        }

        // Query ticket with navigation properties for response
        var ticketWithNav = await context.Tickets
            .Include(t => t.OpenedByUser)
            .Include(t => t.OpenedByProfile)
            .Include(t => t.Branch)
            .Include(t => t.Class)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == ticket.Id, cancellationToken);

        return new CreateTicketResponse
        {
            Id = ticketWithNav!.Id,
            OpenedByUserId = ticketWithNav.OpenedByUserId,
            OpenedByUserName = ticketWithNav.OpenedByUser.Name,
            OpenedByProfileId = ticketWithNav.OpenedByProfileId,
            OpenedByProfileName = ticketWithNav.OpenedByProfile?.DisplayName,
            BranchId = ticketWithNav.BranchId,
            BranchName = ticketWithNav.Branch.Name,
            ClassId = ticketWithNav.ClassId,
            ClassCode = ticketWithNav.Class?.Code,
            ClassTitle = ticketWithNav.Class?.Title,
            Category = ticketWithNav.Category.ToString(),
            Subject = ticketWithNav.Subject,
            Message = ticketWithNav.Message,
            Status = ticketWithNav.Status.ToString(),
            Type = ticketWithNav.Type.ToString(),
            AssignedToUserId = ticketWithNav.AssignedToUserId,
            AssignedToUserName = ticketWithNav.AssignedToUser?.Name,
            CreatedAt = ticketWithNav.CreatedAt,
            UpdatedAt = ticketWithNav.UpdatedAt
        };
    }
}
