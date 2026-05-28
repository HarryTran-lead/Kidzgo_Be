using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Class = Kidzgo.Domain.Classes.Class;

namespace Kidzgo.Application.ReportsV3.GetClassAcademicDashboard;

public sealed class GetClassAcademicDashboardQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetClassAcademicDashboardQuery, ClassAcademicDashboardResponse>
{
    public async Task<Result<ClassAcademicDashboardResponse>> Handle(
        GetClassAcademicDashboardQuery query,
        CancellationToken cancellationToken)
    {
        var accessGuard = new ReportAccessGuard(context, userContext);
        var userResult = await accessGuard.GetCurrentUserAsync(cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<ClassAcademicDashboardResponse>(userResult.Error);
        }

        var currentUser = userResult.Value;
        if (currentUser.Role == UserRole.Parent)
        {
            return Result.Failure<ClassAcademicDashboardResponse>(
                Error.Unauthorized("Report.AccessDenied", "Parent cannot access academic class dashboard."));
        }

        var reportClass = await context.Classes
            .Include(c => c.CurrentModule)
            .FirstOrDefaultAsync(c => c.Id == query.ClassId, cancellationToken);

        if (reportClass is null)
        {
            return Result.Failure<ClassAcademicDashboardResponse>(
                Error.NotFound("Report.ClassNotFound", "Class was not found."));
        }

        if (currentUser.Role == UserRole.Teacher &&
            reportClass.MainTeacherId != currentUser.Id &&
            reportClass.AssistantTeacherId != currentUser.Id)
        {
            return Result.Failure<ClassAcademicDashboardResponse>(
                Error.Unauthorized("Report.AccessDenied", "Teacher can only view dashboard for their classes."));
        }

        var period = await ResolvePeriodAsync(query.PeriodId, cancellationToken);
        var fromUtc = VietnamTime.TreatAsVietnamLocal(period.StartDate.ToDateTime(TimeOnly.MinValue));
        var toUtc = VietnamTime.EndOfVietnamDayUtc(
            VietnamTime.TreatAsVietnamLocal(period.EndDate.ToDateTime(TimeOnly.MinValue)));

        var activeStudentIds = await context.ClassEnrollments
            .Where(e => e.ClassId == reportClass.Id && e.Status == EnrollmentStatus.Active)
            .Select(e => e.StudentProfileId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var totalStudents = activeStudentIds.Count;

        var latestEvaluations = await context.TeacherEvaluations
            .Where(te =>
                activeStudentIds.Contains(te.StudentProfileId) &&
                te.ModuleId == reportClass.CurrentModuleId &&
                te.EvaluatedAt <= toUtc)
            .GroupBy(te => te.StudentProfileId)
            .Select(g => g.OrderByDescending(x => x.EvaluatedAt).First())
            .ToListAsync(cancellationToken);

        var weakStudents = latestEvaluations.Count(te => te.Speaking <= 2 || te.Confidence <= 2);

        var totalSectionsOfModule = reportClass.CurrentModule?.PlannedSessionCount ?? 0;
        if (totalSectionsOfModule <= 0)
        {
            totalSectionsOfModule = await context.Sessions
                .Where(s => s.ClassId == reportClass.Id && s.ModuleId == reportClass.CurrentModuleId && s.Status != SessionStatus.Cancelled)
                .CountAsync(cancellationToken);
        }

        var expectedCompletedSections = await context.Sessions
            .Where(s =>
                s.ClassId == reportClass.Id &&
                s.ModuleId == reportClass.CurrentModuleId &&
                s.Status != SessionStatus.Cancelled &&
                s.PlannedDatetime <= toUtc)
            .CountAsync(cancellationToken);

        var plannedProgress = totalSectionsOfModule <= 0
            ? 0m
            : Math.Min(100m, Math.Round((decimal)expectedCompletedSections * 100 / totalSectionsOfModule, 2));

        var latestProgresses = await context.StudentProgresses
            .Where(sp =>
                activeStudentIds.Contains(sp.StudentProfileId) &&
                sp.ModuleId == reportClass.CurrentModuleId)
            .GroupBy(sp => sp.StudentProfileId)
            .Select(g => g.OrderByDescending(x => x.UpdatedAt).First())
            .ToListAsync(cancellationToken);

        var delayedStudents = latestProgresses.Count(sp => sp.CompletionPercent < plannedProgress - 10);
        var remedialRequired = latestProgresses.Count(sp =>
            sp.Status == Domain.AcademicProgression.StudentProgressStatus.RemedialRequired ||
            sp.PromotionStatus == Domain.AcademicProgression.PromotionStatus.RemedialRequired);

        var latestAssessments = await context.Assessments
            .Where(a =>
                activeStudentIds.Contains(a.StudentProfileId) &&
                a.ModuleId == reportClass.CurrentModuleId &&
                a.AssessedAt <= toUtc)
            .GroupBy(a => a.StudentProfileId)
            .Select(g => g.OrderByDescending(x => x.AssessedAt).First())
            .ToListAsync(cancellationToken);

        var failedAssessments = latestAssessments.Count(a => a.Result == Domain.AcademicProgression.AssessmentResult.Fail);

        var classSessionsInPeriod = await context.Sessions
            .Where(s =>
                s.ClassId == reportClass.Id &&
                s.PlannedDatetime >= fromUtc &&
                s.PlannedDatetime <= toUtc &&
                s.Status != SessionStatus.Cancelled)
            .Select(s => new { s.SectionType, s.Status })
            .ToListAsync(cancellationToken);

        var reviewSections = classSessionsInPeriod.Count(s => s.SectionType == SectionType.Review);
        var totalSections = classSessionsInPeriod.Count;
        var reviewRatio = totalSections == 0
            ? 0m
            : Math.Round((decimal)reviewSections * 100 / totalSections, 2);

        var completedSections = classSessionsInPeriod.Count(s => s.Status == SessionStatus.Completed);
        var actualProgress = totalSectionsOfModule <= 0
            ? 0m
            : Math.Min(100m, Math.Round((decimal)completedSections * 100 / totalSectionsOfModule, 2));

        var riskAlerts = await context.RiskAlerts
            .Where(r =>
                r.ClassId == reportClass.Id &&
                r.ReportPeriodId == period.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success(new ClassAcademicDashboardResponse
        {
            ClassId = reportClass.Id,
            ClassName = reportClass.Title,
            TotalStudents = totalStudents,
            WeakStudents = weakStudents,
            DelayedStudents = delayedStudents,
            FailedAssessments = failedAssessments,
            RemedialRequired = remedialRequired,
            ClassPacing = new ClassPacingDto
            {
                ReviewRatioPercent = reviewRatio,
                PlannedProgressPercent = plannedProgress,
                ActualProgressPercent = actualProgress,
                CurriculumDelayRisk = actualProgress + 0.01m < plannedProgress
            },
            RiskAlerts = riskAlerts.Select(ReportDtoMapper.ToRiskDto).ToList()
        });
    }

    private async Task<ReportPeriod> ResolvePeriodAsync(Guid? periodId, CancellationToken cancellationToken)
    {
        if (periodId.HasValue)
        {
            var matchedPeriod = await context.ReportPeriods
                .FirstOrDefaultAsync(p => p.Id == periodId.Value, cancellationToken);

            if (matchedPeriod is not null)
            {
                return matchedPeriod;
            }
        }

        var today = VietnamTime.TodayDateOnly();
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var code = $"auto-{monthStart:yyyyMM}";
        var fallbackPeriod = await context.ReportPeriods
            .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);

        if (fallbackPeriod is not null)
        {
            return fallbackPeriod;
        }

        return new ReportPeriod
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = $"Auto period {monthStart:MM/yyyy}",
            StartDate = monthStart,
            EndDate = monthEnd,
            Type = ReportPeriodType.Monthly,
            CreatedAt = VietnamTime.UtcNow()
        };
    }
}
