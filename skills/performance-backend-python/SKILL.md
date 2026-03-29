---
name: performance-backend-python
description: >-
  Optimize Python FastAPI backend performance with caching, response compression,
  pagination, async query optimization, and profiling. Use when: adding Redis/in-memory
  caching, GZip middleware, optimizing SQLAlchemy or Motor queries, implementing
  cursor/offset pagination, or profiling Python backend bottlenecks.
argument-hint: 'Describe the backend performance bottleneck or optimization needed.'
---

# Backend Performance Optimization (Python)

## When to Use

- Adding caching to frequently accessed data (Redis, in-memory)
- Optimizing SQLAlchemy async queries or Motor/Beanie queries
- Implementing pagination for large datasets
- Adding response compression (GZip middleware)
- Profiling and fixing Python backend performance bottlenecks
- Optimizing async I/O patterns

## Official Documentation

- [FastAPI Performance](https://fastapi.tiangolo.com/advanced/behind-a-proxy/)
- [SQLAlchemy Performance](https://docs.sqlalchemy.org/en/20/faq/performance.html)
- [redis-py Async](https://redis-py.readthedocs.io/en/stable/examples/asyncio_examples.html)
- [GZipMiddleware](https://www.starlette.io/middleware/#gzipmiddleware)
- [cProfile](https://docs.python.org/3/library/profile.html)

## Procedure

1. Identify bottleneck: database query, I/O wait, serialization, missing index, GIL contention
2. Apply [caching and optimization patterns](./references/backend-performance-python.md)
3. Review [caching sample](./samples/caching_service.py)
4. Add pagination for list endpoints (offset or cursor-based)
5. Use `selectinload()` or `joinedload()` to avoid N+1 queries in SQLAlchemy
6. Use projections (select specific columns) for read-only use cases
7. Measure before and after with `structlog` timing or `py-spy`
8. Add performance-sensitive tests for critical paths

## Performance Review Checklist

| # | Check | Tool / Method |
|---|-------|---------------|
| 1 | All list endpoints accept `skip`/`limit` or cursor params | Code review |
| 2 | Read-only queries avoid loading full ORM objects | Code review |
| 3 | No N+1 queries (use eager loading strategies) | SQLAlchemy echo logging |
| 4 | GZipMiddleware enabled for responses > 500 bytes | Code review |
| 5 | Hot data cached in Redis with appropriate TTL | Code review |
| 6 | Cache invalidation on write operations | Code review |
| 7 | All I/O uses `async def` (no sync blocking in event loop) | Code review |
| 8 | Connection pool configured correctly (SQLAlchemy pool_size) | Config review |
| 9 | Database indexes on frequently queried columns | Schema review |
| 10 | No unbounded queries without `LIMIT` | Code review |
| 11 | JSON serialization uses `orjson` or `ujson` for speed | Config review |
| 12 | Background tasks use `BackgroundTasks` or task queues | Code review |

## Profiling Workflow

1. Enable SQLAlchemy echo: `echo=True` in `create_async_engine`
2. Use `py-spy` for CPU profiling: `py-spy record -o profile.svg -- python main.py`
3. Use `structlog` with timing middleware for request duration
4. Use `memory_profiler` for memory leak detection
5. Use `locust` for load testing
6. Use `httpx` + `pytest-benchmark` for endpoint benchmarks
