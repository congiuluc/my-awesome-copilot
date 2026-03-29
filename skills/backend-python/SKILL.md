---
name: backend-python
description: "Build Python FastAPI backend code following Clean Architecture. Use when: writing Python endpoints/routers, services, repository implementations, middleware, dependency injection, structured logging, health checks, or API response envelope pattern."
argument-hint: 'Describe the router, service, or component to implement.'
---

# Backend Python FastAPI

## When to Use

- Creating or modifying Python backend code following Clean Architecture
- Scaffolding new routers, services, or middleware
- Configuring dependency injection, logging, or health checks
- Reviewing Python backend code for best practices

## Official Documentation

- [FastAPI](https://fastapi.tiangolo.com/)
- [Pydantic v2](https://docs.pydantic.dev/latest/)
- [SQLAlchemy 2.x Async](https://docs.sqlalchemy.org/en/20/orm/extensions/asyncio.html)
- [Motor (Async MongoDB)](https://motor.readthedocs.io/)
- [Beanie ODM](https://beanie-odm.dev/)
- [Alembic Migrations](https://alembic.sqlalchemy.org/)
- [structlog](https://www.structlog.org/)
- [httpx](https://www.python-httpx.org/)

## Procedure

1. Identify the Clean Architecture layer (Domain / Application / Infrastructure / API)
2. Follow the patterns in [Python guidelines](./references/python-guidelines.md)
3. Apply the [code style rules](./references/code-style.md)
4. Review [sample router](./samples/router-sample.py) for complete pattern
5. Wire up dependencies via FastAPI's `Depends()` mechanism
6. Ensure all I/O uses `async def` with proper error handling
7. Wrap all responses in `ApiResponse` envelope model
8. Add Google-style docstrings to all public classes and functions
9. Create corresponding tests (see `testing-backend-python` skill)
