# Sample: Audit entry model and SQLAlchemy event listeners
# Demonstrates automatic entity change tracking for audit trails in FastAPI.

from __future__ import annotations

import json
import uuid
from datetime import datetime, timezone
from typing import Any

import structlog
from sqlalchemy import Column, DateTime, String, Text, event
from sqlalchemy.orm import Session

from app.infrastructure.database import Base

logger = structlog.get_logger()

SENSITIVE_FIELDS = {"password", "password_hash", "token", "secret", "api_key"}


class AuditEntry(Base):
    """Immutable audit trail entry for entity changes. Never update or delete."""

    __tablename__ = "audit_entries"

    id = Column(String(36), primary_key=True, default=lambda: str(uuid.uuid4()))
    timestamp = Column(DateTime(timezone=True), nullable=False,
                       default=lambda: datetime.now(timezone.utc))
    user_id = Column(String(255), nullable=False)
    action = Column(String(50), nullable=False)
    entity_type = Column(String(255), nullable=False)
    entity_id = Column(String(255), nullable=False)
    changes = Column(Text, nullable=True)
    ip_address = Column(String(45), nullable=True)
    correlation_id = Column(String(255), nullable=True)


def _redact_sensitive(data: dict[str, Any]) -> dict[str, Any]:
    """Redact sensitive fields from the changes dict."""
    redacted = {}
    for key, value in data.items():
        if key.lower() in SENSITIVE_FIELDS:
            redacted[key] = "***REDACTED***"
        else:
            redacted[key] = value
    return redacted


def _get_changes(instance: Any, is_update: bool = False) -> str | None:
    """Extract changed fields as JSON for update operations."""
    if not is_update:
        return None
    from sqlalchemy import inspect as sa_inspect
    state = sa_inspect(instance)
    changes = {}
    for attr in state.attrs:
        history = attr.history
        if history.has_changes():
            changes[attr.key] = {
                "old": history.deleted[0] if history.deleted else None,
                "new": history.added[0] if history.added else None,
            }
    if not changes:
        return None
    return json.dumps(_redact_sensitive(changes), default=str)


def create_audit_entry(
    session: Session,
    instance: Any,
    action: str,
    user_id: str = "system",
    ip_address: str | None = None,
    correlation_id: str | None = None,
) -> None:
    """Create an audit entry for the given entity change."""
    try:
        entity_id = str(getattr(instance, "id", "unknown"))
        entry = AuditEntry(
            user_id=user_id,
            action=action,
            entity_type=type(instance).__name__,
            entity_id=entity_id,
            changes=_get_changes(instance, is_update=(action == "UPDATE")),
            ip_address=ip_address,
            correlation_id=correlation_id,
        )
        session.add(entry)
    except Exception:
        logger.warning("Failed to create audit entry",
                       action=action,
                       entity_type=type(instance).__name__)


# --- SQLAlchemy Event Listeners ---
# Register these on your engine or session to enable automatic auditing.

def register_audit_listeners(session_class: type[Session]) -> None:
    """Register SQLAlchemy ORM event listeners for automatic auditing."""

    @event.listens_for(session_class, "after_flush")
    def after_flush(session: Session, flush_context: Any) -> None:
        for instance in session.new:
            create_audit_entry(session, instance, "CREATE")
        for instance in session.dirty:
            create_audit_entry(session, instance, "UPDATE")
        for instance in session.deleted:
            create_audit_entry(session, instance, "DELETE")
