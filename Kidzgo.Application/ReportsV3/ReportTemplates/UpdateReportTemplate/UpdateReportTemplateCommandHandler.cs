using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.ReportTemplates.Shared;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.ReportTemplates.UpdateReportTemplate;

public sealed class UpdateReportTemplateCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<UpdateReportTemplateCommand, ReportTemplateDto>
{
    public async Task<Result<ReportTemplateDto>> Handle(
        UpdateReportTemplateCommand command,
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

        var template = await context.ReportTemplates
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (template is null)
        {
            return Result.Failure<ReportTemplateDto>(
                Error.NotFound("Report.TemplateNotFound", "Report template was not found."));
        }

        var trimmedCode = command.Code.Trim();
        var trimmedName = command.Name.Trim();
        var duplicatedCode = await context.ReportTemplates
            .AnyAsync(
                x => x.Id != command.Id &&
                     x.Code.ToLower() == trimmedCode.ToLower(),
                cancellationToken);

        if (duplicatedCode)
        {
            return Result.Failure<ReportTemplateDto>(
                Error.Conflict("Report.TemplateCodeExists", $"Report template code '{trimmedCode}' already exists."));
        }

        template.Code = trimmedCode;
        template.Name = trimmedName;
        template.Type = command.Type;
        template.IsActive = command.IsActive;
        template.ContentSchema = NormalizeContentSchema(command.ContentSchema, command.Type);

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
