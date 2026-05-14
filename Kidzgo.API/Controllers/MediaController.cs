using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Media.ApproveMedia;
using Kidzgo.Application.Media.CreateMedia;
using Kidzgo.Application.Media.DeleteMedia;
using Kidzgo.Application.Media.GetMedia;
using Kidzgo.Application.Media.GetMediaById;
using Kidzgo.Application.Media.PublishMedia;
using Kidzgo.Application.Media.RejectMedia;
using Kidzgo.Application.Media.ResubmitMedia;
using Kidzgo.Application.Media.UpdateMedia;
using Kidzgo.Domain.Media;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/media")]
[ApiController]
public class MediaController : ControllerBase
{
    private readonly ISender _mediator;

    public MediaController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// UC-238: Teacher/Staff upload ảnh/video
    /// UC-239-242: Gắn tag (Class, Student, Month, Type, Visibility)
    [HttpPost]
    [Authorize(Roles = "Teacher,ManagementStaff")]
    public async Task<IResult> CreateMedia(
        [FromBody] CreateMediaRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateMediaCommand(
            request.BranchId,
            request.ClassId,
            request.StudentProfileId,
            request.MonthTag,
            request.Type,
            request.ContentType,
            request.Url,
            request.MimeType,
            request.FileSize,
            request.OriginalFileName,
            request.DisplayOrder,
            request.OwnershipScope,
            request.Caption,
            request.Visibility
        );

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(m => $"/api/media/{m.Id}");
    }

    /// UC-243: Xem danh sách Media
    /// UC-249: Parent/Student xem album lớp
    /// UC-250: Parent/Student xem album cá nhân
    /// UC-251: Filter Media theo tháng
    [HttpGet]
    [Authorize]
    public async Task<IResult> GetMedia(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? classId,
        [FromQuery] Guid? studentProfileId,
        [FromQuery] string? monthTag,
        [FromQuery] DateTime? date,
        [FromQuery] MediaType? type,
        [FromQuery] MediaContentType? contentType,
        [FromQuery] Visibility? visibility,
        [FromQuery] ApprovalStatus? approvalStatus,
        [FromQuery] bool? isPublished,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMediaQuery(
            branchId,
            classId,
            studentProfileId,
            monthTag,
            date,
            type,
            contentType,
            visibility,
            approvalStatus,
            isPublished,
            pageNumber,
            pageSize
        );

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-244: Xem chi tiết Media
    /// UC-252: Download Media (FE sẽ dùng Url từ response)
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IResult> GetMediaById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetMediaByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-245: Cập nhật Media
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff")]
    public async Task<IResult> UpdateMedia(
        Guid id,
        [FromBody] UpdateMediaRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMediaCommand(
            id,
            request.ClassId,
            request.StudentProfileId,
            request.MonthTag,
            request.ContentType,
            request.Caption,
            request.Visibility
        );

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-246: Xóa Media
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff")]
    public async Task<IResult> DeleteMedia(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteMediaCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-247: Staff/Admin approve Media
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> ApproveMedia(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ApproveMediaCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-247a: Staff/Admin reject Media
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> RejectMedia(
        Guid id,
        [FromBody] RejectMediaRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RejectMediaCommand(id, request.Reason);
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("{id:guid}/resubmit")]
    [Authorize(Roles = "Teacher,ManagementStaff")]
    public async Task<IResult> ResubmitMedia(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ResubmitMediaCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-248: Publish Media lên gallery
    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> PublishMedia(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new PublishMediaCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

