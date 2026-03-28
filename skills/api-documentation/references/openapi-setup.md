# OpenAPI Setup & Patterns

## Registration

```csharp
// Program.cs
builder.Services.AddOpenApi();

// After building the app
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "API Documentation";
    });
}
```

## Endpoint Metadata

Annotate every endpoint:

```csharp
group.MapGet("/", GetAllAsync)
    .WithName("GetAllProducts")
    .WithSummary("Get all products")
    .WithDescription("Retrieves all product entries, ordered by creation date descending.")
    .Produces<ApiResponse<IReadOnlyList<ProductDto>>>(StatusCodes.Status200OK);

group.MapGet("/{id}", GetByIdAsync)
    .WithName("GetProductById")
    .WithSummary("Get a product by ID")
    .WithDescription("Retrieves a single product entry by its unique identifier.")
    .Produces<ApiResponse<ProductDto>>(StatusCodes.Status200OK)
    .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

group.MapPost("/", CreateAsync)
    .WithName("CreateProduct")
    .WithSummary("Create a new product")
    .WithDescription("Creates a new product entry. Returns the created product.")
    .Accepts<CreateProductRequest>("application/json")
    .Produces<ApiResponse<ProductDto>>(StatusCodes.Status201Created)
    .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

group.MapPut("/{id}", UpdateAsync)
    .WithName("UpdateProduct")
    .WithSummary("Update an existing product")
    .Produces<ApiResponse<ProductDto>>(StatusCodes.Status200OK)
    .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
    .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

group.MapDelete("/{id}", DeleteAsync)
    .WithName("DeleteProduct")
    .WithSummary("Delete a product")
    .Produces(StatusCodes.Status204NoContent)
    .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
```

## Grouping & Tags

```csharp
var productGroup = app.MapGroup("/api/products")
    .WithTags("Product")
    .WithOpenApi();

var itemGroup = app.MapGroup("/api/item")
    .WithTags("Item")
    .WithOpenApi();
```

## Schema Conventions

- All request/response models should be records with XML doc comments.
- Use `[Required]` and validation attributes on request models for schema accuracy.
- The `ApiResponse<T>` envelope must appear in all response schemas.

```csharp
/// <summary>
/// Standard API response envelope.
/// </summary>
/// <typeparam name="T">The data type.</typeparam>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="Data">The response payload.</param>
/// <param name="Error">Error message if the operation failed.</param>
public record ApiResponse<T>(bool Success, T? Data, string? Error);
```

## Environment Behavior

| Environment | Swagger UI | OpenAPI JSON |
|-------------|-----------|--------------|
| Development | `/swagger` | `/openapi/v1.json` |
| Staging | `/swagger` (read-only) | `/openapi/v1.json` |
| Production | Disabled | Disabled |

## Rules

- Every public endpoint must have `WithName`, `WithSummary`, and `Produces<T>`.
- Operation IDs must be unique and descriptive (used for client code generation).
- Document all possible status codes (200, 201, 400, 404, 500) with their response types.
- Never expose internal details (stack traces, SQL, connection strings) in documentation.
- Use `WithDescription` for endpoints that need additional context.
