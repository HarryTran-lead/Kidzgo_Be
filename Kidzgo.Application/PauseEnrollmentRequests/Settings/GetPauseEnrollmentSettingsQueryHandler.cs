using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.PauseEnrollmentRequests.Settings;

public sealed class GetPauseEnrollmentSettingsQueryHandler(
    IDbContext context)
    : IQueryHandler<GetPauseEnrollmentSettingsQuery, PauseEnrollmentSettingsResponse>
{
    public async Task<Result<PauseEnrollmentSettingsResponse>> Handle(
        GetPauseEnrollmentSettingsQuery query,
        CancellationToken cancellationToken)
    {
        var settings = await PauseEnrollmentSettingsHelper.GetOrCreateAsync(context, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return PauseEnrollmentSettingsHelper.ToResponse(settings);
    }
}
