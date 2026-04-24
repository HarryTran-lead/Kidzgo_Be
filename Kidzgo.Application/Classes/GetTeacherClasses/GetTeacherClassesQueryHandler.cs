using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.GetTeacherClasses;

public sealed class GetTeacherClassesQueryHandler(
    IDbContext context,
    IUserContext userContext,
    ISchedulePatternParser schedulePatternParser
) : IQueryHandler<GetTeacherClassesQuery, GetTeacherClassesResponse>
{
    public async Task<Result<GetTeacherClassesResponse>> Handle(GetTeacherClassesQuery query, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        // Get classes where teacher is MainTeacher or AssistantTeacher
        var classesQuery = context.Classes
            .Include(c => c.Branch)
            .Include(c => c.Program)
            .Include(c => c.MainTeacher)
            .Include(c => c.AssistantTeacher)
            .Where(c => c.MainTeacherId == userId || c.AssistantTeacherId == userId);

        if (query.TeachingDate.HasValue)
        {
            var dayStartUtc = VietnamTime.TreatAsVietnamLocal(query.TeachingDate.Value.ToDateTime(TimeOnly.MinValue));
            var dayEndUtc = VietnamTime.EndOfVietnamDayUtc(dayStartUtc);

            classesQuery = classesQuery.Where(c => context.Sessions.Any(s =>
                s.ClassId == c.Id &&
                s.PlannedDatetime >= dayStartUtc &&
                s.PlannedDatetime <= dayEndUtc &&
                (s.PlannedTeacherId == userId ||
                 s.ActualTeacherId == userId ||
                 s.PlannedAssistantId == userId ||
                 s.ActualAssistantId == userId)));
        }

        // Get total count
        int totalCount = await classesQuery.CountAsync(cancellationToken);

        // Apply pagination
        var classes = await classesQuery
            .OrderByDescending(c => c.CreatedAt)
            .ThenBy(c => c.Title)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(c => new TeacherClassDto
            {
                Id = c.Id,
                BranchId = c.BranchId,
                BranchName = c.Branch.Name,
                ProgramId = c.ProgramId,
                ProgramName = c.Program.Name,
                Code = c.Code,
                Title = c.Title,
                MainTeacherId = c.MainTeacherId,
                MainTeacherName = c.MainTeacher != null ? c.MainTeacher.Name : null,
                AssistantTeacherId = c.AssistantTeacherId,
                AssistantTeacherName = c.AssistantTeacher != null ? c.AssistantTeacher.Name : null,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status.ToString(),
                Capacity = c.Capacity,
                CurrentEnrollmentCount = c.ClassEnrollments.Count(ce => ce.Status == EnrollmentStatus.Active),
                Role = c.MainTeacherId == userId ? "MainTeacher" : "AssistantTeacher"
            })
            .ToListAsync(cancellationToken);

        var classWeeklySchedules = await classesQuery
            .OrderByDescending(c => c.CreatedAt)
            .ThenBy(c => c.Title)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(c => new { c.Id, c.WeeklyScheduleJson })
            .ToListAsync(cancellationToken);

        var weeklyScheduleByClassId = classWeeklySchedules.ToDictionary(item => item.Id, item => item.WeeklyScheduleJson);

        foreach (var classDto in classes)
        {
            if (!weeklyScheduleByClassId.TryGetValue(classDto.Id, out var weeklyScheduleJson) ||
                string.IsNullOrWhiteSpace(weeklyScheduleJson))
            {
                continue;
            }

            var parseResult = schedulePatternParser.ParseScheduleSlots(weeklyScheduleJson);
            if (parseResult.IsSuccess)
            {
                classDto.WeeklyScheduleSlots.AddRange(parseResult.Value);
            }
        }

        var page = new Page<TeacherClassDto>(
            classes,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return Result.Success(new GetTeacherClassesResponse
        {
            Classes = page
        });
    }
}

