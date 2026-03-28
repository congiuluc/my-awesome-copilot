# C# Code Style

## Naming Conventions

| Element | Style | Example |
|---------|-------|---------|
| Namespace | PascalCase | `MyApp.Core.Models` |
| Class / Record | PascalCase | `ProductService` |
| Interface | IPascalCase | `IProductService` |
| Method | PascalCase | `GetByIdAsync` |
| Property | PascalCase | `CreatedAtUtc` |
| Parameter | camelCase | `productId` |
| Private field | _camelCase | `_logger`, `_repository` |
| Constant | PascalCase | `MaxRetryCount` |
| Enum member | PascalCase | `ProductStatus.Active` |
| Local variable | camelCase | `productDto` |

## File Organization

```csharp
// 1. File-scoped namespace
namespace MyApp.Api.Endpoints;

// 2. Class declaration
/// <summary>
/// Handles product-related API endpoints.
/// </summary>
public static class ProductEndpoints
{
    #region Public Methods

    public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        // ...
    }

    #endregion

    #region Private Methods

    private static async Task<IResult> GetAllAsync(
        IProductService service,
        CancellationToken cancellationToken)
    {
        // ...
    }

    #endregion
}
```

## Rules

- XML doc comments (`///`) on all public members — classes, methods, properties, parameters.
- Use `#region` / `#endregion` to organize: Public Methods, Private Methods, Properties, Fields.
- Max line length: 120 characters — break long lines at commas or operators.
- File-scoped namespaces (`namespace X;`) — never block-scoped.
- Use `global using` in a `GlobalUsings.cs` file for common namespaces.
- Primary constructors for DI injection where appropriate.
- One type per file. File name matches type name.
- Prefer `var` when the type is obvious from the right-hand side.
- Use `string.Empty` over `""`.
- Use `nameof()` for parameter names in exceptions and logging.
- Use pattern matching (`is`, `switch expressions`) over type casting.
- Use collection expressions (`[1, 2, 3]`) where supported.

## Async Method Style

```csharp
/// <summary>
/// Retrieves a product by its unique identifier.
/// </summary>
/// <param name="id">The product identifier.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The product if found; otherwise null.</returns>
public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(id, nameof(id));
    return await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
}
```

## Record / DTO Style

```csharp
/// <summary>
/// Request to create a new product.
/// </summary>
/// <param name="Name">Display name of the product.</param>
/// <param name="Description">Optional description.</param>
/// <param name="DrawDate">Scheduled draw date in UTC.</param>
public record CreateProductRequest(
    string Name,
    string? Description,
    DateTime DrawDate);

/// <summary>
/// Product data returned to API consumers.
/// </summary>
public record ProductDto(
    string Id,
    string Name,
    string? Description,
    DateTime DrawDate,
    DateTime CreatedAtUtc);
```
