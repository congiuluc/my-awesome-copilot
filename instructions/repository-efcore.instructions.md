---
description: "Use when implementing, modifying, or reviewing data access code using Entity Framework Core — including IRepository interface, DbContext, EF Core providers (SQLite, SQL Server, PostgreSQL, Cosmos DB), migrations, or LINQ-based queries."
applyTo: "src/MyApp.Core/**/IRepository*,src/MyApp.Infrastructure/**/Repositories/EfCore/**,src/MyApp.Infrastructure/**/Repositories/Sqlite/**,src/MyApp.Infrastructure/**/Data/**"
---
# EF Core Repository Guidelines

## Interface Definition (Core)

Define a generic `IRepository<T>` in `MyApp.Core`:

```csharp
/// <summary>
/// Generic repository interface for data access operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
```

## EF Core Implementation

- Use **Entity Framework Core** with appropriate provider (SQLite, SQL Server, PostgreSQL).
- Place in `MyApp.Infrastructure/Repositories/EfCore/`.
- Use a `DbContext` scoped per request.
- Apply migrations via EF Core tooling (`dotnet ef`).
- Use `AsNoTracking()` for read-only queries.

```csharp
public class EfCoreProductRepository : IRepository<Product>
{
    private readonly AppDbContext _dbContext;

    public EfCoreProductRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
    // ... other methods
}
```

## Cosmos DB Implementation (Direct SDK)

- Use the **Azure Cosmos DB SDK** (`Microsoft.Azure.Cosmos`) for direct SDK access.
- Place in `MyApp.Infrastructure/Repositories/CosmosDb/`.
- Use `CosmosClient` as a singleton.
- Always specify partition key in queries.

## DI Registration

Switch implementations via configuration:

```csharp
public static IServiceCollection AddEfCoreRepositories(
    this IServiceCollection services,
    IConfiguration config)
{
    var provider = config.GetValue<string>("DatabaseProvider") ?? "Sqlite";

    return provider switch
    {
        "CosmosDb" => services.AddCosmosDbRepositories(config),
        "SqlServer" => services.AddSqlServerRepositories(config),
        _ => services.AddSqliteRepositories(config),
    };
}
```

## Domain Models (Core)

- All entities must have a `string Id` property.
- Use `DateTime` in UTC for all timestamps.
- Add XML doc comments on all public properties.
- Keep models POCO — no framework-specific attributes in Core.
