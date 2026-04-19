using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Kidzgo.API.Infrastructure;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException &&
            (httpContext.RequestAborted.IsCancellationRequested || cancellationToken.IsCancellationRequested))
        {
            logger.LogInformation(
                "Request was canceled for {Method} {Path}{QueryString}. TraceId: {TraceId}. UserId: {UserId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.Request.QueryString,
                httpContext.TraceIdentifier,
                httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous");

            return true;
        }

        // Handle authorization failures
        if (exception is UnauthorizedAccessException)
        {
            logger.LogWarning(exception, "Authorization failed");

            var problemDetails = new ProblemDetails()
            {
                Status = StatusCodes.Status403Forbidden,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
                Title = "Forbidden",
                Detail = "You do not have permission to access this resource."
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        // Handle authentication failures (no token or invalid token)
        if (exception.Message.Contains("authentication", StringComparison.OrdinalIgnoreCase) ||
            exception.Message.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(exception, "Authentication failed");

            var problemDetails = new ProblemDetails()
            {
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
                Title = "Unauthorized",
                Detail = "Authentication required to access this resource."
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        logger.LogError(
            exception,
            "Unhandled exception occurred for {Method} {Path}{QueryString}. TraceId: {TraceId}. UserId: {UserId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.Request.QueryString,
            httpContext.TraceIdentifier,
            httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "anonymous");

        var errorProblemDetails = new ProblemDetails()
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Title = "Server failure"
        };

        httpContext.Response.StatusCode = errorProblemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(errorProblemDetails, cancellationToken);

        return true;
    }
}
