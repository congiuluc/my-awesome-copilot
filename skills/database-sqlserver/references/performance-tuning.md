# SQL Server Performance Tuning

## Index Strategy

### Clustered Index
```sql
-- One per table. Determines physical row order.
-- Best: narrow, unique, ever-increasing (identity/sequential GUID)
-- Avoid: wide columns, random GUIDs (cause page splits)
CREATE CLUSTERED INDEX IX_Products_PK ON Products(Id);
```

### Non-Clustered Indexes
```sql
-- For WHERE, JOIN, ORDER BY columns
CREATE NONCLUSTERED INDEX IX_Products_Status
    ON Products(Status)
    INCLUDE (Name, Price, CreatedAtUtc);  -- covering index

-- Filtered index (partial)
CREATE NONCLUSTERED INDEX IX_Products_ActiveOnly
    ON Products(CreatedAtUtc DESC)
    WHERE Status = 'Active';

-- Composite index (most selective column first)
CREATE NONCLUSTERED INDEX IX_Products_StatusDate
    ON Products(Status, CreatedAtUtc DESC)
    INCLUDE (Name, Price);
```

### Index Maintenance
```sql
-- Check fragmentation
SELECT
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.avg_fragmentation_in_percent,
    ips.page_count
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
WHERE ips.avg_fragmentation_in_percent > 10
ORDER BY ips.avg_fragmentation_in_percent DESC;

-- Rebuild (> 30% fragmentation) or Reorganize (10-30%)
ALTER INDEX IX_Products_Status ON Products REBUILD;
ALTER INDEX IX_Products_Status ON Products REORGANIZE;
```

## Query Optimization

### Execution Plans
```sql
-- View estimated plan
SET SHOWPLAN_XML ON;
GO
SELECT * FROM Products WHERE Status = 'Active';
GO
SET SHOWPLAN_XML OFF;

-- View actual plan with statistics
SET STATISTICS IO ON;
SET STATISTICS TIME ON;
SELECT * FROM Products WHERE Status = 'Active' ORDER BY CreatedAtUtc DESC;
SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;
```

### Common Anti-Patterns

```sql
-- BAD: Function on indexed column
SELECT * FROM Products WHERE YEAR(CreatedAtUtc) = 2026;
-- GOOD: Range predicate preserves index
SELECT * FROM Products
WHERE CreatedAtUtc >= '2026-01-01' AND CreatedAtUtc < '2027-01-01';

-- BAD: Implicit conversion (VARCHAR column compared to NVARCHAR)
SELECT * FROM Products WHERE Name = N'Widget';  -- if Name is VARCHAR
-- GOOD: Match types exactly

-- BAD: SELECT * with unnecessary columns
SELECT * FROM Products;
-- GOOD: Project needed columns
SELECT Id, Name, Price FROM Products;

-- BAD: NOT IN with NULLs
SELECT * FROM Products WHERE Id NOT IN (SELECT ProductId FROM Orders);
-- GOOD: NOT EXISTS
SELECT * FROM Products p
WHERE NOT EXISTS (SELECT 1 FROM Orders o WHERE o.ProductId = p.Id);
```

### Pagination
```sql
-- Offset-based (simple, but slower on deep pages)
SELECT Id, Name, Price, CreatedAtUtc
FROM Products
WHERE Status = 'Active'
ORDER BY CreatedAtUtc DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

-- Keyset pagination (fast at any depth — preferred)
SELECT TOP (@PageSize) Id, Name, Price, CreatedAtUtc
FROM Products
WHERE Status = 'Active'
    AND (CreatedAtUtc < @LastCreatedAtUtc
         OR (CreatedAtUtc = @LastCreatedAtUtc AND Id < @LastId))
ORDER BY CreatedAtUtc DESC, Id DESC;
```

## Query Store (Azure SQL default)

```sql
-- Enable Query Store
ALTER DATABASE MyApp SET QUERY_STORE = ON;

-- Find top resource-consuming queries
SELECT TOP 10
    qt.query_sql_text,
    rs.avg_duration,
    rs.avg_cpu_time,
    rs.avg_logical_io_reads,
    rs.count_executions
FROM sys.query_store_query_text qt
JOIN sys.query_store_query q ON qt.query_text_id = q.query_text_id
JOIN sys.query_store_plan p ON q.query_id = p.query_id
JOIN sys.query_store_runtime_stats rs ON p.plan_id = rs.plan_id
ORDER BY rs.avg_duration DESC;
```

## EF Core Performance

```csharp
// Split queries to avoid cartesian explosion
var orders = await _dbContext.Orders
    .Include(o => o.Product)
    .Include(o => o.Items)
    .AsSplitQuery()
    .AsNoTracking()
    .ToListAsync(cancellationToken);

// Compiled queries for hot paths
private static readonly Func<AppDbContext, string, CancellationToken, Task<Product?>>
    _getByIdQuery = EF.CompileAsyncQuery(
        (AppDbContext ctx, string id, CancellationToken ct) =>
            ctx.Products.AsNoTracking().FirstOrDefault(p => p.Id == id));

// Bulk operations (.NET 7+)
await _dbContext.Products
    .Where(p => p.Status == ProductStatus.Expired)
    .ExecuteUpdateAsync(s => s
        .SetProperty(p => p.Status, ProductStatus.Deleted)
        .SetProperty(p => p.UpdatedAtUtc, DateTimeOffset.UtcNow),
        cancellationToken);
```

## Official References

- [SQL Server Index Architecture](https://learn.microsoft.com/en-us/sql/relational-databases/sql-server-index-design-guide)
- [Query Store](https://learn.microsoft.com/en-us/sql/relational-databases/performance/monitoring-performance-by-using-the-query-store)
- [Execution Plans](https://learn.microsoft.com/en-us/sql/relational-databases/performance/execution-plans)
- [Performance Tuning](https://learn.microsoft.com/en-us/sql/relational-databases/performance/performance-center-for-sql-server-database-engine-and-azure-sql-database)
