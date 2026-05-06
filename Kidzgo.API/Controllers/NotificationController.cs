using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Notifications.BroadcastNotification;
using Kidzgo.Application.Notifications.GetBroadcastNotificationHistory;
using Kidzgo.Application.Notifications.GetNotifications;
using Kidzgo.Application.Notifications.MarkNotificationAsRead;
using Kidzgo.Application.NotificationTemplates.CreateNotificationTemplate;
using Kidzgo.Application.NotificationTemplates.DeleteNotificationTemplate;
using Kidzgo.Application.NotificationTemplates.GetAllNotificationTemplates;
using Kidzgo.Application.NotificationTemplates.GetNotificationTemplateById;
using Kidzgo.Application.NotificationTemplates.UpdateNotificationTemplate;
using Kidzgo.Application.Notifications.RetryNotification;
using Kidzgo.Application.Users.RegisterDeviceToken;
using Kidzgo.Application.Users.DeleteDeviceToken;
using Kidzgo.Domain.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/notifications")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly ISender _mediator;

    public NotificationController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// UC-325-339: Xem danh sách Notifications
    /// UC-338: Xem trạng thái Notification (PENDING/SENT/FAILED)
    /// <param name="profileId">Filter by profile ID</param>
    /// <param name="unreadOnly">Filter unread notifications only</param>
    /// <param name="status">Filter by status (PENDING/SENT/FAILED)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    [HttpGet]
    [Authorize]
    public async Task<IResult> GetNotifications(
        [FromQuery] Guid? profileId,
        [FromQuery] bool? unreadOnly,
        [FromQuery] NotificationStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetNotificationsQuery
        {
            ProfileId = profileId,
            UnreadOnly = unreadOnly,
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-325-339: Admin/Staff broadcast notification
    [HttpPost("broadcast")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> BroadcastNotification(
        [FromBody] BroadcastNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new BroadcastNotificationCommand
        {
            Title = request.Title,
            Content = request.Content,
            Deeplink = request.Deeplink,
            Channel = request.Channel,
            Kind = request.Kind,
            Priority = request.Priority,
            SenderRole = request.SenderRole,
            SenderName = request.SenderName,
            Role = request.Role,
            BranchId = request.BranchId,
            ClassId = request.ClassId,
            StudentProfileId = request.StudentProfileId,
            UserIds = request.UserIds,
            ProfileIds = request.ProfileIds
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(br => $"/api/notifications/broadcast/{br.CampaignId ?? br.Id ?? Guid.Empty}");
    }

    /// View broadcast notification history (grouped)
    [HttpGet("broadcast-history")]
    [HttpGet("broadcast/history")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetBroadcastNotificationHistory(
        [FromQuery] NotificationChannel? channel,
        [FromQuery] string? senderRole,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? classId,
        [FromQuery] Guid? studentProfileId,
        [FromQuery(Name = "from")] DateTime? from,
        [FromQuery(Name = "to")] DateTime? to,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBroadcastNotificationHistoryQuery
        {
            Channel = channel,
            SenderRole = senderRole,
            BranchId = branchId,
            ClassId = classId,
            StudentProfileId = studentProfileId,
            FromDate = from ?? fromDate,
            ToDate = to ?? toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Mark notification as read
    /// <param name="id">Notification ID</param>
    [HttpPatch("{id:guid}/read")]
    [Authorize]
    public async Task<IResult> MarkNotificationAsRead(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new MarkNotificationAsReadCommand
        {
            NotificationIds = new[] { id }
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-325: Tạo Notification Template
    [HttpPost("templates")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CreateNotificationTemplate(
        [FromBody] CreateNotificationTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateNotificationTemplateCommand
        {
            Code = request.Code,
            Channel = request.Channel,
            Title = request.Title,
            Content = request.Content,
            Placeholders = request.Placeholders,
            Category = request.Category,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(t => $"/api/notifications/templates/{t.Id}");
    }

    /// UC-326: Xem danh sách Notification Templates
    [HttpGet("templates")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetAllNotificationTemplates(
        [FromQuery] NotificationChannel? channel,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isDeleted,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllNotificationTemplatesQuery
        {
            Channel = channel,
            IsActive = isActive,
            IsDeleted = isDeleted,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-326: Xem chi tiết Notification Template
    [HttpGet("templates/{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetNotificationTemplateById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetNotificationTemplateByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-327: Cập nhật Notification Template
    [HttpPut("templates/{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateNotificationTemplate(
        Guid id,
        [FromBody] UpdateNotificationTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateNotificationTemplateCommand
        {
            Id = id,
            Channel = request.Channel,
            Title = request.Title,
            Content = request.Content,
            Placeholders = request.Placeholders,
            Category = request.Category,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-327a: Xóa mềm Notification Template
    [HttpDelete("templates/{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> DeleteNotificationTemplate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteNotificationTemplateCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-339: Retry Notification nếu FAILED
    [HttpPost("{id:guid}/retry")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> RetryNotification(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new RetryNotificationCommand
        {
            NotificationId = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// Register device token for push notifications
    [HttpPost("device-token")]
    [Authorize]
    public async Task<IResult> RegisterDeviceToken(
        [FromBody] RegisterDeviceTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RegisterDeviceTokenCommand
        {
            Token = request.Token,
            DeviceType = request.DeviceType,
            DeviceId = request.DeviceId,
            Role = request.Role,
            Browser = request.Browser,
            Locale = request.Locale,
            BranchId = request.BranchId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// Mark multiple notifications as read
    [HttpPatch("read")]
    [Authorize]
    public async Task<IResult> BulkMarkNotificationsAsRead(
        [FromBody] BulkMarkNotificationsAsReadRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new MarkNotificationAsReadCommand
        {
            NotificationIds = request.NotificationIds
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// Delete device token for push notifications (logout or change device)
    [HttpDelete("device-token")]
    [Authorize]
    public async Task<IResult> DeleteDeviceToken(
        [FromBody] DeleteDeviceTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteDeviceTokenCommand
        {
            Token = request.Token,
            DeviceId = request.DeviceId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

