using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Sessions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.GetSessionLessonPlanDocument;

public sealed class GetSessionLessonPlanDocumentQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetSessionLessonPlanDocumentQuery, GetSessionLessonPlanDocumentResponse>
{
    public async Task<Result<GetSessionLessonPlanDocumentResponse>> Handle(
        GetSessionLessonPlanDocumentQuery query,
        CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<GetSessionLessonPlanDocumentResponse>(SessionErrors.UnauthorizedAccess(query.SessionId));
        }

        var session = await context.Sessions
            .AsNoTracking()
            .Include(s => s.Class)
            .Include(s => s.Module)
            .Include(s => s.LessonPlan)
            .Include(s => s.TeachingLog)
                .ThenInclude(x => x!.Lessons)
            .Include(s => s.SessionLessons)
            .FirstOrDefaultAsync(s => s.Id == query.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure<GetSessionLessonPlanDocumentResponse>(SessionErrors.NotFound(query.SessionId));
        }

        if (currentUser.Role == UserRole.Teacher)
        {
            var canAccessSession = session.Class?.MainTeacherId == currentUser.Id ||
                                   session.Class?.AssistantTeacherId == currentUser.Id ||
                                   session.PlannedTeacherId == currentUser.Id ||
                                   session.ActualTeacherId == currentUser.Id;

            if (!canAccessSession)
            {
                return Result.Failure<GetSessionLessonPlanDocumentResponse>(SessionErrors.UnauthorizedAccess(query.SessionId));
            }
        }

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

        var canonicalResolution = await SessionLessonPlanCanonicalResolver.ResolveAsync(
            context,
            linkageSnapshot,
            cancellationToken);

        var consistencyTemplateIds = SessionLessonPlanLinkageResolver.GetConsistencyTemplateIds(
            linkageSnapshot,
            new Dictionary<(Guid SyllabusId, Guid ModuleId, int SessionIndex), Guid>());

        if (consistencyTemplateIds.Count > 1)
        {
            return Result.Failure<GetSessionLessonPlanDocumentResponse>(
                SessionErrors.LessonPlanTemplateInconsistent(
                    session.Id,
                    session.ClassId,
                    session.ModuleId,
                    consistencyTemplateIds));
        }

        var resolvedLinkage = canonicalResolution.Linkage;

        if (!resolvedLinkage.LessonPlanTemplateId.HasValue)
        {
            if (session.ModuleId.HasValue && session.ModuleId.Value != Guid.Empty && session.SessionIndexInModule.HasValue)
            {
                return Result.Failure<GetSessionLessonPlanDocumentResponse>(
                    SessionErrors.CurriculumMappingMissing(
                        session.Id,
                        session.ClassId,
                        session.ModuleId.Value,
                        session.SessionIndexInModule.Value));
            }

            return Result.Failure<GetSessionLessonPlanDocumentResponse>(
                SessionErrors.LessonPlanTemplateMissing(
                    session.Id,
                    session.ClassId,
                    session.ModuleId));
        }

        var template = await context.LessonPlanTemplates
            .AsNoTracking()
            .Include(t => t.Module)
                .ThenInclude(m => m.Level)
                    .ThenInclude(l => l.Program)
            .Include(t => t.Syllabus)
            .Include(t => t.LessonPlanUnit)
            .Include(t => t.CreatedByUser)
            .FirstOrDefaultAsync(
                t => t.Id == resolvedLinkage.LessonPlanTemplateId.Value && !t.IsDeleted,
                cancellationToken);

        if (template is null)
        {
            return Result.Failure<GetSessionLessonPlanDocumentResponse>(
                SessionErrors.LessonPlanDocumentNotFound(
                    session.Id,
                    session.ClassId,
                    resolvedLinkage.LessonPlanTemplateId.Value));
        }

        var lessonProgress = session.TeachingLog?.Lessons
            .OrderBy(x => x.OrderIndex)
            .FirstOrDefault();

        return new GetSessionLessonPlanDocumentResponse
        {
            SessionId = session.Id,
            ClassId = session.ClassId,
            SyllabusId = session.Class?.SyllabusId,
            ModuleId = session.ModuleId,
            ModuleName = session.Module?.Name,
            SessionIndexInModule = session.SessionIndexInModule,
            LessonPlanTemplateId = resolvedLinkage.LessonPlanTemplateId,
            PlannedLessonPlanTemplateId = resolvedLinkage.PlannedLessonPlanTemplateId,
            ActualLessonPlanTemplateId = resolvedLinkage.ActualLessonPlanTemplateId,
            PlannedLessonTitle = resolvedLinkage.PlannedLessonTitle,
            ActualLessonTitle = resolvedLinkage.ActualLessonTitle,
            TeachingLogId = session.TeachingLog?.Id,
            TeachingLogStatus = session.TeachingLog?.Status.ToString(),
            TeachingProgressStatus = lessonProgress?.ProgressStatus.ToString(),
            Document = new SessionLessonPlanDocumentDto
            {
                Id = template.Id,
                SyllabusId = template.SyllabusId,
                SyllabusCode = template.Syllabus.Code,
                SyllabusVersion = template.Syllabus.Version,
                SyllabusTitle = template.Syllabus.Title,
                ModuleId = template.ModuleId,
                ModuleCode = template.Module.Code,
                ModuleName = template.Module.Name,
                LessonPlanUnitId = template.LessonPlanUnitId,
                LessonPlanUnitName = template.LessonPlanUnit?.Name,
                OrderIndexInUnit = template.OrderIndexInUnit,
                LevelId = template.Module.LevelId,
                LevelName = template.Module.Level.Name,
                ProgramId = template.Module.Level.ProgramId,
                ProgramName = template.Module.Level.Program.Name,
                Title = template.Title,
                SessionIndex = template.SessionIndex,
                SessionOrder = template.SessionOrder,
                SyllabusMetadata = template.SyllabusMetadata,
                SyllabusContent = template.SyllabusContent,
                Objectives = template.Objectives,
                LanguageContent = template.LanguageContent,
                Vocabulary = template.Vocabulary,
                Grammar = template.Grammar,
                TeachingMethodology = template.TeachingMethodology,
                TeacherMaterials = template.TeacherMaterials,
                StudentMaterials = template.StudentMaterials,
                Procedure = template.Procedure,
                Evaluation = template.Evaluation,
                SourceFileName = template.SourceFileName,
                Attachment = template.AttachmentUrl,
                IsActive = template.IsActive,
                CreatedBy = template.CreatedBy,
                CreatedByName = template.CreatedByUser?.Name,
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            }
        };
    }
}
