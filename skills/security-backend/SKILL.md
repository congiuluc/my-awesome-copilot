---
name: security-backend
description: >-
  Apply backend security best practices for .NET APIs. Use when: configuring CORS,
  rate limiting, security headers, input validation with FluentValidation, SQL/NoSQL
  injection prevention, secrets management, or reviewing backend code for OWASP
  Top 10 vulnerabilities.
argument-hint: 'Describe the security concern or backend endpoint to secure.'
---

# Backend Security (.NET)

## When to Use

- Validating user input at API boundaries
- Configuring CORS, HSTS, and security headers
- Implementing rate limiting on public endpoints
- Managing secrets via environment variables or Azure Key Vault
- Preventing SQL/Cosmos DB injection
- Reviewing backend code for OWASP Top 10 vulnerabilities

## Official Documentation

- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [Rate Limiting in .NET](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/general/overview)

## Procedure

1. Validate all input at API boundaries — see [input validation reference](./references/input-validation.md)
2. Configure CORS, rate limiting, security headers — see [auth patterns reference](./references/auth-patterns.md)
3. Also review [input security reference](./references/input-security.md)
4. Review [security configuration sample](./samples/security-config-sample.cs)
5. Manage secrets via environment variables or Key Vault — never in code
6. Use parameterized queries for all database access
7. Run `dotnet list package --vulnerable` in CI
8. Review for OWASP Top 10 vulnerabilities
