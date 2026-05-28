namespace Kidzgo.Application.Sessions.Shared;

internal sealed record SessionLessonPlanLinkageSnapshot(
    Guid? SessionTemplateId,
    Guid? LessonPlanTemplateId,
    Guid? PlannedLessonPlanTemplateId,
    Guid? ActualLessonPlanTemplateId,
    Guid? SessionLessonTemplateId,
    Guid? SyllabusId,
    Guid? ModuleId,
    int? SessionIndexInModule);

internal sealed record ResolvedSessionLessonPlanLinkage(
    Guid? LessonPlanTemplateId,
    Guid? PlannedLessonPlanTemplateId,
    Guid? ActualLessonPlanTemplateId,
    string? PlannedLessonTitle,
    string? ActualLessonTitle);

internal static class SessionLessonPlanLinkageResolver
{
    public static ResolvedSessionLessonPlanLinkage Resolve(
        SessionLessonPlanLinkageSnapshot snapshot,
        IReadOnlyDictionary<(Guid SyllabusId, Guid ModuleId, int SessionIndex), Guid> templateBySyllabusModuleAndIndex,
        IReadOnlyDictionary<Guid, string?> titleByTemplateId)
    {
        var sessionTemplateId = NormalizeTemplateId(snapshot.SessionTemplateId);
        var lessonPlanTemplateId = NormalizeTemplateId(snapshot.LessonPlanTemplateId);
        var plannedLessonPlanTemplateId = NormalizeTemplateId(snapshot.PlannedLessonPlanTemplateId);
        var actualLessonPlanTemplateId = NormalizeTemplateId(snapshot.ActualLessonPlanTemplateId);
        var sessionLessonTemplateId = NormalizeTemplateId(snapshot.SessionLessonTemplateId);

        Guid? runtimeTemplateId = null;
        if (snapshot.ModuleId.HasValue &&
            snapshot.SyllabusId.HasValue &&
            snapshot.SyllabusId.Value != Guid.Empty &&
            snapshot.ModuleId.Value != Guid.Empty &&
            snapshot.SessionIndexInModule.HasValue)
        {
            Guid runtimeTemplateCandidate;
            templateBySyllabusModuleAndIndex.TryGetValue(
                (snapshot.SyllabusId.Value, snapshot.ModuleId.Value, snapshot.SessionIndexInModule.Value),
                out runtimeTemplateCandidate);
            runtimeTemplateId = runtimeTemplateCandidate;
        }

        runtimeTemplateId = NormalizeTemplateId(runtimeTemplateId);

        var plannedTemplateId = plannedLessonPlanTemplateId
            ?? runtimeTemplateId
            ?? sessionTemplateId
            ?? lessonPlanTemplateId
            ?? sessionLessonTemplateId
            ;

        var resolvedTemplateId = actualLessonPlanTemplateId ?? plannedTemplateId;

        return new ResolvedSessionLessonPlanLinkage(
            resolvedTemplateId,
            plannedTemplateId,
            actualLessonPlanTemplateId,
            GetTitle(plannedTemplateId, titleByTemplateId),
            GetTitle(actualLessonPlanTemplateId, titleByTemplateId));
    }

    public static Guid? NormalizeTemplateId(Guid? templateId)
    {
        return templateId.HasValue && templateId.Value != Guid.Empty
            ? templateId.Value
            : null;
    }

    public static Guid? ResolveRuntimeTemplateId(
        Guid? syllabusId,
        Guid? moduleId,
        int? sessionIndexInModule,
        IReadOnlyDictionary<(Guid SyllabusId, Guid ModuleId, int SessionIndex), Guid> templateBySyllabusModuleAndIndex)
    {
        if (!syllabusId.HasValue ||
            syllabusId.Value == Guid.Empty ||
            !moduleId.HasValue ||
            moduleId.Value == Guid.Empty ||
            !sessionIndexInModule.HasValue)
        {
            return null;
        }

        return templateBySyllabusModuleAndIndex.TryGetValue((syllabusId.Value, moduleId.Value, sessionIndexInModule.Value), out var templateId)
            ? NormalizeTemplateId(templateId)
            : null;
    }

    public static IReadOnlyCollection<Guid> GetCandidateTemplateIds(
        IEnumerable<SessionLessonPlanLinkageSnapshot> snapshots,
        IReadOnlyDictionary<(Guid SyllabusId, Guid ModuleId, int SessionIndex), Guid> templateBySyllabusModuleAndIndex)
    {
        var ids = new HashSet<Guid>();

        foreach (var snapshot in snapshots)
        {
            AddIfPresent(ids, snapshot.SessionTemplateId);
            AddIfPresent(ids, snapshot.LessonPlanTemplateId);
            AddIfPresent(ids, snapshot.PlannedLessonPlanTemplateId);
            AddIfPresent(ids, snapshot.ActualLessonPlanTemplateId);
            AddIfPresent(ids, snapshot.SessionLessonTemplateId);
            AddIfPresent(ids, ResolveRuntimeTemplateId(snapshot.SyllabusId, snapshot.ModuleId, snapshot.SessionIndexInModule, templateBySyllabusModuleAndIndex));
        }

        return ids;
    }

    public static IReadOnlyCollection<Guid> GetStoredCandidateTemplateIds(
        IEnumerable<SessionLessonPlanLinkageSnapshot> snapshots)
    {
        var ids = new HashSet<Guid>();

        foreach (var snapshot in snapshots)
        {
            AddIfPresent(ids, snapshot.SessionTemplateId);
            AddIfPresent(ids, snapshot.LessonPlanTemplateId);
            AddIfPresent(ids, snapshot.PlannedLessonPlanTemplateId);
            AddIfPresent(ids, snapshot.ActualLessonPlanTemplateId);
            AddIfPresent(ids, snapshot.SessionLessonTemplateId);
        }

        return ids;
    }

    public static IReadOnlyCollection<Guid> GetConsistencyTemplateIds(
        SessionLessonPlanLinkageSnapshot snapshot,
        IReadOnlyDictionary<(Guid SyllabusId, Guid ModuleId, int SessionIndex), Guid> templateBySyllabusModuleAndIndex)
    {
        var ids = new HashSet<Guid>();

        AddIfPresent(ids, snapshot.SessionTemplateId);
        AddIfPresent(ids, snapshot.LessonPlanTemplateId);
        AddIfPresent(ids, snapshot.PlannedLessonPlanTemplateId);
        AddIfPresent(ids, snapshot.SessionLessonTemplateId);

        return ids;
    }

    private static void AddIfPresent(ISet<Guid> ids, Guid? templateId)
    {
        var normalized = NormalizeTemplateId(templateId);
        if (normalized.HasValue)
        {
            ids.Add(normalized.Value);
        }
    }

    private static string? GetTitle(Guid? templateId, IReadOnlyDictionary<Guid, string?> titleByTemplateId)
    {
        return templateId.HasValue && titleByTemplateId.TryGetValue(templateId.Value, out var title)
            ? title
            : null;
    }
}
