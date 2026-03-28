# SQLite Connection Setup

## Connection String

```csharp
// Basic connection string
"Data Source=myapp.db"

// With shared cache (for concurrent connections in the same process)
"Data Source=myapp.db;Cache=Shared"

// In-memory database (lost when connection closes)
"Data Source=:memory:"

// In-memory shared between connections (useful for testing)
"Data Source=InMemoryTest;Mode=Memory;Cache=Shared"

// Read-only access
"Data Source=myapp.db;Mode=ReadOnly"
```

## Essential Pragmas

Configure these at connection open for optimal performance:

```csharp
/// <summary>
/// Configures SQLite pragmas for optimal performance.
/// </summary>
public static class SqliteConnectionExtensions
{
    public static void ConfigurePragmas(this SqliteConnection connection)
    {
        using var command = connection.CreateCommand();

        // WAL mode: allows concurrent reads during writes
        command.CommandText = "PRAGMA journal_mode=WAL;";
        command.ExecuteNonQuery();

        // Synchronous NORMAL: good balance of safety and speed
        command.CommandText = "PRAGMA synchronous=NORMAL;";
        command.ExecuteNonQuery();

        // Memory-mapped I/O: faster reads for databases < available RAM
        command.CommandText = "PRAGMA mmap_size=268435456;";  // 256 MB
        command.ExecuteNonQuery();

        // Cache size: negative = KB, positive = pages
        command.CommandText = "PRAGMA cache_size=-64000;";  // 64 MB
        command.ExecuteNonQuery();

        // Temp store in memory
        command.CommandText = "PRAGMA temp_store=MEMORY;";
        command.ExecuteNonQuery();

        // Foreign keys enforcement (off by default in SQLite!)
        command.CommandText = "PRAGMA foreign_keys=ON;";
        command.ExecuteNonQuery();

        // Busy timeout: wait up to 5 seconds for write lock
        command.CommandText = "PRAGMA busy_timeout=5000;";
        command.ExecuteNonQuery();
    }
}
```

## EF Core Configuration with Pragmas

```csharp
services.AddDbContext<AppDbContext>((sp, options) =>
{
    var connection = new SqliteConnection(config.GetConnectionString("DefaultConnection"));
    connection.Open();
    connection.ConfigurePragmas();

    options.UseSqlite(connection);
});
```

## Dapper Connection Factory

```csharp
/// <summary>
/// SQLite connection factory with automatic pragma configuration.
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
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        connection.ConfigurePragmas();
        return connection;
    }
}
```

## Official References

- [Microsoft.Data.Sqlite Connection Strings](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/connection-strings)
- [SQLite PRAGMA Statements](https://www.sqlite.org/pragma.html)
- [SQLite WAL Mode](https://www.sqlite.org/wal.html)
