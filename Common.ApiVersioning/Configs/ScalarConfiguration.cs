using Scalar.AspNetCore;

namespace Common.ApiVersioning.Configs;

/// <summary>
/// Configuration for Scalar API documentation UI appearance and behavior.
/// </summary>
/// <remarks>
/// <para>
/// This configuration controls the visual presentation and server options in the Scalar interactive API documentation.
/// All properties are optional and have sensible defaults.
/// </para>
/// <para>
/// Scalar is a modern, interactive API documentation tool that renders OpenAPI specifications.
/// Learn more at <see href="https://github.com/scalar/scalar">Scalar GitHub</see>.
/// </para>
/// </remarks>
/// <example>
/// Example configuration in appsettings.json:
/// <code>
/// {
///   "ApiVersioning": {
///     "Scalar": {
///       "Title": "My API Documentation",
///       "Theme": "Saturn",
///       "Layout": "Modern",
///       "Servers": [
///         {
///           "Url": "https://api.example.com",
///           "Description": "Production server"
///         },
///         {
///           "Url": "https://staging.example.com",
///           "Description": "Staging environment"
///         }
///       ]
///     }
///   }
/// }
/// </code>
/// </example>
public class ScalarConfiguration
{
    /// <summary>
    /// Gets or initializes the title displayed in the Scalar API documentation UI.
    /// </summary>
    /// <value>The main title shown at the top of the Scalar UI. Defaults to "API" if not specified.</value>
    /// <remarks>
    /// This is the global title for the entire API documentation interface, not per-version.
    /// Individual version titles are configured via <see cref="ApiVersionInfo.Title"/>.
    /// </remarks>
    public string Title { get; init; } = "API";

    /// <summary>
    /// Gets or initializes the color theme for the Scalar UI.
    /// </summary>
    /// <value>
    /// A <see cref="ScalarTheme"/> enum value. Defaults to <see cref="ScalarTheme.None"/> (uses Scalar's default).
    /// </value>
    /// <remarks>
    /// <para>
    /// Available themes include:
    /// </para>
    /// <list type="bullet">
    ///   <item><term>None</term><description> Default Scalar theme</description></item>
    ///   <item><term>Default</term><description> Scalar's standard theme</description></item>
    ///   <item><term>Alternate</term><description> Alternative color scheme</description></item>
    ///   <item><term>Moon</term><description> Dark theme with blue accents</description></item>
    ///   <item><term>Purple</term><description> Purple-based color scheme</description></item>
    ///   <item><term>Solarized</term><description> Based on Solarized color palette</description></item>
    ///   <item><term>BluePlanet</term><description> Blue planetary theme</description></item>
    ///   <item><term>Saturn</term><description> Saturn-inspired theme</description></item>
    ///   <item><term>Kepler</term><description> Kepler space theme</description></item>
    ///   <item><term>Mars</term><description> Mars-inspired red theme</description></item>
    ///   <item><term>DeepSpace</term><description> Dark space theme</description></item>
    ///   <item><term>Laserwave</term><description> Synthwave-inspired theme</description></item>
    /// </list>
    /// </remarks>
    public ScalarTheme Theme { get; init; } = ScalarTheme.None;

    /// <summary>
    /// Gets or initializes the layout style for the Scalar UI.
    /// </summary>
    /// <value>
    /// A <see cref="ScalarLayout"/> enum value. Defaults to <see cref="ScalarLayout.Modern"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Available layouts:
    /// </para>
    /// <list type="bullet">
    ///   <item><term>Modern</term><description> Contemporary layout with enhanced navigation and features</description></item>
    ///   <item><term>Classic</term><description> Traditional API documentation layout</description></item>
    /// </list>
    /// </remarks>
    public ScalarLayout Layout { get; init; } = ScalarLayout.Modern;

    /// <summary>
    /// Gets or initializes the list of server URLs available for testing API requests in Scalar UI.
    /// </summary>
    /// <value>
    /// A collection of <see cref="ScalarServer"/> objects representing different API environments.
    /// Defaults to an empty list.
    /// </value>
    /// <remarks>
    /// <para>
    /// Servers appear in a dropdown in the Scalar UI, allowing users to switch between
    /// different API environments (production, staging, development, etc.) when testing requests.
    /// </para>
    /// <para>
    /// Each server requires at minimum a URL. Descriptions are optional but recommended for clarity.
    /// </para>
    /// <para>
    /// If no servers are configured, Scalar will use the current request's base URL as the default.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// "Servers": [
    ///   {
    ///     "Url": "https://api.example.com",
    ///     "Description": "Production API"
    ///   },
    ///   {
    ///     "Url": "https://staging-api.example.com",
    ///     "Description": "Staging environment"
    ///   },
    ///   {
    ///     "Url": "http://localhost:5000",
    ///     "Description": "Local development"
    ///   }
    /// ]
    /// </code>
    /// </example>
    public List<ScalarServer> Servers { get; init; } = [];
}
