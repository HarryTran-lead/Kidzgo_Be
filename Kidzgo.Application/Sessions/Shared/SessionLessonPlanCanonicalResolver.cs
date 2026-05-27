using Kidzgo.Application.Abstraction.Data;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.Shared;

internal sealed record CanonicalLessonPlanTemplateMetadata(
    Guid Id,
    Guid SyllabusId,
    Guid ModuleId,
    int SessionIndex,
    string? Title);

internal sealed record SessionLessonPlanCanonicalResolution(
    Guid? ResolvedSyllabusId,
    Guid? RuntimeTemplateId,
    IReadOnlyDictionary<Guid, CanonicalLessonPlanTemplateMetadata> TemplateMetadataById,
    ResolvedSessionLessonPlanLinkage Linkage);

internal static class SessionLessonPlanCanonicalResolver
{
    public static async Task<SessionLessonPlanCanonicalResolution> ResolveAsync(
        IDbContext context,
        SessionLessonPlanLinkageSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        var storedTemplateIds = SessionLessonPlanLinkageResolver.GetStoredCandidateTemplateIds(new[] { snapshot });

        var templateMetadataById = storedTemplateIds.Count == 0
            ? new Dictionary<Guid, CanonicalLessonPlanTemplateMetadata>()
            : await context.LessonPlanTemplates
                .AsNoTracking()
                .Where(t => storedTemplateIds.Contains(t.Id) && !t.IsDeleted)
                .Select(t => new CanonicalLessonPlanTemplateMetadata(
                    t.Id,
                    t.SyllabusId,
                    t.ModuleId,
                    t.SessionIndex,
                    t.Title))
                .ToDictionaryAsync(t => t.Id, t => t, cancellationToken);

        var resolvedSyllabusId = ResolveSyllabusId(snapshot, templateMetadataById);
        CanonicalLessonPlanTemplateMetadata? runtimeTemplate = null;

        if (resolvedSyllabusId.HasValue &&
            resolvedSyllabusId.Value != Guid.Empty &&
            snapshot.ModuleId.HasValue &&
            snapshot.ModuleId.Value != Guid.Empty &&
            snapshot.SessionIndexInModule.HasValue)
        {
            runtimeTemplate = await context.LessonPlanTemplates
                .AsNoTracking()
                .Where(t =>
                    t.SyllabusId == resolvedSyllabusId.Value &&
                    t.ModuleId == snapshot.ModuleId.Value &&
                    t.SessionIndex == snapshot.SessionIndexInModule.Value &&
                    t.IsActive &&
                    !t.IsDeleted)
                .Select(t => new CanonicalLessonPlanTemplateMetadata(
                    t.Id,
                    t.SyllabusId,
                    t.ModuleId,
                    t.SessionIndex,
                    t.Title))
                .FirstOrDefaultAsync(cancellationToken);
        }

        var templateBySyllabusModuleAndIndex = runtimeTemplate is null
            ? new Dictionary<(Guid SyllabusId, Guid ModuleId, int SessionIndex), Guid>()
            : new Dictionary<(Guid SyllabusId, Guid ModuleId, int SessionIndex), Guid>
            {
                [(runtimeTemplate.SyllabusId, runtimeTemplate.ModuleId, runtimeTemplate.SessionIndex)] = runtimeTemplate.Id
            };

        var titleByTemplateId = templateMetadataById.Values
            .ToDictionary(x => x.Id, x => x.Title);

        if (runtimeTemplate is not null)
        {
            titleByTemplateId[runtimeTemplate.Id] = runtimeTemplate.Title;
        }

        var linkage = SessionLessonPlanLinkageResolver.Resolve(
            snapshot,
            templateBySyllabusModuleAndIndex,
            titleByTemplateId);

        return new SessionLessonPlanCanonicalResolution(
            resolvedSyllabusId,
            runtimeTemplate?.Id,
            templateMetadataById,
            linkage);
    }

    private static Guid? ResolveSyllabusId(
        SessionLessonPlanLinkageSnapshot snapshot,
        IReadOnlyDictionary<Guid, CanonicalLessonPlanTemplateMetadata> templateMetadataById)
    {
        if (snapshot.SyllabusId.HasValue && snapshot.SyllabusId.Value != Guid.Empty)
        {
            return snapshot.SyllabusId.Value;
        }

        var syllabusIds = templateMetadataById.Values
            .Select(x => x.SyllabusId)
            .Distinct()
            .ToList();

        return syllabusIds.Count == 1
            ? syllabusIds[0]
            : null;
    }
}
