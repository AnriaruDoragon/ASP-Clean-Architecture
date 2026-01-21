using Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Web.API.Authorization.Requirements;

/// <summary>
/// Authorization requirement for role-based access.
/// </summary>
public sealed class RoleRequirement(params Role[] roles) : IAuthorizationRequirement
{
    public Role[] Roles { get; } = roles;
}
