using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Registrations.GetRegistrationById;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Registrations.GetParentRegistrationById;

public sealed class GetParentRegistrationByIdQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetParentRegistrationByIdQuery, GetRegistrationByIdResponse>
{
    public async Task<Result<GetRegistrationByIdResponse>> Handle(
        GetParentRegistrationByIdQuery query,
        CancellationToken cancellationToken)
    {
        var accessResult = await ParentRegistrationAccessHelper.EnsureRegistrationAccessAsync(
            context,
            userContext,
            query.Id,
            cancellationToken);

        if (!accessResult.IsSuccess)
        {
            return Result.Failure<GetRegistrationByIdResponse>(accessResult.Error);
        }

        return await RegistrationDetailReadModelBuilder.BuildAsync(
            context,
            query.Id,
            cancellationToken);
    }
}
