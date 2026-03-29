---
name: audit-frontend-angular
description: >-
  Implement frontend audit logging for Angular applications. Covers user action
  tracking, page view logging, consent-aware analytics, and audit event shipping
  to backend. Use when: tracking user actions for compliance, logging page views,
  implementing consent management, or shipping audit events in an Angular app.
argument-hint: 'Describe the audit requirement: action tracking, page view logging, consent, or event shipping.'
---

# Frontend Audit Logging (Angular)

## When to Use

- Tracking user actions for compliance or analytics in Angular apps
- Logging page views and navigation events via Angular Router
- Implementing consent-aware tracking (GDPR cookie consent)
- Shipping audit events to a backend collection endpoint
- Reviewing audit coverage in an Angular application

## Key Principles

- **Privacy first** — only track after user consent; honor opt-out preferences.
- **Minimal data** — capture action type and context, not raw user input.
- **Never track PII** — no passwords, tokens, emails, or personal identifiers in audit events.
- **Batch and debounce** — ship events in batches, not on every click.
- **Graceful degradation** — audit failures must never break the UI.
- **Use Angular patterns** — services, interceptors, and Router events.

## Procedure

1. Create an `AuditService` (see [audit service sample](./samples/audit.service.ts))
2. Listen to Angular Router events for page view tracking
3. Track meaningful user actions: form submissions, navigation, setting changes
4. Batch events and ship to a backend endpoint periodically via `HttpClient`
5. Respect consent preferences before tracking
6. Never include PII or sensitive form data in audit events

## Audit Event Structure

| Field | Type | Description |
|-------|------|-------------|
| `eventId` | `string` | Unique event ID (UUID) |
| `timestamp` | `string` | ISO 8601 UTC timestamp |
| `action` | `string` | The user action (e.g., `page_view`, `form_submit`, `button_click`) |
| `page` | `string` | Current route/page |
| `context` | `object?` | Non-sensitive metadata (entity type, entity ID, etc.) |
| `sessionId` | `string` | Browser session ID |

## Constraints

- NEVER track before user consent is given
- NEVER include PII, passwords, or tokens in audit events
- ALWAYS batch events to reduce network calls
- ALWAYS handle shipping failures silently (no user-facing errors)
- ALWAYS respect `Do Not Track` headers and consent preferences
- ALWAYS use Angular `inject()` function — no constructor injection
- ALWAYS use `DestroyRef` + `takeUntilDestroyed()` for subscription cleanup
