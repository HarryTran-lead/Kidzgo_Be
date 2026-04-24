using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Tickets.Notifications;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Tickets;
using Kidzgo.Domain.Tickets.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Tickets.AddTicketComment;

public sealed class AddTicketCommentCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ITemplateRenderer templateRenderer
) : ICommandHandler<AddTicketCommentCommand, AddTicketCommentResponse>
{
    public async Task<Result<AddTicketCommentResponse>> Handle(AddTicketCommentCommand command, CancellationToken cancellationToken)
    {
        // Check if ticket exists
        var ticket = await context.Tickets
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, cancellationToken);

        if (ticket is null)
        {
            return Result.Failure<AddTicketCommentResponse>(TicketCommentErrors.TicketNotFound);
        }

        // Check if ticket is closed
        if (ticket.Status == TicketStatus.Closed)
        {
            return Result.Failure<AddTicketCommentResponse>(TicketCommentErrors.TicketClosed);
        }

        var commenterUserId = userContext.UserId;

        // Check if user exists
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == commenterUserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<AddTicketCommentResponse>(TicketCommentErrors.UserNotFound);
        }

        // Check if profile exists (if provided)
        if (command.CommenterProfileId.HasValue)
        {
            var profile = await context.Profiles
                .FirstOrDefaultAsync(p => p.Id == command.CommenterProfileId.Value && p.UserId == commenterUserId, cancellationToken);

            if (profile is null)
            {
                return Result.Failure<AddTicketCommentResponse>(TicketCommentErrors.ProfileNotFound);
            }
        }

        var now = VietnamTime.UtcNow();
        var comment = new TicketComment
        {
            Id = Guid.NewGuid(),
            TicketId = command.TicketId,
            CommenterUserId = commenterUserId,
            CommenterProfileId = command.CommenterProfileId,
            Message = command.Message,
            AttachmentUrl = command.AttachmentUrl,
            CreatedAt = now
        };

        context.TicketComments.Add(comment);

        // Update ticket status to InProgress if it's Open and commenter is ManagementStaff/Teacher
        if (ticket.Status == TicketStatus.Open && 
            (user.Role == Domain.Users.UserRole.ManagementStaff || user.Role == Domain.Users.UserRole.Teacher))
        {
            ticket.Status = TicketStatus.InProgress;
            ticket.UpdatedAt = now;
        }
        else if (ticket.Status != TicketStatus.Closed)
        {
            ticket.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        var isSupportReply = user.Role is Domain.Users.UserRole.ManagementStaff or
            Domain.Users.UserRole.Teacher or
            Domain.Users.UserRole.Admin;

        if (isSupportReply && ticket.OpenedByUserId != commenterUserId)
        {
            await TicketNotificationHelper.NotifyRequesterReplyAsync(
                context,
                templateRenderer,
                ticket,
                user.Role.ToString(),
                user.Name ?? "System",
                comment.Message,
                cancellationToken);
        }

        // Query comment with navigation properties for response
        var commentWithNav = await context.TicketComments
            .Include(c => c.CommenterUser)
            .Include(c => c.CommenterProfile)
            .FirstOrDefaultAsync(c => c.Id == comment.Id, cancellationToken);

        return new AddTicketCommentResponse
        {
            Id = commentWithNav!.Id,
            TicketId = commentWithNav.TicketId,
            CommenterUserId = commentWithNav.CommenterUserId,
            CommenterUserName = commentWithNav.CommenterUser.Name,
            CommenterProfileId = commentWithNav.CommenterProfileId,
            CommenterProfileName = commentWithNav.CommenterProfile?.DisplayName,
            Message = commentWithNav.Message,
            AttachmentUrl = commentWithNav.AttachmentUrl,
            CreatedAt = commentWithNav.CreatedAt
        };
    }
}
