using Common.ApiVersioning.Configs;

namespace Common.ApiVersioning.Enums;

/// <summary>
/// Defines the lifecycle stages for API versions, from pre-release through end-of-life.
/// </summary>
/// <remarks>
/// <para>
/// This enum enables fine-grained control over API version lifecycle and communication with API consumers.
/// Values are ordered roughly from earliest to latest lifecycle stages.
/// </para>
/// <para>
/// Lifecycle progression typically follows:
/// Internal → Preview/Alpha → Beta → Active/Current → Legacy → Deprecated → Sunset/Retired/Obsolete
/// </para>
/// </remarks>
public enum VersionStatus
{
    /// <summary>
    /// Internal testing only, not exposed to external clients.
    /// </summary>
    /// <remarks>
    /// Use for versions under active development that should only be accessible to internal teams.
    /// </remarks>
    Internal,

    /// <summary>
    /// Early preview release for gathering feedback. May have incomplete features.
    /// </summary>
    /// <remarks>
    /// Suitable for showcasing upcoming features to select partners or early adopters.
    /// No stability guarantees.
    /// </remarks>
    Preview,

    /// <summary>
    /// Alpha release - experimental and subject to significant changes.
    /// </summary>
    /// <remarks>
    /// Features are incomplete and APIs may change drastically.
    /// Not recommended for production use.
    /// </remarks>
    Alpha,

    /// <summary>
    /// Beta release - feature-complete but may still have bugs or minor API changes.
    /// </summary>
    /// <remarks>
    /// Suitable for testing in non-critical environments.
    /// Breaking changes should be minimized but may still occur.
    /// </remarks>
    Beta,

    /// <summary>
    /// Stable, production-ready version actively maintained and recommended for use.
    /// </summary>
    /// <remarks>
    /// This is the primary status for current production APIs.
    /// Multiple versions should not have this status simultaneously.
    /// </remarks>
    Active,

    /// <summary>
    /// Synonym for <see cref="Active"/> - the current recommended version.
    /// </summary>
    /// <remarks>
    /// Use either Active or Current (not both) to indicate the primary production version.
    /// </remarks>
    Current,

    /// <summary>
    /// Older stable version still supported but no longer recommended for new projects.
    /// </summary>
    /// <remarks>
    /// Receives security updates and critical bug fixes but no new features.
    /// Clients should plan migration to newer versions.
    /// </remarks>
    Legacy,

    /// <summary>
    /// Officially deprecated - still functional but will be sunset soon.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Triggers deprecation warnings via HTTP headers (Deprecation: true).
    /// Clients should migrate to the active version immediately.
    /// </para>
    /// <para>
    /// Should have a <see cref="ApiVersionInfo.SunsetDate"/> defined.
    /// </para>
    /// </remarks>
    Deprecated,

    /// <summary>
    /// End-of-life reached - API no longer accepts requests (HTTP 410 Gone).
    /// </summary>
    /// <remarks>
    /// All requests to this version are blocked with migration guidance.
    /// Use when the sunset date has passed.
    /// </remarks>
    Sunset,

    /// <summary>
    /// Synonym for <see cref="Sunset"/> - version has been retired from service.
    /// </summary>
    Retired,

    /// <summary>
    /// Synonym for <see cref="Sunset"/> - version is completely obsolete.
    /// </summary>
    Obsolete,
}
