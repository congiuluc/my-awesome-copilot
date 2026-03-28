---
name: backend-dotnet
description: "Build .NET 10 Minimal API backend code following Clean Architecture. Use when: writing C# endpoints, services, middleware, DI registration, Serilog logging, configuration, health checks, or API response envelope pattern."
argument-hint: 'Describe the endpoint, service, or middleware to implement.'
---

# Backend .NET Minimal API

## When to Use

- Creating or modifying C# backend code following Clean Architecture
- Scaffolding new endpoints, services, or middleware
- Configuring DI, logging, or health checks
- Reviewing backend code for best practices

## Official Documentation

- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Dependency Injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Serilog Documentation](https://serilog.net/)
- [Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration)
- [FluentValidation](https://docs.fluentvalidation.net/)

## Procedure

1. Identify the Clean Architecture layer (Core / Infrastructure / Api)
2. Follow the patterns in [.NET guidelines](./references/dotnet-guidelines.md)
3. Apply the [code style rules](./references/code-style.md)
4. Review [sample endpoint](./samples/endpoint-sample.cs) for complete pattern
5. Wire up DI using extension methods per feature
6. Ensure all I/O is async with `CancellationToken`
7. Wrap all responses in `ApiResponse<T>` envelope
8. Add XML doc comments to all public members
9. Create corresponding tests (see `testing` skill)
