---
description: "Use when configuring .NET Aspire orchestration, OpenTelemetry instrumentation, OTLP exporters, distributed tracing, metrics, or structured logging for local observability."
applyTo: "src/MyApp.AppHost/**,src/MyApp.ServiceDefaults/**"
---
# Aspire & OpenTelemetry Guidelines

## Aspire Orchestration

- **AppHost** is the orchestrator — it discovers, wires, and launches all services.
- Every service project must call `builder.AddServiceDefaults()` in `Program.cs`.
- Every service must call `app.MapDefaultEndpoints()` to expose health and liveness.
- Dashboard is available at `https://localhost:18888` — verify traces, metrics, and logs there.

## Service Defaults

- `AddServiceDefaults()` registers OpenTelemetry, health checks, and resilience.
- Do not duplicate OTel registration in individual services — rely on shared defaults.
- Configure custom metrics and traces **in addition to** defaults, not instead of.

## OpenTelemetry

- Use OTLP exporter for traces, metrics, and logs.
- Add `ActivitySource` for custom spans in domain-critical operations.
- Propagate `Activity.Current` through async call chains — do not create orphan spans.
- Name spans with `{Service}.{Operation}` convention (e.g., `OrderService.CreateOrder`).

## Health Checks

- Register health checks for all external dependencies (database, cache, message broker).
- Use `/health` for readiness and `/alive` for liveness.
- Health check timeout: max 5 seconds per dependency.

## Resource Registration

```csharp
// AppHost Program.cs pattern
var cosmos = builder.AddAzureCosmosDB("cosmos");
var api = builder.AddProject<Projects.MyApp_Api>("api")
    .WithReference(cosmos);
```

- Always name resources with lowercase kebab-case identifiers.
- Use `.WithReference()` to wire dependencies — Aspire injects connection strings automatically.
