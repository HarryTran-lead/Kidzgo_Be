using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Sessions.Shared;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.GetSessions;

public sealed class GetSessionsQueryHandler(
    IDbContext context
) : IQueryHandler<GetSessionsQuery, GetSessionsResponse>
{
    public async Task<Result<GetSessionsResponse>> Handle(GetSessionsQuery query, CancellationToken cancellationToken)
    {
        var sessionsQuery = context.Sessions
            .Include(s => s.Class)
                .ThenInclude(c => c.Branch)
            .Include(s => s.TeachingLog)
                .ThenInclude(t => t!.Lessons)
            .Include(s => s.PlannedRoom)
            .Include(s => s.ActualRoom)
            .Include(s => s.PlannedTeacher)
            .Include(s => s.ActualTeacher)
            .Include(s => s.PlannedAssistant)
            .Include(s => s.ActualAssistant)
            .Include(s => s.SlotType)
            .AsQueryable();

        if (query.ClassId.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.ClassId == query.ClassId.Value);
        }

        if (query.BranchId.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.BranchId == query.BranchId.Value);
        }

        if (query.Status.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.Status == query.Status.Value);
        }

        if (query.From.HasValue)
        {
            var fromUtc = VietnamTime.NormalizeToUtc(query.From.Value);
            sessionsQuery = sessionsQuery.Where(s => s.PlannedDatetime >= fromUtc);
        }

        if (query.To.HasValue)
        {
            var toUtc = VietnamTime.EndOfVietnamDayUtc(VietnamTime.NormalizeToUtc(query.To.Value));
            sessionsQuery = sessionsQuery.Where(s => s.PlannedDatetime <= toUtc);
        }

        var totalCount = await sessionsQuery.CountAsync(cancellationToken);

        var sessionRows = await sessionsQuery
            .OrderBy(s => s.PlannedDatetime)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(s => new
            {
                Id = s.Id,
                Color = s.Color,
                ClassId = s.ClassId,
                SyllabusId = s.Class.SyllabusId,
                ModuleId = s.ModuleId,
                ModuleName = s.Module != null ? s.Module.Name : null,
                SessionTemplateId = s.LessonPlanTemplateId,
                LessonPlanTemplateId = s.LessonPlan != null ? s.LessonPlan.TemplateId : null,
                PlannedLessonPlanTemplateId = s.TeachingLog != null ? s.TeachingLog.PlannedLessonPlanTemplateId : null,
                ActualLessonPlanTemplateId = s.TeachingLog != null ? s.TeachingLog.ActualLessonPlanTemplateId : null,
                SessionLessonTemplateId = s.SessionLessons
                    .OrderBy(l => l.OrderIndex)
                    .Select(l => l.LessonPlanTemplateId)
                    .FirstOrDefault(),
                SessionIndexInModule = s.SessionIndexInModule,
                ClassCode = s.Class.Code,
                ClassTitle = s.Class.Title,
                BranchId = s.BranchId,
                BranchName = s.Branch.Name,
                PlannedDatetime = s.PlannedDatetime,
                ActualDatetime = s.ActualDatetime,
                DurationMinutes = s.DurationMinutes,
                ParticipationType = s.ParticipationType.ToString(),
                SectionType = s.SectionType.ToString(),
                SlotTypeId = s.SlotTypeId,
                SlotTypeCode = s.SlotType != null ? s.SlotType.Code : null,
                Status = s.Status.ToString(),
                PlannedRoomId = s.PlannedRoomId,
                PlannedRoomName = s.PlannedRoom != null ? s.PlannedRoom.Name : null,
                ActualRoomId = s.ActualRoomId,
                ActualRoomName = s.ActualRoom != null ? s.ActualRoom.Name : null,
                PlannedTeacherId = s.PlannedTeacherId,
                PlannedTeacherName = s.PlannedTeacher != null ? s.PlannedTeacher.Name : null,
                ActualTeacherId = s.ActualTeacherId,
                ActualTeacherName = s.ActualTeacher != null ? s.ActualTeacher.Name : null,
                PlannedAssistantId = s.PlannedAssistantId,
                PlannedAssistantName = s.PlannedAssistant != null ? s.PlannedAssistant.Name : null,
                ActualAssistantId = s.ActualAssistantId,
                ActualAssistantName = s.ActualAssistant != null ? s.ActualAssistant.Name : null,
                PlannedLessonTitle = s.TeachingLog != null && s.TeachingLog.PlannedLessonPlanTemplate != null
                    ? s.TeachingLog.PlannedLessonPlanTemplate.Title
                    : null,
                ActualLessonTitle = s.TeachingLog != null && s.TeachingLog.ActualLessonPlanTemplate != null
                    ? s.TeachingLog.ActualLessonPlanTemplate.Title
                    : null,
                TeachingLogId = s.TeachingLog != null ? (Guid?)s.TeachingLog.Id : null,
                TeachingLogStatus = s.TeachingLog != null ? s.TeachingLog.Status.ToString() : null,
                TeachingProgressStatus = s.TeachingLog != null
                    ? s.TeachingLog.Lessons
                        .OrderBy(l => l.OrderIndex)
                        .Select(l => l.ProgressStatus.ToString())
                        .FirstOrDefault()
                    : null,
                ActualTeachingType = s.TeachingLog != null
                    ? s.TeachingLog.ActualTeachingType.ToString()
                    : null
            })
            .ToListAsync(cancellationToken);

        var moduleIds = sessionRows
            .Where(s =>
                s.SyllabusId.HasValue &&
                s.SyllabusId.Value != Guid.Empty &&
                s.ModuleId.HasValue &&
                s.ModuleId.Value != Guid.Empty &&
                s.SessionIndexInModule.HasValue)
            .Select(s => s.ModuleId!.Value)
            .Distinct()
            .ToList();

        var syllabusIds = sessionRows
            .Where(s =>
                s.SyllabusId.HasValue &&
                s.SyllabusId.Value != Guid.Empty &&
                s.ModuleId.HasValue &&
                s.ModuleId.Value != Guid.Empty &&
                s.SessionIndexInModule.HasValue)
            .Select(s => s.SyllabusId!.Value)
            .Distinct()
            .ToList();

        var templateBySyllabusModuleAndIndex = moduleIds.Count == 0 || syllabusIds.Count == 0
            ? new Dictionary<(Guid SyllabusId, Guid ModuleId, int SessionIndex), Guid>()
            : await context.LessonPlanTemplates
                .Where(t => syllabusIds.Contains(t.SyllabusId) && moduleIds.Contains(t.ModuleId) && t.IsActive && !t.IsDeleted)
                .Select(t => new { t.Id, t.SyllabusId, t.ModuleId, t.SessionIndex })
                .ToDictionaryAsync(
                    t => new ValueTuple<Guid, Guid, int>(t.SyllabusId, t.ModuleId, t.SessionIndex),
                    t => t.Id,
                    cancellationToken);

        var linkageSnapshots = sessionRows
            .Select(s => new SessionLessonPlanLinkageSnapshot(
                s.SessionTemplateId,
                s.LessonPlanTemplateId,
                s.PlannedLessonPlanTemplateId,
                s.ActualLessonPlanTemplateId,
                s.SessionLessonTemplateId,
                s.SyllabusId,
                s.ModuleId,
                s.SessionIndexInModule))
            .ToList();

        var templateIds = SessionLessonPlanLinkageResolver.GetCandidateTemplateIds(
            linkageSnapshots,
            templateBySyllabusModuleAndIndex);

        var titleByTemplateId = templateIds.Count == 0
            ? new Dictionary<Guid, string?>()
            : await context.LessonPlanTemplates
                .Where(t => templateIds.Contains(t.Id) && !t.IsDeleted)
                .Select(t => new { t.Id, t.Title })
                .ToDictionaryAsync(t => t.Id, t => t.Title, cancellationToken);

        var items = sessionRows
            .Select(s =>
            {
                var resolvedLinkage = SessionLessonPlanLinkageResolver.Resolve(
                    new SessionLessonPlanLinkageSnapshot(
                        s.SessionTemplateId,
                        s.LessonPlanTemplateId,
                        s.PlannedLessonPlanTemplateId,
                        s.ActualLessonPlanTemplateId,
                        s.SessionLessonTemplateId,
                        s.SyllabusId,
                        s.ModuleId,
                        s.SessionIndexInModule),
                    templateBySyllabusModuleAndIndex,
                    titleByTemplateId);

                return new SessionListItemDto
                {
                Id = s.Id,
                Color = s.Color,
                ClassId = s.ClassId,
                ModuleId = s.ModuleId,
                ModuleName = s.ModuleName,
                LessonPlanTemplateId = resolvedLinkage.LessonPlanTemplateId,
                PlannedLessonPlanTemplateId = resolvedLinkage.PlannedLessonPlanTemplateId,
                SessionIndexInModule = s.SessionIndexInModule,
                ClassCode = s.ClassCode,
                ClassTitle = s.ClassTitle,
                BranchId = s.BranchId,
                BranchName = s.BranchName,
                PlannedDatetime = VietnamTime.ToVietnamDateTime(s.PlannedDatetime),
                ActualDatetime = s.ActualDatetime.HasValue
                    ? VietnamTime.ToVietnamDateTime(s.ActualDatetime.Value)
                    : null,
                DurationMinutes = s.DurationMinutes,
                ParticipationType = s.ParticipationType,
                SectionType = s.SectionType,
                SlotTypeId = s.SlotTypeId,
                SlotTypeCode = s.SlotTypeCode,
                Status = s.Status,
                PlannedRoomId = s.PlannedRoomId,
                PlannedRoomName = s.PlannedRoomName,
                ActualRoomId = s.ActualRoomId,
                ActualRoomName = s.ActualRoomName,
                PlannedTeacherId = s.PlannedTeacherId,
                PlannedTeacherName = s.PlannedTeacherName,
                ActualTeacherId = s.ActualTeacherId,
                ActualTeacherName = s.ActualTeacherName,
                PlannedAssistantId = s.PlannedAssistantId,
                PlannedAssistantName = s.PlannedAssistantName,
                ActualAssistantId = s.ActualAssistantId,
                ActualAssistantName = s.ActualAssistantName,
                PlannedLessonTitle = resolvedLinkage.PlannedLessonTitle ?? s.PlannedLessonTitle,
                ActualLessonPlanTemplateId = resolvedLinkage.ActualLessonPlanTemplateId,
                ActualLessonTitle = resolvedLinkage.ActualLessonTitle ?? s.ActualLessonTitle,
                TeachingLogId = s.TeachingLogId,
                TeachingLogStatus = s.TeachingLogStatus,
                TeachingProgressStatus = s.TeachingProgressStatus,
                ActualTeachingType = s.ActualTeachingType
                };
            })
            .ToList();

        var page = new Page<SessionListItemDto>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetSessionsResponse
        {
            Sessions = page
        };
    }
}


