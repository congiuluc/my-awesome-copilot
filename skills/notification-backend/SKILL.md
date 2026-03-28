---
name: notification-backend
description: >-
  Implement backend real-time notifications with SignalR, push patterns, and
  notification persistence. Use when: setting up SignalR hubs, broadcasting
  events to connected clients, sending targeted user notifications, or building
  a notification storage and retrieval system.
argument-hint: 'Describe the notification scenario (broadcast, user-targeted, event-driven).'
---

# Backend Notifications (.NET / SignalR)

## When to Use

- Sending real-time push notifications to connected frontend clients
- Broadcasting system-wide events (maintenance, announcements)
- Sending targeted notifications to specific users or groups
- Building a notification history/inbox with persistence
- Triggering notifications from background jobs or domain events

## Official Documentation

- [ASP.NET Core SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction)
- [SignalR Hubs](https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs)
- [SignalR Groups](https://learn.microsoft.com/en-us/aspnet/core/signalr/groups)
- [SignalR Authentication](https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz)
- [Background Services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)

## Recommended Libraries

| Library | Purpose |
|---------|---------|
| **Microsoft.AspNetCore.SignalR** | Real-time push (built-in) |
| **MediatR** | Domain event → notification dispatch |
| **Hangfire / Quartz.NET** | Scheduled notification delivery |

## Procedure

1. Set up SignalR hub — see [SignalR patterns](./references/signalr-patterns.md)
2. Review [notification hub sample](./samples/notification-hub-sample.cs)
3. Define notification types in Core layer
4. Register hub endpoint in `Program.cs`
5. Configure CORS for SignalR WebSocket/SSE
6. Implement `INotificationService` for sending from any service
7. Add user/group targeting for scoped notifications
8. Persist notifications for offline users (optional)
9. Add structured logging for all notification dispatches
10. Test with integration tests using `HubConnection` client

## Architecture

```
Domain Event → INotificationService → SignalR Hub → Connected Clients
                                   ↘ Repository (persist for offline)
```
