using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.MakeupCredits.Settings;

public sealed class GetMakeupSettingsQueryHandler(
    IDbContext context
) : IQueryHandler<GetMakeupSettingsQuery, MakeupSettingsResponse>
{
    public async Task<Result<MakeupSettingsResponse>> Handle(
        GetMakeupSettingsQuery query,
        CancellationToken cancellationToken)
    {
        var settings = await MakeupSettingsHelper.GetOrCreateAsync(context, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return MakeupSettingsHelper.ToResponse(settings);
    }
}
