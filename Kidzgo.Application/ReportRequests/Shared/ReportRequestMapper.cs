using Kidzgo.Domain.Reports;
namespace Kidzgo.Application.ReportRequests.Shared;

internal static class ReportRequestMapper
{
    public static ReportRequestDto ToDto(ReportRequest request)
    {
        return new ReportRequestDto
        {
            Id = request.Id,
            ReportType = request.ReportType.ToString(),
            Status = request.Status.ToString(),
            Priority = request.Priority.ToString(),
            AssignedTeacherUserId = request.AssignedTeacherUserId,
            AssignedTeacherName = request.AssignedTeacher.Name,
            RequestedByUserId = request.RequestedByUserId,
            RequestedByName = request.RequestedByUser.Name,
            TargetStudentProfileId = request.TargetStudentProfileId,
            TargetStudentName = request.TargetStudentProfile?.DisplayName,
            TargetClassId = request.TargetClassId,
            TargetClassCode = request.TargetClass?.Code,
            TargetClassTitle = request.TargetClass?.Title,
            TargetSessionId = request.TargetSessionId,
            TargetSessionDate = request.TargetSession is null
                ? null
                : Kidzgo.Domain.Time.VietnamTime.ToVietnamDateTime(request.TargetSession.PlannedDatetime),
            Month = request.Month,
            Year = request.Year,
            Message = request.Message,
            DueAt = request.DueAt,
            LinkedSessionReportId = request.LinkedSessionReportId,
            LinkedMonthlyReportId = request.LinkedMonthlyReportId,
            SubmittedAt = request.SubmittedAt,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt
        };
    }
}
