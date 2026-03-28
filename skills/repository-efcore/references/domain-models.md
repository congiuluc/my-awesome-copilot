# Domain Models

## Entity Base Rules

- All entities must have a `string Id` property.
- Use `DateTime` in UTC for all timestamps — suffix with `Utc`.
- Add XML doc comments on all public properties.
- Keep models POCO — no framework-specific attributes in Core.
- Use `string.Empty` as default for string properties.

## IRepository Interface

```csharp
/// <summary>
/// Generic repository interface for data access operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets an entity by its unique identifier.
    /// </summary>
    Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching a predicate.
    /// </summary>
    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity and returns it.
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity and returns it.
    /// </summary>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its unique identifier.
    /// </summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity with the given ID exists.
    /// </summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of entities, optionally filtered.
    /// </summary>
    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);
}
```

## Entity Example

```csharp
/// <summary>
/// Represents a product entry in the system.
/// </summary>
public class Product
{
    /// <summary>
    /// Unique identifier for the product.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the product.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Scheduled draw date in UTC.
    /// </summary>
    public DateTime DrawDateUtc { get; set; }

    /// <summary>
    /// Current status of the product.
    /// </summary>
    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    /// <summary>
    /// UTC timestamp when the product was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp when the product was last modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }
}
```

## General Rules

- Never expose `DbContext` or `CosmosClient` outside Infrastructure.
- All repository methods must accept `CancellationToken`.
- Use `IReadOnlyList<T>` for collection returns — prevent mutation by callers.
- Generate IDs in the repository layer (GUID) if not provided by the caller.

## Official References

- [Repository Pattern (Microsoft)](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Domain-Driven Design](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/)
