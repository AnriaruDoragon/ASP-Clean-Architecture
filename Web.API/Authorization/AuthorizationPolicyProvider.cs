using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Web.API.Authorization.Requirements;

namespace Web.API.Authorization;

/// <summary>
/// Custom policy provider that creates policies dynamically for role-based authorization.
/// </summary>
public sealed class AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if it's a role-based policy
        if (policyName.StartsWith(RequireRoleAttribute.PolicyPrefix))
        {
            string rolesString = policyName[RequireRoleAttribute.PolicyPrefix.Length..];
            Role[] roles =
            [
                .. rolesString.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(r => (Role)int.Parse(r)),
            ];

            AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new RoleRequirement(roles))
                .Build();

            return policy;
        }

        // Check if it's the email verified policy
        if (policyName == EmailVerifiedAttribute.PolicyName)
        {
            AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new EmailVerifiedRequirement())
                .Build();

            return policy;
        }

        // Fall back to default provider
        return await base.GetPolicyAsync(policyName);
    }
}
