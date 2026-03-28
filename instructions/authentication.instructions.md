---
description: "Use when adding authentication, authorization, JWT validation, policies, or row-level access control to .NET API endpoints."
applyTo: "src/MyApp.Api/Program.cs,src/MyApp.Api/Auth/**,src/MyApp.Core/Auth/**,src/MyApp.Api/Endpoints/**"
---
# Authentication & Authorization Guidelines

## JWT Setup

- Configure JWT bearer in `Program.cs` — load `Authority` and `Audience` from configuration.
- Set `ClockSkew` to 1-2 minutes max.
- Never hardcode secrets — use environment variables or Azure Key Vault.
- Validate issuer, audience, and lifetime on every token.

## Authorization Policies

- Define policies centrally in an extension method, not scattered across endpoints.
- Use `IAuthorizationHandler` for custom requirement logic (e.g., resource ownership).
- Default to deny — explicitly grant access with `.RequireAuthorization()`.

## Row-Level Access Control

| Action | Owner | Shared User | Anonymous |
|--------|-------|-------------|-----------|
| Read | ✅ | ✅ (if shared) | ❌ |
| Update | ✅ | ❌ | ❌ |
| Delete | ✅ | ❌ | ❌ |

- Always filter queries by `OwnerId` or explicit sharing records.
- Verify ownership server-side — never trust client-provided user IDs.
- Use `IOwnedResource` interface for consistent ownership patterns.

## Endpoint Patterns

- `RequireAuthorization()` — any authenticated user.
- `RequireAuthorization("Owner")` — only the resource owner.
- No `[AllowAnonymous]` on data mutation endpoints.
- Extract user identity via `ClaimsPrincipal` extensions, not raw header parsing.

## Claims

- Use `ClaimTypes.NameIdentifier` for user ID.
- Create extension methods for common claims extraction.
- Throw `UnauthorizedException` if required claims are missing.
