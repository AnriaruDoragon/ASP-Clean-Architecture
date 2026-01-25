using Serilog.Context;

namespace Web.API.Middlewares;

/// <summary>
/// Middleware that ensures every request has a correlation ID for distributed tracing.
/// Reads X-Correlation-ID from request headers or generates a new GUID.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        // Get correlation ID from header or generate new one
        string correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        // Store in HttpContext.Items for access throughout request
        context.Items["CorrelationId"] = correlationId;

        // Add to response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        // Push to Serilog LogContext for structured logging
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationIdMiddleware>();
}
