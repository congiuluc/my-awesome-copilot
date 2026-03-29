# Python FastAPI Guidelines

## Framework & Language

- Target **Python 3.12+** (latest stable). Use modern features: type hints, match statements, exception groups.
- Use **FastAPI** as the web framework with **uvicorn** as ASGI server.
- Use **Pydantic v2** for data validation and serialization.
- Use **Protocol** classes for interface definitions.

## Clean Architecture Layers

| Layer | Package | Depends On |
|-------|---------|------------|
| Domain | `app/domain/` | Nothing |
| Application | `app/application/` | Domain |
| Infrastructure | `app/infrastructure/` | Domain, Application |
| API | `app/api/` | Domain, Application, Infrastructure |

- Domain contains: models, value objects, domain exceptions, enums. NO framework imports.
- Application contains: service Protocols, service implementations, repository Protocols.
- Infrastructure contains: repository implementations (SQLAlchemy, Motor/Beanie), external clients, cache.
- API contains: FastAPI routers, Pydantic schemas, exception handlers, middleware, dependency providers.

## API Response Envelope

All endpoints return a standard envelope:

```python
from pydantic import BaseModel
from typing import Generic, TypeVar

T = TypeVar("T")


class ApiResponse(BaseModel, Generic[T]):
    """Standard API response envelope."""

    success: bool
    data: T | None = None
    error: str | None = None

    @classmethod
    def ok(cls, data: T) -> "ApiResponse[T]":
        return cls(success=True, data=data)

    @classmethod
    def fail(cls, error: str) -> "ApiResponse[None]":
        return cls(success=False, error=error)
```

Return `ApiResponse` from every endpoint — never raw objects or primitives.

## Router Organization

- Group related endpoints in a single `APIRouter` with a common prefix.
- Use `tags` for OpenAPI grouping.
- Use `summary` and `description` for endpoint documentation.
- Use `response_model` for typed responses.

```python
from fastapi import APIRouter, Depends, status
from typing import Annotated

router = APIRouter(prefix="/api/products", tags=["Product"])

ProductServiceDep = Annotated[ProductServiceProtocol, Depends(get_product_service)]


@router.get(
    "/",
    response_model=ApiResponse[list[ProductResponse]],
    summary="Get all products",
)
async def get_all(service: ProductServiceDep) -> ApiResponse[list[ProductResponse]]:
    """Retrieve all products."""
    products = await service.find_all()
    return ApiResponse.ok(products)


@router.get(
    "/{product_id}",
    response_model=ApiResponse[ProductResponse],
    summary="Get a product by ID",
    responses={404: {"model": ApiResponse[None]}},
)
async def get_by_id(
    product_id: str,
    service: ProductServiceDep,
) -> ApiResponse[ProductResponse]:
    """Retrieve a product by its unique identifier."""
    product = await service.find_by_id(product_id)
    return ApiResponse.ok(product)


@router.post(
    "/",
    response_model=ApiResponse[ProductResponse],
    status_code=status.HTTP_201_CREATED,
    summary="Create a new product",
)
async def create(
    request: CreateProductRequest,
    service: ProductServiceDep,
) -> ApiResponse[ProductResponse]:
    """Create a new product."""
    product = await service.create(request)
    return ApiResponse.ok(product)
```

## Dependency Injection

- Use FastAPI's `Depends()` mechanism for all service injection.
- Define dependency providers in `app/api/deps.py`.
- Use `Annotated` type aliases for clean signatures.
- Use async generators for database session lifecycle.

```python
# app/api/deps.py
from typing import Annotated, AsyncGenerator
from fastapi import Depends
from sqlalchemy.ext.asyncio import AsyncSession

from app.infrastructure.database import async_session_factory
from app.infrastructure.repositories.product_repository import SqlAlchemyProductRepository
from app.application.services.product_service import ProductServiceImpl


async def get_db_session() -> AsyncGenerator[AsyncSession, None]:
    """Provide a database session per request."""
    async with async_session_factory() as session:
        yield session


async def get_product_service(
    session: Annotated[AsyncSession, Depends(get_db_session)],
) -> ProductServiceImpl:
    """Provide a product service instance."""
    repository = SqlAlchemyProductRepository(session)
    return ProductServiceImpl(repository)
```

## Logging (structlog)

- Use `structlog` for structured logging.
- Use `get_logger()` at module level.
- Use key-value pairs for context: `log.info("processing_product", product_id=product_id)`
- Never log sensitive data (passwords, tokens, PII, connection strings).
- Configure structlog processors in `app/infrastructure/logging.py`.

```python
import structlog

log = structlog.get_logger()


async def find_by_id(self, product_id: str) -> ProductResponse:
    log.debug("fetching_product", product_id=product_id)
    # ...
```

## Async Patterns

- All I/O-bound functions MUST be `async def`.
- Never use blocking calls (`requests`, `time.sleep`) in async context — use `httpx`, `asyncio.sleep`.
- Use `async with` for database sessions, HTTP clients, and file operations.
- Use `asyncio.gather()` for concurrent independent I/O operations.
- Use FastAPI `BackgroundTasks` for non-critical async work.

## Configuration

- Use **Pydantic Settings** (`pydantic-settings`) for typed configuration.
- Environment-specific files: `.env.development`, `.env.staging`, `.env.production`.
- Never hardcode connection strings or secrets — use environment variables.
- Validate configuration at startup.

```python
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Application settings loaded from environment."""

    database_url: str
    redis_url: str = "redis://localhost:6379"
    debug: bool = False
    log_level: str = "INFO"

    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=False,
    )


settings = Settings()
```

## Health Checks

- Add a `/health` endpoint returning JSON with component status.
- Check database connectivity and external dependencies.
- Return 200 for healthy, 503 for unhealthy.

```python
@router.get("/health", tags=["Health"])
async def health_check(
    session: Annotated[AsyncSession, Depends(get_db_session)],
) -> dict:
    """Check application health."""
    checks = {"database": "healthy"}
    try:
        await session.execute(text("SELECT 1"))
    except Exception:
        checks["database"] = "unhealthy"
        return JSONResponse(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            content={"status": "unhealthy", "checks": checks},
        )
    return {"status": "healthy", "checks": checks}
```

## Input Validation

- Validate all incoming requests with Pydantic models.
- Use `Field()` with constraints (`min_length`, `max_length`, `gt`, `pattern`).
- Custom validators with `@field_validator` for complex rules.
- FastAPI automatically returns 422 for validation errors.

```python
from pydantic import BaseModel, Field, field_validator


class CreateProductRequest(BaseModel):
    """Request to create a new product."""

    name: str = Field(..., min_length=1, max_length=100)
    description: str | None = Field(None, max_length=500)
    price: Decimal = Field(..., gt=0)

    @field_validator("name")
    @classmethod
    def name_must_not_be_blank(cls, v: str) -> str:
        if not v.strip():
            raise ValueError("Name must not be blank")
        return v.strip()
```

## Database Patterns (SQLAlchemy 2.x Async)

```python
from sqlalchemy.ext.asyncio import AsyncSession, create_async_engine, async_sessionmaker
from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column


class Base(DeclarativeBase):
    pass


class Product(Base):
    __tablename__ = "products"

    id: Mapped[str] = mapped_column(primary_key=True)
    name: Mapped[str] = mapped_column(String(100))
    description: Mapped[str | None] = mapped_column(String(500))
    price: Mapped[Decimal] = mapped_column(Numeric(10, 2))
    created_at: Mapped[datetime] = mapped_column(default=func.now())
```
