"""Sample: structlog configuration for FastAPI with correlation IDs and JSON output."""

import logging
import uuid
from contextvars import ContextVar

import structlog
from starlette.middleware.base import BaseHTTPMiddleware, RequestResponseEndpoint
from starlette.requests import Request
from starlette.responses import Response

# ---------------------------------------------------------------------------
# Context variable for correlation ID
# ---------------------------------------------------------------------------
correlation_id_var: ContextVar[str] = ContextVar("correlation_id", default="")

# ---------------------------------------------------------------------------
# Sensitive data filter processor
# ---------------------------------------------------------------------------
_SENSITIVE_KEYS = frozenset(
    {"password", "token", "secret", "api_key", "authorization", "credit_card"}
)


def filter_sensitive_keys(
    logger: structlog.types.WrappedLogger,
    method_name: str,
    event_dict: structlog.types.EventDict,
) -> structlog.types.EventDict:
    """Replace sensitive values with ***REDACTED***."""
    for key in list(event_dict.keys()):
        if key.lower() in _SENSITIVE_KEYS:
            event_dict[key] = "***REDACTED***"
    return event_dict


# ---------------------------------------------------------------------------
# structlog configuration
# ---------------------------------------------------------------------------
def configure_logging(*, json_output: bool = False, log_level: str = "INFO") -> None:
    """Configure structlog with appropriate processors for the environment.

    Args:
        json_output: Use JSON renderer (True for production, False for dev).
        log_level: Root log level (DEBUG, INFO, WARNING, ERROR, CRITICAL).
    """
    shared_processors: list[structlog.types.Processor] = [
        structlog.contextvars.merge_contextvars,
        structlog.stdlib.add_log_level,
        structlog.stdlib.add_logger_name,
        structlog.processors.TimeStamper(fmt="iso"),
        structlog.processors.StackInfoRenderer(),
        structlog.processors.UnicodeDecoder(),
        filter_sensitive_keys,
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

    logging.basicConfig(
        format="%(message)s",
        level=getattr(logging, log_level.upper(), logging.INFO),
    )


# ---------------------------------------------------------------------------
# Correlation ID middleware for FastAPI / Starlette
# ---------------------------------------------------------------------------
class CorrelationIdMiddleware(BaseHTTPMiddleware):
    """Attach a correlation ID to every request for distributed tracing."""

    async def dispatch(
        self, request: Request, call_next: RequestResponseEndpoint
    ) -> Response:
        cid = request.headers.get("X-Correlation-ID", str(uuid.uuid4()))
        correlation_id_var.set(cid)
        structlog.contextvars.bind_contextvars(correlation_id=cid)

        response = await call_next(request)
        response.headers["X-Correlation-ID"] = cid

        structlog.contextvars.unbind_contextvars("correlation_id")
        return response


# ---------------------------------------------------------------------------
# Usage example
# ---------------------------------------------------------------------------
# In main.py / app startup:
#
#   from app.logging_config import configure_logging, CorrelationIdMiddleware
#
#   configure_logging(json_output=os.getenv("ENV") == "production")
#   app.add_middleware(CorrelationIdMiddleware)
#
# In any module:
#
#   import structlog
#   logger = structlog.get_logger()
#   logger.info("order_created", order_id="123", total=99.95)
