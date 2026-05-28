using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.GetLatestStudentReport;

public sealed class GetLatestStudentReportQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetLatestStudentReportQuery, StudentReportDetailDto>
{
    public async Task<Result<StudentReportDetailDto>> Handle(
        GetLatestStudentReportQuery query,
        CancellationToken cancellationToken)
    {
        var accessGuard = new ReportAccessGuard(context, userContext);
        var userResult = await accessGuard.GetCurrentUserAsync(cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<StudentReportDetailDto>(userResult.Error);
        }

        var currentUser = userResult.Value;
        var reportsQuery = context.StudentReports
            .Include(r => r.Student)
            .Include(r => r.Class)
            .Include(r => r.ReportPeriod)
            .Where(r => r.StudentId == query.StudentId)
            .AsQueryable();

        if (query.ReportType.HasValue)
        {
            reportsQuery = reportsQuery.Where(r => r.ReportType == query.ReportType.Value);
        }

        if (currentUser.Role == UserRole.Teacher)
        {
            var classIds = await accessGuard.GetTeacherClassIdsAsync(currentUser.Id, cancellationToken);
            reportsQuery = reportsQuery.Where(r => classIds.Contains(r.ClassId));
        }
        else if (currentUser.Role == UserRole.Parent)
        {
            var studentIds = await accessGuard.GetParentStudentIdsAsync(currentUser.Id, cancellationToken);
            if (!studentIds.Contains(query.StudentId))
            {
                return Result.Failure<StudentReportDetailDto>(
                    Error.Unauthorized("Report.AccessDenied", "Parent can only access reports of their children."));
            }

            reportsQuery = reportsQuery.Where(r =>
                r.ReportType == StudentReportType.Parent &&
                r.IsParentPublished);
        }

        var latestReport = await reportsQuery
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestReport is null)
        {
            return Result.Failure<StudentReportDetailDto>(
                Error.NotFound("Report.NotFound", "No report was found for the requested student."));
        }

        var insights = await context.AIInsights
            .Where(i => i.StudentReportId == latestReport.Id)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync(cancellationToken);

        var risks = await context.RiskAlerts
            .Where(r => r.StudentId == latestReport.StudentId && r.ReportPeriodId == latestReport.ReportPeriodId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var recommendations = await context.Recommendations
            .Where(rec => rec.StudentId == latestReport.StudentId)
            .OrderBy(rec => rec.Status)
            .ThenBy(rec => rec.DueAt)
            .ToListAsync(cancellationToken);

        var shareLogs = await context.ReportShareLogs
            .Where(log => log.StudentReportId == latestReport.Id)
            .OrderByDescending(log => log.SentAt)
            .ToListAsync(cancellationToken);

        return Result.Success(ReportDtoMapper.ToDetail(
            latestReport,
            insights.Select(ReportDtoMapper.ToInsightDto).ToList(),
            risks.Select(ReportDtoMapper.ToRiskDto).ToList(),
            recommendations.Select(ReportDtoMapper.ToRecommendationDto).ToList(),
            shareLogs.Select(ReportDtoMapper.ToShareDto).ToList()));
    }
}
