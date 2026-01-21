using Microsoft.AspNetCore.Authorization;

namespace Web.API.Authorization.Requirements;

/// <summary>
/// Authorization requirement for email verification.
/// </summary>
public sealed class EmailVerifiedRequirement : IAuthorizationRequirement;
