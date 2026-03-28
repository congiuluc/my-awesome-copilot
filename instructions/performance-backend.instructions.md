---
description: "Use when optimizing backend performance: caching, response compression, pagination, query optimization, async patterns, and profiling."
applyTo: "src/MyApp.Infrastructure/Caching/**,src/MyApp.Api/Middleware/**"
---
# Backend Performance Guidelines

## Caching

- Cache **hot data** with appropriate TTL — don't cache everything.
- Use `IMemoryCache` for single-instance apps, `IDistributedCache` for scaled-out.
- **Invalidate cache on write operations** — stale data causes bugs.
- Use output caching for idempotent GET endpoints with `[OutputCache]`.
- Cache keys: `{entity}:{id}` or `{entity}:list:{hash}` naming convention.

## Response Compression

- Enable **Brotli + Gzip** compression middleware.
- Register Brotli first (better ratio), Gzip as fallback.
- Only compress responses > 1 KB — small responses have negligible benefit.

## Pagination

- All list endpoints **must** have pagination — no unbounded result sets.
- Use **offset pagination** (`skip`/`take`) for simple cases.
- Use **cursor pagination** for large datasets or real-time feeds.
- Return total count and page metadata in the response envelope.

## Query Optimization

- Read-only queries: always use `AsNoTracking()`.
- Project only needed columns: `.Select(x => new Dto { ... })`.
- Avoid N+1 queries: use `.Include()` or batch queries.
- No unbounded `ToListAsync()` without `.Take()`.
- Log slow queries (> 200ms) for investigation.

## Async & Cancellation

- Use `async`/`await` for **all I/O operations** — never `.Result` or `.Wait()`.
- Propagate `CancellationToken` through the entire call chain.
- Handle `OperationCanceledException` gracefully (don't log as error).

## Profiling

- Use `dotnet-counters` for real-time metrics during development.
- Use the Aspire Dashboard to monitor request timing.
- Use `BenchmarkDotNet` for micro-benchmarks of hot paths.
