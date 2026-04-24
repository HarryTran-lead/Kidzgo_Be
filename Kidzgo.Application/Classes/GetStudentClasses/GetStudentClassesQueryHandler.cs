using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.GetStudentClasses;

public sealed class GetStudentClassesQueryHandler(
    IDbContext context,
    IUserContext userContext,
    ISchedulePatternParser schedulePatternParser
) : IQueryHandler<GetStudentClassesQuery, GetStudentClassesResponse>
{
    public async Task<Result<GetStudentClassesResponse>> Handle(GetStudentClassesQuery query, CancellationToken cancellationToken)
    {
        // Get StudentId from context (token)
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<GetStudentClassesResponse>(ProfileErrors.StudentNotFound);
        }

        // Verify student exists and is active
        var profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == studentId.Value, cancellationToken);

        if (profile == null)
        {
            return Result.Failure<GetStudentClassesResponse>(ProfileErrors.NotFound(studentId.Value));
        }

        if (profile.ProfileType != Domain.Users.ProfileType.Student)
        {
            return Result.Failure<GetStudentClassesResponse>(ProfileErrors.StudentNotFound);
        }

        if (!profile.IsActive || profile.IsDeleted)
        {
            return Result.Failure<GetStudentClassesResponse>(ProfileErrors.StudentNotFound);
        }

        // Verify the student belongs to the current user
        if (profile.UserId != userContext.UserId)
        {
            return Result.Failure<GetStudentClassesResponse>(ProfileErrors.StudentNotFound);
        }

        // Get enrollments where student is enrolled (Status = Active) and Class exists
        var enrollmentsQuery = context.ClassEnrollments
            .Where(ce => ce.StudentProfileId == studentId.Value && ce.Status == EnrollmentStatus.Active)
            .Where(ce => ce.Class != null);

        // Get total count
        int totalCount = await enrollmentsQuery.CountAsync(cancellationToken);

        // Load enrollments with all related data, order and paginate
        var enrollments = await enrollmentsQuery
            .Include(ce => ce.Class!)
                .ThenInclude(c => c.Branch)
            .Include(ce => ce.Class!)
                .ThenInclude(c => c.Program)
            .Include(ce => ce.Class!)
                .ThenInclude(c => c.MainTeacher)
            .Include(ce => ce.Class!)
                .ThenInclude(c => c.AssistantTeacher)
            .Include(ce => ce.Class!)
                .ThenInclude(c => c.ClassEnrollments)
            .Include(ce => ce.Class!)
                .ThenInclude(c => c.Sessions)
            .OrderByDescending(ce => ce.Class!.CreatedAt)
            .ThenBy(ce => ce.Class!.Title)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync(cancellationToken);

        var classDtos = enrollments.Select(ce => new StudentClassDto
        {
            Id = ce.Class.Id,
            BranchId = ce.Class.BranchId,
            BranchName = ce.Class.Branch.Name,
            ProgramId = ce.Class.ProgramId,
            ProgramName = ce.Class.Program != null ? ce.Class.Program.Name : null,
            Code = ce.Class.Code,
            Title = ce.Class.Title,
            MainTeacherId = ce.Class.MainTeacherId,
            MainTeacherName = ce.Class.MainTeacher != null ? ce.Class.MainTeacher.Name : null,
            AssistantTeacherId = ce.Class.AssistantTeacherId,
            AssistantTeacherName = ce.Class.AssistantTeacher != null ? ce.Class.AssistantTeacher.Name : null,
            StartDate = ce.Class.StartDate,
            EndDate = ce.Class.EndDate,
            Status = ce.Class.Status.ToString(),
            Capacity = ce.Class.Capacity,
            CurrentEnrollmentCount = ce.Class.ClassEnrollments.Count(e => e.Status == EnrollmentStatus.Active),
            WeeklyScheduleSlots = ce.Class.WeeklyScheduleJson is null
                ? []
                : ParseSlots(ce.Class.WeeklyScheduleJson),
            TotalSessions = ce.Class.Sessions.Count,
            CompletedSessions = ce.Class.Sessions.Count(s => s.Status == Domain.Sessions.SessionStatus.Completed),
            EnrollDate = ce.EnrollDate,
            EnrollmentStatus = ce.Status.ToString()
        }).ToList();

        var page = new Page<StudentClassDto>(
            classDtos,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return Result.Success(new GetStudentClassesResponse
        {
            Classes = page
        });
    }

    private List<ScheduleSlot> ParseSlots(string weeklyScheduleJson)
    {
        var parseResult = schedulePatternParser.ParseScheduleSlots(weeklyScheduleJson);
        return parseResult.IsSuccess ? parseResult.Value : [];
    }
}

