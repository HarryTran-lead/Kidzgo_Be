using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.FaqCategories.CreateFaqCategory;
using Kidzgo.Application.FaqCategories.DeleteFaqCategory;
using Kidzgo.Application.FaqCategories.GetFaqCategories;
using Kidzgo.Application.FaqCategories.UpdateFaqCategory;
using Kidzgo.Application.Faqs.CreateFaq;
using Kidzgo.Application.Faqs.DeleteFaq;
using Kidzgo.Application.Faqs.GetFaqs;
using Kidzgo.Application.Faqs.UpdateFaq;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/faqs")]
[ApiController]
public class FaqController : ControllerBase
{
    private readonly ISender _mediator;

    public FaqController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IResult> GetPublishedFaqCategories(CancellationToken cancellationToken = default)
    {
        var query = new GetFaqCategoriesQuery
        {
            PublicOnly = true
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IResult> GetPublishedFaqs(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFaqsQuery
        {
            CategoryId = categoryId,
            SearchTerm = searchTerm,
            PublicOnly = true,
            IsPublished = true,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("admin/categories")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetFaqCategories(
        [FromQuery] bool includeInactive = true,
        [FromQuery] bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFaqCategoriesQuery
        {
            IncludeInactive = includeInactive,
            IncludeDeleted = includeDeleted
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("categories")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CreateFaqCategory(
        [FromBody] CreateFaqCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateFaqCategoryCommand
        {
            Name = request.Name,
            Icon = request.Icon,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(category => $"/api/faqs/categories/{category.Id}");
    }

    [HttpPut("categories/{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateFaqCategory(
        Guid id,
        [FromBody] UpdateFaqCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateFaqCategoryCommand
        {
            Id = id,
            Name = request.Name,
            Icon = request.Icon,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete("categories/{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> DeleteFaqCategory(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteFaqCategoryCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("admin/items")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetFaqs(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isPublished,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFaqsQuery
        {
            CategoryId = categoryId,
            SearchTerm = searchTerm,
            IsPublished = isPublished,
            IncludeDeleted = includeDeleted,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CreateFaq(
        [FromBody] CreateFaqRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateFaqCommand
        {
            CategoryId = request.CategoryId,
            Question = request.Question,
            Answer = request.Answer,
            SortOrder = request.SortOrder,
            IsPublished = request.IsPublished
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(faq => $"/api/faqs/{faq.Id}");
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateFaq(
        Guid id,
        [FromBody] UpdateFaqRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateFaqCommand
        {
            Id = id,
            CategoryId = request.CategoryId,
            Question = request.Question,
            Answer = request.Answer,
            SortOrder = request.SortOrder,
            IsPublished = request.IsPublished
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> DeleteFaq(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteFaqCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
