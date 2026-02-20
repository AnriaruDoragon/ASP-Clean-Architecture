namespace Web.API.Extensions;

/// <summary>
/// HTTPS configuration extensions.
/// </summary>
public static class HttpsExtensions
{
    /// <summary>
    /// Conditionally adds HTTPS redirection based on configuration.
    /// </summary>
    public static IApplicationBuilder UseConditionalHttpsRedirection(
        this IApplicationBuilder app,
        IConfiguration configuration
    )
    {
        bool enforceHttps = configuration.GetValue("Security:EnforceHttps", true);

        if (enforceHttps)
            app.UseHttpsRedirection();

        return app;
    }
}
