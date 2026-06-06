using Kidzgo.Domain.Classes;

namespace Kidzgo.Application.Classes;

internal static class ClassLifecycleStatusHelper
{
    internal static ClassStatus ResolveInitialStatus(DateOnly startDate, DateTime now)
    {
        return startDate <= VietnamTime.ToVietnamDateOnly(now)
            ? ClassStatus.Active
            : ClassStatus.Planned;
    }

    internal static ClassStatus ResolveScheduledStatus(
        ClassStatus currentStatus,
        DateOnly startDate,
        DateOnly today)
    {
        if (currentStatus is ClassStatus.Closed or ClassStatus.Completed or ClassStatus.Cancelled or ClassStatus.Suspended)
        {
            return currentStatus;
        }

        if (startDate <= today)
        {
            return currentStatus == ClassStatus.Full
                ? ClassStatus.Full
                : ClassStatus.Active;
        }

        return currentStatus == ClassStatus.Full
            ? ClassStatus.Full
            : currentStatus == ClassStatus.Recruiting
                ? ClassStatus.Recruiting
                : ClassStatus.Planned;
    }

    internal static bool ShouldActivateOnFirstSession(ClassStatus status)
    {
        return status is ClassStatus.Planned or ClassStatus.Recruiting;
    }
}
