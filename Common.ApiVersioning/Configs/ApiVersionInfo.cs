using Common.ApiVersioning.Enums;

namespace Common.ApiVersioning.Configs;

/// <summary>
/// Represents metadata and lifecycle information for a single API version.
/// </summary>
/// <remarks>
/// <para>
/// This class captures all information needed to document, version, and manage the lifecycle
/// of an API version including its identifier, semantic version, status, and deprecation timeline.
/// </para>
/// <para>
/// The <see cref="Name"/> property serves as the unique identifier and must match the controller
/// namespace suffix (e.g., Controllers.V1 → Name = "v1").
/// </para>
/// <para>
/// Semantic versioning is enforced via the <see cref="Version"/> property, though only the
/// Major version component is used for API routing and grouping.
/// </para>
/// </remarks>
/// <example>
/// Example configuration:
/// <code>
/// {
///   "Name": "v1",
///   "Version": "1.2.3",
///   "Status": "Deprecated",
///   "DeprecationDate": "2025-01-15T00:00:00Z",
///   "SunsetDate": "2025-06-01T00:00:00Z",
///   "Title": "API v1 - Legacy",
///   "Description": "Original API version, deprecated in favor of v2"
/// }
/// </code>
/// </example>
public class ApiVersionInfo
{
    /// <summary>
    /// Gets or initializes the unique identifier for this API version.
    /// </summary>
    /// <value>
    /// A string identifier (typically "v1", "v2", etc.) that matches both the OpenAPI document name
    /// and the controller namespace suffix.
    /// </value>
    /// <remarks>
    /// <para>
    /// This value must match your controller namespace convention. For example:
    /// </para>
    /// <list type="bullet">
    ///   <item><c>Name = "v1"</c> → namespace <c>YourApi.Controllers.V1</c></item>
    ///   <item><c>Name = "v2"</c> → namespace <c>YourApi.Controllers.V2</c></item>
    /// </list>
    /// <para>
    /// Used as the GroupName in API Explorer and the document identifier in OpenAPI/Scalar UI.
    /// </para>
    /// </remarks>
    /// <example>v1</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the semantic version string in Major.Minor.Patch format.
    /// </summary>
    /// <value>
    /// A version string following semantic versioning (e.g., "1.2.3", "2.0.0").
    /// Only the Major component is required; Minor and Patch default to 0 if omitted.
    /// </value>
    /// <remarks>
    /// <para>
    /// While the full semantic version is stored and displayed, only the <see cref="MajorVersion"/>
    /// is used for API routing and grouping. This allows you to track detailed versioning for
    /// documentation purposes while maintaining simple URL-based versioning.
    /// </para>
    /// <para>
    /// Examples of valid version strings: "1", "1.0", "1.0.0", "2.3.5"
    /// </para>
    /// </remarks>
    /// <example>1.2.3</example>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the current lifecycle status of this API version.
    /// </summary>
    /// <value>
    /// A <see cref="VersionStatus"/> enum value indicating the version's lifecycle stage.
    /// </value>
    /// <remarks>
    /// <para>
    /// The status affects how the API behaves:
    /// </para>
    /// <list type="bullet">
    ///   <item><term>Active/Current</term><description>Fully supported, recommended for new development</description></item>
    ///   <item><term>Beta/Alpha/Preview</term><description>Available but may change, not production-ready</description></item>
    ///   <item><term>Deprecated</term><description>Still functional but discouraged, will be sunset soon</description></item>
    ///   <item><term>Sunset/Retired/Obsolete</term><description>Returns HTTP 410 Gone, no longer accepts requests</description></item>
    /// </list>
    /// <para>
    /// See <see cref="Middlewares.ApiVersioningDeprecationMiddleware"/> for enforcement details.
    /// </para>
    /// </remarks>
    /// <example><see cref="VersionStatus.Active"/></example>
    public VersionStatus Status { get; init; }

    /// <summary>
    /// Gets or initializes the date when this API version was or will be officially deprecated.
    /// </summary>
    /// <value>
    /// A UTC datetime when deprecation begins, or <c>null</c> if not applicable.
    /// </value>
    /// <remarks>
    /// <para>
    /// When set for non-deprecated versions, this date is included in the "Deprecation" HTTP header
    /// to inform clients of planned deprecation (RFC 8594).
    /// </para>
    /// <para>
    /// For versions with <see cref="Status"/> = <see cref="VersionStatus.Deprecated"/>,
    /// this date should be in the past.
    /// </para>
    /// </remarks>
    /// <example>2025-02-02T00:00:00Z</example>
    public DateTime? DeprecationDate { get; init; }

    /// <summary>
    /// Gets or initializes the date when this API version will cease to function (end-of-life).
    /// </summary>
    /// <value>
    /// A UTC datetime when the version reaches EOL, or <c>null</c> if not scheduled.
    /// </value>
    /// <remarks>
    /// <para>
    /// When set, this date is included in the "Sunset" HTTP header (RFC 8594) to inform clients
    /// when the API will stop accepting requests.
    /// </para>
    /// <para>
    /// After this date passes, the version should have a sunset status
    /// (<see cref="VersionStatus.Sunset"/>, <see cref="VersionStatus.Retired"/>, or <see cref="VersionStatus.Obsolete"/>)
    /// and requests will receive HTTP 410 Gone.
    /// </para>
    /// </remarks>
    /// <example>2025-06-01T00:00:00Z</example>
    public DateTime? SunsetDate { get; init; }

    /// <summary>
    /// Gets or initializes the human-readable title for this version's API documentation.
    /// </summary>
    /// <value>
    /// A descriptive title shown in the Scalar UI and OpenAPI document, or <c>null</c> to use a default.
    /// </value>
    /// <remarks>
    /// This appears as the document title in Scalar's version selector dropdown.
    /// If not provided, defaults to "API".
    /// </remarks>
    public string? Title { get; init; }

    /// <summary>
    /// Gets or initializes the detailed description for this version's API documentation.
    /// </summary>
    /// <value>
    /// A markdown-formatted description displayed in the Scalar UI, or <c>null</c> for no description.
    /// </value>
    /// <remarks>
    /// <para>
    /// Supports markdown formatting for rich documentation.
    /// Use this to explain what's new, what changed, migration guides, etc.
    /// </para>
    /// </remarks>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the major version component extracted from <see cref="Version"/>.
    /// </summary>
    /// <value>The major version number, or 1 if parsing fails.</value>
    /// <remarks>
    /// This is the primary version identifier used for routing and API grouping.
    /// </remarks>
    public int MajorVersion => int.TryParse(Version.Split('.')[0], out int majorVersion) ? majorVersion : 1;

    /// <summary>
    /// Gets the minor version component extracted from <see cref="Version"/>.
    /// </summary>
    /// <value>The minor version number, or 0 if not specified or parsing fails.</value>
    public int MinorVersion => int.TryParse(Version.Split('.')[1], out int minorVersion) ? minorVersion : 0;

    /// <summary>
    /// Gets the patch version component extracted from <see cref="Version"/>.
    /// </summary>
    /// <value>The patch version number, or 0 if not specified or parsing fails.</value>
    public int PatchVersion => int.TryParse(Version.Split('.')[2], out int patchVersion) ? patchVersion : 0;

    /// <summary>
    /// Gets an <see cref="Asp.Versioning.ApiVersion"/> instance constructed from this version info.
    /// </summary>
    /// <value>
    /// An ApiVersion object using <see cref="MajorVersion"/>, <see cref="MinorVersion"/>,
    /// and <see cref="Status"/> as the version string.
    /// </value>
    /// <remarks>
    /// This is used for compatibility with ASP.NET Core API Versioning library.
    /// </remarks>
    public Asp.Versioning.ApiVersion ApiVersion => new(MajorVersion, MinorVersion);

    /// <summary>
    /// Gets a value indicating whether this version is in deprecated status.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Status"/> equals <see cref="VersionStatus.Deprecated"/>; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Deprecated versions still function but emit deprecation warnings via HTTP headers.
    /// </remarks>
    public bool IsDeprecated => Status == VersionStatus.Deprecated;

    /// <summary>
    /// Gets a value indicating whether this version has reached end-of-life and no longer accepts requests.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Status"/> is Sunset, Retired, or Obsolete; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Sunset versions return HTTP 410 Gone and do not process requests.
    /// See <see cref="Middlewares.ApiVersioningDeprecationMiddleware"/> for enforcement.
    /// </remarks>
    public bool IsSunset => Status is (VersionStatus.Sunset or VersionStatus.Retired or VersionStatus.Obsolete);
}
