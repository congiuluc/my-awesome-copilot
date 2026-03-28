---
description: "Use when writing or modifying SQL Server data access code. Covers connection pooling, parameterized queries, indexing strategies, resilience, and query optimization."
applyTo: "src/MyApp.Infrastructure/**/SqlServer*"
---
# SQL Server Guidelines

## Data Types

- Use `NVARCHAR` for Unicode text, `VARCHAR` for ASCII-only.
- Use `DATETIMEOFFSET` for timestamps — it stores timezone information.
- Use `DECIMAL(18,2)` for money — never `FLOAT` or `MONEY`.
- Prefer `BIT` for booleans, `UNIQUEIDENTIFIER` for GUIDs.

## Query Safety

- **Always use parameterized queries** — SQL injection is the #1 database vulnerability.
- Never concatenate user input into SQL strings.
- Use stored procedures or parameterized commands exclusively.

## Indexing

- Design indexes around **query patterns**, not table structure.
- Use **covering indexes** with `INCLUDE` columns to avoid key lookups.
- Clustered index on the primary key; nonclustered for query optimization.
- Monitor unused indexes — they slow down writes with no read benefit.

```sql
CREATE NONCLUSTERED INDEX IX_Orders_CustomerId
ON Orders(CustomerId)
INCLUDE (OrderDate, TotalAmount);
```

## Connection & Resilience

- Use `AddDbContextPool<T>()` for connection pooling with EF Core.
- Enable `EnableRetryOnFailure()` for Azure SQL transient fault handling.
- Use `READ COMMITTED SNAPSHOT` isolation to reduce blocking.
- Configure `MinPoolSize` and `MaxPoolSize` in connection string for high-throughput.

## Query Optimization

- Use `AsNoTracking()` for read-only queries.
- Project only needed columns with `.Select()`.
- Avoid N+1 queries — use `.Include()` for related data.
- Monitor with **Query Store** (enabled by default in Azure SQL).
- Use execution plans to verify index usage.
