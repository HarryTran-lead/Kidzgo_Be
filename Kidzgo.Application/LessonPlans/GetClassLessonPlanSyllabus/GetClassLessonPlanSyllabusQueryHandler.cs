using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
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
                s.PlannedDatetime,
                s.PlannedTeacherId,
                PlannedTeacherName = s.PlannedTeacher != null ? s.PlannedTeacher.Name : null,
                s.ActualTeacherId,
                ActualTeacherName = s.ActualTeacher != null ? s.ActualTeacher.Name : null
            })
            .ToListAsync(cancellationToken);

        var lessonPlans = await context.LessonPlans
            .Where(lp => lp.ClassId == classEntity.Id && !lp.IsDeleted)
            .ToListAsync(cancellationToken);

        var templates = await context.LessonPlanTemplates
            .Where(t => t.ProgramId == classEntity.ProgramId && t.IsActive && !t.IsDeleted)
            .OrderBy(t => t.SessionIndex)
            .ToListAsync(cancellationToken);

        var lessonPlanBySessionId = lessonPlans.ToDictionary(lp => lp.SessionId);
        var templateById = templates.ToDictionary(t => t.Id);
        var templateBySessionIndex = templates.ToDictionary(t => t.SessionIndex);
        var metadata = templates
            .Select(t => t.SyllabusMetadata)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        var responseSessions = new List<ClassLessonPlanSyllabusSessionDto>(sessions.Count);

        for (int i = 0; i < sessions.Count; i++)
        {
            var session = sessions[i];
            var sessionIndex = i + 1;
            lessonPlanBySessionId.TryGetValue(session.Id, out var lessonPlan);

            Kidzgo.Domain.LessonPlans.LessonPlanTemplate? template = null;
            if (lessonPlan?.TemplateId.HasValue == true)
            {
                templateById.TryGetValue(lessonPlan.TemplateId.Value, out template);
            }

            template ??= templateBySessionIndex.GetValueOrDefault(sessionIndex);

            var canEdit = currentUser.Role != UserRole.Teacher ||
                          session.PlannedTeacherId == currentUser.Id ||
                          session.ActualTeacherId == currentUser.Id;

            responseSessions.Add(new ClassLessonPlanSyllabusSessionDto
            {
                SessionId = session.Id,
                SessionIndex = sessionIndex,
                SessionDate = VietnamTime.ToVietnamDateTime(session.PlannedDatetime),
                PlannedTeacherId = session.PlannedTeacherId,
                PlannedTeacherName = session.PlannedTeacherName,
                ActualTeacherId = session.ActualTeacherId,
                ActualTeacherName = session.ActualTeacherName,
                LessonPlanId = lessonPlan?.Id,
                TemplateId = template?.Id ?? lessonPlan?.TemplateId,
                TemplateTitle = template?.Title,
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
            ProgramId = classEntity.ProgramId,
            ProgramName = classEntity.Program.Name,
            SyllabusMetadata = metadata,
            Sessions = responseSessions
        };
    }
}
