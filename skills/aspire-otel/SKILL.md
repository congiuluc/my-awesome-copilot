---
name: aspire-otel
description: >-
  Set up .NET Aspire dashboard and configure OpenTelemetry for local development
  monitoring. Use when: adding Aspire orchestration, OpenTelemetry instrumentation,
  OTLP exporter, distributed tracing, metrics, structured logging, Aspire dashboard,
  service defaults, or local observability.
argument-hint: 'Describe what to set up: Aspire dashboard, OpenTelemetry instrumentation, or both'
---

# .NET Aspire & OpenTelemetry — Local Development Monitoring

## When to Use

- Adding .NET Aspire orchestration to an existing or new solution
- Setting up the Aspire Dashboard for local development monitoring
- Instrumenting .NET services with OpenTelemetry (traces, metrics, logs)
- Configuring OTLP exporters for telemetry collection
- Running Aspire Dashboard standalone via Docker
- Troubleshooting missing telemetry or dashboard connectivity

## Official Documentation

- [.NET Aspire Overview](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview)
- [.NET Aspire Dashboard](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/overview)
- [Aspire Service Defaults](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-defaults)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/dotnet/)
- [OpenTelemetry in .NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/telemetry)
- [Aspire Dashboard Standalone Mode](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/standalone)

## Procedure

1. Load [Aspire scaffolding reference](./references/aspire-scaffold.md)
2. Load [OpenTelemetry instrumentation reference](./references/otel-instrumentation.md)
3. Load [Dashboard configuration reference](./references/dashboard-config.md)
4. Review [sample AppHost](./samples/apphost-sample.cs) and [sample ServiceDefaults](./samples/service-defaults-sample.cs)
5. Determine scope: Aspire orchestration, standalone dashboard, or manual OTel
6. Implement based on decision flow below
7. Verify telemetry appears in dashboard (traces, metrics, logs)

## Decision Flow

```
User request
  ├─ "Add Aspire to my solution"
  │    → Phase 1: Scaffold AppHost + ServiceDefaults
  │    → Phase 2: Configure dashboard (auto with AppHost)
  │    → Phase 3: Wire up OpenTelemetry via ServiceDefaults
  │
  ├─ "Run Aspire Dashboard standalone"
  │    → Phase 2: Docker standalone dashboard
  │    → Phase 3: Manual OpenTelemetry setup (no Aspire SDK)
  │
  ├─ "Add OpenTelemetry only"
  │    → Phase 3: Manual OTel instrumentation
  │
  └─ "Full local observability"
       → Phase 1 + 2 + 3 + health checks
```

## Completion Checklist

- [ ] AppHost project created and references all service projects (if using Aspire)
- [ ] ServiceDefaults project created (if using Aspire)
- [ ] All services call `builder.AddServiceDefaults()` and `app.MapDefaultEndpoints()`
- [ ] Dashboard accessible at `https://localhost:18888`
- [ ] Traces, metrics, and logs visible in dashboard
- [ ] Health check endpoints registered (`/health`, `/alive`)
- [ ] `dotnet build` succeeds for the entire solution
