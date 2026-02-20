using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Web.API.Authorization.Requirements;

namespace Web.API.Authorization.Handlers;

/// <summary>
/// Handles role-based authorization by checking roles in the database.
/// This ensures role changes take effect immediately (not cached in JWT).
/// </summary>
public sealed class RoleAuthorizationHandler(IServiceScopeFactory scopeFactory) : AuthorizationHandler<RoleRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement
    )
    {
        Claim? userIdClaim =
            context.User.FindFirst(ClaimTypes.NameIdentifier) ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            context.Fail();
            return;
        }

        // Use a new scope to get a fresh DbContext
        using IServiceScope scope = scopeFactory.CreateScope();
        IApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        User? user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            context.Fail();
            return;
        }

        // Check if user has any of the required roles
        if (user.HasAnyRole(requirement.Roles))
            context.Succeed(requirement);
        else
            context.Fail();
    }
}
