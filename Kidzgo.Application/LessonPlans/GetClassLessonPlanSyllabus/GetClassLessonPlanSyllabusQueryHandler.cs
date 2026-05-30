using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Sessions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlans.GetClassLessonPlanSyllabus;

public sealed class GetClassLessonPlanSyllabusQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetClassLessonPlanSyllabusQuery, GetClassLessonPlanSyllabusResponse>
{
    public async Task<Result<GetClassLessonPlanSyllabusResponse>> Handle(
        GetClassLessonPlanSyllabusQuery query,
        CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<GetClassLessonPlanSyllabusResponse>(LessonPlanErrors.Unauthorized);
        }

        var classEntity = await context.Classes
            .Include(c => c.Program)
            .Include(c => c.Syllabus)
            .FirstOrDefaultAsync(c => c.Id == query.ClassId, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<GetClassLessonPlanSyllabusResponse>(
                LessonPlanErrors.ClassNotFound(query.ClassId));
        }

        if (currentUser.Role == UserRole.Teacher)
        {
            var canAccessClass = classEntity.MainTeacherId == currentUser.Id ||
                                 classEntity.AssistantTeacherId == currentUser.Id ||
                                 await context.Sessions.AnyAsync(
                                     s => s.ClassId == classEntity.Id &&
                                          (s.PlannedTeacherId == currentUser.Id || s.ActualTeacherId == currentUser.Id),
                                     cancellationToken);

            if (!canAccessClass)
            {
                return Result.Failure<GetClassLessonPlanSyllabusResponse>(LessonPlanErrors.Unauthorized);
            }
        }

        var sessions = await context.Sessions
            .Where(s => s.ClassId == classEntity.Id)
            .OrderBy(s => s.PlannedDatetime)
            .ThenBy(s => s.CreatedAt)
            .Select(s => new
            {
                s.Id,
                s.ModuleId,
                s.LessonPlanTemplateId,
                s.SessionIndexInModule,
                s.PlannedDatetime,
                s.PlannedTeacherId,
                PlannedTeacherName = s.PlannedTeacher != null ? s.PlannedTeacher.Name : null,
                s.ActualTeacherId,
                ActualTeacherName = s.ActualTeacher != null ? s.ActualTeacher.Name : null,
                PlannedLessonPlanTemplateId = s.TeachingLog != null ? s.TeachingLog.PlannedLessonPlanTemplateId : null,
                ActualLessonPlanTemplateId = s.TeachingLog != null ? s.TeachingLog.ActualLessonPlanTemplateId : null,
                SessionLessonTemplateId = s.SessionLessons
                    .OrderBy(l => l.OrderIndex)
                    .Select(l => l.LessonPlanTemplateId)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var lessonPlans = await context.LessonPlans
            .Where(lp => lp.ClassId == classEntity.Id && !lp.IsDeleted)
            .ToListAsync(cancellationToken);

        var lessonPlanBySessionId = lessonPlans.ToDictionary(lp => lp.SessionId);
        var moduleIds = sessions
            .Where(x => classEntity.SyllabusId.HasValue && x.ModuleId.HasValue)
            .Select(x => x.ModuleId!.Value)
            .Distinct()
            .ToList();
        var templates = await context.LessonPlanTemplates
            .Include(t => t.LessonPlanUnit)
            .Where(t =>
                classEntity.SyllabusId.HasValue &&
                t.SyllabusId == classEntity.SyllabusId.Value &&
                moduleIds.Contains(t.ModuleId) &&
                t.IsActive &&
                !t.IsDeleted)
            .ToListAsync(cancellationToken);
        var templateById = templates.ToDictionary(t => t.Id);
        var templateIdBySyllabusModuleAndIndex = templates.ToDictionary(
            t => (t.SyllabusId, t.ModuleId, t.SessionIndex),
            t => t.Id);
        var titleByTemplateId = templates.ToDictionary(t => t.Id, t => t.Title);
        var metadata = templates
            .Select(t => t.SyllabusMetadata)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        var responseSessions = new List<ClassLessonPlanSyllabusSessionDto>(sessions.Count);

        for (int i = 0; i < sessions.Count; i++)
        {
            var session = sessions[i];
            var sessionIndex = i + 1;
            lessonPlanBySessionId.TryGetValue(session.Id, out var lessonPlan);
            var linkageSnapshot = new SessionLessonPlanLinkageSnapshot(
                session.LessonPlanTemplateId,
                lessonPlan?.TemplateId,
                session.PlannedLessonPlanTemplateId,
                session.ActualLessonPlanTemplateId,
                session.SessionLessonTemplateId,
                classEntity.SyllabusId,
                session.ModuleId,
                session.SessionIndexInModule);
            var resolvedLinkage = SessionLessonPlanLinkageResolver.Resolve(
                linkageSnapshot,
                templateIdBySyllabusModuleAndIndex,
                titleByTemplateId);

            var template = resolvedLinkage.LessonPlanTemplateId.HasValue
                ? templateById.GetValueOrDefault(resolvedLinkage.LessonPlanTemplateId.Value)
                : null;

            var canEdit = currentUser.Role != UserRole.Teacher ||
                          session.PlannedTeacherId == currentUser.Id ||
                          session.ActualTeacherId == currentUser.Id;

            responseSessions.Add(new ClassLessonPlanSyllabusSessionDto
            {
                SessionId = session.Id,
                SessionIndex = sessionIndex,
                SyllabusId = classEntity.SyllabusId,
                ModuleId = session.ModuleId,
                SessionIndexInModule = session.SessionIndexInModule,
                SessionDate = VietnamTime.ToVietnamDateTime(session.PlannedDatetime),
                RowRef = $"session:{session.Id:D}",
                UnitName = template?.LessonPlanUnit?.Name,
                LessonTitle = resolvedLinkage.ActualLessonTitle
                    ?? resolvedLinkage.PlannedLessonTitle
                    ?? template?.Title,
                PlannedTeacherId = session.PlannedTeacherId,
                PlannedTeacherName = session.PlannedTeacherName,
                ActualTeacherId = session.ActualTeacherId,
                ActualTeacherName = session.ActualTeacherName,
                LessonPlanId = lessonPlan?.Id,
                TemplateId = resolvedLinkage.LessonPlanTemplateId,
                PlannedLessonPlanTemplateId = resolvedLinkage.PlannedLessonPlanTemplateId,
                ActualLessonPlanTemplateId = resolvedLinkage.ActualLessonPlanTemplateId,
                TemplateTitle = template?.Title,
                PlannedLessonTitle = resolvedLinkage.PlannedLessonTitle,
                ActualLessonTitle = resolvedLinkage.ActualLessonTitle,
                TemplateSyllabusContent = template?.SyllabusContent,
                PlannedContent = lessonPlan?.PlannedContent ?? template?.SyllabusContent,
                ActualContent = lessonPlan?.ActualContent,
                ActualHomework = lessonPlan?.ActualHomework,
                TeacherNotes = lessonPlan?.TeacherNotes,
                CanEdit = canEdit
            });
        }

        return new GetClassLessonPlanSyllabusResponse
        {
            ClassId = classEntity.Id,
            ClassCode = classEntity.Code,
            ClassTitle = classEntity.Title,
            SyllabusId = classEntity.SyllabusId,
            SyllabusCode = classEntity.Syllabus?.Code,
            SyllabusVersion = classEntity.Syllabus?.Version,
            SyllabusTitle = classEntity.Syllabus?.Title,
            SourceFileName = classEntity.Syllabus?.SourceFileName,
            AttachmentUrl = classEntity.Syllabus?.AttachmentUrl,
            ProgramId = classEntity.ProgramId,
            LevelId = classEntity.LevelId,
            ProgramName = classEntity.Program.Name,
            SyllabusMetadata = metadata,
            Sessions = responseSessions
        };
    }
}
