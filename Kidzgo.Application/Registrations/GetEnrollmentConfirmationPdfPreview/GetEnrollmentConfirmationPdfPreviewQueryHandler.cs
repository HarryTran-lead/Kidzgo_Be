using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.Registrations.GetEnrollmentConfirmationPdfPreview;

public sealed class GetEnrollmentConfirmationPdfPreviewQueryHandler(
    IDbContext context,
    IUserContext userContext,
    IFileStorageService fileStorage,
    ILogger<GetEnrollmentConfirmationPdfPreviewQueryHandler> logger)
    : IQueryHandler<GetEnrollmentConfirmationPdfPreviewQuery, GetEnrollmentConfirmationPdfPreviewResponse>
{
    public async Task<Result<GetEnrollmentConfirmationPdfPreviewResponse>> Handle(
        GetEnrollmentConfirmationPdfPreviewQuery query,
        CancellationToken cancellationToken)
    {
        var buildResult = await EnrollmentConfirmationPdfPreviewBuilder.BuildAsync(
            context,
            userContext,
            query.RegistrationId,
            query.Track,
            query.FormType,
            cancellationToken);

        if (buildResult.IsFailure)
        {
            return Result.Failure<GetEnrollmentConfirmationPdfPreviewResponse>(buildResult.Error);
        }
        return Result.Success(EnrollmentConfirmationPdfPreviewResponseMapper.Map(
            buildResult.Value,
            query.Track,
            query.FormType,
            fileStorage,
            logger));
    }
}
