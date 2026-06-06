using Kidzgo.Application.Classes;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Classes;
using Xunit;

namespace Kidzgo.Application.Tests;

public sealed class ClassLifecycleStatusHelperTests
{
    [Fact]
    public void ResolveInitialStatus_returns_planned_for_future_class()
    {
        var now = new DateTime(2026, 6, 6, 8, 0, 0, DateTimeKind.Utc);
        var startDate = new DateOnly(2026, 6, 15);

        var status = ClassLifecycleStatusHelper.ResolveInitialStatus(startDate, now);

        Assert.Equal(ClassStatus.Planned, status);
    }

    [Fact]
    public void ResolveInitialStatus_returns_active_for_class_starting_today()
    {
        var now = new DateTime(2026, 6, 6, 8, 0, 0, DateTimeKind.Utc);
        var startDate = VietnamTime.ToVietnamDateOnly(now);

        var status = ClassLifecycleStatusHelper.ResolveInitialStatus(startDate, now);

        Assert.Equal(ClassStatus.Active, status);
    }

    [Fact]
    public void ResolveScheduledStatus_preserves_recruiting_for_future_class()
    {
        var today = new DateOnly(2026, 6, 6);
        var startDate = new DateOnly(2026, 6, 15);

        var status = ClassLifecycleStatusHelper.ResolveScheduledStatus(
            ClassStatus.Recruiting,
            startDate,
            today);

        Assert.Equal(ClassStatus.Recruiting, status);
    }

    [Fact]
    public void ShouldActivateOnFirstSession_returns_true_for_planned_and_recruiting()
    {
        Assert.True(ClassLifecycleStatusHelper.ShouldActivateOnFirstSession(ClassStatus.Planned));
        Assert.True(ClassLifecycleStatusHelper.ShouldActivateOnFirstSession(ClassStatus.Recruiting));
        Assert.False(ClassLifecycleStatusHelper.ShouldActivateOnFirstSession(ClassStatus.Active));
    }
}
