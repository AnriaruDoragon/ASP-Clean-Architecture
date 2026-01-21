using Microsoft.AspNetCore.Authorization;

namespace Web.API.Authorization;

/// <summary>
/// Marks an endpoint as public (no authentication required).
/// Use this on endpoints that should be accessible without a valid JWT token.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PublicAttribute : AllowAnonymousAttribute;
