using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Web.API.Authorization.Requirements;

namespace Web.API.Authorization.Handlers;

/// <summary>
/// Handles email verification authorization by checking the database.
/// Use this as a template for creating similar custom guards.
/// </summary>
public sealed class EmailVerifiedAuthorizationHandler(IServiceScopeFactory scopeFactory)
    : AuthorizationHandler<EmailVerifiedRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EmailVerifiedRequirement requirement
    )
    {
        Claim? userIdClaim =
            context.User.FindFirst(ClaimTypes.NameIdentifier) ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            context.Fail();
            return;
        }

        using IServiceScope scope = scopeFactory.CreateScope();
        IApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        bool isEmailVerified = await dbContext
            .Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.EmailVerified)
            .FirstOrDefaultAsync();

        if (isEmailVerified)
            context.Succeed(requirement);
        else
            context.Fail();
    }
}
