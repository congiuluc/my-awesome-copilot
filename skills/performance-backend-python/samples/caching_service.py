# Sample: Redis caching service with FastAPI dependency injection
# Demonstrates the complete caching pattern for a Python backend service.

from typing import Any

import json
import redis.asyncio as redis
from pydantic import BaseModel, Field


class CacheService:
    """Async Redis caching service with TTL and pattern-based invalidation."""

    def __init__(self, redis_client: redis.Redis) -> None:
        self._redis = redis_client

    async def get(self, key: str) -> Any | None:
        """Get a cached value by key, returning None on miss."""
        data = await self._redis.get(key)
        if data is None:
            return None
        return json.loads(data)

    async def set(
        self, key: str, value: Any, ttl_seconds: int = 600
    ) -> None:
        """Set a cached value with a TTL in seconds."""
        await self._redis.set(
            key, json.dumps(value, default=str), ex=ttl_seconds
        )

    async def delete(self, key: str) -> None:
        """Invalidate a single cache entry."""
        await self._redis.delete(key)

    async def delete_pattern(self, pattern: str) -> None:
        """Invalidate all cache entries matching a glob pattern."""
        async for key in self._redis.scan_iter(match=pattern):
            await self._redis.delete(key)


# --- Dependency provider ---

async def get_cache_service() -> CacheService:
    """FastAPI dependency that provides a CacheService instance."""
    client = redis.from_url("redis://localhost:6379", decode_responses=True)
    return CacheService(client)
