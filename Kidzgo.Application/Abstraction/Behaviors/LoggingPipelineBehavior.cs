using Kidzgo.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.Abstraction.Behaviors;

internal sealed class LoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;

        logger.LogInformation("Processing request {RequestName}", requestName);

        try
        {
            TResponse result = await next();

            if (result.IsSuccess)
            {
                logger.LogInformation("Completed request {RequestName}", requestName);
            }

            else
            {
                logger.LogWarning(
                    "Completed request {RequestName} with application error {ErrorCode}: {ErrorDescription}",
                    requestName,
                    result.Error.Code,
                    result.Error.Description);
            }

            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation(
                "Request {RequestName} was canceled by the client or upstream caller",
                requestName);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception while processing request {RequestName}", requestName);
            throw;
        }
    }
}
