using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportsV3.GetBranchDashboard;

public sealed class GetBranchDashboardQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetBranchDashboardQuery, BranchDashboardResponse>
{
    public async Task<Result<BranchDashboardResponse>> Handle(
        GetBranchDashboardQuery query,
        CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<BranchDashboardResponse>(
                Error.NotFound("Report.UserNotFound", "Current user was not found."));
        }

        if (currentUser.Role is not (UserRole.Admin or UserRole.ManagementStaff))
        {
            return Result.Failure<BranchDashboardResponse>(
                Error.Unauthorized("Report.AccessDenied", "Only admin/management can view branch dashboard."));
        }

        var branchExists = await context.Branches.AnyAsync(b => b.Id == query.BranchId, cancellationToken);
        if (!branchExists)
        {
            return Result.Failure<BranchDashboardResponse>(
                Error.NotFound("Report.BranchNotFound", "Branch was not found."));
        }

        var activeClassIds = await context.Classes
            .Where(c =>
                c.BranchId == query.BranchId &&
                (c.Status == ClassStatus.Active || c.Status == ClassStatus.Full))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var totalActiveClasses = activeClassIds.Count;
        var totalActiveStudents = await context.ClassEnrollments
            .Where(e => activeClassIds.Contains(e.ClassId) && e.Status == EnrollmentStatus.Active)
            .Select(e => e.StudentProfileId)
            .Distinct()
            .CountAsync(cancellationToken);

        var openBranchRisks = await context.RiskAlerts
            .Where(r =>
                r.BranchId == query.BranchId &&
                r.Status == RiskAlertStatus.Open)
            .ToListAsync(cancellationToken);

        var riskStudents = openBranchRisks
            .Where(r => r.StudentId.HasValue)
            .Select(r => r.StudentId!.Value)
            .Distinct()
            .Count();

        var riskClasses = openBranchRisks
            .Where(r => r.ClassId.HasValue)
            .Select(r => r.ClassId!.Value)
            .Distinct()
            .Count();

        var packageExpiringCount = openBranchRisks
            .Count(r => r.RiskType == RiskType.PackageExpiring && r.StudentId.HasValue);

        var assessmentFailCount = openBranchRisks
            .Count(r => r.RiskType == RiskType.AcademicFail && r.StudentId.HasValue);

        return Result.Success(new BranchDashboardResponse
        {
            BranchId = query.BranchId,
            TotalActiveClasses = totalActiveClasses,
            TotalActiveStudents = totalActiveStudents,
            RiskStudents = riskStudents,
            RiskClasses = riskClasses,
            PackageExpiringCount = packageExpiringCount,
            AssessmentFailCount = assessmentFailCount
        });
    }
}
