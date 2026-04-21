using Kidzgo.API.Extensions;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Dashboard;
using Kidzgo.Domain.Classes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.API.Controllers;

[Route("api/dashboard")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IDbContext _context;
    private readonly ISender _mediator;

    public DashboardController(ISender mediator, IDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    /// <summary>
    /// Get dashboard data with filters
    /// </summary>
    /// <param name="branchId">Filter by branch ID</param>
    /// <param name="startDate">Filter by start date</param>
    /// <param name="endDate">Filter by end date</param>
    [HttpGet("overall")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff,Teacher")]
    public async Task<IResult> GetDashboard(
        [FromQuery] Guid? branchId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery
        {
            BranchId = branchId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.MatchOk();
        }

        var from = startDate ?? VietnamTime.UtcNow().AddMonths(-5);
        var to = endDate ?? VietnamTime.UtcNow();

        var branchQuery = _context.Branches.AsNoTracking();
        if (branchId.HasValue)
        {
            branchQuery = branchQuery.Where(b => b.Id == branchId.Value);
        }

        var branchSummaries = await branchQuery
            .OrderBy(b => b.Name)
            .Select(b => new
            {
                branchId = b.Id,
                branchName = b.Name,
                totalStudents = _context.Profiles.Count(p =>
                    p.ProfileType == Kidzgo.Domain.Users.ProfileType.Student &&
                    !p.IsDeleted &&
                    p.ClassEnrollments.Any(e => e.Class.BranchId == b.Id)),
                totalRevenue = _context.Payments
                    .Where(p => p.PaidAt != null && p.Invoice.BranchId == b.Id)
                    .Sum(p => (decimal?)p.Amount) ?? 0m,
                attendanceRate = _context.Attendances
                    .Where(a => a.Session.BranchId == b.Id)
                    .Count() == 0
                    ? 0
                    : Math.Round(
                        (decimal)_context.Attendances.Count(a => a.Session.BranchId == b.Id && a.AttendanceStatus == Kidzgo.Domain.Sessions.AttendanceStatus.Present) * 100 /
                        _context.Attendances.Count(a => a.Session.BranchId == b.Id),
                        2),
                activeClasses = _context.Classes.Count(c => c.BranchId == b.Id && c.Status == ClassStatus.Active)
            })
            .ToListAsync(cancellationToken);

        var paymentsQuery = _context.Payments
            .AsNoTracking()
            .Where(p => p.PaidAt != null && p.PaidAt >= from && p.PaidAt <= to);
        if (branchId.HasValue)
        {
            paymentsQuery = paymentsQuery.Where(p => p.Invoice.BranchId == branchId.Value);
        }

        var payments = await paymentsQuery
            .Select(p => new { paidAt = p.PaidAt!.Value, p.Amount })
            .ToListAsync(cancellationToken);

        var revenueTrend = payments
            .GroupBy(p => $"{p.paidAt.Year:D4}-{p.paidAt.Month:D2}")
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                period = g.Key,
                amount = g.Sum(x => x.Amount),
                transactionCount = g.Count()
            })
            .ToList();

        var studentDistribution = await branchQuery
            .OrderBy(b => b.Name)
            .Select(b => new
            {
                branchId = b.Id,
                branchName = b.Name,
                studentCount = _context.Profiles.Count(p =>
                    p.ProfileType == Kidzgo.Domain.Users.ProfileType.Student &&
                    !p.IsDeleted &&
                    p.ClassEnrollments.Any(e => e.Class.BranchId == b.Id))
            })
            .ToListAsync(cancellationToken);

        var attendanceQuery = _context.Attendances
            .AsNoTracking()
            .Where(a => a.Session.PlannedDatetime >= from && a.Session.PlannedDatetime <= to);
        if (branchId.HasValue)
        {
            attendanceQuery = attendanceQuery.Where(a => a.Session.BranchId == branchId.Value);
        }

        var attendanceRows = await attendanceQuery
            .Select(a => new
            {
                a.Session.PlannedDatetime,
                a.AttendanceStatus
            })
            .ToListAsync(cancellationToken);

        var attendanceTrend = attendanceRows
            .GroupBy(a =>
            {
                var plannedAtInVietnam = VietnamTime.ToVietnamDateTime(a.PlannedDatetime);
                return $"{plannedAtInVietnam.Year:D4}-{plannedAtInVietnam.Month:D2}";
            })
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                period = g.Key,
                total = g.Count(),
                present = g.Count(x => x.AttendanceStatus == Kidzgo.Domain.Sessions.AttendanceStatus.Present),
                attendanceRate = g.Count() == 0 ? 0 : Math.Round((decimal)g.Count(x => x.AttendanceStatus == Kidzgo.Domain.Sessions.AttendanceStatus.Present) * 100 / g.Count(), 2)
            })
            .ToList();

        return Results.Ok(new
        {
            success = true,
            data = new
            {
                branchSummaries,
                revenueTrend,
                studentDistribution,
                attendanceTrend,
                summary = result.Value
            }
        });
    }
    
    [HttpGet("student")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff,Teacher")]
    public async Task<IResult> GetStudentDashboard(
        [FromQuery] Guid? branchId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery
        {
            BranchId = branchId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.Map(r => r.Students).MatchOk();
    }

    /// <summary>
    /// Get academic dashboard (attendance, homework, leave, makeup)
    /// </summary>
    [HttpGet("academic")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> GetAcademicDashboard(
        [FromQuery] Guid? branchId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery
        {
            BranchId = branchId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        
        return result.Map(r => new
        {
            r.Attendance,
            r.Homework,
            r.Leave,
            r.MakeupCredits
        }).MatchOk();
    }

    /// <summary>
    /// Get financial dashboard (revenue, outstanding debt)
    /// </summary>
    [HttpGet("finance")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> GetFinanceDashboard(
        [FromQuery] Guid? branchId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery
        {
            BranchId = branchId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        
        return result.Map(r => r.Finance).MatchOk();
    }

    /// <summary>
    /// Get HR dashboard (staff, work hours, payroll)
    /// </summary>
    [HttpGet("hr")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetHrDashboard(
        [FromQuery] Guid? branchId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery
        {
            BranchId = branchId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        
        return result.Map(r => r.HumanResources).MatchOk();
    }

    /// <summary>
    /// Get CRM dashboard (leads, placement tests)
    /// </summary>
    [HttpGet("leads")]
    [Authorize(Roles = "Admin,ManagementStaff,SalesStaff")]
    public async Task<IResult> GetCrmDashboard(
        [FromQuery] Guid? branchId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery
        {
            BranchId = branchId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        
        return result.Map(r => new
        {
            r.Leads,
            r.PlacementTests
        }).MatchOk();
    }
}

