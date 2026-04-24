using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.GetClasses;

public sealed class GetClassesQueryHandler(
    IDbContext context,
    IUserContext userContext,
    ISchedulePatternParser schedulePatternParser
) : IQueryHandler<GetClassesQuery, GetClassesResponse>
{
    public async Task<Result<GetClassesResponse>> Handle(GetClassesQuery query, CancellationToken cancellationToken)
    {
        var classesQuery = context.Classes
            .Include(c => c.Branch)
            .Include(c => c.Program)
            .Include(c => c.Room)
            .Include(c => c.MainTeacher)
            .Include(c => c.AssistantTeacher)
            .AsQueryable();

        if (userContext.ParentId.HasValue)
        {
            classesQuery = classesQuery.Where(c => c.Program.IsMakeup);
        }

        // Filter by branch
        if (query.BranchId.HasValue)
        {
            classesQuery = classesQuery.Where(c => c.BranchId == query.BranchId.Value);
        }

        // Filter by program
        if (query.ProgramId.HasValue)
        {
            classesQuery = classesQuery.Where(c => c.ProgramId == query.ProgramId.Value);
        }

        if (query.TeacherId.HasValue)
        {
            classesQuery = classesQuery.Where(c =>
                c.MainTeacherId == query.TeacherId.Value ||
                c.AssistantTeacherId == query.TeacherId.Value);
        }

        // Filter by status
        if (query.Status.HasValue)
        {
            classesQuery = classesQuery.Where(c => c.Status == query.Status.Value);
        }

        // Filter by student (chỉ những lớp mà học sinh đang ghi danh active)
        if (query.StudentId.HasValue)
        {
            var studentId = query.StudentId.Value;
            classesQuery = classesQuery.Where(c =>
                c.ClassEnrollments.Any(ce =>
                    ce.StudentProfileId == studentId &&
                    ce.Status == Domain.Classes.EnrollmentStatus.Active));
        }

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            classesQuery = classesQuery.Where(c =>
                c.Code.Contains(query.SearchTerm) ||
                c.Title.Contains(query.SearchTerm));
        }

        // Get total count
        int totalCount = await classesQuery.CountAsync(cancellationToken);

        // Apply pagination
        // Sắp xếp theo CreatedAt descending để Class mới tạo nằm đầu danh sách
        var classes = await classesQuery
            .OrderByDescending(c => c.CreatedAt)
            .ThenBy(c => c.Code)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(c => new ClassDto
            {
                Id = c.Id,
                BranchId = c.BranchId,
                BranchName = c.Branch.Name,
                ProgramId = c.ProgramId,
                ProgramName = c.Program.Name,
                Code = c.Code,
                Title = c.Title,
                RoomId = c.RoomId,
                RoomName = c.Room != null ? c.Room.Name : null,
                MainTeacherId = c.MainTeacherId,
                MainTeacherName = c.MainTeacher != null ? c.MainTeacher.Name : null,
                AssistantTeacherId = c.AssistantTeacherId,
                AssistantTeacherName = c.AssistantTeacher != null ? c.AssistantTeacher.Name : null,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status.ToString(),
                Capacity = c.Capacity,
                CurrentEnrollmentCount = c.ClassEnrollments.Count(ce => ce.Status == Domain.Classes.EnrollmentStatus.Active),
                Description = c.Description,
                TotalSessions = c.Sessions.Count(),
                CompletedSessions = c.Sessions.Count(s => s.Status == Domain.Sessions.SessionStatus.Completed)
            })
            .ToListAsync(cancellationToken);

        var classWeeklySchedules = await classesQuery
            .OrderByDescending(c => c.CreatedAt)
            .ThenBy(c => c.Code)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(c => new { c.Id, c.WeeklyScheduleJson })
            .ToListAsync(cancellationToken);

        var weeklyScheduleByClassId = classWeeklySchedules.ToDictionary(item => item.Id, item => item.WeeklyScheduleJson);
        var pagedClassIds = classWeeklySchedules.Select(item => item.Id).ToList();
        var scheduleSegments = await context.ClassScheduleSegments
            .AsNoTracking()
            .Where(segment => pagedClassIds.Contains(segment.ClassId))
            .OrderBy(segment => segment.EffectiveFrom)
            .ToListAsync(cancellationToken);
        var scheduleSegmentsByClassId = scheduleSegments
            .GroupBy(segment => segment.ClassId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(segment => new WeeklyScheduleSegmentWindow(
                        segment.EffectiveFrom,
                        segment.EffectiveTo,
                        segment.WeeklyScheduleJson))
                    .ToList());
        var today = VietnamTime.TodayDateOnly();

        foreach (var classDto in classes)
        {
            if (!weeklyScheduleByClassId.TryGetValue(classDto.Id, out var weeklyScheduleJson) ||
                (string.IsNullOrWhiteSpace(weeklyScheduleJson) &&
                 !scheduleSegmentsByClassId.ContainsKey(classDto.Id)))
            {
                continue;
            }

            scheduleSegmentsByClassId.TryGetValue(classDto.Id, out var segmentWindows);
            var effectiveWeeklyScheduleJson = SchedulePatternSupport.ResolveEffectiveWeeklyScheduleJson(
                weeklyScheduleJson,
                segmentWindows ?? [],
                today);
            if (string.IsNullOrWhiteSpace(effectiveWeeklyScheduleJson))
            {
                continue;
            }

            var parseResult = schedulePatternParser.ParseScheduleSlots(effectiveWeeklyScheduleJson);
            if (parseResult.IsSuccess)
            {
                classDto.WeeklyScheduleSlots.AddRange(parseResult.Value);
            }
        }

        var page = new Page<ClassDto>(
            classes,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetClassesResponse
        {
            Classes = page
        };
    }
}

