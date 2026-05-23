namespace Kidzgo.Application.Sessions.Shared;

internal sealed record SessionLessonPlanLinkageSnapshot(
    Guid? SessionTemplateId,
    Guid? LessonPlanTemplateId,
    Guid? PlannedLessonPlanTemplateId,
    Guid? ActualLessonPlanTemplateId,
    Guid? SessionLessonTemplateId,
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
        IReadOnlyDictionary<(Guid ModuleId, int SessionIndex), Guid> templateByModuleAndIndex,
        IReadOnlyDictionary<Guid, string?> titleByTemplateId)
    {
        var sessionTemplateId = NormalizeTemplateId(snapshot.SessionTemplateId);
        var lessonPlanTemplateId = NormalizeTemplateId(snapshot.LessonPlanTemplateId);
        var plannedLessonPlanTemplateId = NormalizeTemplateId(snapshot.PlannedLessonPlanTemplateId);
        var actualLessonPlanTemplateId = NormalizeTemplateId(snapshot.ActualLessonPlanTemplateId);
        var sessionLessonTemplateId = NormalizeTemplateId(snapshot.SessionLessonTemplateId);

        Guid? runtimeTemplateId = null;
        if (snapshot.ModuleId.HasValue &&
            snapshot.ModuleId.Value != Guid.Empty &&
            snapshot.SessionIndexInModule.HasValue)
        {
            Guid runtimeTemplateCandidate;
            templateByModuleAndIndex.TryGetValue(
                (snapshot.ModuleId.Value, snapshot.SessionIndexInModule.Value),
                out runtimeTemplateCandidate);
            runtimeTemplateId = runtimeTemplateCandidate;
        }

        runtimeTemplateId = NormalizeTemplateId(runtimeTemplateId);

        var plannedTemplateId = plannedLessonPlanTemplateId
            ?? sessionTemplateId
            ?? lessonPlanTemplateId
            ?? sessionLessonTemplateId
            ?? runtimeTemplateId;

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
        Guid? moduleId,
        int? sessionIndexInModule,
        IReadOnlyDictionary<(Guid ModuleId, int SessionIndex), Guid> templateByModuleAndIndex)
    {
        if (!moduleId.HasValue || moduleId.Value == Guid.Empty || !sessionIndexInModule.HasValue)
        {
            return null;
        }

        return templateByModuleAndIndex.TryGetValue((moduleId.Value, sessionIndexInModule.Value), out var templateId)
            ? NormalizeTemplateId(templateId)
            : null;
    }

    public static IReadOnlyCollection<Guid> GetCandidateTemplateIds(
        IEnumerable<SessionLessonPlanLinkageSnapshot> snapshots,
        IReadOnlyDictionary<(Guid ModuleId, int SessionIndex), Guid> templateByModuleAndIndex)
    {
        var ids = new HashSet<Guid>();

        foreach (var snapshot in snapshots)
        {
            AddIfPresent(ids, snapshot.SessionTemplateId);
            AddIfPresent(ids, snapshot.LessonPlanTemplateId);
            AddIfPresent(ids, snapshot.PlannedLessonPlanTemplateId);
            AddIfPresent(ids, snapshot.ActualLessonPlanTemplateId);
            AddIfPresent(ids, snapshot.SessionLessonTemplateId);
            AddIfPresent(ids, ResolveRuntimeTemplateId(snapshot.ModuleId, snapshot.SessionIndexInModule, templateByModuleAndIndex));
        }

        return ids;
    }

    public static IReadOnlyCollection<Guid> GetConsistencyTemplateIds(
        SessionLessonPlanLinkageSnapshot snapshot,
        IReadOnlyDictionary<(Guid ModuleId, int SessionIndex), Guid> templateByModuleAndIndex)
    {
        var ids = new HashSet<Guid>();

        AddIfPresent(ids, snapshot.SessionTemplateId);
        AddIfPresent(ids, snapshot.LessonPlanTemplateId);
        AddIfPresent(ids, snapshot.PlannedLessonPlanTemplateId);
        AddIfPresent(ids, snapshot.SessionLessonTemplateId);
        AddIfPresent(ids, ResolveRuntimeTemplateId(snapshot.ModuleId, snapshot.SessionIndexInModule, templateByModuleAndIndex));

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
