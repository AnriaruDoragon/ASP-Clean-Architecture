# Authentication & Authorization

## Design Decisions

### Why JWT + Refresh Tokens?

JWTs are stateless — the server doesn't need to hit a database on every request to verify identity. But stateless means you can't revoke them. Refresh tokens solve this: short-lived access tokens (15 minutes) for speed, long-lived refresh tokens (7 days) stored in the database for revocability.

### Why Secure by Default?

Most frameworks require you to add `[Authorize]` to each endpoint. Forget one and you have an unauthenticated route in production. This template inverts the default:

```csharp
// Program.cs — fallback policy requires authentication on ALL endpoints
options.FallbackPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();
```

To make an endpoint public, you explicitly opt out:

```csharp
[Public]                      // No authentication required (alias for [AllowAnonymous])
[RequireRole(Role.Admin)]     // Requires Admin role (checked against DB, not JWT)
[EmailVerified]               // Requires verified email (checked against DB)
```

### Why Check Roles Against the Database?

JWTs carry claims set at token creation time. If you put roles in the JWT, revoking a user's admin access doesn't take effect until their token expires. This template puts only the user ID in the JWT and checks roles against the database on each request via custom authorization handlers. Role changes take effect immediately.

## Auth Endpoints

| Endpoint              | Method | Auth     | Description              |
|-----------------------|--------|----------|--------------------------|
| `/Auth/Register`      | POST   | Public   | Register new user        |
| `/Auth/Login`         | POST   | Public   | Login, returns tokens    |
| `/Auth/Refresh`       | POST   | Public   | Refresh access token     |
| `/Auth/Logout`        | POST   | Required | Revoke refresh token(s)  |
| `/Auth/Me`            | GET    | Required | Get current user profile |
| `/Auth/Sessions`      | GET    | Required | List active sessions     |
| `/Auth/Sessions/{id}` | DELETE | Required | Revoke specific session  |
| `/Auth/ForgotPassword`| POST   | Public   | Initiate password reset  |
| `/Auth/ResetPassword` | POST   | Public   | Reset password with token|
| `/Auth/SendVerificationEmail` | POST | Required | Send verification email |
| `/Auth/VerifyEmail`   | POST   | Required | Verify email with token  |

## Multi-Device Support

- Each login creates a separate refresh token (session)
- Users can be logged in on multiple devices simultaneously
- Sessions can be viewed and revoked individually
- Device name and user agent stored for identification

## JWT Configuration

Configure via environment variables or `appsettings.json`:

```bash
JWT__SECRETKEY=your-secret-key-min-32-chars
JWT__ISSUER=MyApp
JWT__AUDIENCE=MyApp
JWT__ACCESSTOKENEXPIRATIONMINUTES=15
JWT__REFRESHTOKENEXPIRATIONDAYS=7
```

The access token contains only:
- `sub` — User ID
- `jti` — Unique token ID

No roles, no email, no other claims. Authorization is checked against the database.

## ICurrentUserService

Handlers that need the current user inject `ICurrentUserService`:

```csharp
public sealed class LogoutCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        Guid userId = currentUserService.UserId; // Throws if not authenticated
        // ...
    }
}
```

`UserId` is `Guid`, not `Guid?`. If called from a non-authenticated endpoint, it throws `InvalidOperationException`, which the exception middleware converts to a 500. This is intentional: it surfaces developer mistakes loudly rather than masquerading as a user-facing 401.

You don't need to guard against null — the fallback policy guarantees authentication before the handler runs.

## Creating Custom Guards

Use `[EmailVerified]` as a template:

1. **Attribute** in `Web.API/Authorization/`:
   ```csharp
   [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
   public sealed class EmailVerifiedAttribute : AuthorizeAttribute
   {
       public EmailVerifiedAttribute() => Policy = "EmailVerified";
   }
   ```

2. **Requirement** in `Web.API/Authorization/Requirements/`:
   ```csharp
   public sealed class EmailVerifiedRequirement : IAuthorizationRequirement;
   ```

3. **Handler** in `Web.API/Authorization/Handlers/`:
   ```csharp
   public sealed class EmailVerifiedAuthorizationHandler
       : AuthorizationHandler<EmailVerifiedRequirement>
   {
       protected override async Task HandleRequirementAsync(
           AuthorizationHandlerContext context, EmailVerifiedRequirement requirement)
       {
           // Extract user ID, query database, call context.Succeed() or context.Fail()
       }
   }
   ```

4. **Register** the policy name in `AuthorizationPolicyProvider.cs`
