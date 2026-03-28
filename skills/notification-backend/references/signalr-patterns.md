# SignalR Notification Patterns

> Official reference: [ASP.NET Core SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction)

## Setup

```bash
# SignalR is included in ASP.NET Core — no extra NuGet needed for server
# For typed hub clients, no additional packages required
```

### Program.cs Registration

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();

// CORS for SignalR (allow WebSocket + SSE)
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalR", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Frontend dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR
    });
});

var app = builder.Build();

app.UseCors("SignalR");
app.MapHub<NotificationHub>("/hubs/notifications");
app.Run();
```

## Hub Implementation

### Strongly-Typed Hub

```csharp
/// <summary>
/// Defines methods the server can invoke on connected clients.
/// </summary>
public interface INotificationClient
{
    /// <summary>
    /// Receives a notification message with type classification.
    /// </summary>
    Task ReceiveNotification(string message, string type);

    /// <summary>
    /// Receives a structured notification object.
    /// </summary>
    Task ReceiveStructuredNotification(NotificationDto notification);
}

/// <summary>
/// SignalR hub for real-time notifications.
/// </summary>
public class NotificationHub : Hub<INotificationClient>
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation(
            "Client connected: {ConnectionId}",
            Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            "Client disconnected: {ConnectionId}, Reason: {Reason}",
            Context.ConnectionId,
            exception?.Message ?? "Normal");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Allows authenticated users to join a notification group.
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation(
            "Connection {ConnectionId} joined group {Group}",
            Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Removes user from a notification group.
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
```

## Notification Service (Server-Side Dispatch)

```csharp
/// <summary>
/// Service interface for sending notifications from any backend component.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Broadcasts a notification to all connected clients.
    /// </summary>
    Task BroadcastAsync(
        string message,
        string type = "info",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to a specific user by their user ID.
    /// </summary>
    Task SendToUserAsync(
        string userId,
        string message,
        string type = "info",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to all users in a group.
    /// </summary>
    Task SendToGroupAsync(
        string groupName,
        string message,
        string type = "info",
        CancellationToken cancellationToken = default);
}

/// <summary>
/// SignalR-based implementation of the notification service.
/// </summary>
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<NotificationHub, INotificationClient> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task BroadcastAsync(
        string message,
        string type = "info",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Broadcasting notification: {Type} - {Message}", type, message);
        await _hubContext.Clients.All
            .ReceiveNotification(message, type);
    }

    public async Task SendToUserAsync(
        string userId,
        string message,
        string type = "info",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending notification to user {UserId}: {Type} - {Message}",
            userId, type, message);
        await _hubContext.Clients.User(userId)
            .ReceiveNotification(message, type);
    }

    public async Task SendToGroupAsync(
        string groupName,
        string message,
        string type = "info",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending notification to group {Group}: {Type} - {Message}",
            groupName, type, message);
        await _hubContext.Clients.Group(groupName)
            .ReceiveNotification(message, type);
    }
}
```

## Notification DTO

```csharp
/// <summary>
/// Structured notification data transfer object.
/// </summary>
public record NotificationDto
{
    /// <summary>Unique notification identifier.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Notification type: success, error, warning, info.</summary>
    public string Type { get; init; } = "info";

    /// <summary>Notification title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Notification message body.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>UTC timestamp of creation.</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Whether the notification has been read.</summary>
    public bool IsRead { get; init; }
}
```

## Usage from Any Service

```csharp
public class ProductService
{
    private readonly IRepository<Product> _repository;
    private readonly INotificationService _notifications;

    public ProductService(
        IRepository<Product> repository,
        INotificationService notifications)
    {
        _repository = repository;
        _notifications = notifications;
    }

    public async Task<Product> CreateAsync(
        CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        var product = new Product { Name = dto.Name };
        await _repository.AddAsync(product, cancellationToken);

        // Notify all connected clients
        await _notifications.BroadcastAsync(
            $"New product added: {product.Name}",
            "success",
            cancellationToken);

        return product;
    }
}
```

## Official References

- [SignalR Hub Protocol](https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs)
- [SignalR Groups](https://learn.microsoft.com/en-us/aspnet/core/signalr/groups)
- [SignalR Strongly-Typed Hubs](https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs#strongly-typed-hubs)
- [Send Messages from Outside a Hub](https://learn.microsoft.com/en-us/aspnet/core/signalr/hubcontext)
