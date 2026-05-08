using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using ProfileType = Kidzgo.Domain.Users.ProfileType;

namespace Kidzgo.Application.SessionReports.GetSessionReportById;

public sealed class GetSessionReportByIdQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetSessionReportByIdQuery, GetSessionReportByIdResponse>
{
    public async Task<Result<GetSessionReportByIdResponse>> Handle(
        GetSessionReportByIdQuery query,
        CancellationToken cancellationToken)
    {
        var sessionReport = await context.SessionReports
            .Include(sr => sr.Session)
                .ThenInclude(s => s.Class)
                    .ThenInclude(c => c.Branch)
            .Include(sr => sr.StudentProfile)
            .Include(sr => sr.TeacherUser)
            .FirstOrDefaultAsync(sr => sr.Id == query.Id, cancellationToken);

        if (sessionReport is null)
        {
            return Result.Failure<GetSessionReportByIdResponse>(SessionReportErrors.NotFound(query.Id));
        }

        // Get current user
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<GetSessionReportByIdResponse>(
                Error.NotFound("User.NotFound", "User not found"));
        }

        // Authorization check based on user role
        if (currentUser.Role == UserRole.Teacher)
        {
            // Teacher can only view reports of their classes
            var isTeacherOfClass = await context.Classes
                .AnyAsync(c => c.Id == sessionReport.Session.ClassId &&
                             (c.MainTeacherId == currentUser.Id || c.AssistantTeacherId == currentUser.Id),
                    cancellationToken);

            if (!isTeacherOfClass)
            {
                return Result.Failure<GetSessionReportByIdResponse>(
                    Error.Validation("SessionReport.Unauthorized", "You can only view reports of your classes"));
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
                return Result.Failure<GetSessionReportByIdResponse>(
                    Error.NotFound("SessionReport.Unauthorized", "User profile not found"));
            }

            // Parent can only view reports of their children
            var isOwner = await context.ParentStudentLinks
                .AnyAsync(psl => psl.ParentProfileId == parentProfile.Id &&
                               psl.StudentProfileId == sessionReport.StudentProfileId,
                    cancellationToken);

            if (!isOwner)
            {
                return Result.Failure<GetSessionReportByIdResponse>(
                    Error.Validation("SessionReport.Unauthorized", "You can only view reports of your children"));
            }

            // Parent can only see published reports
            if (sessionReport.Status != ReportStatus.Published)
            {
                return Result.Failure<GetSessionReportByIdResponse>(
                    Error.Validation("SessionReport.NotPublished", "This report is not published yet"));
            }
        }
        else if (currentUser.Role == UserRole.Parent)
        {
            var studentProfile = await context.Profiles
                .FirstOrDefaultAsync(
                    p => p.UserId == currentUser.Id &&
                         p.ProfileType == ProfileType.Student &&
                         p.IsActive,
                    cancellationToken);

            if (studentProfile is null)
            {
                return Result.Failure<GetSessionReportByIdResponse>(
                    Error.NotFound("SessionReport.Unauthorized", "Student profile not found"));
            }

            // Student can only see their own published reports
            if (sessionReport.StudentProfileId != studentProfile.Id)
            {
                return Result.Failure<GetSessionReportByIdResponse>(
                    Error.Validation("SessionReport.Unauthorized", "You can only view your own reports"));
            }

            if (sessionReport.Status != ReportStatus.Published)
            {
                return Result.Failure<GetSessionReportByIdResponse>(
                    Error.Validation("SessionReport.NotPublished", "This report is not published yet"));
            }
        }
        // Admin/ManagementStaff can view all reports (no additional check)

        // Get comments - query directly from ReportComments table to handle both old data 
        // (where ReportId might be set) and new data (where SessionReportId is set)
        var comments = await context.ReportComments
            .Where(c => c.SessionReportId == sessionReport.Id)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        // Get commenter names separately
        var commenterIds = comments.Select(c => c.CommenterId).Distinct().ToList();
        var commenters = await context.Users
            .Where(u => commenterIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name, cancellationToken);

        var commentDtos = comments.Select(c => new SessionReportCommentDto
        {
            Id = c.Id,
            CommenterId = c.CommenterId,
            CommenterName = commenters.GetValueOrDefault(c.CommenterId) ?? "Unknown",
            Content = c.Content,
            CreatedAt = c.CreatedAt
        }).ToList();

        return new GetSessionReportByIdResponse
        {
            Id = sessionReport.Id,
            SessionId = sessionReport.SessionId,
            SessionDate = VietnamTime.ToVietnamDateTime(sessionReport.Session.PlannedDatetime),
            SessionStatus = sessionReport.Session.Status.ToString(),
            ClassId = sessionReport.Session.ClassId,
            ClassCode = sessionReport.Session.Class.Code,
            ClassTitle = sessionReport.Session.Class.Title,
            BranchId = sessionReport.Session.BranchId,
            BranchName = sessionReport.Session.Class.Branch.Name,
            StudentProfileId = sessionReport.StudentProfileId,
            StudentName = sessionReport.StudentProfile.DisplayName,
            TeacherUserId = sessionReport.TeacherUserId,
            TeacherName = sessionReport.TeacherUser.Name,
            ReportDate = sessionReport.ReportDate,
            Feedback = sessionReport.Feedback,
            Status = sessionReport.Status.ToString(),
            PublishedAt = sessionReport.PublishedAt,
            AiGeneratedSummary = sessionReport.AiGeneratedSummary,
            IsMonthlyCompiled = sessionReport.IsMonthlyCompiled,
            CreatedAt = sessionReport.CreatedAt,
            UpdatedAt = sessionReport.UpdatedAt,
            Comments = commentDtos
        };
    }
}
