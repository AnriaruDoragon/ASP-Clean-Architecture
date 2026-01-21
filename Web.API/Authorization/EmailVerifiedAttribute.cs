using Microsoft.AspNetCore.Authorization;

namespace Web.API.Authorization;

/// <summary>
/// Requires the user to have a verified email address.
/// Example of a custom authorization attribute - use as template for similar guards.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class EmailVerifiedAttribute : AuthorizeAttribute
{
    public const string PolicyName = "EmailVerified";

    public EmailVerifiedAttribute()
    {
        Policy = PolicyName;
    }
}
