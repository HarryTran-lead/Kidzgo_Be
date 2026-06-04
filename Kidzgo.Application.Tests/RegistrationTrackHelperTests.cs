using Kidzgo.Application.Registrations;
using Kidzgo.Domain.Registrations;
using Xunit;

namespace Kidzgo.Application.Tests;

public sealed class RegistrationTrackHelperTests
{
    [Fact]
    public void ResolveTargetLevelId_returns_primary_level_for_primary_track()
    {
        var primaryLevelId = Guid.NewGuid();
        var secondaryLevelId = Guid.NewGuid();
        var registration = new Registration
        {
            LevelId = primaryLevelId,
            SecondaryLevelId = secondaryLevelId
        };

        var resolvedLevelId = RegistrationTrackHelper.ResolveTargetLevelId(
            registration,
            RegistrationTrackHelper.PrimaryTrack);

        Assert.Equal(primaryLevelId, resolvedLevelId);
    }

    [Fact]
    public void ResolveTargetLevelId_returns_secondary_level_for_secondary_track()
    {
        var primaryLevelId = Guid.NewGuid();
        var secondaryLevelId = Guid.NewGuid();
        var registration = new Registration
        {
            LevelId = primaryLevelId,
            SecondaryLevelId = secondaryLevelId
        };

        var resolvedLevelId = RegistrationTrackHelper.ResolveTargetLevelId(
            registration,
            RegistrationTrackHelper.SecondaryTrack);

        Assert.Equal(secondaryLevelId, resolvedLevelId);
    }

    [Fact]
    public void ResolveTargetLevelId_defaults_to_primary_level_for_unknown_track()
    {
        var primaryLevelId = Guid.NewGuid();
        var secondaryLevelId = Guid.NewGuid();
        var registration = new Registration
        {
            LevelId = primaryLevelId,
            SecondaryLevelId = secondaryLevelId
        };

        var resolvedLevelId = RegistrationTrackHelper.ResolveTargetLevelId(
            registration,
            "unexpected-track");

        Assert.Equal(primaryLevelId, resolvedLevelId);
    }
}
