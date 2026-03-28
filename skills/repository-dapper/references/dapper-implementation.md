# Dapper Implementation

## Setup

- Install `Dapper` NuGet package.
- Install the ADO.NET provider for your database:
  - `Microsoft.Data.SqlClient` — SQL Server
  - `Npgsql` — PostgreSQL
  - `Microsoft.Data.Sqlite` — SQLite
  - `MySqlConnector` — MySQL/MariaDB
- Place in `{App}.Infrastructure/Repositories/Dapper/`.
- Use `IDbConnectionFactory` for creating connections — enables testability.

## Connection Factory

```csharp
/// <summary>
/// Factory for creating database connections.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates a new database connection.
    /// </summary>
    IDbConnection CreateConnection();
}

/// <summary>
/// SQL Server connection factory.
/// </summary>
public class SqlServerConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlServerConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}

/// <summary>
/// PostgreSQL connection factory.
/// </summary>
public class PostgreSqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public PostgreSqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}

/// <summary>
/// SQLite connection factory.
/// </summary>
public class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public IDbConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}
```

## Repository

```csharp
/// <summary>
/// Dapper repository implementation for Product.
/// </summary>
public class DapperProductRepository : IRepository<Product>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DapperProductRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT Id, Name, Description, Price, CreatedAtUtc, UpdatedAtUtc FROM Products WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Product>(command).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT Id, Name, Description, Price, CreatedAtUtc, UpdatedAtUtc FROM Products ORDER BY CreatedAtUtc DESC";

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        var result = await connection.QueryAsync<Product>(command).ConfigureAwait(false);
        return result.AsList().AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> FindAsync(
        Expression<Func<Product, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // NOTE: Dapper does not natively support Expression<Func<T, bool>>.
        // For Dapper repositories, prefer adding purpose-specific query methods
        // (e.g., FindByStatusAsync, FindByPriceRangeAsync) instead.
        // This implementation uses a simple fallback approach.
        var allItems = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        var compiled = predicate.Compile();
        return allItems.Where(compiled).ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = Guid.NewGuid().ToString();
        }
        entity.CreatedAtUtc = DateTime.UtcNow;

        const string sql = """
            INSERT INTO Products (Id, Name, Description, Price, CreatedAtUtc, UpdatedAtUtc)
            VALUES (@Id, @Name, @Description, @Price, @CreatedAtUtc, @UpdatedAtUtc)
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, entity, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command).ConfigureAwait(false);
        return entity;
    }

    /// <inheritdoc />
    public async Task<Product> UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAtUtc = DateTime.UtcNow;

        const string sql = """
            UPDATE Products
            SET Name = @Name, Description = @Description, Price = @Price, UpdatedAtUtc = @UpdatedAtUtc
            WHERE Id = @Id
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, entity, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command).ConfigureAwait(false);
        return entity;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Products WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(1) FROM Products WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        var count = await connection.ExecuteScalarAsync<int>(command).ConfigureAwait(false);
        return count > 0;
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        Expression<Func<Product, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        // For simple count without predicate, use direct SQL
        if (predicate is null)
        {
            const string sql = "SELECT COUNT(*) FROM Products";
            using var connection = _connectionFactory.CreateConnection();
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
            return await connection.ExecuteScalarAsync<int>(command).ConfigureAwait(false);
        }

        // For filtered counts, prefer adding specific count methods.
        // Fallback: fetch all and count in-memory.
        var allItems = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        return allItems.Count(predicate.Compile());
    }
}
```

## SQL Schema Management

Unlike EF Core, Dapper does not manage schema. Use one of these approaches:

### Option 1: SQL Migration Scripts

```
migrations/
├── 001_CreateProductsTable.sql
├── 002_AddPriceColumn.sql
└── 003_AddIndexOnStatus.sql
```

```sql
-- 001_CreateProductsTable.sql
CREATE TABLE IF NOT EXISTS Products (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    Price DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedAtUtc DATETIME NOT NULL,
    UpdatedAtUtc DATETIME
);
```

### Option 2: DbUp or FluentMigrator

```csharp
// Using DbUp
var upgrader = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
    .LogToConsole()
    .Build();

upgrader.PerformUpgrade();
```

## Performance Best Practices

- Use `const string sql` for query text — avoids string allocation per call.
- Use `QuerySingleOrDefaultAsync` instead of `QueryFirstOrDefaultAsync` only when you expect 0 or 1 results.
- Use `CommandDefinition` to pass `CancellationToken`.
- Use `Buffered = false` for streaming large result sets.
- Use bulk operations with `Execute` + collections for batch inserts.
- Connections are pooled by ADO.NET — `using` disposes back to pool, not closed.

## Official References

- [Dapper Documentation](https://www.learndapper.com/)
- [Dapper GitHub](https://github.com/DapperLib/Dapper)
- [ADO.NET Connection Pooling](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-connection-pooling)
