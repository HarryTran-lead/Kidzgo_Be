using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.SessionReports.GetTeacherSessionReports;

public sealed class GetTeacherSessionReportsQueryHandler(
    IDbContext context
) : IQueryHandler<GetTeacherSessionReportsQuery, GetTeacherSessionReportsResponse>
{
    public async Task<Result<GetTeacherSessionReportsResponse>> Handle(
        GetTeacherSessionReportsQuery query,
        CancellationToken cancellationToken)
    {
        // Calculate date range for the month
        var fromDate = new DateOnly(query.Year, query.Month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);

        var reportsQuery = context.SessionReports
            .Include(sr => sr.Session)
                .ThenInclude(s => s.Class)
            .Include(sr => sr.StudentProfile)
            .Where(sr => sr.TeacherUserId == query.TeacherUserId &&
                        sr.ReportDate >= fromDate &&
                        sr.ReportDate <= toDate);

        var totalCount = await reportsQuery.CountAsync(cancellationToken);

        var reportRows = await reportsQuery
            .OrderByDescending(sr => sr.ReportDate)
            .ThenByDescending(sr => sr.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(sr => new TeacherSessionReportListItemDto
            {
                Id = sr.Id,
                SessionId = sr.SessionId,
                SessionDate = sr.Session.PlannedDatetime,
                ClassId = sr.Session.ClassId,
                ClassCode = sr.Session.Class.Code,
                ClassTitle = sr.Session.Class.Title,
                StudentProfileId = sr.StudentProfileId,
                StudentName = sr.StudentProfile.DisplayName,
                ReportDate = sr.ReportDate,
                Feedback = sr.Feedback,
                IsMonthlyCompiled = sr.IsMonthlyCompiled,
                CreatedAt = sr.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var items = reportRows
            .Select(sr => new TeacherSessionReportListItemDto
            {
                Id = sr.Id,
                SessionId = sr.SessionId,
                SessionDate = VietnamTime.ToVietnamDateTime(sr.SessionDate),
                ClassId = sr.ClassId,
                ClassCode = sr.ClassCode,
                ClassTitle = sr.ClassTitle,
                StudentProfileId = sr.StudentProfileId,
                StudentName = sr.StudentName,
                ReportDate = sr.ReportDate,
                Feedback = sr.Feedback,
                IsMonthlyCompiled = sr.IsMonthlyCompiled,
                CreatedAt = sr.CreatedAt
            })
            .ToList();

        var page = new Page<TeacherSessionReportListItemDto>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetTeacherSessionReportsResponse
        {
            TeacherUserId = query.TeacherUserId,
            Year = query.Year,
            Month = query.Month,
            SessionReports = page
        };
    }
}
