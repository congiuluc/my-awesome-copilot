---
name: performance-backend
description: >-
  Optimize backend .NET performance with caching, response compression,
  pagination, and database query optimization. Use when: adding memory or
  distributed caching, output caching, response compression, optimizing EF Core
  or Cosmos DB queries, or implementing cursor/offset pagination.
argument-hint: 'Describe the backend performance bottleneck or optimization needed.'
---

# Backend Performance Optimization (.NET)

## When to Use

- Adding caching to frequently accessed data
- Optimizing database queries or repository methods
- Implementing pagination for large datasets
- Adding response compression to the API
- Profiling and fixing backend performance bottlenecks

## Official Documentation

- [Response Caching in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/overview)
- [Output Caching](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output)
- [Response Compression](https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression)
- [EF Core Performance](https://learn.microsoft.com/en-us/ef/core/performance/)

## Procedure

1. Identify bottleneck: database query, I/O wait, serialization, missing index
2. Apply [caching and compression patterns](./references/backend-performance.md)
3. Review [caching sample](./samples/caching-sample.cs)
4. Add pagination for list endpoints (offset or cursor-based)
5. Use `AsNoTracking()` for read-only EF Core queries
6. Project only needed columns with `.Select()`
7. Measure before and after with structured logging
8. Add performance-sensitive tests for critical paths

## Performance Review Checklist

Use this checklist when reviewing backend code for performance issues:

| # | Check | Tool / Method |
|---|-------|---------------|
| 1 | All list endpoints have pagination | Code review |
| 2 | Read-only queries use `AsNoTracking()` | Code review |
| 3 | Only needed columns projected with `.Select()` | Code review |
| 4 | No N+1 query patterns (use `.Include()` or batch) | EF Core logging |
| 5 | Response compression enabled (Brotli + Gzip) | `curl -H 'Accept-Encoding: br'` |
| 6 | Hot data cached with appropriate TTL | Code review |
| 7 | Cache invalidation on write operations | Code review |
| 8 | Async/await used for all I/O (no `.Result` or `.Wait()`) | Code review |
| 9 | `CancellationToken` propagated through call chain | Code review |
| 10 | No unbounded `ToListAsync()` without `Take()` | Code review |
| 11 | Database indexes on frequently queried columns | Schema review |
| 12 | Connection pooling configured correctly | Config review |

## Profiling Workflow

1. Enable EF Core query logging: `optionsBuilder.LogTo(Console.WriteLine)`
2. Use `dotnet-counters` for real-time metrics: `dotnet-counters monitor`
3. Use `dotnet-trace` for detailed profiling: `dotnet-trace collect`
4. Check Aspire dashboard for request timing and dependencies
5. Run `BenchmarkDotNet` for micro-benchmarks on critical paths
6. Compare before/after with structured Serilog timing logs

## Official Profiling References

- [dotnet-counters](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters)
- [dotnet-trace](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
- [EF Core Query Logging](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/)
