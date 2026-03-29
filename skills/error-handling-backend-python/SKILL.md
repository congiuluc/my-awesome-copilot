---
name: error-handling-backend-python
description: >-
  Implement consistent Python FastAPI error handling with global exception handlers,
  custom exceptions, and structured error responses. Use when: building exception
  handlers, creating domain exceptions, mapping errors to HTTP status codes, or
  structured error logging with structlog.
argument-hint: 'Describe the error scenario or exception type to handle.'
---

# Backend Error Handling (Python)

## When to Use

- Setting up global exception handlers in FastAPI
- Creating custom exception types for domain errors
- Implementing the standard `ApiResponse` error envelope
- Mapping exceptions to HTTP status codes
- Logging errors with structlog structured context

## Official Documentation

- [FastAPI Error Handling](https://fastapi.tiangolo.com/tutorial/handling-errors/)
- [Starlette Exception Handlers](https://www.starlette.io/exceptions/)
- [structlog](https://www.structlog.org/)
- [tenacity (retries)](https://tenacity.readthedocs.io/)

## Procedure

1. Create custom domain exceptions in `domain/exceptions/`
2. Register global exception handlers in the FastAPI app
3. Review [Python error handling patterns](./references/python-error-handling.md) for advanced scenarios
4. Review [exception handler sample](./samples/exception_handlers.py)
4. Map exceptions to HTTP status codes
5. Return `ApiResponse` envelope with error details — never expose stack traces
6. Log errors with structlog bound context (correlation ID, user ID)
7. Use tenacity for retry logic on external calls
8. Never swallow exceptions silently

## Exception Hierarchy

```
AppException (base)
├── NotFoundException          → 404
├── ValidationException        → 400
├── ConflictException          → 409
├── UnauthorizedException      → 401
├── ForbiddenException         → 403
└── BusinessRuleException      → 422
```

## Custom Exception Base

```python
class AppException(Exception):
    """Base exception for all domain errors."""

    def __init__(self, message: str, status_code: int = 500) -> None:
        self.message = message
        self.status_code = status_code
        super().__init__(message)


class NotFoundException(AppException):
    def __init__(self, message: str = "Resource not found") -> None:
        super().__init__(message, status_code=404)


class ValidationException(AppException):
    def __init__(self, message: str = "Validation failed") -> None:
        super().__init__(message, status_code=400)


class ConflictException(AppException):
    def __init__(self, message: str = "Resource conflict") -> None:
        super().__init__(message, status_code=409)
```

## Global Exception Handlers

```python
import structlog
from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse

logger = structlog.get_logger()


def register_exception_handlers(app: FastAPI) -> None:
    @app.exception_handler(AppException)
    async def app_exception_handler(request: Request, exc: AppException) -> JSONResponse:
        logger.warning("domain_error", error=exc.message, status=exc.status_code)
        return JSONResponse(
            status_code=exc.status_code,
            content={"success": False, "data": None, "error": exc.message},
        )

    @app.exception_handler(Exception)
    async def unhandled_exception_handler(request: Request, exc: Exception) -> JSONResponse:
        logger.error("unhandled_error", error=str(exc), exc_info=True)
        return JSONResponse(
            status_code=500,
            content={"success": False, "data": None, "error": "An unexpected error occurred."},
        )
```

## Retry Pattern with tenacity

```python
from tenacity import retry, stop_after_attempt, wait_exponential

@retry(stop=stop_after_attempt(3), wait=wait_exponential(min=1, max=10))
async def call_external_service() -> dict:
    async with httpx.AsyncClient() as client:
        response = await client.get("https://api.example.com/data")
        response.raise_for_status()
        return response.json()
```

## Constraints

- NEVER expose stack traces in API responses
- ALWAYS log the full exception at `error` level for unexpected errors
- ALWAYS use typed exceptions — avoid raising raw `Exception`
- ALWAYS include correlation ID in error logs via structlog bound context
- ALWAYS return `ApiResponse` envelope — never raw error bodies
