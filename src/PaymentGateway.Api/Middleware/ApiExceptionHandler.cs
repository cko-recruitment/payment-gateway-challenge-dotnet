using System.Diagnostics;

using Microsoft.AspNetCore.Diagnostics;

namespace PaymentGateway.Api.Middleware;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Unhandled exception processing {Path}. Trace {TraceId}",
            httpContext.Request.Path,
            Activity.Current?.TraceId.ToString());

        await Results.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "An unexpected error occurred.",
            instance: httpContext.Request.Path
        ).ExecuteAsync(httpContext);

        return true;
    }
}