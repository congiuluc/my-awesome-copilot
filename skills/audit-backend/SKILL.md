---
name: audit-backend
description: >-
  Implement audit logging and audit trails for .NET backend applications.
  Covers entity change tracking, user action logging, audit middleware,
  tamper-proof audit storage, and compliance patterns. Use when: adding audit
  trails to data changes, logging user actions, implementing compliance
  requirements (GDPR, SOC2, HIPAA), or reviewing audit coverage.
argument-hint: 'Describe the audit requirement: entity tracking, action logging, compliance, or review.'
---

# Audit Logging (.NET)

## When to Use

- Adding audit trails for entity create/update/delete operations
- Logging user actions (login, permission changes, data access)
- Implementing compliance audit requirements (GDPR, SOC2, HIPAA)
- Reviewing existing audit coverage for gaps
- Adding tamper-proof audit storage

## Official Documentation

- [EF Core Interceptors](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors)
- [EF Core Change Tracker](https://learn.microsoft.com/en-us/ef/core/change-tracking/)

## Key Principles

- **Audit is separate from logging** — audit events are business-critical records, not debug logs.
- **Immutable and append-only** — audit records must never be updated or deleted.
- **Who, What, When, Where** — every audit entry captures the actor, action, timestamp, and target.
- **Sensitive data handling** — store what changed but redact PII where required.
- **Async and non-blocking** — audit writes must not slow down the primary operation.

## Procedure

1. Define the `AuditEntry` domain model (see [audit model sample](./samples/audit-entry.cs))
2. Implement an EF Core `SaveChangesInterceptor` for automatic entity tracking
3. Add an audit middleware for HTTP request-level actions
4. Store audit records in a dedicated, append-only table
5. Include the actor (user ID), action type, entity type, entity ID, timestamp, and changes
6. Redact sensitive fields (passwords, tokens, PII) before persisting
7. Add retention policies and archival for compliance

## Audit Entry Structure

| Field | Type | Description |
|-------|------|-------------|
| `Id` | `string` | Unique audit entry ID |
| `Timestamp` | `DateTimeOffset` | When the action occurred (UTC) |
| `UserId` | `string` | Who performed the action |
| `Action` | `string` | Create, Update, Delete, Access, Login, etc. |
| `EntityType` | `string` | The type of entity affected |
| `EntityId` | `string` | The ID of the entity affected |
| `Changes` | `string?` | JSON diff of old/new values (for updates) |
| `IpAddress` | `string?` | Client IP address |
| `CorrelationId` | `string?` | Request correlation ID |

## Constraints

- NEVER update or delete audit records
- ALWAYS capture the acting user from the authentication context
- ALWAYS use UTC timestamps
- ALWAYS redact sensitive fields before persisting
- NEVER let audit failures break the primary operation (use try/catch)
