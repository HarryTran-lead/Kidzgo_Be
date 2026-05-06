using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests;

internal static class PlacementTestDuplicateGuard
{
    public static async Task<Result> EnsureLeadChildCanCreateInitialPlacementTestAsync(
        IDbContext context,
        Guid leadId,
        Guid leadChildId,
        bool includeLegacyLeadFallback,
        CancellationToken cancellationToken)
    {
        var hasExistingPlacementTest = await context.PlacementTests
            .AsNoTracking()
            .AnyAsync(
                pt =>
                    pt.Status == PlacementTestStatus.Scheduled &&
                    (pt.LeadChildId == leadChildId ||
                     (includeLegacyLeadFallback &&
                      pt.LeadChildId == null &&
                      pt.LeadId == leadId)),
                cancellationToken);

        return hasExistingPlacementTest
            ? Result.Failure(PlacementTestErrors.ActivePlacementTestAlreadyExistsForChild(leadChildId))
            : Result.Success();
    }

    public static async Task<Result> EnsureStudentProfileCanBeAssignedAsync(
        IDbContext context,
        Guid studentProfileId,
        Guid? excludePlacementTestId,
        CancellationToken cancellationToken)
    {
        var hasExistingPlacementTest = await context.PlacementTests
            .AsNoTracking()
            .AnyAsync(
                pt =>
                    pt.Id != excludePlacementTestId &&
                    pt.StudentProfileId == studentProfileId &&
                    pt.Status == PlacementTestStatus.Scheduled,
                cancellationToken);

        return hasExistingPlacementTest
            ? Result.Failure(PlacementTestErrors.ActivePlacementTestAlreadyExistsForStudent(studentProfileId))
            : Result.Success();
    }
}
