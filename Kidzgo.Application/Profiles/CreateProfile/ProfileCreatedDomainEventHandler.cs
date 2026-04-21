using System.Net.Mail;
using System.Net;
using System.Text;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Users.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.Profiles.CreateProfile;

public sealed class ProfileCreatedDomainEventHandler(
    IDbContext context,
    IMailService mailService,
    ITemplateRenderer templateRenderer,
    IClientUrlProvider clientUrlProvider,
    ILogger<ProfileCreatedDomainEventHandler> logger
) : INotificationHandler<ProfileCreatedDomainEvent>
{
    private const string TemplateCode = "PROFILE_CREATED";

    public async Task Handle(ProfileCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(notification.Email) || notification.Profiles.Count == 0)
        {
            return;
        }

        try
        {
            EmailTemplate? template = await context.EmailTemplates
                .Where(t => t.Code == TemplateCode && t.IsActive && !t.IsDeleted)
                .SingleOrDefaultAsync(cancellationToken);

            if (template is null)
            {
                return;
            }

            var orderedProfiles = notification.Profiles.ToList();

            var firstProfile = orderedProfiles[0];
            var verifyLink = $"{clientUrlProvider.GetApiUrl()}/api/profiles/{firstProfile.ProfileId}/reactivate-and-update";
            var recipientName = string.IsNullOrWhiteSpace(notification.RecipientName)
                ? firstProfile.DisplayName
                : notification.RecipientName;

            var placeholders = new Dictionary<string, string>
            {
                ["recipient_name"] = recipientName,
                ["profile_count"] = orderedProfiles.Count.ToString(),
                ["profile_names"] = string.Join(", ", orderedProfiles.Select(profile => profile.DisplayName)),
                ["profiles_html"] = BuildProfilesHtml(orderedProfiles),
                ["email"] = notification.Email,
                ["phone"] = notification.Phone,
                ["password"] = notification.Password,
                ["pin"] = notification.Pin,
                ["verify_link"] = verifyLink,
                // Backward-compatible placeholders for older template revisions.
                ["profile_name"] = firstProfile.DisplayName,
                ["profile_type"] = firstProfile.ProfileType,
                ["full_name"] = firstProfile.FullName,
                ["gender"] = firstProfile.Gender,
                ["birth_day"] = firstProfile.Birthday,
                ["update_link"] = verifyLink
            };

            string subject = templateRenderer.Render(template.Subject, placeholders);
            string body = templateRenderer.Render(template.Body ?? string.Empty, placeholders);

            await mailService.SendEmailAsync(notification.Email, subject, body, cancellationToken);
        }
        catch (SmtpException exception)
        {
            logger.LogWarning(
                exception,
                "Failed to send profile created email for user {UserId} to {Email}. Approval will continue.",
                notification.UserId,
                notification.Email);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unexpected error while preparing or sending profile created email for user {UserId} to {Email}. Approval will continue.",
                notification.UserId,
                notification.Email);
        }
    }

    private static string BuildProfilesHtml(IEnumerable<ProfileCreatedEmailProfile> profiles)
    {
        var builder = new StringBuilder();

        foreach (var profile in profiles)
        {
            builder.Append(
                $$"""
                  <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border:1px solid #e2e8f0;border-radius:12px;background:#f8fafc;margin:0 0 16px 0;">
                    <tr>
                      <td style="padding:16px 18px;">
                        <p style="margin:0 0 8px 0;font-size:13px;color:#64748b;">Hồ sơ {{Encode(profile.ProfileType)}}</p>
                        <p style="margin:0 0 6px 0;font-size:14px;"><strong>Tên hiển thị:</strong> {{Encode(profile.DisplayName)}}</p>
                        <p style="margin:0 0 6px 0;font-size:14px;"><strong>Họ tên:</strong> {{Encode(profile.FullName)}}</p>
                        <p style="margin:0 0 6px 0;font-size:14px;"><strong>Loại hồ sơ:</strong> {{Encode(profile.ProfileType)}}</p>
                        <p style="margin:0 0 6px 0;font-size:14px;"><strong>Giới tính:</strong> {{Encode(profile.Gender)}}</p>
                        <p style="margin:0;font-size:14px;"><strong>Ngày sinh:</strong> {{Encode(profile.Birthday)}}</p>
                      </td>
                    </tr>
                  </table>
                  """);
        }

        return builder.ToString();
    }

    private static string Encode(string? value)
    {
        return WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(value) ? "-" : value);
    }
}
