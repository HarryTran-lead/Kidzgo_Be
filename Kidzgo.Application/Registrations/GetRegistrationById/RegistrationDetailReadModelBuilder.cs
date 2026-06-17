using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations.Errors;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.GetRegistrationById;

internal static class RegistrationDetailReadModelBuilder
{
    internal static async Task<Result<GetRegistrationByIdResponse>> BuildAsync(
        IDbContext context,
        Guid registrationId,
        CancellationToken cancellationToken)
    {
        var registration = await context.Registrations
            .AsNoTracking()
            .Include(r => r.StudentProfile)
            .Include(r => r.Branch)
            .Include(r => r.Program)
            .Include(r => r.Level)
            .Include(r => r.SecondaryLevel)
            .Include(r => r.TuitionPlan)
            .Include(r => r.Class)
            .Include(r => r.SecondaryClass)
            .FirstOrDefaultAsync(r => r.Id == registrationId, cancellationToken);

        if (registration == null)
        {
            return Result.Failure<GetRegistrationByIdResponse>(RegistrationErrors.NotFound(registrationId));
        }

        var actualStudyEnrollments = await context.ClassEnrollments
            .AsNoTracking()
            .Include(e => e.Class)
                .ThenInclude(c => c.Program)
            .Include(e => e.ScheduleSegments)
            .Where(e => e.RegistrationId == registration.Id)
            .ToListAsync(cancellationToken);

        var studentBranchState = await context.StudentBranchStates
            .AsNoTracking()
            .Include(x => x.HomeBranch)
            .Include(x => x.ActiveBranch)
            .FirstOrDefaultAsync(x => x.StudentProfileId == registration.StudentProfileId, cancellationToken);

        var firstStudySessionRow = await context.StudentSessionAssignments
            .AsNoTracking()
            .Where(a => a.RegistrationId == registration.Id &&
                        a.Status == StudentSessionAssignmentStatus.Assigned)
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
                PlannedDatetime = VietnamTime.ToVietnamDateTime(firstStudySessionRow.PlannedDatetime),
                StudyDate = VietnamTime.ToVietnamDateOnly(firstStudySessionRow.PlannedDatetime)
            };

        return Result.Success(new GetRegistrationByIdResponse
        {
            Id = registration.Id,
            StudentProfileId = registration.StudentProfileId,
            StudentName = registration.StudentProfile.DisplayName,
            StudentHomeBranchId = studentBranchState?.HomeBranchId,
            StudentHomeBranchName = studentBranchState?.HomeBranch.Name,
            StudentActiveBranchId = studentBranchState?.ActiveBranchId,
            StudentActiveBranchName = studentBranchState?.ActiveBranch.Name,
            IsCrossBranchRegistration = studentBranchState is not null && studentBranchState.ActiveBranchId != registration.BranchId,
            BranchId = registration.BranchId,
            BranchName = registration.Branch.Name,
            ProgramId = registration.ProgramId,
            ProgramName = registration.Program.Name,
            LevelId = registration.LevelId,
            LevelName = registration.Level.Name,
            SecondaryLevelId = registration.SecondaryLevelId,
            SecondaryLevelName = registration.SecondaryLevel?.Name,
            SecondaryLevelSkillFocus = registration.SecondaryProgramSkillFocus,
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
            DiscountCampaignId = registration.DiscountCampaignId,
            DiscountCampaignName = registration.DiscountCampaignName,
            DiscountType = registration.DiscountType?.ToString(),
            DiscountValue = registration.DiscountValue,
            OriginalTuitionAmount = registration.OriginalTuitionAmount ?? registration.TuitionPlan.TuitionAmount,
            DiscountAmount = registration.DiscountAmount ?? 0m,
            CarryOverCreditAmount = registration.CarryOverCreditAmount ?? 0m,
            FinalTuitionAmount = registration.FinalTuitionAmount ?? ((registration.OriginalTuitionAmount ?? registration.TuitionPlan.TuitionAmount) - (registration.DiscountAmount ?? 0m) - (registration.CarryOverCreditAmount ?? 0m)),
            FirstStudySession = firstStudySession,
            ActualStudySchedules = RegistrationActualStudyScheduleMapper.Map(actualStudyEnrollments),
            CreatedAt = registration.CreatedAt,
            UpdatedAt = registration.UpdatedAt
        });
    }
}
