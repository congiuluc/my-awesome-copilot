# SQLite Migrations

## EF Core Migrations

```bash
# Create migration
dotnet ef migrations add InitialCreate \
    --project src/MyApp.Infrastructure \
    --startup-project src/MyApp.Api

# Apply migration
dotnet ef database update \
    --project src/MyApp.Infrastructure \
    --startup-project src/MyApp.Api

# Generate SQL script (for manual review/apply)
dotnet ef migrations script \
    --project src/MyApp.Infrastructure \
    --startup-project src/MyApp.Api \
    --output migrations.sql
```

## SQLite Migration Limitations

SQLite does not support these `ALTER TABLE` operations (EF Core handles via table rebuild):

| Operation | Supported | EF Core Workaround |
|-----------|-----------|-------------------|
| Add column | ✅ Yes | Native `ALTER TABLE ADD COLUMN` |
| Drop column | ❌ No | Rebuilds table automatically |
| Rename column | ✅ Yes (3.25+) | `ALTER TABLE RENAME COLUMN` |
| Change column type | ❌ No | Rebuilds table automatically |
| Add foreign key | ❌ No | Rebuilds table automatically |

## Manual SQL Migrations (for Dapper)

```
migrations/
├── 001_CreateProductsTable.sql
├── 002_AddStatusIndex.sql
├── 003_AddDescriptionColumn.sql
└── _applied.txt
```

```sql
-- 001_CreateProductsTable.sql
CREATE TABLE IF NOT EXISTS Products (
    Id TEXT PRIMARY KEY NOT NULL,
    Name TEXT NOT NULL,
    Price TEXT NOT NULL DEFAULT '0',
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT
) STRICT;

CREATE INDEX IF NOT EXISTS IX_Products_CreatedAtUtc ON Products(CreatedAtUtc DESC);
```

## Migration Runner (Simple)

```csharp
/// <summary>
/// Applies SQL migration scripts in order.
/// </summary>
public class SqliteMigrationRunner
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<SqliteMigrationRunner> _logger;

    public SqliteMigrationRunner(
        IDbConnectionFactory connectionFactory,
        ILogger<SqliteMigrationRunner> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    /// <summary>
    /// Runs all pending migrations.
    /// </summary>
    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Create migration tracking table
        await connection.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS __Migrations (
                Id TEXT PRIMARY KEY NOT NULL,
                AppliedAtUtc TEXT NOT NULL
            )
            """);

        var applied = (await connection.QueryAsync<string>(
            "SELECT Id FROM __Migrations")).ToHashSet();

        var scripts = Directory.GetFiles("migrations", "*.sql")
            .OrderBy(f => f)
            .Where(f => !applied.Contains(Path.GetFileNameWithoutExtension(f)));

        foreach (var script in scripts)
        {
            var name = Path.GetFileNameWithoutExtension(script);
            var sql = await File.ReadAllTextAsync(script, cancellationToken);

            await connection.ExecuteAsync(sql);
            await connection.ExecuteAsync(
                "INSERT INTO __Migrations (Id, AppliedAtUtc) VALUES (@Id, @AppliedAtUtc)",
                new { Id = name, AppliedAtUtc = DateTime.UtcNow.ToString("O") });

            _logger.LogInformation("Applied migration: {MigrationName}", name);
        }
    }
}
```

## Backup Before Migration

```csharp
// Always backup before migrations
File.Copy("myapp.db", $"myapp.db.backup.{DateTime.UtcNow:yyyyMMddHHmmss}");
```

## Official References

- [EF Core SQLite Limitations](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations)
- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [SQLite ALTER TABLE](https://www.sqlite.org/lang_altertable.html)
