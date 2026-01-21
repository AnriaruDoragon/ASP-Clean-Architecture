using Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Web.API.Authorization;

/// <summary>
/// Requires the user to have at least one of the specified roles.
/// Roles are checked against the database, not JWT claims.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireRoleAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "RequireRole_";

    public RequireRoleAttribute(params Role[] roles)
    {
        // Create policy name from roles
        Policy = PolicyPrefix + string.Join(",", roles.Select(r => (int)r));
    }
}
