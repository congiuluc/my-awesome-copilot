---
description: "Use when writing, modifying, or reviewing Python FastAPI backend code. Covers FastAPI patterns, Clean Architecture, dependency injection, structlog logging, Pydantic validation, and API response conventions."
applyTo: "src/app/**"
---
# Backend Python FastAPI Guidelines

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
- Infrastructure contains: repository implementations, external service clients, database sessions.
- API contains: FastAPI routers, Pydantic schemas, exception handlers, middleware, dependency providers.

## API Response Envelope

All endpoints return a standard envelope:

```python
class ApiResponse(BaseModel, Generic[T]):
    success: bool
    data: T | None = None
    error: str | None = None
```

## Key Conventions

- **Google-style docstrings** on all public classes and functions.
- **Type hints** on all function signatures and class attributes.
- **snake_case** for functions, methods, variables, modules.
- **PascalCase** for classes.
- **UPPER_SNAKE_CASE** for constants.
- **120 character** max line length.
- **`async def`** for all I/O-bound operations.
- **Pydantic models** for all request/response schemas.
- **`Depends()`** for dependency injection.
- **structlog** for structured logging.
- **Pydantic Settings** for typed configuration.
- Health check endpoint at `/health`.
