---
description: "Use when writing, modifying, or reviewing C# backend code. Covers .NET 10 Minimal API patterns, Clean Architecture, DI, middleware, Serilog logging, and API response conventions."
applyTo: "src/MyApp.Api/**,src/MyApp.Core/**,src/MyApp.Infrastructure/**"
---
# Backend .NET Minimal API Guidelines

## Framework & Language

- Target **.NET 10** (latest LTS). Use C# 14 features where appropriate.
- Use **Minimal API** style (`app.MapGet`, `app.MapPost`, etc.) — no controllers.
- Use `record` types for DTOs and request/response models.
- Use `readonly record struct` for small value types.

## Clean Architecture Layers

| Layer | Project | Depends On |
|-------|---------|------------|
| Domain / Core | `MyApp.Core` | Nothing |
| Infrastructure | `MyApp.Infrastructure` | Core |
| Presentation | `MyApp.Api` | Core, Infrastructure |

- Core contains: domain models, interfaces (`IRepository<T>`), DTOs, enums, exceptions.
- Infrastructure contains: repository implementations (SQLite, Cosmos DB), external service clients.
- Api contains: endpoint definitions, middleware, DI registration, configuration.

## API Response Envelope

All endpoints return a standard envelope:

```csharp
public record ApiResponse<T>(bool Success, T? Data, string? Error);
```

Return `ApiResponse` from every endpoint — never raw objects or primitives.

## Endpoint Organization

- Group related endpoints using `MapGroup()` with a common prefix.
- Place endpoint definitions in separate static classes under `Endpoints/` folder.
- Use `TypedResults` for compile-time return type checking.

```csharp
public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Product");
        group.MapGet("/", GetAllAsync);
        group.MapPost("/", CreateAsync);
        return group;
    }
}
```

## Dependency Injection

- Register services in dedicated `IServiceCollection` extension methods per feature.
- Use **scoped** for request-level services and repositories.
- Use **singleton** for configuration objects and HTTP clients.
- Use **transient** only for lightweight stateless services.

## Middleware

- Register middleware in this order: error handling → request logging → response compression → authentication → authorization → CORS → endpoints.
- Use `IExceptionHandler` for global error handling — return `ApiResponse` with error details.
- Use Serilog `RequestLoggingMiddleware` for structured request logs.

## Logging (Serilog)

- Configure Serilog in `Program.cs` with console + file sinks.
- Use structured logging: `Log.Information("Processing product {ProductId}", productId);`
- Never log sensitive data (passwords, tokens, PII).
- Use `ILogger<T>` injected via DI — avoid static `Log` calls in business logic.

## Async & Cancellation

- All I/O-bound methods must be `async` and accept `CancellationToken`.
- Pass `CancellationToken` from endpoint handlers through to repository methods.
- Use `ConfigureAwait(false)` in library code (Core, Infrastructure), not in Api layer.

## Configuration

- Use `IOptions<T>` / `IOptionsSnapshot<T>` pattern for typed settings.
- Environment-specific files: `appsettings.json`, `appsettings.Development.json`, `appsettings.Production.json`.
- Never hardcode connection strings or secrets — use environment variables or Azure Key Vault.

## Health Checks

- Register health checks for database connectivity and external dependencies.
- Map health endpoint at `/health` returning JSON with component status.

## C# Code Style

- XML doc comments on all public members.
- `camelCase` for parameters, `_camelCase` for private fields (e.g., `private readonly ILogger<T> _logger`).
- Use `#region` / `#endregion` to organize code sections.
- Max line length: 120 characters.
- File-scoped namespaces (`namespace X;`).
- Use `global using` for common namespaces.
- Primary constructors for DI injection where appropriate.
