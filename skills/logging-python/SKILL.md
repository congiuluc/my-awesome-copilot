---
name: logging-python
description: >-
  Configure structured logging with structlog for Python FastAPI applications.
  Covers processor pipelines, context variables, correlation IDs, log levels,
  sensitive data filtering, and JSON formatting. Use when: setting up structlog,
  configuring log output, adding context vars, filtering sensitive data, or
  troubleshooting logging issues in Python.
argument-hint: 'Describe the logging requirement: structlog setup, context vars, JSON output, or filtering.'
---

# Structured Logging with structlog (Python)

## When to Use

- Setting up structlog in a FastAPI project
- Configuring JSON log output for production
- Adding context variables (correlation IDs, user IDs, operation names)
- Filtering sensitive data from log output
- Adjusting log levels per module
- Troubleshooting log output issues

## Official Documentation

- [structlog Documentation](https://www.structlog.org/en/stable/)
- [structlog + FastAPI](https://www.structlog.org/en/stable/frameworks.html)
- [Python logging module](https://docs.python.org/3/library/logging.html)

## Key Principles

- **Structured, not f-string formatted** — use key-value pairs via `bind()` and event kwargs.
- **Context variables for request scope** — use `structlog.contextvars` for per-request context.
- **Sensitive data never logged** — filter passwords, tokens, PII via processors.
- **Correlation is mandatory** — every request gets a correlation ID for tracing.
- **Levels are meaningful** — use the right level for the right situation.

## Procedure

1. Configure structlog in application startup
2. Review [structlog config sample](./samples/structlog_config.py)
3. Add correlation ID middleware for FastAPI
4. Use structlog bound loggers — never f-string formatting
5. Configure JSON output for production
6. Add sensitive data filtering processor

## Log Levels Guide

| Level | Use When | Examples |
|-------|----------|---------|
| `debug` | Developer diagnostics | Cache hit/miss, config loaded, request parsed |
| `info` | Normal operations | Request started, user logged in, order created |
| `warning` | Expected but notable | Not found, validation failed, slow query |
| `error` | Unexpected failures | Unhandled exceptions, external service down |
| `critical` | App cannot continue | Database unreachable, startup failure |

## Structured Logging Patterns

```python
import structlog

logger = structlog.get_logger()

# ✅ Good — structured with key-value pairs
logger.info("order_created", order_id=order_id, user_id=user_id)

# ✅ Good — with bound context
log = logger.bind(order_id=order_id)
log.info("processing_order")
log.info("order_shipped", tracking="ABC123")

# ❌ Bad — f-string formatting destroys structure
logger.info(f"Order {order_id} created for user {user_id}")
```

## structlog Configuration

```python
import structlog
import logging

def configure_logging(json_output: bool = False) -> None:
    """Configure structlog with appropriate processors."""
    shared_processors: list[structlog.types.Processor] = [
        structlog.contextvars.merge_contextvars,
        structlog.stdlib.add_log_level,
        structlog.stdlib.add_logger_name,
        structlog.processors.TimeStamper(fmt="iso"),
        structlog.processors.StackInfoRenderer(),
        structlog.processors.UnicodeDecoder(),
        _filter_sensitive_keys,
    ]

    if json_output:
        shared_processors.append(structlog.processors.JSONRenderer())
    else:
        shared_processors.append(structlog.dev.ConsoleRenderer())

    structlog.configure(
        processors=shared_processors,
        wrapper_class=structlog.stdlib.BoundLogger,
        context_class=dict,
        logger_factory=structlog.stdlib.LoggerFactory(),
        cache_logger_on_first_use=True,
    )

    logging.basicConfig(format="%(message)s", level=logging.INFO)
```

## Correlation ID Middleware

```python
import uuid
from contextvars import ContextVar
from starlette.middleware.base import BaseHTTPMiddleware, RequestResponseEndpoint
from starlette.requests import Request
from starlette.responses import Response
import structlog

correlation_id_var: ContextVar[str] = ContextVar("correlation_id", default="")

class CorrelationIdMiddleware(BaseHTTPMiddleware):
    async def dispatch(
        self, request: Request, call_next: RequestResponseEndpoint
    ) -> Response:
        correlation_id = request.headers.get(
            "X-Correlation-ID", str(uuid.uuid4())
        )
        correlation_id_var.set(correlation_id)
        structlog.contextvars.bind_contextvars(correlation_id=correlation_id)

        response = await call_next(request)
        response.headers["X-Correlation-ID"] = correlation_id

        structlog.contextvars.unbind_contextvars("correlation_id")
        return response
```

## Sensitive Data Filter

```python
_SENSITIVE_KEYS = {"password", "token", "secret", "api_key", "authorization", "credit_card"}

def _filter_sensitive_keys(
    logger: structlog.types.WrappedLogger,
    method_name: str,
    event_dict: structlog.types.EventDict,
) -> structlog.types.EventDict:
    for key in list(event_dict.keys()):
        if key.lower() in _SENSITIVE_KEYS:
            event_dict[key] = "***REDACTED***"
    return event_dict
```
