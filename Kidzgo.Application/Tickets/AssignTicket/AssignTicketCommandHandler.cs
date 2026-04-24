using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Tickets.Notifications;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Tickets.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Tickets.AssignTicket;

public sealed class AssignTicketCommandHandler(
    IDbContext context,
    ITemplateRenderer templateRenderer
) : ICommandHandler<AssignTicketCommand, AssignTicketResponse>
{
    public async Task<Result<AssignTicketResponse>> Handle(AssignTicketCommand command, CancellationToken cancellationToken)
    {
        var ticket = await context.Tickets
            .Include(t => t.OpenedByUser)
            .Include(t => t.OpenedByProfile)
            .Include(t => t.Branch)
            .Include(t => t.Class)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

        if (ticket is null)
        {
            return Result.Failure<AssignTicketResponse>(TicketErrors.NotFound(command.Id));
        }

        // Check if assigned user exists and is ManagementStaff or Teacher
        var assignedUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == command.AssignedToUserId && 
                (u.Role == Domain.Users.UserRole.ManagementStaff || u.Role == Domain.Users.UserRole.Teacher), cancellationToken);

        if (assignedUser is null)
        {
            return Result.Failure<AssignTicketResponse>(TicketErrors.AssignedUserNotFound);
        }

        // Check if assigned user belongs to the same branch as the ticket
        if (assignedUser.BranchId != ticket.BranchId)
        {
            return Result.Failure<AssignTicketResponse>(TicketErrors.AssignedUserBranchMismatch);
        }

        var previousAssignedToUserId = ticket.AssignedToUserId;
        ticket.AssignedToUserId = command.AssignedToUserId;
        
        // Auto-update status to InProgress if ticket is Open
        if (ticket.Status == Domain.Tickets.TicketStatus.Open)
        {
            ticket.Status = Domain.Tickets.TicketStatus.InProgress;
        }

        ticket.UpdatedAt = VietnamTime.UtcNow();
        await context.SaveChangesAsync(cancellationToken);

        if (previousAssignedToUserId != command.AssignedToUserId)
        {
            await TicketNotificationHelper.NotifyAssignedSupportAsync(
                context,
                templateRenderer,
                ticket,
                assignedUser.Id,
                assignedUser.Role.ToString(),
                "System",
                "System",
                cancellationToken);
        }

        // Query ticket with navigation properties for response
        var ticketWithNav = await context.Tickets
            .Include(t => t.OpenedByUser)
            .Include(t => t.OpenedByProfile)
            .Include(t => t.Branch)
            .Include(t => t.Class)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == ticket.Id, cancellationToken);

        return new AssignTicketResponse
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
            AssignedToUserId = ticketWithNav.AssignedToUserId,
            AssignedToUserName = ticketWithNav.AssignedToUser?.Name,
            CreatedAt = ticketWithNav.CreatedAt,
            UpdatedAt = ticketWithNav.UpdatedAt
        };
    }
}
