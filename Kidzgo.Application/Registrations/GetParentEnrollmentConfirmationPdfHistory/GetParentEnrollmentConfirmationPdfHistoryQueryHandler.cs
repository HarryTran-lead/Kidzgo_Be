using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Application.Registrations.GetEnrollmentConfirmationPdfHistory;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.Registrations.GetParentEnrollmentConfirmationPdfHistory;

public sealed class GetParentEnrollmentConfirmationPdfHistoryQueryHandler(
    IDbContext context,
    IUserContext userContext,
    IFileStorageService fileStorage,
    ILogger<GetParentEnrollmentConfirmationPdfHistoryQueryHandler> logger)
    : IQueryHandler<GetParentEnrollmentConfirmationPdfHistoryQuery, GetEnrollmentConfirmationPdfHistoryResponse>
{
    public async Task<Result<GetEnrollmentConfirmationPdfHistoryResponse>> Handle(
        GetParentEnrollmentConfirmationPdfHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var accessResult = await ParentRegistrationAccessHelper.EnsureRegistrationAccessAsync(
            context,
            userContext,
            query.RegistrationId,
            cancellationToken);

        if (!accessResult.IsSuccess)
        {
            return Result.Failure<GetEnrollmentConfirmationPdfHistoryResponse>(accessResult.Error);
        }

        return await EnrollmentConfirmationPdfHistoryReadModelBuilder.BuildAsync(
            context,
            fileStorage,
            logger,
            query.RegistrationId,
            query.Track,
            query.FormType,
            query.PageNumber,
            query.PageSize,
            cancellationToken);
    }
}
