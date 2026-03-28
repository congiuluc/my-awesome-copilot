---
name: authentication
description: "Implement authentication and authorization for .NET Minimal API. Covers JWT bearer tokens, claims-based authorization, policy-based RBAC, row-level access control, and middleware setup. Use when: adding auth to endpoints, configuring JWT validation, creating authorization policies, implementing owner vs shared user access, or setting up identity."
argument-hint: 'Describe the auth requirement: JWT setup, policy, role check, or access control pattern.'
---

# Authentication & Authorization

## When to Use

- Adding authentication to a new or existing API
- Configuring JWT bearer token validation
- Creating authorization policies (role-based, claims-based)
- Implementing row-level access control (owner vs shared users)
- Adding `[Authorize]` or policy requirements to endpoints
- Setting up identity and user management

## Official Documentation

- [ASP.NET Core Authentication](https://learn.microsoft.com/aspnet/core/security/authentication/)
- [Policy-based Authorization](https://learn.microsoft.com/aspnet/core/security/authorization/policies)
- [JWT Bearer Authentication](https://learn.microsoft.com/aspnet/core/security/authentication/jwt)

## Key Principles

- **Authenticate at the boundary** — validate tokens in middleware, not in services.
- **Authorize with policies** — never check roles inline; define policies centrally.
- **Row-level access** — every data query must filter by ownership or explicit sharing.
- **Least privilege** — default to deny; grant access explicitly.
- **Claims are the contract** — services receive user identity through claims, not tokens.

## Procedure

### 1. JWT Configuration

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
```

- Never hardcode secrets — load from configuration or Key Vault.
- Set `ClockSkew` to a small value (1-2 minutes).

### 2. Authorization Policies

```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Owner", policy =>
        policy.Requirements.Add(new ResourceOwnerRequirement()))
    .AddPolicy("Admin", policy =>
        policy.RequireClaim("role", "admin"))
    .AddPolicy("SharedAccess", policy =>
        policy.Requirements.Add(new SharedAccessRequirement()));
```

- Define policies in a central extension method.
- Use `IAuthorizationHandler` for custom requirement logic.

### 3. Row-Level Access Control

```csharp
/// <summary>
/// Checks if the current user owns the resource or has shared access.
/// </summary>
public class ResourceOwnerHandler : AuthorizationHandler<ResourceOwnerRequirement, IOwnedResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerRequirement requirement,
        IOwnedResource resource)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (resource.OwnerId == userId)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
```

- Owners have full control (read, update, delete).
- Shared users have read-only access unless explicitly granted more.
- Always verify ownership server-side — never trust client claims alone.

### 4. Endpoint Authorization

```csharp
group.MapGet("/{id}", GetByIdAsync)
    .RequireAuthorization();

group.MapPut("/{id}", UpdateAsync)
    .RequireAuthorization("Owner");

group.MapDelete("/{id}", DeleteAsync)
    .RequireAuthorization("Owner");
```

- `RequireAuthorization()` enforces authentication.
- `RequireAuthorization("PolicyName")` enforces a specific policy.
- No anonymous endpoints for data mutation.

### 5. Claims Extraction in Services

```csharp
public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.NameIdentifier)?.Value
           ?? throw new UnauthorizedException("User ID claim missing");

    public static string GetEmail(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
}
```

- Extract claims via extension methods — keep services clean.
- Throw `UnauthorizedException` if required claims are missing.

## Anti-Patterns

- ❌ Checking `if (user.IsInRole("admin"))` inline in endpoint handlers
- ❌ Storing JWT secret keys in `appsettings.json`
- ❌ Skipping ownership checks for "convenience"
- ❌ Using `[AllowAnonymous]` on mutation endpoints
- ❌ Trusting client-provided user IDs without server-side verification
