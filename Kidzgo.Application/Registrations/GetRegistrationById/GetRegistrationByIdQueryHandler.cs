using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Registrations.GetRegistrationById;

public sealed class GetRegistrationByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetRegistrationByIdQuery, GetRegistrationByIdResponse>
{
    public async Task<Result<GetRegistrationByIdResponse>> Handle(
        GetRegistrationByIdQuery query,
        CancellationToken cancellationToken)
    {
        return await RegistrationDetailReadModelBuilder.BuildAsync(context, query.Id, cancellationToken);
    }
}
