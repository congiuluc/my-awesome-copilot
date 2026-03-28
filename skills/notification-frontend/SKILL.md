---
name: notification-frontend
description: >-
  Implement frontend toast notifications and alert systems with accessible,
  stackable, auto-dismissing patterns. Use when: showing success/error/info
  toasts, building a notification center, creating alert banners, or integrating
  real-time notifications from SignalR/WebSocket.
argument-hint: 'Describe the notification type (toast, banner, real-time) and trigger.'
---

# Frontend Notifications (React)

## When to Use

- Showing success/error/warning/info toast messages after actions
- Building a notification center or inbox
- Displaying persistent alert banners
- Handling real-time push notifications from backend (SignalR/WebSocket)
- Showing form submission confirmations

## Official Documentation

- [Sonner (React Toast Library)](https://sonner.emilkowal.ski/)
- [React Hot Toast](https://react-hot-toast.com/)
- [WAI-ARIA Alert Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/alert/)
- [WAI-ARIA Status Role](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/Roles/status_role)

## Recommended Libraries

| Library | Best For | Size |
|---------|----------|------|
| **Sonner** | Modern toast system, unstyled/styled | ~5 KB |
| **React Hot Toast** | Simple toast with promise support | ~5 KB |
| **Custom** | Full control, minimal dependency | 0 KB |

## Procedure

1. Choose approach — see [notification patterns](./references/notification-patterns.md)
2. Review [toast implementation sample](./samples/toast-system-sample.tsx)
3. Install library: `npm install sonner` (recommended)
4. Add `<Toaster />` to app root layout
5. Use `toast.success()`, `toast.error()`, etc. in event handlers
6. Ensure all toasts are accessible (`role="status"` or `role="alert"`)
7. Support keyboard dismissal (Escape key)
8. Auto-dismiss after 4-5 seconds (keep error toasts longer or manual)
9. Stack multiple toasts without overlapping content
10. For real-time: connect to SignalR hub and pipe server events to toasts

## Accessibility Rules

- Success/info: `role="status"` with `aria-live="polite"`
- Error/warning: `role="alert"` with `aria-live="assertive"`
- Toast must be dismissible via Escape key or close button
- Close button must have `aria-label="Dismiss notification"`
- Toast text must meet 4.5:1 contrast ratio
- Never rely solely on color — include icons with the message type
