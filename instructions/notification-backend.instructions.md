---
description: "Use when implementing backend real-time notifications with SignalR. Covers hub patterns, user/group targeting, notification persistence, and structured logging."
applyTo: "src/MyApp.Api/Hubs/**,src/MyApp.Core/**/Notification*,src/MyApp.Infrastructure/**/Notification*"
---
# Backend Notification Guidelines (SignalR)

## Hub Design

- Use **strongly-typed hubs** (`Hub<INotificationClient>`) for compile-time safety.
- Define a client interface with all push methods:
  ```csharp
  public interface INotificationClient
  {
      Task ReceiveNotification(NotificationDto notification);
      Task ReceiveSystemAlert(string message);
  }
  ```
- Keep hub methods thin — delegate business logic to services.

## Sending Notifications

- Inject `IHubContext<NotificationHub, INotificationClient>` into services.
- Use `Clients.User(userId)` for targeted notifications.
- Use `Clients.Group(groupName)` for scoped broadcasts.
- Use `Clients.All` only for system-wide announcements.

## Architecture Pattern

```
Domain Event → INotificationService → IHubContext → Connected Clients
```

- Services never reference the hub directly — always go through `IHubContext`.
- Log every notification dispatch with structured context (recipient, type, correlation ID).

## Connection Management

- Configure CORS for SignalR WebSocket and Server-Sent Events transports.
- Add `[Authorize]` on the hub class for authenticated connections.
- Map user connections using `Context.UserIdentifier` (from claims).
- Handle reconnection gracefully — clients should retry with exponential backoff.

## Notification Persistence (Optional)

- Persist notifications for offline users — deliver on reconnect.
- Mark notifications as read/unread with timestamps.
- Implement pagination for notification history.
