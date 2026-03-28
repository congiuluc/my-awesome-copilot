# SQLite Performance Tuning

## Index Strategy

```sql
-- Single column index for frequent filters
CREATE INDEX IX_Products_Status ON Products(Status);

-- Composite index (column order matters — most selective first)
CREATE INDEX IX_Products_Status_CreatedAtUtc ON Products(Status, CreatedAtUtc DESC);

-- Covering index (includes all columns needed — avoids table lookup)
CREATE INDEX IX_Products_Status_Covering ON Products(Status, CreatedAtUtc)
    INCLUDE (Name, Price)  -- SQLite doesn't support INCLUDE; list all columns instead:
;
-- In SQLite, create a composite index with all needed columns:
CREATE INDEX IX_Products_StatusCovering ON Products(Status, CreatedAtUtc, Name, Price);

-- Partial index (only indexes rows matching a condition)
CREATE INDEX IX_Products_Active ON Products(CreatedAtUtc)
    WHERE Status = 'Active';

-- Unique index
CREATE UNIQUE INDEX IX_Products_Name ON Products(Name) WHERE Status != 'Deleted';
```

## Query Optimization

### Use EXPLAIN QUERY PLAN

```sql
-- Check if your query uses indexes
EXPLAIN QUERY PLAN SELECT * FROM Products WHERE Status = 'Active' ORDER BY CreatedAtUtc DESC;
-- Good: SEARCH Products USING INDEX IX_Products_Status_CreatedAtUtc
-- Bad:  SCAN Products (full table scan)
```

### Avoid Common Anti-Patterns

```sql
-- BAD: Function on indexed column prevents index use
SELECT * FROM Products WHERE lower(Name) = 'widget';
-- GOOD: Use COLLATE NOCASE or store normalized
SELECT * FROM Products WHERE Name = 'widget' COLLATE NOCASE;

-- BAD: LIKE with leading wildcard = full scan
SELECT * FROM Products WHERE Name LIKE '%widget%';
-- GOOD: Use FTS5 for full-text search
SELECT * FROM Products_fts WHERE Products_fts MATCH 'widget';

-- BAD: SELECT * when you only need a few columns
SELECT * FROM Products WHERE Status = 'Active';
-- GOOD: Project only needed columns
SELECT Id, Name, Price FROM Products WHERE Status = 'Active';
```

### Batch Operations

```sql
-- BAD: Individual inserts in a loop (each is a transaction)
INSERT INTO Products VALUES (...);
INSERT INTO Products VALUES (...);

-- GOOD: Wrap in explicit transaction
BEGIN TRANSACTION;
INSERT INTO Products VALUES (...);
INSERT INTO Products VALUES (...);
COMMIT;
```

## EF Core Performance Tips for SQLite

```csharp
// Use AsNoTracking for read-only queries
var products = await _dbContext.Products
    .AsNoTracking()
    .Where(p => p.Status == ProductStatus.Active)
    .OrderByDescending(p => p.CreatedAtUtc)
    .Take(50)
    .ToListAsync(cancellationToken);

// Batch SaveChanges — EF Core batches automatically with SQLite
// But consider using ExecuteUpdate/ExecuteDelete for bulk ops (.NET 7+)
await _dbContext.Products
    .Where(p => p.Status == ProductStatus.Expired)
    .ExecuteDeleteAsync(cancellationToken);
```

## Full-Text Search (FTS5)

```sql
-- Create FTS5 virtual table
CREATE VIRTUAL TABLE Products_fts USING fts5(
    Name,
    Description,
    content=Products,
    content_rowid=rowid
);

-- Populate FTS index
INSERT INTO Products_fts(Products_fts) VALUES('rebuild');

-- Search
SELECT p.* FROM Products p
INNER JOIN Products_fts fts ON p.rowid = fts.rowid
WHERE Products_fts MATCH 'search term'
ORDER BY rank;
```

## Monitoring

```csharp
// Check database file size
var fileInfo = new FileInfo("myapp.db");
var sizeMb = fileInfo.Length / (1024.0 * 1024.0);

// Check WAL file size (compact periodically)
// PRAGMA wal_checkpoint(TRUNCATE);

// Analyze query performance
// PRAGMA compile_options; -- shows build-time options
// .timer ON -- in sqlite3 CLI
```

## Vacuuming

```sql
-- Reclaim space after large deletes
VACUUM;

-- Auto-vacuum (set before creating tables)
PRAGMA auto_vacuum = INCREMENTAL;
PRAGMA incremental_vacuum(100);  -- free 100 pages
```

## Official References

- [SQLite Query Planner](https://www.sqlite.org/queryplanner.html)
- [SQLite FTS5](https://www.sqlite.org/fts5.html)
- [SQLite EXPLAIN](https://www.sqlite.org/eqp.html)
- [SQLite Performance Tips](https://www.sqlite.org/np1queryprob.html)
