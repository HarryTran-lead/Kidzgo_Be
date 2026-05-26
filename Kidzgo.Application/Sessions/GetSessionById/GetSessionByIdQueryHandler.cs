using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Sessions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.GetSessionById;

public sealed class GetSessionByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetSessionByIdQuery, GetSessionByIdResponse>
{
    public async Task<Result<GetSessionByIdResponse>> Handle(GetSessionByIdQuery query, CancellationToken cancellationToken)
    {
        var session = await context.Sessions
            .Include(s => s.Class)
                .ThenInclude(c => c.Branch)
            .Include(s => s.Class)
                .ThenInclude(c => c.Program)
            .Include(s => s.Class)
                .ThenInclude(c => c.ClassEnrollments)
            .Include(s => s.Branch)
            .Include(s => s.Module)
            .Include(s => s.PlannedRoom)
            .Include(s => s.ActualRoom)
            .Include(s => s.PlannedTeacher)
            .Include(s => s.ActualTeacher)
            .Include(s => s.PlannedAssistant)
            .Include(s => s.ActualAssistant)
            .Include(s => s.SlotType)
            .Include(s => s.LessonPlan)
            .Include(s => s.SessionLessons)
            .Include(s => s.LessonPlanTemplate)
            .Include(s => s.TeachingLog)
                .ThenInclude(x => x!.Lessons)
            .Include(s => s.TeachingLog)
                .ThenInclude(x => x!.PlannedLessonPlanTemplate)
            .Include(s => s.TeachingLog)
                .ThenInclude(x => x!.ActualLessonPlanTemplate)
            .Include(s => s.Attendances)
                .ThenInclude(a => a.StudentProfile)
            .FirstOrDefaultAsync(s => s.Id == query.SessionId, cancellationToken);

        if (session == null)
        {
            return Result.Failure<GetSessionByIdResponse>(SessionErrors.NotFound(query.SessionId));
        }

        // Calculate attendance summary
        var totalStudents = session.Class?.ClassEnrollments
            ?.Count(ce => ce.Status == EnrollmentStatus.Active)
            ?? 0;

        var attendances = session.Attendances.ToList();
        var presentCount = attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Present);
        var absentCount = attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Absent);
        var makeupCount = attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Makeup);
        var notMarkedCount = Math.Max(totalStudents - attendances.Count, 0);
        var branchName = session.Branch?.Name ?? session.Class?.Branch?.Name ?? string.Empty;
        var templateBySyllabusModuleAndIndex =
            session.Class?.SyllabusId.HasValue == true &&
            session.Class.SyllabusId.Value != Guid.Empty &&
            session.ModuleId.HasValue &&
            session.ModuleId.Value != Guid.Empty &&
            session.SessionIndexInModule.HasValue
            ? await context.LessonPlanTemplates
                .Where(t =>
                    t.SyllabusId == session.Class.SyllabusId.Value &&
                    t.ModuleId == session.ModuleId.Value &&
                    t.SessionIndex == session.SessionIndexInModule.Value &&
                    t.IsActive &&
                    !t.IsDeleted)
                .ToDictionaryAsync(
                    t => new ValueTuple<Guid, Guid, int>(t.SyllabusId, t.ModuleId, t.SessionIndex),
                    t => t.Id,
                    cancellationToken)
            : new Dictionary<(Guid SyllabusId, Guid ModuleId, int SessionIndex), Guid>();

        var linkageSnapshot = new SessionLessonPlanLinkageSnapshot(
            session.LessonPlanTemplateId,
            session.LessonPlan?.TemplateId,
            session.TeachingLog?.PlannedLessonPlanTemplateId,
            session.TeachingLog?.ActualLessonPlanTemplateId,
            session.SessionLessons
                .OrderBy(x => x.OrderIndex)
                .Select(x => x.LessonPlanTemplateId)
                .FirstOrDefault(),
            session.Class?.SyllabusId,
            session.ModuleId,
            session.SessionIndexInModule);

        var templateIds = SessionLessonPlanLinkageResolver.GetCandidateTemplateIds(
            new[] { linkageSnapshot },
            templateBySyllabusModuleAndIndex);

        var titleByTemplateId = templateIds.Count == 0
            ? new Dictionary<Guid, string?>()
            : await context.LessonPlanTemplates
                .Where(t => templateIds.Contains(t.Id) && !t.IsDeleted)
                .Select(t => new { t.Id, t.Title })
                .ToDictionaryAsync(t => t.Id, t => t.Title, cancellationToken);

        var resolvedLinkage = SessionLessonPlanLinkageResolver.Resolve(
            linkageSnapshot,
            templateBySyllabusModuleAndIndex,
            titleByTemplateId);

        var sessionDto = new SessionDetailDto
        {
            Id = session.Id,
            Color = session.Color,
            ClassId = session.ClassId,
            ModuleId = session.ModuleId,
            ModuleName = session.Module?.Name,
            LessonPlanTemplateId = resolvedLinkage.LessonPlanTemplateId,
            PlannedLessonPlanTemplateId = resolvedLinkage.PlannedLessonPlanTemplateId,
            SessionIndexInModule = session.SessionIndexInModule,
            ClassCode = session.Class?.Code ?? string.Empty,
            ClassTitle = session.Class?.Title ?? string.Empty,
            BranchId = session.BranchId,
            BranchName = branchName,
            PlannedDatetime = Kidzgo.Domain.Time.VietnamTime.ToVietnamDateTime(session.PlannedDatetime),
            ActualDatetime = session.ActualDatetime.HasValue
                ? Kidzgo.Domain.Time.VietnamTime.ToVietnamDateTime(session.ActualDatetime.Value)
                : null,
            DurationMinutes = session.DurationMinutes,
            ParticipationType = session.ParticipationType.ToString(),
            SectionType = session.SectionType.ToString(),
            SlotTypeId = session.SlotTypeId,
            SlotTypeCode = session.SlotType?.Code,
            Status = session.Status.ToString(),
            PlannedRoomId = session.PlannedRoomId,
            PlannedRoomName = session.PlannedRoom != null ? session.PlannedRoom.Name : null,
            ActualRoomId = session.ActualRoomId,
            ActualRoomName = session.ActualRoom != null ? session.ActualRoom.Name : null,
            PlannedTeacherId = session.PlannedTeacherId,
            PlannedTeacherName = session.PlannedTeacher != null ? session.PlannedTeacher.Name : null,
            ActualTeacherId = session.ActualTeacherId,
            ActualTeacherName = session.ActualTeacher != null ? session.ActualTeacher.Name : null,
            PlannedAssistantId = session.PlannedAssistantId,
            PlannedAssistantName = session.PlannedAssistant != null ? session.PlannedAssistant.Name : null,
            ActualAssistantId = session.ActualAssistantId,
            ActualAssistantName = session.ActualAssistant != null ? session.ActualAssistant.Name : null,
            LessonPlanId = session.LessonPlan != null ? session.LessonPlan.Id : null,
            LessonPlanLink = session.LessonPlan != null ? $"/api/lesson-plans/{session.LessonPlan.Id}" : null,
            PlannedLessonTitle = resolvedLinkage.PlannedLessonTitle,
            ActualLessonPlanTemplateId = resolvedLinkage.ActualLessonPlanTemplateId,
            ActualLessonTitle = resolvedLinkage.ActualLessonTitle,
            TeachingLogId = session.TeachingLog?.Id,
            TeachingLogStatus = session.TeachingLog?.Status.ToString(),
            TeachingProgressStatus = session.TeachingLog?.Lessons
                .OrderBy(x => x.OrderIndex)
                .Select(x => x.ProgressStatus.ToString())
                .FirstOrDefault(),
            ActualTeachingType = session.TeachingLog?.ActualTeachingType.ToString(),
            ActualContent = session.TeachingLog?.ActualContent,
            ActualHomework = session.TeachingLog?.ActualHomework,
            TeacherNote = session.TeachingLog?.TeacherNote,
            AttendanceSummary = new AttendanceSummaryDto
            {
                TotalStudents = totalStudents,
                PresentCount = presentCount,
                AbsentCount = absentCount,
                MakeupCount = makeupCount,
                NotMarkedCount = notMarkedCount
            }
        };

        return Result.Success(new GetSessionByIdResponse
        {
            Session = sessionDto
        });
    }
}

