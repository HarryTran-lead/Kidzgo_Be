using Kidzgo.Domain.Registrations;

namespace Kidzgo.Application.Registrations;

internal static class RegistrationTrackHelper
{
    internal const string PrimaryTrack = "primary";
    internal const string SecondaryTrack = "secondary";

    internal static string NormalizeTrack(string? track)
    {
        return string.Equals(track, SecondaryTrack, StringComparison.OrdinalIgnoreCase)
            ? SecondaryTrack
            : PrimaryTrack;
    }

    internal static RegistrationTrackType ToTrackType(string? track)
    {
        return string.Equals(track, SecondaryTrack, StringComparison.OrdinalIgnoreCase)
            ? RegistrationTrackType.Secondary
            : RegistrationTrackType.Primary;
    }

    internal static string ToTrackName(RegistrationTrackType trackType)
    {
        return trackType == RegistrationTrackType.Secondary
            ? SecondaryTrack
            : PrimaryTrack;
    }

    internal static bool TryParseEntryType(string? entryType, out EntryType parsedEntryType)
    {
        switch (entryType?.Trim().ToLowerInvariant())
        {
            case null:
            case "":
            case "immediate":
                parsedEntryType = EntryType.Immediate;
                return true;
            case "wait":
                parsedEntryType = EntryType.Wait;
                return true;
            case "retake":
                parsedEntryType = EntryType.Retake;
                return true;
            default:
                parsedEntryType = default;
                return false;
        }
    }

    internal static string? ToApiEntryType(EntryType? entryType)
    {
        return entryType switch
        {
            null => null,
            EntryType.Wait => nameof(EntryType.Wait),
            EntryType.Retake => nameof(EntryType.Retake),
            _ => nameof(EntryType.Immediate)
        };
    }

    internal static RegistrationStatus ResolveStatus(Registration registration)
    {
        var hasImmediateTrack =
            HasAssignedTrack(registration.ClassId, registration.EntryType, EntryType.Immediate) ||
            HasAssignedTrack(registration.SecondaryClassId, registration.SecondaryEntryType, EntryType.Immediate);

        if (hasImmediateTrack)
        {
            return RegistrationStatus.Studying;
        }

        var hasAssignedTrack =
            HasAssignedTrack(registration.ClassId, registration.EntryType, EntryType.Retake) ||
            HasAssignedTrack(registration.SecondaryClassId, registration.SecondaryEntryType, EntryType.Retake) ||
            HasAssignedTrack(registration.ClassId, registration.EntryType, EntryType.Makeup) ||
            HasAssignedTrack(registration.SecondaryClassId, registration.SecondaryEntryType, EntryType.Makeup);

        if (hasAssignedTrack)
        {
            return RegistrationStatus.ClassAssigned;
        }

        if (registration.ClassId == null ||
            (registration.SecondaryLevelId.HasValue && registration.SecondaryClassId == null))
        {
            return RegistrationStatus.WaitingForClass;
        }

        return RegistrationStatus.New;
    }

    private static bool HasAssignedTrack(Guid? classId, EntryType? currentEntryType, EntryType expectedEntryType)
    {
        return classId.HasValue && currentEntryType == expectedEntryType;
    }
}
