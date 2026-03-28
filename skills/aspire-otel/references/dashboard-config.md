# Dashboard Configuration

## Integrated Mode (via AppHost)

Dashboard launches automatically when running the AppHost:

```bash
dotnet run --project {App}.AppHost
```

- **Dashboard URL**: `https://localhost:18888` (default)
- **OTLP endpoint**: Auto-configured for all referenced services

## Standalone Mode (Docker)

Run the dashboard as a standalone container for non-Aspire apps:

```bash
docker run --rm -it -d \
    -p 18888:18888 \
    -p 4317:18889 \
    --name aspire-dashboard \
    mcr.microsoft.com/dotnet/aspire-dashboard:latest
```

- **Dashboard UI**: `http://localhost:18888`
- **OTLP gRPC endpoint**: `http://localhost:4317`

Services send telemetry to `http://localhost:4317` via OTLP gRPC.

## Standalone with Configuration

```bash
docker run --rm -it -d \
    -p 18888:18888 \
    -p 4317:18889 \
    -e DASHBOARD__FRONTEND__AUTHMODE=Unsecured \
    -e DASHBOARD__OTLP__AUTHMODE=ApiKey \
    -e DASHBOARD__OTLP__PRIMARYAPIKEY=my-dev-key \
    --name aspire-dashboard \
    mcr.microsoft.com/dotnet/aspire-dashboard:latest
```

## Docker Compose (Standalone Dashboard)

```yaml
services:
  aspire-dashboard:
    image: mcr.microsoft.com/dotnet/aspire-dashboard:latest
    ports:
      - "18888:18888"    # Dashboard UI
      - "4317:18889"     # OTLP gRPC ingestion
    environment:
      - DASHBOARD__FRONTEND__AUTHMODE=Unsecured
    restart: unless-stopped
```

## Authentication Options

| Mode | `DASHBOARD__FRONTEND__AUTHMODE` | Use Case |
|------|--------------------------------|----------|
| `BrowserToken` | Default — token in container logs | Local dev |
| `Unsecured` | No authentication | Trusted networks / local dev |
| `OpenIdConnect` | OIDC provider (Entra ID, etc.) | Production / shared |

## Dashboard Features

| Tab | Shows |
|-----|-------|
| **Structured Logs** | All `ILogger` output with structured properties |
| **Traces** | Distributed traces across services (waterfall view) |
| **Metrics** | ASP.NET Core, HTTP client, runtime, and custom metrics |
| **Resources** | Running services with health status |

## Troubleshooting

| Issue | Fix |
|-------|-----|
| No traces in dashboard | Check `OTEL_EXPORTER_OTLP_ENDPOINT` points to dashboard's OTLP port |
| Dashboard not starting | Ensure Docker is running, port 18888 not in use |
| "Connection refused" | Verify OTLP port mapping: host 4317 → container 18889 |
| Missing custom metrics | Register custom `Meter` name in `.WithMetrics()` |
| Token auth prompt | Check container logs for browser token value |
