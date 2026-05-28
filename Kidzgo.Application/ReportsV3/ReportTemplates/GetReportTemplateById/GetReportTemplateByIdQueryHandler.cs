using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.ReportTemplates.Shared;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.ReportTemplates.GetReportTemplateById;

public sealed class GetReportTemplateByIdQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetReportTemplateByIdQuery, ReportTemplateDto>
{
    public async Task<Result<ReportTemplateDto>> Handle(
        GetReportTemplateByIdQuery query,
        CancellationToken cancellationToken)
    {
        var accessGuard = new ReportAccessGuard(context, userContext);
        var currentUserResult = await accessGuard.GetCurrentUserAsync(cancellationToken);
        if (currentUserResult.IsFailure)
        {
            return Result.Failure<ReportTemplateDto>(currentUserResult.Error);
        }

        if (!ReportTemplateAccessHelper.CanView(currentUserResult.Value.Role))
        {
            return Result.Failure<ReportTemplateDto>(
                Error.Unauthorized("Report.AccessDenied", "Only admin or management staff can view report templates."));
        }

        var template = await context.ReportTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

        if (template is null)
        {
            return Result.Failure<ReportTemplateDto>(
                Error.NotFound("Report.TemplateNotFound", "Report template was not found."));
        }

        return Result.Success(ReportTemplateMapper.ToDto(template));
    }
}
