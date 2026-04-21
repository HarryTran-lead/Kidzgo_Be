using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using ProfileType = Kidzgo.Domain.Users.ProfileType;

namespace Kidzgo.Application.MonthlyReports.GetMonthlyReportById;

/// <summary>
/// UC-179: Teacher xem draft Monthly Report
/// UC-186: Parent/Student xem Monthly Report
/// </summary>
public sealed class GetMonthlyReportByIdQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetMonthlyReportByIdQuery, GetMonthlyReportByIdResponse>
{
    public async Task<Result<GetMonthlyReportByIdResponse>> Handle(
        GetMonthlyReportByIdQuery query,
        CancellationToken cancellationToken)
    {
        var report = await context.StudentMonthlyReports
            .Include(r => r.StudentProfile)
            .Include(r => r.Class)
                .ThenInclude(c => c!.Program)
            .Include(r => r.SubmittedByUser)
            .Include(r => r.ReviewedByUser)
            .Include(r => r.ReportComments)
                .ThenInclude(c => c.CommenterUser)
            .FirstOrDefaultAsync(r => r.Id == query.ReportId, cancellationToken);

        if (report is null)
        {
            return Result.Failure<GetMonthlyReportByIdResponse>(
                MonthlyReportErrors.NotFound(query.ReportId));
        }

        // Authorization check
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<GetMonthlyReportByIdResponse>(
                Error.NotFound("User.NotFound", "User not found"));
        }

        // Check permissions
        if (currentUser.Role == UserRole.Teacher)
        {
            // Teacher can only view reports of their classes
            var isTeacherOfClass = await context.Classes
                .AnyAsync(c => c.Id == report.ClassId &&
                             (c.MainTeacherId == currentUser.Id || c.AssistantTeacherId == currentUser.Id),
                    cancellationToken);

            if (!isTeacherOfClass)
            {
                return Result.Failure<GetMonthlyReportByIdResponse>(
                    Error.Validation("MonthlyReport.Unauthorized", "You can only view reports of your classes"));
            }
        }
        else if (currentUser.Role == UserRole.Parent)
        {
            var parentProfile = await context.Profiles
                .FirstOrDefaultAsync(
                    p => p.UserId == currentUser.Id &&
                         p.ProfileType == ProfileType.Parent &&
                         p.IsActive,
                    cancellationToken);

            if (parentProfile is null)
            {
                return Result.Failure<GetMonthlyReportByIdResponse>(
                    Error.NotFound("MonthlyReport.Unauthorized", "User profile not found"));
            }

            // Parent can only view reports of their children
            var isOwner = await context.ParentStudentLinks
                .AnyAsync(psl => psl.ParentProfileId == parentProfile.Id &&
                               psl.StudentProfileId == report.StudentProfileId,
                    cancellationToken);

            if (!isOwner)
            {
                return Result.Failure<GetMonthlyReportByIdResponse>(
                    Error.Validation("MonthlyReport.Unauthorized", "You can only view reports of your children"));
            }

            // Parent can only see published reports
            if (report.Status != ReportStatus.Published)
            {
                return Result.Failure<GetMonthlyReportByIdResponse>(
                    Error.Validation("MonthlyReport.NotPublished", "This report is not published yet"));
            }
        }
        // Staff/Admin can view all reports

        // Get report data
        var reportData = await context.MonthlyReportData
            .FirstOrDefaultAsync(rd => rd.ReportId == report.Id, cancellationToken);

        var comments = report.ReportComments
            .OrderBy(c => c.CreatedAt)
            .Select(c => new ReportCommentDto
            {
                Id = c.Id,
                CommenterId = c.CommenterId,
                CommenterName = c.CommenterUser?.Name ?? "Unknown",
                Content = c.Content,
                CreatedAt = c.CreatedAt
            })
            .ToList();

        // Determine which content to show
        var content = report.Status == ReportStatus.Published
            ? report.FinalContent ?? report.DraftContent
            : report.DraftContent;

        return new GetMonthlyReportByIdResponse
        {
            Id = report.Id,
            StudentProfileId = report.StudentProfileId,
            StudentName = report.StudentProfile.DisplayName,
            ClassId = report.ClassId,
            ClassName = report.Class?.Title,
            ProgramId = report.Class?.ProgramId,
            ProgramName = report.Class?.Program?.Name,
            JobId = report.JobId,
            Month = report.Month,
            Year = report.Year,
            DraftContent = report.DraftContent,
            FinalContent = report.FinalContent,
            Status = report.Status.ToString(),
            PdfUrl = report.PdfUrl,
            PdfGeneratedAt = report.PdfGeneratedAt,
            SubmittedBy = report.SubmittedBy,
            SubmittedByName = report.SubmittedByUser?.Name,
            ReviewedBy = report.ReviewedBy,
            ReviewedByName = report.ReviewedByUser?.Name,
            ReviewedAt = report.ReviewedAt,
            PublishedAt = report.PublishedAt,
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt,
            Data = reportData != null ? new MonthlyReportDataDto
            {
                AttendanceData = reportData.AttendanceData,
                HomeworkData = reportData.HomeworkData,
                TestData = reportData.TestData,
                MissionData = reportData.MissionData,
                NotesData = reportData.NotesData,
                TopicsData = reportData.TopicsData
            } : null,
            Comments = comments
        };
    }
}

