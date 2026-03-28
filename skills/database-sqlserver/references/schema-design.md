# SQL Server Schema Design

## Data Types

| C# Type | SQL Server Type | Notes |
|---------|----------------|-------|
| `string` | `NVARCHAR(n)` / `NVARCHAR(MAX)` | Unicode. Use `VARCHAR` for ASCII-only |
| `int` | `INT` | 4 bytes |
| `long` | `BIGINT` | 8 bytes |
| `decimal` | `DECIMAL(18,2)` | Exact numeric — use for money |
| `double` | `FLOAT` | Approximate — avoid for money |
| `bool` | `BIT` | 0/1 |
| `DateTime` | `DATETIME2(7)` | 100ns precision |
| `DateTimeOffset` | `DATETIMEOFFSET(7)` | With timezone offset — preferred |
| `Guid` | `UNIQUEIDENTIFIER` | 16 bytes, poor for clustered index |
| `byte[]` | `VARBINARY(MAX)` | Binary data |

## Table Design

```sql
CREATE TABLE Products (
    -- Use sequential GUID or BIGINT identity for clustered key
    Id NVARCHAR(36) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Price DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Draft',
    CreatedAtUtc DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UpdatedAtUtc DATETIMEOFFSET NULL,

    CONSTRAINT PK_Products PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_Products_Price CHECK (Price >= 0),
    CONSTRAINT CK_Products_Status CHECK (
        Status IN ('Draft', 'Active', 'Expired', 'Deleted')
    )
);
```

## EF Core Fluent Configuration

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>(entity =>
    {
        entity.ToTable("Products");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
            .HasMaxLength(36)
            .IsRequired();

        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.Description)
            .HasMaxLength(500);

        entity.Property(e => e.Price)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        entity.Property(e => e.Status)
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(ProductStatus.Draft);

        entity.Property(e => e.CreatedAtUtc)
            .HasDefaultValueSql("SYSDATETIMEOFFSET()");

        // Indexes
        entity.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Products_Status");

        entity.HasIndex(e => e.CreatedAtUtc)
            .HasDatabaseName("IX_Products_CreatedAtUtc")
            .IsDescending();

        entity.HasIndex(e => new { e.Status, e.CreatedAtUtc })
            .HasDatabaseName("IX_Products_Status_CreatedAtUtc");
    });
}
```

## Relationships

```csharp
modelBuilder.Entity<Order>(entity =>
{
    entity.HasOne(o => o.Product)
        .WithMany(p => p.Orders)
        .HasForeignKey(o => o.ProductId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasIndex(o => o.ProductId)
        .HasDatabaseName("IX_Orders_ProductId");
});
```

## Temporal Tables (SQL Server 2016+)

```sql
CREATE TABLE Products (
    Id NVARCHAR(36) NOT NULL PRIMARY KEY CLUSTERED,
    Name NVARCHAR(100) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.ProductsHistory));
```

```csharp
// EF Core 6+ temporal table support
entity.ToTable("Products", b => b.IsTemporal());

// Query historical data
var historicalProducts = await _dbContext.Products
    .TemporalAsOf(someDate)
    .ToListAsync();
```

## Official References

- [SQL Server Data Types](https://learn.microsoft.com/en-us/sql/t-sql/data-types/data-types-transact-sql)
- [SQL Server Temporal Tables](https://learn.microsoft.com/en-us/sql/relational-databases/tables/temporal-tables)
- [EF Core SQL Server](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/)
