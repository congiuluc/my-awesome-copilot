// Sample: Aspire ServiceDefaults Extensions.cs
// Provides shared OpenTelemetry, health checks, and resilience configuration.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Shared defaults for all services in the Aspire orchestration.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Adds service defaults: OpenTelemetry, health checks, resilience, service discovery.
    /// Call in each service's Program.cs before building the app.
    /// </summary>
    public static IHostApplicationBuilder AddServiceDefaults(
        this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    /// <summary>
    /// Configures OpenTelemetry with traces, metrics, and structured logging.
    /// </summary>
    public static IHostApplicationBuilder ConfigureOpenTelemetry(
        this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                       .AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    /// <summary>
    /// Adds OTLP exporter if endpoint is configured.
    /// </summary>
    private static IHostApplicationBuilder AddOpenTelemetryExporters(
        this IHostApplicationBuilder builder)
    {
        var useOtlp = !string.IsNullOrWhiteSpace(
            builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlp)
        {
            builder.Services.AddOpenTelemetry()
                .UseOtlpExporter();
        }

        return builder;
    }

    /// <summary>
    /// Adds default health checks: ready and live probes.
    /// </summary>
    public static IHostApplicationBuilder AddDefaultHealthChecks(
        this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(),
                tags: ["live"]);

        return builder;
    }

    /// <summary>
    /// Maps health check endpoints. Call after building the app.
    /// </summary>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Readiness probe — all checks
        app.MapHealthChecks("/health");

        // Liveness probe — only "live" tagged checks
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        return app;
    }
}

// --- Usage in each service's Program.cs ---
//
// var builder = WebApplication.CreateBuilder(args);
// builder.AddServiceDefaults();   // <-- Add this
// // ... other registrations ...
//
// var app = builder.Build();
// app.MapDefaultEndpoints();      // <-- Add this
// // ... other middleware ...
// app.Run();
