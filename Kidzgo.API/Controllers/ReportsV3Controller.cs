using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.ReportsV3.GenerateReport;
using Kidzgo.Application.ReportsV3.GetBranchDashboard;
using Kidzgo.Application.ReportsV3.GetClassAcademicDashboard;
using Kidzgo.Application.ReportsV3.GetClassRiskAlerts;
using Kidzgo.Application.ReportsV3.GetLatestStudentReport;
using Kidzgo.Application.ReportsV3.GetParentReport;
using Kidzgo.Application.ReportsV3.GetReportById;
using Kidzgo.Application.ReportsV3.ReportPeriods.CreateReportPeriod;
using Kidzgo.Application.ReportsV3.ReportPeriods.DeleteReportPeriod;
using Kidzgo.Application.ReportsV3.ReportPeriods.GetReportPeriodById;
using Kidzgo.Application.ReportsV3.ReportPeriods.GetReportPeriods;
using Kidzgo.Application.ReportsV3.ReportPeriods.UpdateReportPeriod;
using Kidzgo.Application.ReportsV3.RiskRuleConfigs;
using Kidzgo.Application.ReportsV3.GetStudentRecommendations;
using Kidzgo.Application.ReportsV3.GetStudentReports;
using Kidzgo.Application.ReportsV3.ReportTemplates.CreateReportTemplate;
using Kidzgo.Application.ReportsV3.ReportTemplates.DeleteReportTemplate;
using Kidzgo.Application.ReportsV3.ReportTemplates.GetReportTemplateById;
using Kidzgo.Application.ReportsV3.ReportTemplates.GetReportTemplates;
using Kidzgo.Application.ReportsV3.ReportTemplates.UpdateReportTemplate;
using Kidzgo.Application.ReportsV3.PublishReportToParent;
using Kidzgo.Application.ReportsV3.ShareReport;
using Kidzgo.Domain.Reports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api")]
[ApiController]
[Authorize]
public sealed class ReportsV3Controller : ControllerBase
{
    private readonly ISender _mediator;

    public ReportsV3Controller(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("reports/generate")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> GenerateReport(
        [FromBody] GenerateReportRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseReportType(request.ReportType, out var reportType))
        {
            return Results.BadRequest(new { message = "Invalid reportType. Allowed values: parent, academic, internal." });
        }

        var command = new GenerateReportCommand
        {
            StudentId = request.StudentId,
            ClassId = request.ClassId,
            BranchId = request.BranchId,
            PeriodId = request.PeriodId,
            ReportType = reportType,
            IdempotencyKey = request.IdempotencyKey
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("reports/{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher,Parent")]
    public async Task<IResult> GetReportById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetReportByIdQuery { ReportId = id }, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("students/{id:guid}/reports")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher,Parent")]
    public async Task<IResult> GetStudentReports(
        [FromRoute] Guid id,
        [FromQuery] Guid? classId,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? periodId,
        [FromQuery] string? reportType,
        [FromQuery] string? status,
        [FromQuery] string? q,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryParseNullableEnum(reportType, out StudentReportType? parsedReportType))
        {
            return Results.BadRequest(new { message = "Invalid reportType filter." });
        }

        if (!TryParseNullableEnum(status, out StudentReportStatus? parsedStatus))
        {
            return Results.BadRequest(new { message = "Invalid status filter." });
        }

        var query = new GetStudentReportsQuery
        {
            StudentId = id,
            ClassId = classId,
            BranchId = branchId,
            PeriodId = periodId,
            ReportType = parsedReportType,
            Status = parsedStatus,
            Q = q,
            From = from,
            To = to,
            SortBy = sortBy,
            SortDir = sortDir,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("students/{id:guid}/reports/latest")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher,Parent")]
    public async Task<IResult> GetLatestReport(
        [FromRoute] Guid id,
        [FromQuery] string? reportType,
        CancellationToken cancellationToken)
    {
        if (!TryParseNullableEnum(reportType, out StudentReportType? parsedReportType))
        {
            return Results.BadRequest(new { message = "Invalid reportType filter." });
        }

        var result = await _mediator.Send(
            new GetLatestStudentReportQuery
            {
                StudentId = id,
                ReportType = parsedReportType
            },
            cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("students/{id:guid}/parent-report")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher,Parent")]
    public async Task<IResult> GetParentReport(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetParentReportQuery { StudentId = id },
            cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("classes/{id:guid}/academic-dashboard")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> GetClassAcademicDashboard(
        [FromRoute] Guid id,
        [FromQuery] Guid? periodId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetClassAcademicDashboardQuery
            {
                ClassId = id,
                PeriodId = periodId
            },
            cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("classes/{id:guid}/risk-alerts")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> GetClassRiskAlerts(
        [FromRoute] Guid id,
        [FromQuery] string? riskType,
        [FromQuery] string? severity,
        [FromQuery] string? status,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryParseNullableEnum(riskType, out RiskType? parsedRiskType))
        {
            return Results.BadRequest(new { message = "Invalid riskType filter." });
        }

        if (!TryParseNullableEnum(severity, out RiskSeverity? parsedSeverity))
        {
            return Results.BadRequest(new { message = "Invalid severity filter." });
        }

        if (!TryParseNullableEnum(status, out RiskAlertStatus? parsedStatus))
        {
            return Results.BadRequest(new { message = "Invalid status filter." });
        }

        var result = await _mediator.Send(
            new GetClassRiskAlertsQuery
            {
                ClassId = id,
                RiskType = parsedRiskType,
                Severity = parsedSeverity,
                Status = parsedStatus,
                SortBy = sortBy,
                SortDir = sortDir,
                Page = page,
                PageSize = pageSize
            },
            cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("reports/periods")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetReportPeriods(
        [FromQuery] string? type,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] string? q,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryParseNullableEnum(type, out ReportPeriodType? parsedType) ||
            (parsedType is not null && !Enum.IsDefined(parsedType.Value)))
        {
            return Results.BadRequest(new { message = "Invalid period type filter." });
        }

        var result = await _mediator.Send(
            new GetReportPeriodsQuery
            {
                Type = parsedType,
                From = from,
                To = to,
                Q = q,
                SortBy = sortBy,
                SortDir = sortDir,
                Page = page,
                PageSize = pageSize
            },
            cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("reports/periods/{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetReportPeriodById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetReportPeriodByIdQuery { Id = id }, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("reports/periods")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CreateReportPeriod(
        [FromBody] CreateReportPeriodRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseNullableEnum(request.Type, out ReportPeriodType? parsedType) ||
            parsedType is null ||
            !Enum.IsDefined(parsedType.Value))
        {
            return Results.BadRequest(new { message = "Invalid period type. Allowed values: weekly, monthly, module, custom." });
        }

        var result = await _mediator.Send(
            new CreateReportPeriodCommand
            {
                Code = request.Code,
                Name = request.Name,
                Type = parsedType.Value,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            },
            cancellationToken);

        return result.MatchCreated(period => $"/api/reports/periods/{period.Id}");
    }

    [HttpPut("reports/periods/{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateReportPeriod(
        [FromRoute] Guid id,
        [FromBody] UpdateReportPeriodRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseNullableEnum(request.Type, out ReportPeriodType? parsedType) ||
            parsedType is null ||
            !Enum.IsDefined(parsedType.Value))
        {
            return Results.BadRequest(new { message = "Invalid period type. Allowed values: weekly, monthly, module, custom." });
        }

        var result = await _mediator.Send(
            new UpdateReportPeriodCommand
            {
                Id = id,
                Code = request.Code,
                Name = request.Name,
                Type = parsedType.Value,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            },
            cancellationToken);

        return result.MatchOk();
    }

    [HttpDelete("reports/periods/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> DeleteReportPeriod(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteReportPeriodCommand { Id = id }, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("reports/templates")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetReportTemplates(
        [FromQuery] string? type,
        [FromQuery] bool? isActive,
        [FromQuery] string? q,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryParseNullableEnum(type, out ReportTemplateType? parsedType))
        {
            return Results.BadRequest(new { message = "Invalid template type filter." });
        }

        var result = await _mediator.Send(
            new GetReportTemplatesQuery
            {
                Type = parsedType,
                IsActive = isActive,
                Q = q,
                SortBy = sortBy,
                SortDir = sortDir,
                Page = page,
                PageSize = pageSize
            },
            cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("reports/templates/{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetReportTemplateById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetReportTemplateByIdQuery { Id = id }, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("reports/templates")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> CreateReportTemplate(
        [FromBody] CreateReportTemplateRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseNullableEnum(request.Type, out ReportTemplateType? parsedType) || parsedType is null)
        {
            return Results.BadRequest(new { message = "Invalid template type. Allowed values: parent, academic, class, branch, internal." });
        }

        var result = await _mediator.Send(
            new CreateReportTemplateCommand
            {
                Code = request.Code,
                Name = request.Name,
                Type = parsedType.Value,
                ContentSchema = request.ContentSchema,
                IsActive = request.IsActive
            },
            cancellationToken);

        return result.MatchCreated(template => $"/api/reports/templates/{template.Id}");
    }

    [HttpPut("reports/templates/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> UpdateReportTemplate(
        [FromRoute] Guid id,
        [FromBody] UpdateReportTemplateRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseNullableEnum(request.Type, out ReportTemplateType? parsedType) || parsedType is null)
        {
            return Results.BadRequest(new { message = "Invalid template type. Allowed values: parent, academic, class, branch, internal." });
        }

        var result = await _mediator.Send(
            new UpdateReportTemplateCommand
            {
                Id = id,
                Code = request.Code,
                Name = request.Name,
                Type = parsedType.Value,
                ContentSchema = request.ContentSchema,
                IsActive = request.IsActive
            },
            cancellationToken);

        return result.MatchOk();
    }

    [HttpDelete("reports/templates/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> DeleteReportTemplate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteReportTemplateCommand { Id = id }, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("reports/risk-rules")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> GetRiskRuleConfigs(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRiskRuleConfigsQuery(), cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("reports/risk-rules/{riskType}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> UpdateRiskRuleConfig(
        [FromRoute] string riskType,
        [FromBody] UpdateRiskRuleConfigRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseNullableEnum(riskType, out RiskType? parsedRiskType) || parsedRiskType is null)
        {
            return Results.BadRequest(new { message = "Invalid riskType." });
        }

        var result = await _mediator.Send(
            new UpdateRiskRuleConfigCommand
            {
                RiskType = parsedRiskType.Value,
                IsActive = request.IsActive,
                Score = request.Score,
                ParametersJson = request.ParametersJson
            },
            cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("students/{id:guid}/recommendations")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> GetStudentRecommendations(
        [FromRoute] Guid id,
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] DateTime? dueFrom,
        [FromQuery] DateTime? dueTo,
        [FromQuery] bool? overdue,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryParseNullableEnum(status, out RecommendationStatus? parsedStatus))
        {
            return Results.BadRequest(new { message = "Invalid recommendation status filter." });
        }

        if (!TryParseNullableEnum(priority, out RecommendationPriority? parsedPriority))
        {
            return Results.BadRequest(new { message = "Invalid recommendation priority filter." });
        }

        var result = await _mediator.Send(
            new GetStudentRecommendationsQuery
            {
                StudentId = id,
                Status = parsedStatus,
                Priority = parsedPriority,
                DueFrom = dueFrom,
                DueTo = dueTo,
                Overdue = overdue,
                SortBy = sortBy,
                SortDir = sortDir,
                Page = page,
                PageSize = pageSize
            },
            cancellationToken);

        return result.MatchOk();
    }

    [HttpPost("reports/{id:guid}/share")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> ShareReport(
        [FromRoute] Guid id,
        [FromBody] ShareReportRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseShareChannel(request.Channel, out var channel))
        {
            return Results.BadRequest(new { message = "Invalid channel. Allowed values: app, email, zalo, sms." });
        }

        var result = await _mediator.Send(
            new ShareReportCommand
            {
                ReportId = id,
                Channel = channel,
                RecipientName = request.RecipientName,
                RecipientContact = request.RecipientContact,
                ProviderMessageId = request.ProviderMessageId
            },
            cancellationToken);

        return result.MatchOk();
    }

    [HttpPost("reports/{id:guid}/publish-to-parent")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> PublishReportToParent(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new PublishReportToParentCommand
            {
                ReportId = id
            },
            cancellationToken);

        return result.MatchOk();
    }

    [HttpPost("reports/share-callback")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ShareCallback(
        [FromBody] ShareCallbackRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseNullableEnum(request.Status, out ReportShareStatus? parsedStatus) || parsedStatus is null)
        {
            return Results.BadRequest(new { message = "Invalid callback status." });
        }

        var result = await _mediator.Send(
            new UpdateShareCallbackCommand
            {
                ProviderMessageId = request.ProviderMessageId,
                Status = parsedStatus.Value,
                ViewedAt = request.ViewedAt,
                ErrorMessage = request.ErrorMessage
            },
            cancellationToken);

        return result.MatchOk();
    }

    [HttpPost("reports/{id:guid}/mark-viewed")]
    [Authorize(Roles = "Parent,Admin,ManagementStaff")]
    public async Task<IResult> MarkReportViewed(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new MarkReportViewedCommand { ReportId = id },
            cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("branches/{id:guid}/dashboard")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetBranchDashboard(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBranchDashboardQuery { BranchId = id }, cancellationToken);
        return result.MatchOk();
    }

    private static bool TryParseReportType(string value, out StudentReportType reportType)
    {
        switch ((value ?? string.Empty).Trim().ToLowerInvariant())
        {
            case "parent":
                reportType = StudentReportType.Parent;
                return true;
            case "academic":
                reportType = StudentReportType.Academic;
                return true;
            case "internal":
                reportType = StudentReportType.Internal;
                return true;
            default:
                reportType = default;
                return false;
        }
    }

    private static bool TryParseShareChannel(string value, out ReportShareChannel channel)
    {
        switch ((value ?? string.Empty).Trim().ToLowerInvariant())
        {
            case "app":
                channel = ReportShareChannel.App;
                return true;
            case "email":
                channel = ReportShareChannel.Email;
                return true;
            case "zalo":
                channel = ReportShareChannel.Zalo;
                return true;
            case "sms":
                channel = ReportShareChannel.Sms;
                return true;
            default:
                channel = default;
                return false;
        }
    }

    private static bool TryParseNullableEnum<TEnum>(string? value, out TEnum? parsed)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            parsed = null;
            return true;
        }

        if (Enum.TryParse<TEnum>(value, true, out var enumValue))
        {
            parsed = enumValue;
            return true;
        }

        var normalized = value.Replace("_", string.Empty).Replace("-", string.Empty);
        foreach (var name in Enum.GetNames<TEnum>())
        {
            var candidate = name.Replace("_", string.Empty).Replace("-", string.Empty);
            if (candidate.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            {
                parsed = Enum.Parse<TEnum>(name);
                return true;
            }
        }

        parsed = null;
        return false;
    }
}
