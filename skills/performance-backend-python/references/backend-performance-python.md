# Python Backend Performance Patterns

## Caching with Redis

### Redis Service

```python
import json
from typing import Any

import redis.asyncio as redis


class CacheService:
    """Async Redis caching service with TTL support."""

    def __init__(self, redis_client: redis.Redis) -> None:
        self._redis = redis_client

    async def get(self, key: str) -> Any | None:
        """Get a cached value by key."""
        data = await self._redis.get(key)
        if data is None:
            return None
        return json.loads(data)

    async def set(self, key: str, value: Any, ttl_seconds: int = 600) -> None:
        """Set a cached value with TTL."""
        await self._redis.set(key, json.dumps(value, default=str), ex=ttl_seconds)

    async def delete(self, key: str) -> None:
        """Invalidate a cache entry."""
        await self._redis.delete(key)

    async def delete_pattern(self, pattern: str) -> None:
        """Invalidate all keys matching a pattern."""
        async for key in self._redis.scan_iter(match=pattern):
            await self._redis.delete(key)
```

### Usage in Service Layer

```python
class ProductService:
    """Product service with caching."""

    def __init__(self, repo: ProductRepository, cache: CacheService) -> None:
        self._repo = repo
        self._cache = cache

    async def get_by_id(self, product_id: str) -> ProductDto:
        cached = await self._cache.get(f"product:{product_id}")
        if cached:
            return ProductDto(**cached)

        product = await self._repo.get_by_id(product_id)
        await self._cache.set(f"product:{product_id}", product.model_dump(), ttl_seconds=600)
        return ProductDto.from_domain(product)

    async def update(self, product_id: str, data: UpdateProductRequest) -> ProductDto:
        product = await self._repo.update(product_id, data)
        await self._cache.delete(f"product:{product_id}")
        return ProductDto.from_domain(product)
```

## In-Memory Caching with `cachetools`

```python
from cachetools import TTLCache
from functools import lru_cache

# For simple, fast in-memory caches (single-process only)
_config_cache: TTLCache = TTLCache(maxsize=100, ttl=300)
```

## Response Compression (GZip)

```python
from starlette.middleware.gzip import GZipMiddleware

app.add_middleware(GZipMiddleware, minimum_size=500)
```

## Pagination

### Offset-Based

```python
from pydantic import BaseModel, Field


class PaginationParams(BaseModel):
    skip: int = Field(default=0, ge=0)
    limit: int = Field(default=20, ge=1, le=100)


class PaginatedResponse(BaseModel, Generic[T]):
    items: list[T]
    total: int
    skip: int
    limit: int
    has_more: bool


# In router:
@router.get("/products")
async def list_products(
    pagination: PaginationParams = Depends(),
    service: ProductService = Depends(get_product_service),
) -> ApiResponse[PaginatedResponse[ProductDto]]:
    result = await service.list(skip=pagination.skip, limit=pagination.limit)
    return ApiResponse(success=True, data=result)
```

### Cursor-Based

```python
class CursorParams(BaseModel):
    cursor: str | None = None
    limit: int = Field(default=20, ge=1, le=100)


class CursorResponse(BaseModel, Generic[T]):
    items: list[T]
    next_cursor: str | None
    has_more: bool
```

## SQLAlchemy Async Query Optimization

### Eager Loading (avoid N+1)

```python
from sqlalchemy.orm import selectinload, joinedload

# selectinload: separate SELECT for related objects (good for collections)
stmt = select(Product).options(selectinload(Product.reviews))

# joinedload: JOIN in single query (good for single related objects)
stmt = select(Order).options(joinedload(Order.customer))
```

### Column Projection

```python
# Load only needed columns instead of full ORM objects
stmt = select(Product.id, Product.name, Product.price).where(Product.active == True)
```

### Read-Only with `execution_options`

```python
# For read-only queries, skip dirty checking
async with session.begin():
    result = await session.execute(
        stmt.execution_options(populate_existing=False)
    )
```

## Fast JSON Serialization

```python
# Use orjson for 2-10x faster JSON serialization
# pip install orjson
from fastapi.responses import ORJSONResponse

app = FastAPI(default_response_class=ORJSONResponse)
```

## Background Tasks

```python
from fastapi import BackgroundTasks

@router.post("/reports")
async def generate_report(
    request: ReportRequest,
    background_tasks: BackgroundTasks,
) -> ApiResponse[str]:
    background_tasks.add_task(build_report, request)
    return ApiResponse(success=True, data="Report generation started")
```

## Connection Pool Tuning

```python
from sqlalchemy.ext.asyncio import create_async_engine

engine = create_async_engine(
    database_url,
    pool_size=10,          # Max persistent connections
    max_overflow=20,       # Extra connections under load
    pool_timeout=30,       # Seconds to wait for connection
    pool_recycle=1800,     # Recycle connections after 30 min
    pool_pre_ping=True,    # Validate connections before use
)
```
