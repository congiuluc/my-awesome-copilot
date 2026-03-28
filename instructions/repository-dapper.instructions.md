---
description: "Use when implementing, modifying, or reviewing data access code using Dapper — including IRepository interface, raw SQL queries, stored procedures, multi-mapping, bulk operations, or IDbConnectionFactory."
applyTo: "src/MyApp.Core/**/IRepository*,src/MyApp.Infrastructure/**/Repositories/Dapper/**"
---
# Dapper Repository Guidelines

## Interface Definition (Core)

Use the same generic `IRepository<T>` from Core (shared with EF Core):

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

## Dapper Implementation

- Use **Dapper** micro-ORM with raw SQL queries.
- Place in `MyApp.Infrastructure/Repositories/Dapper/`.
- Use `IDbConnectionFactory` for creating connections — enables testability.
- Always use parameterized queries — never concatenate user input.
- Use `CommandDefinition` to pass `CancellationToken`.

```csharp
public class DapperProductRepository : IRepository<Product>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DapperProductRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT Id, Name, Description, Price, CreatedAtUtc, UpdatedAtUtc FROM Products WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Product>(command);
    }
    // ... other methods
}
```

## DI Registration

```csharp
public static IServiceCollection AddDapperRepositories(
    this IServiceCollection services,
    IConfiguration config)
{
    services.AddSingleton<IDbConnectionFactory>(
        new SqlServerConnectionFactory(config.GetConnectionString("DefaultConnection")!));

    services.AddScoped<IRepository<Product>, DapperProductRepository>();
    return services;
}
```

## Schema Management

Unlike EF Core, Dapper does not manage schema. Use SQL migration scripts or tools like DbUp or FluentMigrator.

## Domain Models (Core)

- All entities must have a `string Id` property.
- Use `DateTime` in UTC for all timestamps.
- Add XML doc comments on all public properties.
- Keep models POCO — no framework-specific attributes in Core.
