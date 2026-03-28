using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

// =============================================================================
// Notification Hub — Strongly Typed
// =============================================================================

/// <summary>
/// Defines methods the server can invoke on connected clients.
/// </summary>
public interface INotificationClient
{
    /// <summary>Receives a simple notification with type.</summary>
    Task ReceiveNotification(string message, string type);

    /// <summary>Receives a structured notification object.</summary>
    Task ReceiveStructuredNotification(NotificationDto notification);
}

/// <summary>
/// Real-time notification hub using SignalR.
/// Supports broadcast, user-targeted, and group-targeted notifications.
/// </summary>
public class NotificationHub : Hub<INotificationClient>
{
    #region Fields

    private readonly ILogger<NotificationHub> _logger;

    #endregion

    #region Constructor

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    #endregion

    #region Connection Lifecycle

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation(
            "Client connected: {ConnectionId}", Context.ConnectionId);
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

    #endregion

    #region Group Management

    /// <summary>
    /// Adds the calling connection to a notification group.
    /// </summary>
    /// <param name="groupName">The group to join.</param>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation(
            "Connection {ConnectionId} joined group {Group}",
            Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Removes the calling connection from a notification group.
    /// </summary>
    /// <param name="groupName">The group to leave.</param>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation(
            "Connection {ConnectionId} left group {Group}",
            Context.ConnectionId, groupName);
    }

    #endregion
}

// =============================================================================
// Notification DTO
// =============================================================================

/// <summary>
/// Structured notification for client consumption.
/// </summary>
public record NotificationDto
{
    /// <summary>Unique notification identifier.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Notification type: success, error, warning, info.</summary>
    public string Type { get; init; } = "info";

    /// <summary>Short notification title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Notification message body.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>UTC timestamp of creation.</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Whether the notification has been read by the user.</summary>
    public bool IsRead { get; init; }
}

// =============================================================================
// Notification Service — Send from Any Backend Component
// =============================================================================

/// <summary>
/// Service for dispatching notifications via SignalR from any backend component.
/// Register as scoped: builder.Services.AddScoped&lt;INotificationService, SignalRNotificationService&gt;();
/// </summary>
public interface INotificationService
{
    /// <summary>Broadcasts to all connected clients.</summary>
    Task BroadcastAsync(
        string message,
        string type = "info",
        CancellationToken cancellationToken = default);

    /// <summary>Sends to a specific authenticated user.</summary>
    Task SendToUserAsync(
        string userId,
        string message,
        string type = "info",
        CancellationToken cancellationToken = default);

    /// <summary>Sends to all connections in a group.</summary>
    Task SendToGroupAsync(
        string groupName,
        string message,
        string type = "info",
        CancellationToken cancellationToken = default);
}

public class SignalRNotificationService : INotificationService
{
    #region Fields

    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    #endregion

    #region Constructor

    public SignalRNotificationService(
        IHubContext<NotificationHub, INotificationClient> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    #endregion

    #region INotificationService

    public async Task BroadcastAsync(
        string message,
        string type = "info",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Broadcasting notification: {Type} - {Message}", type, message);
        await _hubContext.Clients.All.ReceiveNotification(message, type);
    }

    public async Task SendToUserAsync(
        string userId,
        string message,
        string type = "info",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending to user {UserId}: {Type} - {Message}",
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
            "Sending to group {Group}: {Type} - {Message}",
            groupName, type, message);
        await _hubContext.Clients.Group(groupName)
            .ReceiveNotification(message, type);
    }

    #endregion
}
