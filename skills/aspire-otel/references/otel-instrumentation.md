# OpenTelemetry Instrumentation

## Via Aspire ServiceDefaults (Recommended)

The ServiceDefaults project configures OpenTelemetry automatically. The generated
`Extensions.cs` provides traces, metrics, and structured logs out of the box.

```csharp
public static IHostApplicationBuilder ConfigureOpenTelemetry(
    this IHostApplicationBuilder builder)
{
    // Structured logging via OpenTelemetry
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
```

## Manual Setup (Without Aspire SDK)

Install NuGet packages:

```bash
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
dotnet add package OpenTelemetry.Instrumentation.Runtime
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
```

Configure in `Program.cs`:

```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
        resource.AddService(serviceName: "my-api"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddOtlpExporter(opts =>
               {
                   opts.Endpoint = new Uri("http://localhost:4317");
                   opts.Protocol = OtlpExportProtocol.Grpc;
               });
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddRuntimeInstrumentation()
               .AddOtlpExporter(opts =>
               {
                   opts.Endpoint = new Uri("http://localhost:4317");
                   opts.Protocol = OtlpExportProtocol.Grpc;
               });
    });

// Structured logs
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.AddOtlpExporter(opts =>
    {
        opts.Endpoint = new Uri("http://localhost:4317");
        opts.Protocol = OtlpExportProtocol.Grpc;
    });
});
```

## Custom Activity Sources

Create custom traces for domain operations:

```csharp
using System.Diagnostics;

public static class DiagnosticsConfig
{
    public static readonly ActivitySource Source = new("MyApp.Api");
}

// Usage in a service method:
public async Task<ProductDto> CreateAsync(
    CreateProductRequest request,
    CancellationToken cancellationToken)
{
    using var activity = DiagnosticsConfig.Source.StartActivity("CreateProduct");
    activity?.SetTag("product.name", request.Name);

    var product = await _repository.AddAsync(entity, cancellationToken);

    activity?.SetTag("product.id", product.Id);
    return product.ToDto();
}
```

Register the custom source:

```csharp
.WithTracing(tracing =>
{
    tracing.AddSource("MyApp.Api")  // Must match ActivitySource name
           .AddAspNetCoreInstrumentation()
           .AddHttpClientInstrumentation();
})
```

## Custom Metrics

```csharp
using System.Diagnostics.Metrics;

public static class AppMetrics
{
    private static readonly Meter Meter = new("MyApp.Api");

    public static readonly Counter<long> ProductsCreated =
        Meter.CreateCounter<long>("products.created", "count", "Products created");

    public static readonly Histogram<double> RequestDuration =
        Meter.CreateHistogram<double>("request.duration", "ms", "Request duration");
}

// Usage:
AppMetrics.ProductsCreated.Add(1, new KeyValuePair<string, object?>("category", "widgets"));
```

## Environment Variables

| Variable | Description | Default |
|----------|------------|---------|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | OTLP collector endpoint | `http://localhost:4317` |
| `OTEL_EXPORTER_OTLP_PROTOCOL` | Protocol: `grpc` or `http/protobuf` | `grpc` |
| `OTEL_SERVICE_NAME` | Service name in traces | Application name |
| `OTEL_RESOURCE_ATTRIBUTES` | Additional resource attributes | — |
