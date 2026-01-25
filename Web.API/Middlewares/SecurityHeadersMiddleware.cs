namespace Web.API.Middlewares;

/// <summary>
/// Middleware that adds essential security headers for API responses.
/// Focused on headers relevant for APIs (not HTML-serving applications).
/// </summary>
public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Prevent MIME type sniffing
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // Control referrer information leakage
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Prevent caching of sensitive responses (can be overridden per-endpoint)
        context.Response.Headers["Cache-Control"] = "no-store";
        context.Response.Headers["Pragma"] = "no-cache";

        await next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.UseMiddleware<SecurityHeadersMiddleware>();
}
