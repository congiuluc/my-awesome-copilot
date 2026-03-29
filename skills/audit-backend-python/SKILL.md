---
name: audit-backend-python
description: >-
  Implement audit logging and audit trails for Python FastAPI applications.
  Covers SQLAlchemy event listeners, middleware-based action logging, Pydantic
  audit models, and compliance patterns. Use when: adding audit trails to
  data changes, logging user actions, implementing compliance requirements
  (GDPR, SOC2, HIPAA), or reviewing audit coverage in Python.
argument-hint: 'Describe the audit requirement: entity tracking, action logging, compliance, or review.'
---

# Audit Logging (Python FastAPI)

## When to Use

- Adding audit trails for entity create/update/delete operations
- Logging user actions (login, permission changes, data access)
- Implementing compliance audit requirements (GDPR, SOC2, HIPAA)
- Reviewing existing audit coverage for gaps
- Adding tamper-proof audit storage

## Official Documentation

- [SQLAlchemy Events](https://docs.sqlalchemy.org/en/20/core/event.html)
- [SQLAlchemy ORM Events](https://docs.sqlalchemy.org/en/20/orm/events.html)
- [FastAPI Middleware](https://fastapi.tiangolo.com/tutorial/middleware/)

## Key Principles

- **Audit is separate from logging** — audit events are business-critical records, not debug logs.
- **Immutable and append-only** — audit records must never be updated or deleted.
- **Who, What, When, Where** — every audit entry captures the actor, action, timestamp, and target.
- **Sensitive data handling** — store what changed but redact PII where required.
- **Async and non-blocking** — audit writes must not slow down the primary operation.

## Procedure

1. Define the `AuditEntry` Pydantic/SQLAlchemy model (see [audit model sample](./samples/audit_entry.py))
2. Implement SQLAlchemy `after_insert`, `after_update`, `after_delete` event listeners
3. Add a FastAPI middleware for HTTP request-level action auditing
4. Store audit records in a dedicated, append-only table
5. Include the actor (user ID), action type, entity type, entity ID, timestamp, and changes
6. Redact sensitive fields (passwords, tokens, PII) before persisting
7. Add retention policies and archival for compliance

## Audit Entry Structure

| Field | Type | Description |
|-------|------|-------------|
| `id` | `str` | Unique audit entry ID (UUID) |
| `timestamp` | `datetime` | When the action occurred (UTC) |
| `user_id` | `str` | Who performed the action |
| `action` | `str` | CREATE, UPDATE, DELETE, ACCESS, LOGIN, etc. |
| `entity_type` | `str` | The type of entity affected |
| `entity_id` | `str` | The ID of the entity affected |
| `changes` | `str \| None` | JSON diff of old/new values (for updates) |
| `ip_address` | `str \| None` | Client IP address |
| `correlation_id` | `str \| None` | Request correlation ID |

## Constraints

- NEVER update or delete audit records
- ALWAYS capture the acting user from the request authentication context
- ALWAYS use UTC timestamps (`datetime.now(timezone.utc)`)
- ALWAYS redact sensitive fields before persisting
- NEVER let audit failures break the primary operation (use try/except)
