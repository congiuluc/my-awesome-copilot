---
name: audit-backend-java
description: >-
  Implement audit logging and audit trails for Java Spring Boot applications.
  Covers JPA entity listeners, Hibernate Envers, user action logging, audit
  interceptors, and compliance patterns. Use when: adding audit trails to
  data changes, logging user actions, implementing compliance requirements
  (GDPR, SOC2, HIPAA), or reviewing audit coverage in Java.
argument-hint: 'Describe the audit requirement: entity tracking, action logging, compliance, or review.'
---

# Audit Logging (Java Spring Boot)

## When to Use

- Adding audit trails for entity create/update/delete operations
- Logging user actions (login, permission changes, data access)
- Implementing compliance audit requirements (GDPR, SOC2, HIPAA)
- Reviewing existing audit coverage for gaps
- Configuring Hibernate Envers for historical entity versioning

## Official Documentation

- [Spring Data JPA Auditing](https://docs.spring.io/spring-data/jpa/reference/auditing.html)
- [Hibernate Envers](https://hibernate.org/orm/envers/)
- [JPA Entity Listeners](https://jakarta.ee/specifications/persistence/3.1/)

## Key Principles

- **Audit is separate from logging** — audit events are business-critical records, not debug logs.
- **Immutable and append-only** — audit records must never be updated or deleted.
- **Who, What, When, Where** — every audit entry captures the actor, action, timestamp, and target.
- **Sensitive data handling** — store what changed but redact PII where required.
- **Async and non-blocking** — audit writes must not slow down the primary operation.

## Procedure

1. Enable Spring Data JPA Auditing (`@EnableJpaAuditing`)
2. Add `@CreatedBy`, `@LastModifiedBy`, `@CreatedDate`, `@LastModifiedDate` to base entities
3. Implement an `AuditorAware<String>` bean for the current user
4. For full change tracking, add a JPA `@EntityListener` (see [audit listener sample](./samples/AuditEntityListener.java))
5. Alternatively, use Hibernate Envers for automatic entity versioning
6. Store audit records in a dedicated, append-only table
7. Redact sensitive fields before persisting

## Audit Entry Structure

| Field | Type | Description |
|-------|------|-------------|
| `id` | `String` | Unique audit entry ID (UUID) |
| `timestamp` | `Instant` | When the action occurred (UTC) |
| `userId` | `String` | Who performed the action |
| `action` | `String` | CREATE, UPDATE, DELETE, ACCESS, LOGIN, etc. |
| `entityType` | `String` | The type of entity affected |
| `entityId` | `String` | The ID of the entity affected |
| `changes` | `String` | JSON diff of old/new values (for updates) |
| `ipAddress` | `String` | Client IP address |
| `correlationId` | `String` | Request correlation ID from MDC |

## Constraints

- NEVER update or delete audit records
- ALWAYS capture the acting user from the SecurityContext
- ALWAYS use UTC timestamps (`Instant`)
- ALWAYS redact sensitive fields before persisting
- NEVER let audit failures break the primary operation (use try/catch)
