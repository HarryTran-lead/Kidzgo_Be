using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Application.Registrations.GetEnrollmentConfirmationPdfPreview;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.Registrations.GetParentEnrollmentConfirmationPdfPreview;

public sealed class GetParentEnrollmentConfirmationPdfPreviewQueryHandler(
    IDbContext context,
    IUserContext userContext,
    IFileStorageService fileStorage,
    ILogger<GetParentEnrollmentConfirmationPdfPreviewQueryHandler> logger)
    : IQueryHandler<GetParentEnrollmentConfirmationPdfPreviewQuery, GetEnrollmentConfirmationPdfPreviewResponse>
{
    public async Task<Result<GetEnrollmentConfirmationPdfPreviewResponse>> Handle(
        GetParentEnrollmentConfirmationPdfPreviewQuery query,
        CancellationToken cancellationToken)
    {
        var accessResult = await ParentRegistrationAccessHelper.EnsureRegistrationAccessAsync(
            context,
            userContext,
            query.RegistrationId,
            cancellationToken);

        if (!accessResult.IsSuccess)
        {
            return Result.Failure<GetEnrollmentConfirmationPdfPreviewResponse>(accessResult.Error);
        }

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
