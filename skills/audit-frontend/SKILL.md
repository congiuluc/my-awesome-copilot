---
name: audit-frontend
description: >-
  Implement frontend audit logging for React applications. Covers user action
  tracking, page view logging, consent-aware analytics, audit event shipping
  to backend, and privacy-compliant tracking. Use when: tracking user actions
  for compliance, logging page views, implementing consent management, or
  shipping audit events to a backend service.
argument-hint: 'Describe the audit requirement: action tracking, page view logging, consent, or event shipping.'
---

# Frontend Audit Logging (React)

## When to Use

- Tracking user actions for compliance or analytics
- Logging page views and navigation events
- Implementing consent-aware tracking (GDPR cookie consent)
- Shipping audit events to a backend collection endpoint
- Reviewing audit coverage in a React application

## Key Principles

- **Privacy first** — only track after user consent; honor opt-out preferences.
- **Minimal data** — capture action type and context, not raw user input.
- **Never track PII** — no passwords, tokens, emails, or personal identifiers in audit events.
- **Batch and debounce** — ship events in batches, not on every click.
- **Graceful degradation** — audit failures must never break the UI.

## Procedure

1. Create an `AuditService` (see [audit service sample](./samples/audit-service.ts))
2. Track meaningful user actions: form submissions, navigation, setting changes
3. Use a `useAuditAction` hook for declarative tracking in components
4. Batch events and ship to a backend endpoint periodically
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
