# Aspire Scaffolding

## Phase 1: Add Aspire to an Existing Solution

### 1.1 Create AppHost Project

```bash
dotnet new aspire-apphost -n {App}.AppHost
dotnet sln add {App}.AppHost/{App}.AppHost.csproj
```

### 1.2 Create ServiceDefaults Project

```bash
dotnet new aspire-servicedefaults -n {App}.ServiceDefaults
dotnet sln add {App}.ServiceDefaults/{App}.ServiceDefaults.csproj
```

### 1.3 Wire Up References

AppHost must reference every service project:

```bash
dotnet add {App}.AppHost reference ../src/{App}.Api/{App}.Api.csproj
```

Every service project must reference ServiceDefaults:

```bash
dotnet add src/{App}.Api reference {App}.ServiceDefaults/{App}.ServiceDefaults.csproj
```

### 1.4 Solution Structure (After Aspire)

```
src/
  {App}.Api/              # Minimal API project
  {App}.Core/             # Domain models, interfaces
  {App}.Infrastructure/   # Repository implementations
  {frontend-app}/         # React frontend
{App}.AppHost/            # Aspire orchestration host
{App}.ServiceDefaults/    # Shared OpenTelemetry + health checks
tests/
```

### 1.5 Configure AppHost Program.cs

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add your API project
var api = builder.AddProject<Projects.{App}_Api>("api");

// Add frontend (if containerized)
// var web = builder.AddNpmApp("web", "../src/{frontend-app}")
//     .WithReference(api)
//     .WithHttpEndpoint(port: 5173, targetPort: 5173);

builder.Build().Run();
```

### 1.6 Configure Each Service

In each service's `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (OpenTelemetry, health checks, resilience)
builder.AddServiceDefaults();

// ... other service registrations ...

var app = builder.Build();

// Map default health check endpoints
app.MapDefaultEndpoints();

// ... other middleware ...

app.Run();
```

## Prerequisites

- .NET 10.0+ SDK (or latest stable)
- Docker Desktop (for standalone dashboard or containerized services)
- `dotnet workload install aspire` (installs Aspire project templates)

## Useful Commands

```bash
# Install Aspire workload
dotnet workload install aspire

# Check installed workloads
dotnet workload list

# Run the AppHost (launches dashboard + all services)
dotnet run --project {App}.AppHost

# Update Aspire packages
dotnet workload update
```
