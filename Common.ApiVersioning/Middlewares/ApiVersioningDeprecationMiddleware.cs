using Asp.Versioning;
using Common.ApiVersioning.Configs;
using Microsoft.AspNetCore.Http;

namespace Common.ApiVersioning.Middlewares;

/// <summary>
/// Middleware that manages API version lifecycle by detecting and handling deprecated and sunset versions.
/// Adds appropriate HTTP headers to inform clients about version status and blocks requests to EOL versions.
/// </summary>
/// <remarks>
/// <para>
/// This middleware inspects the requested API version and applies the following behaviors:
/// </para>
/// <list type="bullet">
///   <item>
///     <term>Sunset (EOL) Versions</term>
///     <description> Returns HTTP 410 Gone and blocks the request with a migration message</description>
///   </item>
///   <item>
///     <term>Deprecated Versions</term>
///     <description> Adds "Deprecation: true" header and optional "Sunset" header with EOL date</description>
///   </item>
///   <item>
///     <term>Active Versions</term>
///     <description> Adds "Deprecation" header with future deprecation date if scheduled</description>
///   </item>
/// </list>
/// <para>
/// All responses include an "X-API-Version-Status" header indicating the version's current lifecycle status.
/// When deprecation or sunset is detected, an "X-API-Info" header provides migration guidance to the default version.
/// </para>
/// </remarks>
/// <example>
/// Register in the middleware pipeline:
/// <code>
/// app.UseMiddleware&lt;ApiVersioningDeprecationMiddleware&gt;();
/// // or
/// app.UseApiVersioningDeprecation();
/// </code>
/// </example>
public class ApiVersioningDeprecationMiddleware(RequestDelegate next, ApiVersionConfiguration apiVersionConfiguration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        IApiVersioningFeature? versioningFeature = context.Features.Get<IApiVersioningFeature>();

        if (versioningFeature?.RawRequestedApiVersion != null)
        {
            string versionName = $"v{versioningFeature.RequestedApiVersion?.MajorVersion ?? apiVersionConfiguration.DefaultVersion.MajorVersion}";
            ApiVersionInfo? versionInfo = apiVersionConfiguration.GetVersionInfo(versionName);

            if (versionInfo != null)
            {
                context.Response.Headers.Append("X-API-Version-Status", versionInfo.Status.ToString().ToLower());

                if (versionInfo.IsSunset)
                {
                    context.Response.StatusCode = StatusCodes.Status410Gone;
                    context.Response.ContentType = "application/problem+json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        type = "https://httpstatuses.io/410",
                        title = "Gone",
                        status = 410,
                        detail = $"API version {versionName} has reached end-of-life and no longer accepts requests.",
                        instance = context.Request.Path.Value,
                        migrateToVersion = apiVersionConfiguration.DefaultVersion.Version
                    });
                    return;
                }

                if (versionInfo.IsDeprecated)
                {
                    context.Response.Headers.Append("Deprecation", "true");
                    context.Response.Headers.Append("X-API-Info",
                        $"This API version is deprecated. Please migrate to v{apiVersionConfiguration.DefaultVersion.Version}");
                    if (versionInfo.SunsetDate.HasValue)
                        context.Response.Headers.Append("Sunset", versionInfo.SunsetDate.Value.ToString("R"));
                }
                else if (versionInfo.DeprecationDate.HasValue)
                {
                    context.Response.Headers.Append("Deprecation", versionInfo.DeprecationDate.Value.ToString("R"));
                }
            }
        }

        await next(context);
    }
}
