using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.GetReportById;

public sealed class GetReportByIdQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetReportByIdQuery, StudentReportDetailDto>
{
    public async Task<Result<StudentReportDetailDto>> Handle(
        GetReportByIdQuery query,
        CancellationToken cancellationToken)
    {
        var report = await context.StudentReports
            .Include(r => r.Student)
            .Include(r => r.Class)
            .Include(r => r.ReportPeriod)
            .FirstOrDefaultAsync(r => r.Id == query.ReportId, cancellationToken);

        if (report is null)
        {
            return Result.Failure<StudentReportDetailDto>(
                Error.NotFound("Report.NotFound", "Report was not found."));
        }

        var accessGuard = new ReportAccessGuard(context, userContext);
        var userResult = await accessGuard.GetCurrentUserAsync(cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<StudentReportDetailDto>(userResult.Error);
        }

        var currentUser = userResult.Value;

        if (currentUser.Role == UserRole.Teacher)
        {
            var teacherClassIds = await accessGuard.GetTeacherClassIdsAsync(currentUser.Id, cancellationToken);
            if (!teacherClassIds.Contains(report.ClassId))
            {
                return Result.Failure<StudentReportDetailDto>(
                    Error.Unauthorized("Report.AccessDenied", "Teacher cannot access report outside their classes."));
            }
        }
        else if (currentUser.Role == UserRole.Parent)
        {
            var parentStudentIds = await accessGuard.GetParentStudentIdsAsync(currentUser.Id, cancellationToken);
            if (!parentStudentIds.Contains(report.StudentId))
            {
                return Result.Failure<StudentReportDetailDto>(
                    Error.Unauthorized("Report.AccessDenied", "Parent can only access child reports."));
            }

            if (report.ReportType != StudentReportType.Parent)
            {
                return Result.Failure<StudentReportDetailDto>(
                    Error.Unauthorized("Report.ParentViewOnly", "Parent can only view parent report type."));
            }

            if (!report.IsParentPublished)
            {
                return Result.Failure<StudentReportDetailDto>(
                    Error.Validation("Report.NotPublished", "This report is not published to parent yet."));
            }
        }

        var insights = await context.AIInsights
            .Where(i => i.StudentReportId == report.Id)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync(cancellationToken);

        var risks = await context.RiskAlerts
            .Where(r => r.StudentId == report.StudentId && r.ReportPeriodId == report.ReportPeriodId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var recommendations = await context.Recommendations
            .Where(rec => rec.StudentId == report.StudentId)
            .OrderBy(rec => rec.Status)
            .ThenBy(rec => rec.DueAt)
            .ToListAsync(cancellationToken);

        var shareLogs = await context.ReportShareLogs
            .Where(log => log.StudentReportId == report.Id)
            .OrderByDescending(log => log.SentAt)
            .ToListAsync(cancellationToken);

        var response = ReportDtoMapper.ToDetail(
            report,
            insights.Select(ReportDtoMapper.ToInsightDto).ToList(),
            risks.Select(ReportDtoMapper.ToRiskDto).ToList(),
            recommendations.Select(ReportDtoMapper.ToRecommendationDto).ToList(),
            shareLogs.Select(ReportDtoMapper.ToShareDto).ToList());

        return Result.Success(response);
    }
}
