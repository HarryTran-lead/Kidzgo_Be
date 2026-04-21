using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations.Errors;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.GetRegistrationById;

public sealed class GetRegistrationByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetRegistrationByIdQuery, GetRegistrationByIdResponse>
{
    public async Task<Result<GetRegistrationByIdResponse>> Handle(
        GetRegistrationByIdQuery query,
        CancellationToken cancellationToken)
    {
        var registration = await context.Registrations
            .Include(r => r.StudentProfile)
            .Include(r => r.Branch)
            .Include(r => r.Program)
            .Include(r => r.SecondaryProgram)
            .Include(r => r.TuitionPlan)
            .Include(r => r.Class)
            .Include(r => r.SecondaryClass)
            .FirstOrDefaultAsync(r => r.Id == query.Id, cancellationToken);

        if (registration == null)
        {
            return Result.Failure<GetRegistrationByIdResponse>(RegistrationErrors.NotFound(query.Id));
        }

        var actualStudyEnrollments = await context.ClassEnrollments
            .AsNoTracking()
            .Include(e => e.Class)
                .ThenInclude(c => c.Program)
            .Include(e => e.ScheduleSegments)
            .Where(e => e.RegistrationId == registration.Id && e.Status == EnrollmentStatus.Active)
            .ToListAsync(cancellationToken);

        var firstStudySessionRow = await context.StudentSessionAssignments
            .AsNoTracking()
            .Where(a => a.RegistrationId == registration.Id &&
                        a.Status == StudentSessionAssignmentStatus.Assigned &&
                        a.ClassEnrollment.Status == EnrollmentStatus.Active)
            .OrderBy(a => a.Session.PlannedDatetime)
            .Select(a => new
            {
                a.SessionId,
                a.ClassEnrollmentId,
                a.Track,
                a.Session.ClassId,
                ClassName = a.Session.Class.Title,
                a.Session.PlannedDatetime
            })
            .FirstOrDefaultAsync(cancellationToken);

        var firstStudySession = firstStudySessionRow is null
            ? null
            : new RegistrationFirstStudySessionDto
            {
                SessionId = firstStudySessionRow.SessionId,
                ClassEnrollmentId = firstStudySessionRow.ClassEnrollmentId,
                Track = RegistrationTrackHelper.ToTrackName(firstStudySessionRow.Track),
                ClassId = firstStudySessionRow.ClassId,
                ClassName = firstStudySessionRow.ClassName,
                PlannedDatetime = firstStudySessionRow.PlannedDatetime,
                StudyDate = VietnamTime.ToVietnamDateOnly(firstStudySessionRow.PlannedDatetime)
            };

        return new GetRegistrationByIdResponse
        {
            Id = registration.Id,
            StudentProfileId = registration.StudentProfileId,
            StudentName = registration.StudentProfile.DisplayName,
            BranchId = registration.BranchId,
            BranchName = registration.Branch.Name,
            ProgramId = registration.ProgramId,
            ProgramName = registration.Program.Name,
            SecondaryProgramId = registration.SecondaryProgramId,
            SecondaryProgramName = registration.SecondaryProgram?.Name,
            SecondaryProgramSkillFocus = registration.SecondaryProgramSkillFocus,
            TuitionPlanId = registration.TuitionPlanId,
            TuitionPlanName = registration.TuitionPlan.Name,
            RegistrationDate = registration.RegistrationDate,
            ExpectedStartDate = registration.ExpectedStartDate,
            ActualStartDate = registration.ActualStartDate,
            PreferredSchedule = registration.PreferredSchedule,
            Note = registration.Note,
            Status = registration.Status.ToString(),
            ClassId = registration.ClassId,
            ClassName = registration.Class?.Title,
            EntryType = RegistrationTrackHelper.ToApiEntryType(registration.EntryType),
            SecondaryClassId = registration.SecondaryClassId,
            SecondaryClassName = registration.SecondaryClass?.Title,
            SecondaryEntryType = RegistrationTrackHelper.ToApiEntryType(registration.SecondaryEntryType),
            TotalSessions = registration.TotalSessions,
            UsedSessions = registration.UsedSessions,
            RemainingSessions = registration.RemainingSessions,
            OriginalRegistrationId = registration.OriginalRegistrationId,
            OperationType = registration.OperationType?.ToString(),
            FirstStudySession = firstStudySession,
            ActualStudySchedules = RegistrationActualStudyScheduleMapper.Map(actualStudyEnrollments),
            CreatedAt = registration.CreatedAt,
            UpdatedAt = registration.UpdatedAt
        };
    }
}
