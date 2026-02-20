using System.Diagnostics;
using Microsoft.Extensions.Primitives;

namespace Web.API.Middlewares;

/// <summary>
/// Middleware that logs HTTP request and response details at Debug level.
/// Excludes health check endpoints and sanitizes sensitive headers.
/// </summary>
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    private static readonly HashSet<string> s_excludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/health/live",
        "/health/ready",
    };

    private static readonly HashSet<string> s_sensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "X-API-Key",
        "X-Auth-Token",
    };

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for excluded paths
        if (s_excludedPaths.Contains(context.Request.Path.Value ?? string.Empty))
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        // Log request
        if (logger.IsEnabled(LogLevel.Debug))
            LogRequest(context.Request);

        await next(context);

        stopwatch.Stop();

        // Log response
        if (logger.IsEnabled(LogLevel.Debug))
            LogResponse(context.Response, stopwatch.ElapsedMilliseconds);
    }

    private void LogRequest(HttpRequest request)
    {
        Dictionary<string, string> headers = SanitizeHeaders(request.Headers);

        logger.LogDebug(
            "HTTP {Method} {Path}{QueryString} - Headers: {Headers}",
            request.Method,
            request.Path,
            request.QueryString,
            headers
        );
    }

    private void LogResponse(HttpResponse response, long elapsedMs)
    {
        logger.LogDebug("HTTP Response {StatusCode} in {ElapsedMs}ms", response.StatusCode, elapsedMs);
    }

    private static Dictionary<string, string> SanitizeHeaders(IHeaderDictionary headers)
    {
        var sanitized = new Dictionary<string, string>();

        foreach (KeyValuePair<string, StringValues> header in headers)
            sanitized[header.Key] = s_sensitiveHeaders.Contains(header.Key) ? "[REDACTED]" : header.Value.ToString();

        return sanitized;
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app) =>
        app.UseMiddleware<RequestLoggingMiddleware>();
}
