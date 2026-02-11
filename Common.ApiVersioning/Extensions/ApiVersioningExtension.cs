using System.Net;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Common.ApiVersioning.Configs;
using Common.ApiVersioning.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace Common.ApiVersioning.Extensions;

/// <summary>
/// Configures API versioning, OpenAPI document generation, and Scalar UI integration for ASP.NET Core applications.
/// </summary>
/// <remarks>
/// <para>
/// This class provides extension methods that combine ASP.NET Core API Versioning with OpenAPI 3.0
/// and Scalar documentation UI. It reads configuration from appsettings.json and automatically
/// generates versioned OpenAPI documents with proper metadata.
/// </para>
/// <para>
/// Typical usage in Program.cs:
/// </para>
/// <code>
/// // In ConfigureServices
/// builder.Services.AddApiVersioningServices(builder.Configuration);
///
/// // In Configure pipeline
/// app.UseScalarApiReference();
/// </code>
/// </remarks>
public static class ApiVersioningExtension
{
    /// <summary>
    /// Registers API versioning services, OpenAPI document generation, and configuration binding.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">
    /// Application configuration containing the "ApiVersioning" section.
    /// If the section is missing or invalid, a default v1.0.0 Active version is created.
    /// </param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method performs the following setup:
    /// </para>
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///     Binds <see cref="ApiVersionConfiguration"/> from appsettings.json "ApiVersioning" section
    ///     and registers it as a singleton service
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     Configures ASP.NET Core API Versioning to read versions from the "x-api-version" header
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     Enables <see cref="VersionByNamespaceConvention"/> for automatic version detection from controller namespaces
    ///     (e.g., Controllers.V1 → version 1)
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     Generates OpenAPI 3.0 documents for each configured version with metadata from <see cref="ApiVersionInfo"/>
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Configuration Example (appsettings.json):
    /// </para>
    /// <code>
    /// {
    ///   "ApiVersioning": {
    ///     "ScalarTitle": "My API Documentation",
    ///     "Versions": [
    ///       {
    ///         "Name": "v1",
    ///         "Version": "1.0.0",
    ///         "Status": "Active",
    ///         "Title": "API v1",
    ///         "Description": "Current production version"
    ///       }
    ///     ]
    ///   }
    /// }
    /// </code>
    /// <para>
    /// API Versioning Configuration:
    /// </para>
    /// <list type="bullet">
    ///   <item>Versions are read from the "x-api-version" request header</item>
    ///   <item>When no version is specified, the default version from configuration is used</item>
    ///   <item>Invalid versions return HTTP 400 Bad Request</item>
    ///   <item>All supported versions are reported in the "api-supported-versions" response header</item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown during validation if configuration is invalid (see <see cref="ApiVersionConfiguration.Validate"/>).
    /// </exception>
    /// <seealso cref="UseScalarApiReference"/>
    /// <seealso cref="ApiVersionConfiguration"/>
    public static IServiceCollection AddApiVersioningServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        ApiVersionConfiguration? apiVersionConfiguration = configuration.GetSection("ApiVersioning").Get<ApiVersionConfiguration>();
        if (apiVersionConfiguration is null)
        {
            apiVersionConfiguration = new ApiVersionConfiguration();
            apiVersionConfiguration.Validate();
        }

        services.AddSingleton(apiVersionConfiguration);

        services
            .AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.DefaultApiVersion = apiVersionConfiguration.DefaultVersion.ApiVersion;
                options.UnsupportedApiVersionStatusCode = (int)HttpStatusCode.BadRequest;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader("x-api-version")
                );
            })
            .AddMvc(options =>
            {
                options.Conventions.Add(new VersionByNamespaceConvention());
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = false;
            });

        // Register FluentValidation OpenAPI transformers for DI resolution
        services.AddSingleton<FluentValidationSchemaTransformer>();
        services.AddSingleton<FluentValidationOperationTransformer>();

        foreach (ApiVersionInfo apiVersionInfo in apiVersionConfiguration.Versions)
        {
            services.AddOpenApi(apiVersionInfo.Name, options =>
            {
                options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;

                options.AddDocumentTransformer((document, _, _) =>
                {
                    document.Info.Version = $"v{apiVersionInfo.Version} ({apiVersionInfo.Status})";
                    document.Info.Title = apiVersionInfo.Title ?? "API";
                    document.Info.Description = apiVersionInfo.Description ?? string.Empty;

                    return Task.CompletedTask;
                });

                // Fix missing numeric types (.NET 10 omits 'type' for int, double, etc.)
                options.AddSchemaTransformer<NumericTypeSchemaTransformer>();

                // Add FluentValidation rules to OpenAPI schemas (request bodies)
                options.AddSchemaTransformer<FluentValidationSchemaTransformer>();

                // Add FluentValidation rules to OpenAPI parameters (query/route)
                options.AddOperationTransformer<FluentValidationOperationTransformer>();
            });
        }

        return services;
    }

    /// <summary>
    /// Maps OpenAPI document endpoints and configures Scalar API documentation UI.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <returns>The application builder for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method sets up the middleware pipeline to server:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///     OpenAPI JSON documents at <c>/openapi/{version}.json</c> for each configured version
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     Scalar interactive documentation UI at <c>/scalar/v1</c> (or the default Scalar route)
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Versions in the Scalar UI dropdown are ordered by semantic version (newest first):
    /// Major (descending) → Minor (descending) → Patch (descending)
    /// </para>
    /// <para>
    /// Some Scalar UI options can be configured with <see cref="ApiVersionConfiguration.Scalar"/>.
    /// </para>
    /// <para>
    /// <strong>Prerequisites:</strong> Must call <see cref="AddApiVersioningServices"/> first.
    /// </para>
    /// </remarks>
    /// <example>
    /// Typical Program.cs setup:
    /// <code>
    /// var app = builder.Build();
    ///
    /// if (app.Environment.IsDevelopment())
    /// {
    ///     app.UseScalarApiReference();
    /// }
    ///
    /// app.MapControllers();
    /// app.Run();
    /// </code>
    /// </example>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <see cref="AddApiVersioningServices"/> was not called,
    /// causing <see cref="ApiVersionConfiguration"/> to be unavailable in DI.
    /// </exception>
    /// <seealso cref="AddApiVersioningServices"/>
    public static IApplicationBuilder UseScalarApiReference(this IApplicationBuilder app)
    {
        return app.UseRouting().UseEndpoints(endpoints =>
        {
            ApiVersionConfiguration apiVersionConfiguration =
                app.ApplicationServices.GetRequiredService<ApiVersionConfiguration>();

            endpoints.MapOpenApi();

            endpoints.MapScalarApiReference(options =>
            {
                ScalarConfiguration scalarConfiguration = apiVersionConfiguration.Scalar;

                options.Title = scalarConfiguration.Title;
                options.Theme = scalarConfiguration.Theme;
                options.Layout = scalarConfiguration.Layout;
                options.Servers = scalarConfiguration.Servers;

                options.AddDocuments(
                    apiVersionConfiguration.Versions
                        .OrderByDescending(v => v.MajorVersion)
                        .ThenByDescending(v => v.MinorVersion)
                        .ThenByDescending(v => v.PatchVersion)
                        .Select(v => v.Name)
                        .ToArray()
                );
            });
        });
    }
}
