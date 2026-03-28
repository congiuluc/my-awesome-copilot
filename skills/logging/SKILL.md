---
name: logging
description: "Configure structured logging with Serilog for .NET applications. Covers sink configuration, enrichment, correlation IDs, log levels, sensitive data filtering, and diagnostic context. Use when: setting up Serilog, configuring log sinks, adding structured context to log entries, filtering sensitive data, or troubleshooting logging issues."
argument-hint: 'Describe the logging requirement: sink setup, enrichment, filtering, or structured context.'
---

# Structured Logging with Serilog

## When to Use

- Setting up Serilog in a new .NET project
- Configuring log sinks (console, file, Seq, Application Insights)
- Adding structured context (correlation IDs, user IDs, operation names)
- Filtering sensitive data from log output
- Adjusting log levels per namespace
- Troubleshooting log output issues

## Official Documentation

- [Serilog Wiki](https://github.com/serilog/serilog/wiki)
- [Serilog.AspNetCore](https://github.com/serilog/serilog-aspnetcore)
- [Serilog Enrichers](https://github.com/serilog/serilog/wiki/Enrichment)

## Key Principles

- **Structured, not string-formatted** — use message templates with named placeholders.
- **Context over messages** — enrich logs with properties, not long descriptions.
- **Sensitive data never logged** — filter passwords, tokens, PII at the sink level.
- **Correlation is mandatory** — every request gets a correlation ID for tracing.
- **Levels are meaningful** — use the right level for the right situation.

## Procedure

### 1. Bootstrap Configuration

```csharp
builder.Host.UseSerilog((context, services, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("Application", "MyApp.Api")
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30));
```

### 2. Log Levels Guide

| Level | Use When | Examples |
|-------|----------|---------|
| `Verbose` | Internal framework detail | EF Core query SQL |
| `Debug` | Developer diagnostics | Cache hit/miss, config loaded |
| `Information` | Normal operations | Request started, user logged in |
| `Warning` | Expected but notable | Not found, validation failed, slow query |
| `Error` | Unexpected failures | Unhandled exceptions, external service down |
| `Fatal` | App cannot continue | Database unreachable, startup failure |

### 3. Structured Logging Patterns

```csharp
// ✅ Good — structured with named placeholders
logger.LogInformation("Order {OrderId} created for user {UserId}", orderId, userId);

// ❌ Bad — string interpolation destroys structure
logger.LogInformation($"Order {orderId} created for user {userId}");
```

### 4. Correlation ID Middleware

```csharp
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
        ?? Activity.Current?.Id
        ?? Guid.NewGuid().ToString();
    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        await next();
    }
});
```

### 5. Sensitive Data Filtering

```csharp
// In appsettings.json: override noisy or sensitive namespaces
"Serilog": {
    "MinimumLevel": {
        "Default": "Information",
        "Override": {
            "Microsoft.AspNetCore": "Warning",
            "Microsoft.EntityFrameworkCore.Database.Command": "Warning",
            "System.Net.Http.HttpClient": "Warning"
        }
    }
}
```

- Never log: passwords, tokens, API keys, credit cards, SSNs.
- Use `[LogMasked]` or destructuring policies for sensitive properties.
- Set `Microsoft.AspNetCore` to `Warning` to suppress noisy request logs.

### 6. Request Logging Middleware

```csharp
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("UserId",
            httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous");
    };
});
```

## Anti-Patterns

- ❌ `logger.LogInformation($"User {userId}")` — kills structured logging
- ❌ `catch (Exception ex) { /* silently swallowed */ }` — always log or rethrow
- ❌ `logger.LogError(ex.Message)` — log the full exception: `logger.LogError(ex, "...")`
- ❌ Logging at `Information` level for high-frequency operations (per-record in batch)
