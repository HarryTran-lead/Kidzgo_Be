using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.ReportTemplates.Shared;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.ReportTemplates.DeleteReportTemplate;

public sealed class DeleteReportTemplateCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<DeleteReportTemplateCommand, bool>
{
    public async Task<Result<bool>> Handle(
        DeleteReportTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var accessGuard = new ReportAccessGuard(context, userContext);
        var currentUserResult = await accessGuard.GetCurrentUserAsync(cancellationToken);
        if (currentUserResult.IsFailure)
        {
            return Result.Failure<bool>(currentUserResult.Error);
        }

        if (!ReportTemplateAccessHelper.CanManage(currentUserResult.Value.Role))
        {
            return Result.Failure<bool>(
                Error.Unauthorized("Report.AccessDenied", "Only admin can manage report templates."));
        }

        var template = await context.ReportTemplates
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (template is null)
        {
            return Result.Failure<bool>(
                Error.NotFound("Report.TemplateNotFound", "Report template was not found."));
        }

        var hasRuns = await context.ReportRuns
            .AnyAsync(x => x.ReportTemplateId == command.Id, cancellationToken);

        if (hasRuns)
        {
            template.IsActive = false;
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success(true);
        }

        context.ReportTemplates.Remove(template);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
