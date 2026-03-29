// Sample: Serilog bootstrap with correlation ID middleware
// Complete pattern for structured logging in .NET 10 Minimal API.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.Diagnostics;

namespace MyApp.Api;

/// <summary>
/// Configures Serilog structured logging with console/file sinks and correlation IDs.
/// </summary>
public static class LoggingSample
{
    #region Bootstrap

    /// <summary>
    /// Configures Serilog on the host builder with console, file sinks and enrichment.
    /// </summary>
    public static void ConfigureLogging(WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, config) => config
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Application", "MyApp.Api")
            .Filter.ByExcluding(e =>
                e.Properties.ContainsKey("RequestPath")
                && e.Properties["RequestPath"].ToString().Contains("/health"))
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj} " +
                "{Properties:j}{NewLine}{Exception}")
            .WriteTo.File("logs/app-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                restrictedToMinimumLevel: LogEventLevel.Information));
    }

    #endregion

    #region Correlation ID Middleware

    /// <summary>
    /// Adds correlation ID from request header or generates a new one.
    /// Pushes it to Serilog LogContext for all downstream log entries.
    /// </summary>
    public static void UseCorrelationId(WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                ?? Activity.Current?.Id
                ?? Guid.NewGuid().ToString();

            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                context.Response.Headers["X-Correlation-ID"] = correlationId;
                await next();
            }
        });
    }

    #endregion

    #region Usage Examples

    /// <summary>
    /// Demonstrates structured logging best practices in a service method.
    /// </summary>
    public static void LoggingExamples(ILogger<object> logger)
    {
        var orderId = "ORD-123";
        var userId = "USR-456";

        // ✅ Good — structured with named placeholders
        logger.LogInformation("Order {OrderId} created for user {UserId}", orderId, userId);

        // ✅ Good — with scoped context
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["OrderId"] = orderId,
            ["UserId"] = userId,
        }))
        {
            logger.LogInformation("Processing order");
            logger.LogDebug("Validating order items");
        }

        // ❌ Bad — string interpolation (breaks structured logging)
        // logger.LogInformation($"Order {orderId} created for user {userId}");
    }

    #endregion
}
