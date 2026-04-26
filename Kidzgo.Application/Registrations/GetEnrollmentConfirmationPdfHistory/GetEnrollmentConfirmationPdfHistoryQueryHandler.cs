using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.Registrations.GetEnrollmentConfirmationPdfHistory;

public sealed class GetEnrollmentConfirmationPdfHistoryQueryHandler(
    IDbContext context,
    IFileStorageService fileStorage,
    ILogger<GetEnrollmentConfirmationPdfHistoryQueryHandler> logger)
    : IQueryHandler<GetEnrollmentConfirmationPdfHistoryQuery, GetEnrollmentConfirmationPdfHistoryResponse>
{
    public async Task<Result<GetEnrollmentConfirmationPdfHistoryResponse>> Handle(
        GetEnrollmentConfirmationPdfHistoryQuery query,
        CancellationToken cancellationToken)
    {
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
