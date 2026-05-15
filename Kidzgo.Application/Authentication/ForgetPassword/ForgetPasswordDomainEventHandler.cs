using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Authentication.ForgetPassword;

public sealed class ForgetPasswordDomainEventHandler(
    IDbContext context,
    IMailService mailService,
    IClientUrlProvider clientUrlProvider,
    ITemplateRenderer templateRenderer
) : INotificationHandler<ForgetPasswordDomainEvent>
{
    private const string TemplateCode = "FORGOT_PASSWORD";

    public async Task Handle(ForgetPasswordDomainEvent notification, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .SingleOrDefaultAsync(u => u.Id == notification.UserId, cancellationToken);

        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        EmailTemplate? template = await context.EmailTemplates
            .Where(t => t.Code == TemplateCode && t.IsActive && !t.IsDeleted)
            .SingleOrDefaultAsync(cancellationToken);

        if (template is null)
        {
            return;
        }

        // Tạo token reset password có hạn sử dụng
        var token = Guid.NewGuid().ToString("N");

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = token,
            ExpiresAt = VietnamTime.UtcNow().AddHours(1)
        };

        context.PasswordResetTokens.Add(resetToken);
        await context.SaveChangesAsync(cancellationToken);

        var placeholders = new Dictionary<string, string>
        {
            ["user_name"] = user.Username ?? user.Email,
            ["reset_link"] = $"{clientUrlProvider.GetFrontendUrl()}/vi/auth/reset-password?token={token}"
        };

        string subject = template.Subject;
        string body = templateRenderer.Render(template.Body ?? string.Empty, placeholders);

        await mailService.SendEmailAsync(user.Email, subject, body, cancellationToken);
    }
}


