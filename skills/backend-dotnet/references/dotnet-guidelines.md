# .NET Minimal API Guidelines

## Framework & Language

- Target **.NET 10** (latest LTS). Use C# 14 features where appropriate.
- Use **Minimal API** style (`app.MapGet`, `app.MapPost`, etc.) — no controllers.
- Use `record` types for DTOs and request/response models.
- Use `readonly record struct` for small value types.

## Clean Architecture Layers

| Layer | Project | Depends On |
|-------|---------|------------|
| Domain / Core | `{App}.Core` | Nothing |
| Infrastructure | `{App}.Infrastructure` | Core |
| Presentation | `{App}.Api` | Core, Infrastructure |

- Core contains: domain models, interfaces (`IRepository<T>`), DTOs, enums, exceptions, validators.
- Infrastructure contains: repository implementations (SQLite, Cosmos DB), external service clients, caching.
- Api contains: endpoint definitions, middleware, DI registration, configuration, filters.

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
- Add input validation using `FluentValidation` or `IEndpointFilter`.

```csharp
public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Product")
            .RequireAuthorization();
        
        group.MapGet("/", GetAllAsync)
            .WithName("GetAllProducts")
            .WithSummary("Get all products")
            .Produces<ApiResponse<IReadOnlyList<ProductDto>>>();
        
        group.MapGet("/{id}", GetByIdAsync)
            .WithName("GetProductById")
            .Produces<ApiResponse<ProductDto>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
        
        group.MapPost("/", CreateAsync)
            .WithName("CreateProduct")
            .Produces<ApiResponse<ProductDto>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .AddEndpointFilter<ValidationFilter<CreateProductRequest>>();
        
        return group;
    }
}
```

## Dependency Injection

- Register services in dedicated `IServiceCollection` extension methods per feature.
- Use **scoped** for request-level services and repositories.
- Use **singleton** for configuration objects and HTTP clients.
- Use **transient** only for lightweight stateless services.
- Use `Keyed services` when multiple implementations of the same interface exist.

```csharp
public static class ProductServiceExtensions
{
    public static IServiceCollection AddProductServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IValidator<CreateProductRequest>, CreateProductRequestValidator>();
        return services;
    }
}
```

## Middleware Pipeline

Register middleware in this order:

```csharp
// 1. Exception handling (outermost)
app.UseExceptionHandler();
// 2. Request logging
app.UseSerilogRequestLogging();
// 3. Response compression
app.UseResponseCompression();
// 4. HTTPS redirection
app.UseHttpsRedirection();
// 5. CORS
app.UseCors();
// 6. Authentication
app.UseAuthentication();
// 7. Authorization
app.UseAuthorization();
// 8. Rate limiting
app.UseRateLimiter();
// 9. Endpoints
app.MapEndpoints();
```

## Logging (Serilog)

- Configure Serilog in `Program.cs` with console + file sinks.
- Use structured logging: `Log.Information("Processing product {ProductId}", productId);`
- Never log sensitive data (passwords, tokens, PII, connection strings).
- Use `ILogger<T>` injected via DI — avoid static `Log` calls in business logic.
- Add correlation IDs to all log entries.

```csharp
builder.Host.UseSerilog((context, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));
```

## Async & Cancellation

- All I/O-bound methods must be `async` and accept `CancellationToken`.
- Pass `CancellationToken` from endpoint handlers through to repository methods.
- Use `ConfigureAwait(false)` in library code (Core, Infrastructure), not in Api layer.
- Avoid `async void` — always return `Task` or `ValueTask`.

## Configuration

- Use `IOptions<T>` / `IOptionsSnapshot<T>` pattern for typed settings.
- Environment-specific files: `appsettings.json`, `appsettings.Development.json`, `appsettings.Production.json`.
- Never hardcode connection strings or secrets — use environment variables or Azure Key Vault.
- Validate configuration at startup with `ValidateOnStart()`.

```csharp
services.AddOptions<CosmosDbSettings>()
    .BindConfiguration("CosmosDb")
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

## Health Checks

- Register health checks for database connectivity and external dependencies.
- Map health endpoint at `/health` returning JSON with component status.
- Add readiness and liveness probes separately for Kubernetes.

```csharp
builder.Services.AddHealthChecks()
    .AddSqlite(connectionString, name: "sqlite")
    .AddCheck<CosmosDbHealthCheck>("cosmosdb");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
```

## Input Validation

- Validate all incoming requests at the API boundary.
- Use `FluentValidation` for complex validation rules.
- Use endpoint filters to apply validation automatically.
- Return `400 Bad Request` with structured error messages.

```csharp
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DrawDate).GreaterThan(DateTime.UtcNow);
    }
}
```
