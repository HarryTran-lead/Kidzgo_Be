using System.Security.Claims;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace Kidzgo.API.Middlewares;

public class RequestContextLoggingMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeaderName = "Correlation-Id";

    public Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty("CorrelationId", GetCorrelationId(context)))
        using (LogContext.PushProperty("TraceIdentifier", context.TraceIdentifier))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value ?? string.Empty))
        using (LogContext.PushProperty("QueryString", context.Request.QueryString.Value ?? string.Empty))
        using (LogContext.PushProperty("UserId", GetUserId(context)))
        {
            return next.Invoke(context);
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        context.Request.Headers.TryGetValue(
            CorrelationIdHeaderName,
            out StringValues correlationId);

        return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
    }

    private static string GetUserId(HttpContext context)
    {
        return context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? "anonymous";
    }
}


