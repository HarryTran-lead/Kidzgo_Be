using Kidzgo.API.Extensions;
using Kidzgo.API.Infrastructure;
using Kidzgo.API.Requests;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Classes.GetTeacherClasses;
using Kidzgo.Application.Classes.GetTeacherClassStudents;
using Kidzgo.Application.Classes.GetTeacherStudents;
using Kidzgo.Application.Sessions.GetTeacherTimetable;
using Kidzgo.Application.Users.GetCurrentUser;
using Kidzgo.Application.Users.GetTeacherOverview;
using Kidzgo.Application.Users.UpdateCurrentUser;
using Kidzgo.Domain.Payroll;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.API.Controllers;

[Route("api/teacher")]
[ApiController]
[Authorize]
public class TeacherController : ControllerBase
{
    private readonly IDbContext _context;
    private readonly ISender _mediator;
    private readonly IUserContext _userContext;

    public TeacherController(ISender mediator, IDbContext context, IUserContext userContext)
    {
        _mediator = mediator;
        _context = context;
        _userContext = userContext;
    }

    [HttpGet("classes")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> GetClasses(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateOnly? teachingDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTeacherClassesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TeachingDate = teachingDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("classes/{classId:guid}/students")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> GetClassStudents(
        Guid classId,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTeacherClassStudentsQuery
        {
            ClassId = classId,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("students")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> GetStudents(
        [FromQuery] Guid? classId,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTeacherStudentsQuery
        {
            ClassId = classId,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("timetable")]
    [Authorize(Roles = "Teacher,Admin,ManagementStaff")]
    public async Task<IResult> GetTimetable(
        [FromQuery] Guid? teacherUserId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? classId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTeacherTimetableQuery
        {
            TeacherUserId = teacherUserId,
            From = from,
            To = to,
            BranchId = branchId,
            ClassId = classId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> GetTeacherDashboard(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTeacherOverviewQuery(), cancellationToken);
        if (!result.IsSuccess)
        {
            return CustomResults.Problem(result);
        }

        return OkData(new
        {
            stats = result.Value.Statistics,
            todayClasses = result.Value.UpcomingSessions
                .Where(s => VietnamTime.ToVietnamDateOnly(s.PlannedDatetime) == VietnamTime.TodayDateOnly())
                .ToList(),
            upcomingClasses = result.Value.UpcomingSessions,
            alerts = result.Value.OpenTickets.Take(5).ToList(),
            recentActivities = result.Value.RecentAttendances,
            pendingTasks = result.Value.PendingHomeworks.Cast<object>()
                .Concat(result.Value.PendingReports.Cast<object>())
                .ToList()
        });
    }

    [HttpGet("profile")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> GetTeacherProfile(CancellationToken cancellationToken)
    {
        var currentUserResult = await _mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        if (!currentUserResult.IsSuccess)
        {
            return CustomResults.Problem(currentUserResult);
        }

        var overviewResult = await _mediator.Send(new GetTeacherOverviewQuery(), cancellationToken);
        if (!overviewResult.IsSuccess)
        {
            return CustomResults.Problem(overviewResult);
        }

        return OkData(new
        {
            basicInfo = new
            {
                currentUserResult.Value.Id,
                currentUserResult.Value.FullName,
                currentUserResult.Value.Email,
                currentUserResult.Value.PhoneNumber,
                currentUserResult.Value.AvatarUrl,
                currentUserResult.Value.BranchId,
                branchName = currentUserResult.Value.Branch?.Name
            },
            bio = (string?)null,
            skills = Array.Empty<string>(),
            certificates = Array.Empty<object>(),
            teachingStats = overviewResult.Value.Statistics
        });
    }

    [HttpPut("profile")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> UpdateTeacherProfile(
        [FromBody] UpdateCurrentUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCurrentUserCommand
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            AvatarUrl = request.AvatarUrl,
            Profiles = request.Profiles?.Select(p => new Kidzgo.Application.Users.UpdateCurrentUser.UpdateProfileDto
            {
                Id = p.Id,
                DisplayName = p.DisplayName
            }).ToList()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("timesheet")]
    [Authorize(Roles = "Teacher,Admin,ManagementStaff")]
    public async Task<IResult> GetTeacherTimesheet(
        [FromQuery] Guid? teacherUserId,
        [FromQuery] int? year,
        CancellationToken cancellationToken)
    {
        var targetTeacherId = User.IsInRole("Teacher")
            ? _userContext.UserId
            : teacherUserId ?? _userContext.UserId;

        var teacherProfile = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == targetTeacherId)
            .Select(u => new
            {
                u.Id,
                u.TeacherCompensationType
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (teacherProfile is null)
        {
            return Results.Problem(title: "Teacher", detail: "Teacher not found", statusCode: StatusCodes.Status404NotFound);
        }

        var compensationSettings = await _context.TeacherCompensationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        var standardSessionDurationMinutes = compensationSettings?.StandardSessionDurationMinutes > 0
            ? compensationSettings.StandardSessionDurationMinutes
            : 90;

        var workHoursQuery = _context.MonthlyWorkHours
            .AsNoTracking()
            .Where(m => m.StaffUserId == targetTeacherId);

        if (year.HasValue)
        {
            workHoursQuery = workHoursQuery.Where(m => m.Year == year.Value);
        }

        var workHours = await workHoursQuery
            .Select(m => new
            {
                m.Year,
                m.Month,
                m.TotalHours,
                m.TeachingSessions,
                m.IsLocked,
                ContractHourlyRate = m.Contract.HourlyRate
            })
            .ToListAsync(cancellationToken);

        var sessionsQuery = _context.Sessions
            .AsNoTracking()
            .Where(s =>
                s.Status != SessionStatus.Cancelled &&
                (((s.ActualTeacherId ?? s.PlannedTeacherId) == targetTeacherId) ||
                 ((s.ActualAssistantId ?? s.PlannedAssistantId) == targetTeacherId)));

        if (year.HasValue)
        {
            var yearStartUtc = VietnamTime.TreatAsVietnamLocal(new DateTime(year.Value, 1, 1, 0, 0, 0));
            var yearEndUtc = VietnamTime.EndOfVietnamDayUtc(
                VietnamTime.TreatAsVietnamLocal(new DateTime(year.Value, 12, 31, 0, 0, 0)));

            sessionsQuery = sessionsQuery.Where(s =>
                (s.ActualDatetime ?? s.PlannedDatetime) >= yearStartUtc &&
                (s.ActualDatetime ?? s.PlannedDatetime) <= yearEndUtc);
        }

        var teacherSessions = await sessionsQuery
            .Select(s => new TeacherTimesheetSessionRow
            {
                Id = s.Id,
                OccurredAt = s.ActualDatetime ?? s.PlannedDatetime,
                DurationMinutes = s.DurationMinutes,
                IsAssistant = (s.ActualAssistantId ?? s.PlannedAssistantId) == targetTeacherId
            })
            .ToListAsync(cancellationToken);

        var sessionIds = teacherSessions.Select(s => s.Id).ToList();

        var sessionRoleOverrides = sessionIds.Count == 0
            ? new List<TeacherSessionRoleOverride>()
            : await _context.SessionRoles
                .AsNoTracking()
                .Where(sr => sr.StaffUserId == targetTeacherId && sessionIds.Contains(sr.SessionId))
                .Select(sr => new TeacherSessionRoleOverride
                {
                    SessionId = sr.SessionId,
                    RoleType = sr.RoleType,
                    PayableUnitPrice = sr.PayableUnitPrice,
                    PayableAllowance = sr.PayableAllowance
                })
                .ToListAsync(cancellationToken);

        var contracts = await _context.Contracts
            .AsNoTracking()
            .Where(c => c.StaffUserId == targetTeacherId)
            .Select(c => new TeacherContractRate
            {
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                HourlyRate = c.HourlyRate,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);

        var workHoursLookup = workHours.ToDictionary(
            x => GetMonthKey(x.Year, x.Month),
            x => x);

        var roleOverridesLookup = sessionRoleOverrides
            .GroupBy(x => x.SessionId)
            .ToDictionary(x => x.Key, x => (IReadOnlyCollection<TeacherSessionRoleOverride>)x.ToList());

        var effectiveTeacherType = teacherProfile.TeacherCompensationType ?? TeacherCompensationType.VietnameseTeacher;

        var sessionSummaries = teacherSessions
            .Select(session =>
            {
                var occurredAtInVietnam = VietnamTime.ToVietnamDateTime(session.OccurredAt);
                roleOverridesLookup.TryGetValue(session.Id, out var sessionOverrides);
                var sessionRoleOverride = SelectMatchingSessionRole(session.IsAssistant, sessionOverrides);

                var defaultSessionRate = ResolveDefaultSessionRate(
                    session.IsAssistant,
                    effectiveTeacherType,
                    compensationSettings);

                var contractHourlyRate = ResolveContractHourlyRate(
                    contracts,
                    VietnamTime.ToVietnamDateOnly(session.OccurredAt));
                var fallbackSessionRate = contractHourlyRate.HasValue
                    ? Math.Round(contractHourlyRate.Value * standardSessionDurationMinutes / 60m, 2)
                    : 0m;

                var effectiveSessionRate = sessionRoleOverride?.PayableUnitPrice
                    ?? (defaultSessionRate > 0 ? defaultSessionRate : fallbackSessionRate);

                var allowance = sessionRoleOverride?.PayableAllowance ?? 0m;
                var proratedIncome = standardSessionDurationMinutes <= 0
                    ? 0m
                    : Math.Round(effectiveSessionRate * session.DurationMinutes / standardSessionDurationMinutes, 2);

                return new
                {
                    session.OccurredAt,
                    OccurredAtInVietnam = occurredAtInVietnam,
                    session.DurationMinutes,
                    SessionRate = effectiveSessionRate,
                    Income = proratedIncome + allowance
                };
            })
            .ToList();

        var sessionMonthLookup = sessionSummaries
            .GroupBy(x => GetMonthKey(x.OccurredAtInVietnam.Year, x.OccurredAtInVietnam.Month))
            .ToDictionary(
                x => x.Key,
                x => new
                {
                    Hours = Math.Round(x.Sum(item => item.DurationMinutes) / 60m, 2),
                    Income = Math.Round(x.Sum(item => item.Income), 2),
                    AverageSessionRate = x.Any() ? Math.Round(x.Average(item => item.SessionRate), 2) : 0m,
                    SessionCount = x.Count()
                });

        var monthKeys = workHoursLookup.Keys
            .Union(sessionMonthLookup.Keys)
            .OrderBy(x => x)
            .ToList();

        var monthlyData = monthKeys
            .Select(monthKey =>
            {
                workHoursLookup.TryGetValue(monthKey, out var workHour);
                sessionMonthLookup.TryGetValue(monthKey, out var sessionMonth);

                return new
                {
                    month = monthKey,
                    hours = workHour?.TotalHours ?? sessionMonth?.Hours ?? 0m,
                    income = sessionMonth?.Income ?? 0m,
                    rate = sessionMonth?.AverageSessionRate
                        ?? (workHour?.ContractHourlyRate.HasValue == true
                            ? Math.Round(workHour.ContractHourlyRate.Value * standardSessionDurationMinutes / 60m, 2)
                            : 0m),
                    classCount = workHour?.TeachingSessions ?? sessionMonth?.SessionCount ?? 0,
                    status = workHour?.IsLocked == true ? "Locked" : "Open"
                };
            })
            .ToList();

        return OkData(new
        {
            teacherCompensationType = effectiveTeacherType.ToString(),
            standardSessionDurationMinutes,
            defaultRates = new
            {
                foreignTeacher = compensationSettings?.ForeignTeacherDefaultSessionRate ?? 0m,
                vietnameseTeacher = compensationSettings?.VietnameseTeacherDefaultSessionRate ?? 0m,
                assistant = compensationSettings?.AssistantDefaultSessionRate ?? 0m
            },
            monthlyData,
            yearlySummary = new
            {
                totalHours = monthlyData.Sum(x => x.hours),
                totalIncome = monthlyData.Sum(x => x.income),
                averagePerMonth = monthlyData.Count == 0 ? 0 : Math.Round(monthlyData.Average(x => x.income), 2),
                totalClasses = monthlyData.Sum(x => x.classCount)
            }
        });
    }

    private static IResult OkData<T>(T data)
    {
        return Results.Ok(ApiResult<T>.Success(data));
    }

    private static string GetMonthKey(int year, int month)
    {
        return $"{year:D4}-{month:D2}";
    }

    private static decimal ResolveDefaultSessionRate(
        bool isAssistant,
        TeacherCompensationType teacherCompensationType,
        TeacherCompensationSettings? compensationSettings)
    {
        if (isAssistant)
        {
            return compensationSettings?.AssistantDefaultSessionRate ?? 0m;
        }

        return teacherCompensationType switch
        {
            TeacherCompensationType.ForeignTeacher => compensationSettings?.ForeignTeacherDefaultSessionRate ?? 0m,
            TeacherCompensationType.Assistant => compensationSettings?.AssistantDefaultSessionRate ?? 0m,
            _ => compensationSettings?.VietnameseTeacherDefaultSessionRate ?? 0m
        };
    }

    private static decimal? ResolveContractHourlyRate(
        IReadOnlyCollection<TeacherContractRate> contracts,
        DateOnly sessionDate)
    {
        return contracts
            .Where(c => c.StartDate <= sessionDate && (!c.EndDate.HasValue || c.EndDate.Value >= sessionDate))
            .OrderByDescending(c => c.IsActive)
            .ThenByDescending(c => c.StartDate)
            .Select(c => c.HourlyRate)
            .FirstOrDefault();
    }

    private static TeacherSessionRoleOverride? SelectMatchingSessionRole(
        bool isAssistant,
        IReadOnlyCollection<TeacherSessionRoleOverride>? sessionOverrides)
    {
        if (sessionOverrides is null || sessionOverrides.Count == 0)
        {
            return null;
        }

        return sessionOverrides.FirstOrDefault(overrideItem =>
                   isAssistant
                       ? overrideItem.RoleType == SessionRoleType.Assistant
                       : overrideItem.RoleType != SessionRoleType.Assistant)
               ?? sessionOverrides.First();
    }

    private sealed class TeacherTimesheetSessionRow
    {
        public Guid Id { get; init; }
        public DateTime OccurredAt { get; init; }
        public int DurationMinutes { get; init; }
        public bool IsAssistant { get; init; }
    }

    private sealed class TeacherSessionRoleOverride
    {
        public Guid SessionId { get; init; }
        public SessionRoleType RoleType { get; init; }
        public decimal? PayableUnitPrice { get; init; }
        public decimal? PayableAllowance { get; init; }
    }

    private sealed class TeacherContractRate
    {
        public DateOnly StartDate { get; init; }
        public DateOnly? EndDate { get; init; }
        public decimal? HourlyRate { get; init; }
        public bool IsActive { get; init; }
    }
}
