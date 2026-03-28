---
description: "Use when implementing backend security: CORS, rate limiting, security headers, input validation, secrets management, injection prevention, and OWASP Top 10 compliance."
applyTo: "src/MyApp.Api/Middleware/**,src/MyApp.Api/Endpoints/**,src/MyApp.Api/Program.cs"
---
# Backend Security Guidelines

## Input Validation

- Validate **all input** at API boundaries — use FluentValidation.
- Validate early, fail fast — reject invalid requests before processing.
- Return `400 Bad Request` with specific field-level errors.
- Never trust client-provided IDs for authorization — always verify ownership server-side.

## Injection Prevention

- Use **parameterized queries exclusively** — never concatenate user input into SQL/NoSQL.
- Use ORM/Builders API for query construction.
- Sanitize and encode output when rendering user-provided content.

## CORS & Headers

- Configure CORS with explicit allowed origins — never use `AllowAnyOrigin()` in production.
- Enable HSTS with a minimum `max-age` of 1 year.
- Add security headers: `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`.

## Rate Limiting

- Implement rate limiting on **all public endpoints**.
- Use sliding window or token bucket algorithms.
- Return `429 Too Many Requests` with `Retry-After` header.
- Apply stricter limits on authentication endpoints.

## Secrets Management

- **Never hardcode** secrets, connection strings, or API keys in code.
- Use environment variables or Azure Key Vault.
- Rotate secrets regularly — design for zero-downtime rotation.
- Never log secrets or PII.

## Vulnerability Scanning

- Run `dotnet list package --vulnerable` in CI pipelines.
- Keep NuGet packages updated — patch security advisories promptly.
- Scan container images for known CVEs.

## Authentication & Authorization

- Enforce row-level access control — owners vs. shared users.
- Validate JWT tokens on every request, not just at login.
- Use policy-based authorization for fine-grained permissions.
