# SQLite Schema Design

## Type Affinity

SQLite uses type affinity, not strict types. Use `STRICT` tables when possible:

```sql
-- Standard table (type affinity — flexible but risky)
CREATE TABLE Products (
    Id TEXT PRIMARY KEY NOT NULL,
    Name TEXT NOT NULL,
    Price REAL NOT NULL DEFAULT 0,
    CreatedAtUtc TEXT NOT NULL
);

-- STRICT table (SQLite 3.37+ — enforces types)
CREATE TABLE Products (
    Id TEXT PRIMARY KEY NOT NULL,
    Name TEXT NOT NULL,
    Price REAL NOT NULL DEFAULT 0,
    Quantity INTEGER NOT NULL DEFAULT 0,
    CreatedAtUtc TEXT NOT NULL
) STRICT;
```

## Supported Strict Types

| SQLite Type | C# Type | Notes |
|-------------|---------|-------|
| `INTEGER` | `int`, `long`, `bool` | 64-bit signed |
| `REAL` | `double`, `float` | 8-byte IEEE float |
| `TEXT` | `string`, `DateTime`, `Guid`, `decimal` | UTF-8 encoded |
| `BLOB` | `byte[]` | Binary data |
| `ANY` | — | Only in STRICT tables |

## Date/Time Storage

SQLite has no native date type. Choose one approach and be consistent:

```sql
-- Option 1: ISO 8601 strings (recommended — human-readable, sortable)
CreatedAtUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now'))

-- Option 2: Unix timestamps (compact, fast comparisons)
CreatedAtUtc INTEGER NOT NULL DEFAULT (unixepoch())
```

## EF Core Fluent Configuration

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasMaxLength(36);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Description).HasMaxLength(500);

        // SQLite doesn't natively support decimal — store as TEXT
        entity.Property(e => e.Price)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Indexes
        entity.HasIndex(e => e.Status);
        entity.HasIndex(e => e.CreatedAtUtc);
        entity.HasIndex(e => new { e.Status, e.CreatedAtUtc })
            .HasDatabaseName("IX_Products_Status_CreatedAtUtc");
    });
}
```

## Constraints and Relationships

```sql
CREATE TABLE Orders (
    Id TEXT PRIMARY KEY NOT NULL,
    ProductId TEXT NOT NULL,
    Quantity INTEGER NOT NULL CHECK(Quantity > 0),
    CreatedAtUtc TEXT NOT NULL,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
) STRICT;
```

## Limitations to Know

- No `ALTER COLUMN` — must recreate table to change column types
- No native `BOOLEAN` — uses `INTEGER` (0/1)
- No native `DECIMAL` — use `TEXT` for precision-sensitive values (money)
- Max database size: 281 TB (practical limit ~1 TB)
- Single writer at a time (WAL mode allows concurrent readers)

## Official References

- [SQLite Data Types](https://www.sqlite.org/datatype3.html)
- [SQLite STRICT Tables](https://www.sqlite.org/stricttables.html)
- [SQLite CREATE TABLE](https://www.sqlite.org/lang_createtable.html)
