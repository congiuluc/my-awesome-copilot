---
description: "Use when configuring, modifying, or reviewing Swagger/OpenAPI documentation, API versioning, or endpoint metadata. Covers auto-generated docs, schema annotations, and API conventions."
applyTo: "src/MyApp.Api/**"
---
# API Documentation (Swagger/OpenAPI) Guidelines

## Setup

- Use the built-in OpenAPI support in .NET Minimal APIs.
- Register OpenAPI in `Program.cs`:

```csharp
builder.Services.AddOpenApi();
// ...
app.MapOpenApi();
```

- Include Swagger UI via `Swashbuckle` or `Scalar` for interactive exploration in development.

## Endpoint Metadata

Annotate every endpoint with summary, description, and response types:

```csharp
group.MapGet("/{id}", GetByIdAsync)
    .WithName("GetProductById")
    .WithSummary("Get a product by ID")
    .WithDescription("Retrieves a single product entry by its unique identifier.")
    .Produces<ApiResponse<ProductDto>>(StatusCodes.Status200OK)
    .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
```

## Grouping & Tags

- Use `.WithTags("Product")` on route groups to organize endpoints by feature.
- Tags map directly to sections in the Swagger UI.

## Schema Conventions

- All request/response models should be records or classes with XML doc comments — these flow into OpenAPI descriptions.
- Use `[Required]` and validation attributes on request models for schema accuracy.
- The standard `ApiResponse<T>` envelope must appear in all response schemas.

## Environment Behavior

| Environment | Swagger UI | OpenAPI JSON |
|-------------|-----------|--------------|
| Development | Enabled at `/swagger` | `/openapi/v1.json` |
| Staging | Enabled (read-only) | `/openapi/v1.json` |
| Production | Disabled | Disabled |

Conditionally enable in `Program.cs`:

```csharp
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "MyApp API"));
}
```

## Rules

- Every public endpoint must have `WithName`, `WithSummary`, and `Produces<T>` metadata.
- Keep operation IDs unique and descriptive — they're used for client code generation.
- Document all possible status codes (200, 400, 404, 500) with their response types.
- Never expose internal implementation details (stack traces, SQL) in API documentation.
