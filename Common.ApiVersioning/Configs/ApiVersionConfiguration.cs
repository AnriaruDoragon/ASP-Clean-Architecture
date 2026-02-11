using Common.ApiVersioning.Enums;

namespace Common.ApiVersioning.Configs;

/// <summary>
/// Represents the complete API versioning configuration, including all available versions and their lifecycle states.
/// Provides methods to query active versions, retrieve specific version information, and validate configuration integrity.
/// </summary>
/// <remarks>
/// <para>
/// This configuration is typically populated from the "ApiVersioning" section in <c>appsettings.json</c>.
/// If no versions are configured, a default v1.0.0 version with Active status is automatically created during validation.
/// </para>
/// <para>
/// Configuration rules enforced by <see cref="Validate"/>:
/// </para>
/// <list type="bullet">
///   <item>At least one version must have <see cref="VersionStatus.Active"/> or <see cref="VersionStatus.Current"/> status</item>
///   <item>Only one version can be Active/Current at a time</item>
///   <item>All versions must have a non-empty Name and Version</item>
///   <item>Version strings must follow semantic versioning format (Major.Minor.Patch)</item>
/// </list>
/// </remarks>
/// <example>
/// Example configuration in appsettings.json:
/// <code>
/// {
///   "ApiVersioning": {
///     "Versions": [
///       {
///         "Name": "v1",
///         "Version": "1.0.0",
///         "Status": "Active",
///         "Title": "API v1",
///         "Description": "Current stable version"
///       },
///       {
///         "Name": "v2",
///         "Version": "2.0.0",
///         "Status": "Beta",
///         "Title": "API v2 (Beta)",
///         "Description": "Next generation API"
///       }
///     ],
///     "Scalar": {
///         "Title": "My API",
///         "Theme": "Default",
///         "Layout": "Modern",
///         "Servers": [
///             {
///                 "Url": "https://prod.example.com/",
///                 "Description": "Productions server"
///             },
///             {
///                 "Url": "https://dev.example.com/"
///             }
///         ]
///     }
///   }
/// }
/// </code>
/// </example>
public class ApiVersionConfiguration
{
    /// <summary>
    /// Gets or initializes the list of all API versions available in the application.
    /// </summary>
    /// <value>
    /// A collection of <see cref="ApiVersionInfo"/> objects representing each API version.
    /// If empty at validation time, a default v1.0.0 Active version is automatically added.
    /// </value>
    /// <remarks>
    /// Versions in this list can have any lifecycle status (Active, Deprecated, Sunset, etc.).
    /// Use <see cref="GetActiveVersions"/> to filter only non-deprecated versions.
    /// </remarks>
    public List<ApiVersionInfo> Versions { get; init; } = [];

    /// <summary>
    /// Gets or initializes the Scalar UI configuration for API documentation appearance and behavior.
    /// </summary>
    /// <value>
    /// A <see cref="ScalarConfiguration"/> object containing theme, layout, and server settings.
    /// Defaults to a new instance with standard values if not configured.
    /// </value>
    /// <remarks>
    /// Controls the visual presentation of the Scalar API documentation interface including
    /// color theme, layout style, and available server environments for API testing.
    /// </remarks>
    /// <seealso cref="ScalarConfiguration"/>
    public ScalarConfiguration Scalar { get; init; } = new();

    /// <summary>
    /// Retrieves all versions that are currently active and accepting requests.
    /// </summary>
    /// <returns>
    /// An enumerable of <see cref="ApiVersionInfo"/> objects excluding Legacy, Deprecated, Sunset, Retired, and Obsolete versions.
    /// </returns>
    /// <remarks>
    /// Active versions include: Internal, Preview, Alpha, Beta, Active, and Current statuses.
    /// These versions will accept API requests without deprecation warnings.
    /// </remarks>
    public IEnumerable<ApiVersionInfo> GetActiveVersions() =>
        Versions.Where(v =>
            v.Status
                is not (
                    VersionStatus.Legacy
                    or VersionStatus.Deprecated
                    or VersionStatus.Sunset
                    or VersionStatus.Retired
                    or VersionStatus.Obsolete
                )
        );

    /// <summary>
    /// Gets the default API version to use when clients don't specify a version.
    /// </summary>
    /// <value>
    /// The first active version from <see cref="GetActiveVersions"/>.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no active version exists in the configuration.
    /// </exception>
    /// <remarks>
    /// This version is used for unversioned API requests and as the recommended migration target
    /// in deprecation warnings sent by <see cref="Middlewares.ApiVersioningDeprecationMiddleware"/>.
    /// </remarks>
    public ApiVersionInfo DefaultVersion =>
        GetActiveVersions().FirstOrDefault()
        ?? throw new InvalidOperationException("No active version defined in configuration");

    /// <summary>
    /// Retrieves version information by its unique name identifier.
    /// </summary>
    /// <param name="versionName">The version name (e.g., "v1", "v2"). Case-insensitive.</param>
    /// <returns>
    /// The <see cref="ApiVersionInfo"/> matching the specified name, or <c>null</c> if not found.
    /// </returns>
    /// <remarks>
    /// Version name matching is case-insensitive to handle variations in client requests.
    /// </remarks>
    public ApiVersionInfo? GetVersionInfo(string versionName) =>
        Versions.FirstOrDefault(v => v.Name.Equals(versionName, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Validates the configuration for correctness and applies default values if necessary.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when validation fails due to:
    /// <list type="bullet">
    ///   <item>No Active or Current version exists</item>
    ///   <item>Multiple versions marked as Active or Current</item>
    ///   <item>Empty version names</item>
    ///   <item>Empty or invalid version strings</item>
    ///   <item>Version strings not following Major.Minor.Patch format</item>
    /// </list>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method should be called after binding configuration from appsettings.json.
    /// If no versions are configured, it automatically creates a default v1.0.0 Active version.
    /// </para>
    /// <para>
    /// Called automatically by <see cref="Extensions.ApiVersioningExtension.AddApiVersioningServices"/>.
    /// </para>
    /// </remarks>
    public void Validate()
    {
        if (Versions.Count == 0)
        {
            Versions.Add(
                new ApiVersionInfo
                {
                    Name = "v1",
                    Version = "1.0.0",
                    Status = VersionStatus.Active,
                    Title = "API",
                    Description =
                        "Default API version. See ApiVersioning README.md for more information about customizing API versions.",
                }
            );
        }

        if (Versions.All(v => v.Status is not (VersionStatus.Active or VersionStatus.Current)))
            throw new InvalidOperationException("At least one version must have 'Active' or 'Current' status");

        if (Versions.Count(v => v.Status is VersionStatus.Active or VersionStatus.Current) > 1)
            throw new InvalidOperationException("Only one version can have 'Active' or 'Current' status");

        if (Versions.Any(version => string.IsNullOrWhiteSpace(version.Name)))
            throw new InvalidOperationException("Version name cannot be empty");

        if (Versions.Any(v => string.IsNullOrWhiteSpace(v.Version)))
            throw new InvalidOperationException("Version cannot be empty");

        if (Versions.Any(v => !IsValidSemanticVersion(v.Version)))
            throw new InvalidOperationException(
                "Versions must follow semantic versioning format (Major.Minor.Patch), e.g., '1.0.0'"
            );
    }

    private static bool IsValidSemanticVersion(string version)
    {
        string[] parts = version.Split('.');
        return parts.Length is >= 1 and <= 3 && parts.All(p => int.TryParse(p, out _));
    }
}
