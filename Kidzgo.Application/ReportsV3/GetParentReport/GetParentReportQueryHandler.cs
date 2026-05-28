using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.GetParentReport;

public sealed class GetParentReportQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetParentReportQuery, ParentReportViewResponse>
{
    public async Task<Result<ParentReportViewResponse>> Handle(
        GetParentReportQuery query,
        CancellationToken cancellationToken)
    {
        var accessGuard = new ReportAccessGuard(context, userContext);
        var userResult = await accessGuard.GetCurrentUserAsync(cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<ParentReportViewResponse>(userResult.Error);
        }

        var currentUser = userResult.Value;
        var reportQuery = context.StudentReports
            .Include(r => r.Student)
            .Include(r => r.Class)
            .Include(r => r.ReportPeriod)
            .Where(r => r.StudentId == query.StudentId && r.ReportType == StudentReportType.Parent);

        if (currentUser.Role == UserRole.Parent)
        {
            var studentIds = await accessGuard.GetParentStudentIdsAsync(currentUser.Id, cancellationToken);
            if (!studentIds.Contains(query.StudentId))
            {
                return Result.Failure<ParentReportViewResponse>(
                    Error.Unauthorized("Report.AccessDenied", "Parent can only view reports of linked students."));
            }

            reportQuery = reportQuery.Where(r => r.IsParentPublished);
        }
        else if (currentUser.Role == UserRole.Teacher)
        {
            var classIds = await accessGuard.GetTeacherClassIdsAsync(currentUser.Id, cancellationToken);
            reportQuery = reportQuery.Where(r => classIds.Contains(r.ClassId));
        }
        else if (currentUser.Role is not (UserRole.Admin or UserRole.ManagementStaff))
        {
            return Result.Failure<ParentReportViewResponse>(
                Error.Unauthorized("Report.AccessDenied", "Current role cannot view parent reports."));
        }

        var report = await reportQuery
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (report is null)
        {
            return Result.Failure<ParentReportViewResponse>(
                Error.NotFound("Report.NotFound", "Parent report was not found."));
        }

        ReportSnapshot? snapshot;
        try
        {
            snapshot = JsonSerializer.Deserialize<ReportSnapshot>(report.SnapshotJson, ReportJson.SnapshotOptions);
        }
        catch (JsonException)
        {
            snapshot = null;
        }

        if (snapshot is null)
        {
            return Result.Failure<ParentReportViewResponse>(
                Error.Problem("Report.SnapshotInvalid", "Report snapshot could not be parsed."));
        }

        var pendingRecommendations = await context.Recommendations
            .Where(r =>
                r.StudentId == query.StudentId &&
                (r.Status == RecommendationStatus.Pending || r.Status == RecommendationStatus.Accepted))
            .OrderBy(r => r.DueAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        var remedialSessions = await context.Attendances
            .CountAsync(a =>
                a.StudentProfileId == query.StudentId &&
                a.Session.ClassId == report.ClassId &&
                a.Session.SectionType == Domain.Sessions.SectionType.Remedial &&
                a.AttendanceStatus == Domain.Sessions.AttendanceStatus.Present,
                cancellationToken);

        var remedialStatus = remedialSessions > 0 ? "in_progress" : null;

        return Result.Success(new ParentReportViewResponse
        {
            ReportId = report.Id,
            StudentId = report.StudentId,
            StudentName = report.Student.DisplayName,
            ClassName = report.Class.Title,
            PeriodFrom = report.ReportPeriod.StartDate,
            PeriodTo = report.ReportPeriod.EndDate,
            AttendanceRate = snapshot.AttendanceSummary.AttendanceRate,
            CompletionPercent = snapshot.LearningProgress.CompletionPercent,
            TeacherComment = snapshot.AssessmentSummary.TeacherComment,
            ParentMessage = snapshot.ParentMessage,
            RemainingTickets = snapshot.TicketSummary.Remaining,
            Strengths = snapshot.Strengths,
            Recommendations = pendingRecommendations.Select(r => r.Content).ToList(),
            RemedialStatus = remedialStatus
        });
    }
}
