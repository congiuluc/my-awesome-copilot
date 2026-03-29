// Sample: Audit entry model and EF Core SaveChanges interceptor
// Demonstrates automatic entity change tracking for audit trails.

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MyApp.Infrastructure.Audit;

#region Audit Entry Model

/// <summary>
/// Represents an immutable audit trail entry for entity changes.
/// </summary>
public record AuditEntry
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public required string UserId { get; init; }
    public required string Action { get; init; }
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public string? Changes { get; init; }
    public string? IpAddress { get; init; }
    public string? CorrelationId { get; init; }
}

#endregion

#region SaveChanges Interceptor

/// <summary>
/// EF Core interceptor that automatically creates audit entries for tracked entity changes.
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly HashSet<string> _sensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "PasswordHash", "Token", "Secret", "ApiKey"
    };

    public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null) return ValueTask.FromResult(result);

        var auditEntries = new List<AuditEntry>();
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value ?? "system";
        var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        var correlationId = _httpContextAccessor.HttpContext?.Response?.Headers["X-Correlation-ID"]
            .FirstOrDefault();

        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry
            {
                UserId = userId,
                Action = entry.State.ToString(),
                EntityType = entry.Entity.GetType().Name,
                EntityId = GetPrimaryKey(entry),
                Changes = entry.State == EntityState.Modified ? GetChanges(entry) : null,
                IpAddress = ipAddress,
                CorrelationId = correlationId,
            };
            auditEntries.Add(auditEntry);
        }

        if (auditEntries.Count > 0)
        {
            eventData.Context.Set<AuditEntry>().AddRange(auditEntries);
        }

        return ValueTask.FromResult(result);
    }

    private static string GetPrimaryKey(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var key = entry.Properties
            .Where(p => p.Metadata.IsPrimaryKey())
            .Select(p => p.CurrentValue?.ToString() ?? "")
            .FirstOrDefault();
        return key ?? "unknown";
    }

    private static string? GetChanges(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var changes = new Dictionary<string, object?>();
        foreach (var property in entry.Properties.Where(p => p.IsModified))
        {
            var name = property.Metadata.Name;
            if (_sensitiveFields.Contains(name))
            {
                changes[name] = new { Old = "***REDACTED***", New = "***REDACTED***" };
            }
            else
            {
                changes[name] = new { Old = property.OriginalValue, New = property.CurrentValue };
            }
        }
        return changes.Count > 0 ? JsonSerializer.Serialize(changes) : null;
    }
}

#endregion
