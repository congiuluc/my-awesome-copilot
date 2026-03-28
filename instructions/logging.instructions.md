---
description: "Use when configuring Serilog, logging sinks, structured log context, correlation IDs, or sensitive data filtering."
applyTo: "src/MyApp.Api/Program.cs,src/MyApp.Api/Middleware/**,**/appsettings*.json"
---
# Logging Guidelines (Serilog)

## Structured Logging

- Always use **message templates** with named placeholders — never string interpolation.
  ```csharp
  // ✅ logger.LogInformation("Order {OrderId} created by {UserId}", orderId, userId);
  // ❌ logger.LogInformation($"Order {orderId} created by {userId}");
  ```
- Include relevant context: entity IDs, user IDs, operation names.

## Log Levels

| Level | Use For |
|-------|---------|
| `Information` | Normal operations (request started, entity created) |
| `Warning` | Expected but notable (not found, validation failed, slow query > 200ms) |
| `Error` | Unexpected failures (unhandled exceptions, external service errors) |

- Never log at `Information` for high-frequency per-record operations.

## Sensitive Data

- **Never log**: passwords, tokens, API keys, PII, credit card numbers.
- Override noisy namespaces to `Warning`: `Microsoft.AspNetCore`, `Microsoft.EntityFrameworkCore`.
- Use `[LogMasked]` or destructuring exclusions for sensitive properties.

## Correlation

- Every request must carry a correlation ID in `X-Correlation-ID` header.
- Push correlation ID to Serilog `LogContext` for all log entries.
- Return correlation ID in response headers for client troubleshooting.

## Exception Logging

- Always log the full exception object: `logger.LogError(ex, "Failed to process {OrderId}", orderId)`.
- Never swallow exceptions silently.
- Log at `Warning` for expected errors, `Error` for unexpected.
