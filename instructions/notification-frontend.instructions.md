---
description: "Use when implementing frontend toast notifications, alert systems, or real-time notification integration. Covers accessible toasts, auto-dismiss, SignalR client, and notification center patterns."
applyTo: "src/web-app/src/components/notification*/**,src/web-app/src/components/toast*/**,src/web-app/src/components/Notification*/**,src/web-app/src/components/Toast*/**"
---
# Frontend Notification Guidelines

## Toast Library

- Use **Sonner** as the toast library (~5 KB, accessible, stackable).
- Place `<Toaster />` once in the app root layout.

```tsx
import { toast } from 'sonner';
toast.success('Profile updated');
toast.error('Failed to save changes');
```

## Auto-Dismiss & Duration

- Success/info toasts: auto-dismiss after **4-5 seconds**.
- Error/warning toasts: keep visible **longer** (8-10 seconds) or until dismissed.
- All toasts must be dismissible via close button or **Escape** key.

## Accessibility

| Type | ARIA Role | Live Region |
|------|-----------|-------------|
| Success / Info | `role="status"` | `aria-live="polite"` |
| Error / Warning | `role="alert"` | `aria-live="assertive"` |

- Maintain **4.5:1** contrast ratio for toast text.
- Stack multiple toasts without overlapping — newest on top.
- Screen readers must announce toast content automatically.

## SignalR Client Integration

- Initialize SignalR connection once at app startup.
- Listen for `ReceiveNotification` events and dispatch to toast or notification center.
- Handle reconnection with exponential backoff.
- Show a subtle connection status indicator when disconnected.

## Notification Center (Optional)

- Display persistent notifications in a dropdown or sidebar panel.
- Show unread count as a badge on the bell icon.
- Mark notifications as read on open/click.
- Load history with infinite scroll or pagination.
