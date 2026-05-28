using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.ReportTemplates.Shared;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.ReportTemplates.CreateReportTemplate;

public sealed class CreateReportTemplateCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<CreateReportTemplateCommand, ReportTemplateDto>
{
    public async Task<Result<ReportTemplateDto>> Handle(
        CreateReportTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var accessGuard = new ReportAccessGuard(context, userContext);
        var currentUserResult = await accessGuard.GetCurrentUserAsync(cancellationToken);
        if (currentUserResult.IsFailure)
        {
            return Result.Failure<ReportTemplateDto>(currentUserResult.Error);
        }

        if (!ReportTemplateAccessHelper.CanManage(currentUserResult.Value.Role))
        {
            return Result.Failure<ReportTemplateDto>(
                Error.Unauthorized("Report.AccessDenied", "Only admin can manage report templates."));
        }

        var trimmedCode = command.Code.Trim();
        var trimmedName = command.Name.Trim();
        var existingCode = await context.ReportTemplates
            .AnyAsync(
                x => x.Code.ToLower() == trimmedCode.ToLower(),
                cancellationToken);

        if (existingCode)
        {
            return Result.Failure<ReportTemplateDto>(
                Error.Conflict("Report.TemplateCodeExists", $"Report template code '{trimmedCode}' already exists."));
        }

        var now = VietnamTime.UtcNow();
        var template = new ReportTemplate
        {
            Id = Guid.NewGuid(),
            Code = trimmedCode,
            Name = trimmedName,
            Type = command.Type,
            ContentSchema = NormalizeContentSchema(command.ContentSchema, command.Type),
            IsActive = command.IsActive,
            CreatedBy = currentUserResult.Value.Id,
            CreatedAt = now
        };

        context.ReportTemplates.Add(template);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(ReportTemplateMapper.ToDto(template));
    }

    private static string NormalizeContentSchema(string? contentSchema, ReportTemplateType templateType)
    {
        if (string.IsNullOrWhiteSpace(contentSchema))
        {
            return ReportTemplateRuntime.CreateDefaultSchemaJson(templateType);
        }

        return contentSchema.Trim();
    }
}
