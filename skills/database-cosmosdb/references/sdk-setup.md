# Cosmos DB SDK Setup

## NuGet Package

```xml
<PackageReference Include="Microsoft.Azure.Cosmos" Version="3.*" />
```

## CosmosClient Singleton

**CRITICAL**: `CosmosClient` MUST be registered as a singleton. Creating multiple instances degrades performance.

```csharp
// Program.cs — Register singleton CosmosClient
builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("CosmosDb")
        ?? throw new InvalidOperationException("CosmosDb connection string is required.");

    return new CosmosClient(connectionString, new CosmosClientOptions
    {
        ApplicationName = "MyApp",
        ConnectionMode = ConnectionMode.Direct,          // Always use Direct mode
        ConsistencyLevel = ConsistencyLevel.Session,      // Default; override per-request if needed
        MaxRetryAttemptsOnRateLimitedRequests = 9,
        MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),
        CosmosClientTelemetryOptions = new CosmosClientTelemetryOptions
        {
            DisableDistributedTracing = false              // Enable OpenTelemetry traces
        },
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    });
});
```

## Connection Strings

### Local Development with Emulator

```json
{
  "ConnectionStrings": {
    "CosmosDb": "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
  }
}
```

### Azure with Managed Identity (Preferred for Production)

```csharp
builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var endpoint = builder.Configuration["CosmosDb:Endpoint"]
        ?? throw new InvalidOperationException("CosmosDb endpoint is required.");

    return new CosmosClient(endpoint, new DefaultAzureCredential(), new CosmosClientOptions
    {
        ApplicationName = "MyApp",
        ConnectionMode = ConnectionMode.Direct
    });
});
```

```json
{
  "CosmosDb": {
    "Endpoint": "https://myaccount.documents.azure.com:443/",
    "DatabaseName": "MyDatabase",
    "ContainerName": "Items"
  }
}
```

## Container Reference Helper

```csharp
/// <summary>
/// Provides typed access to Cosmos DB containers.
/// </summary>
public class CosmosContainerFactory
{
    private readonly CosmosClient _cosmosClient;
    private readonly IConfiguration _configuration;

    public CosmosContainerFactory(CosmosClient cosmosClient, IConfiguration configuration)
    {
        _cosmosClient = cosmosClient;
        _configuration = configuration;
    }

    /// <summary>Gets the primary container.</summary>
    public Container GetContainer()
    {
        var databaseName = _configuration["CosmosDb:DatabaseName"]
            ?? throw new InvalidOperationException("CosmosDb:DatabaseName is required.");
        var containerName = _configuration["CosmosDb:ContainerName"]
            ?? throw new InvalidOperationException("CosmosDb:ContainerName is required.");

        return _cosmosClient.GetContainer(databaseName, containerName);
    }

    /// <summary>Gets a specific container by name.</summary>
    public Container GetContainer(string containerName)
    {
        var databaseName = _configuration["CosmosDb:DatabaseName"]
            ?? throw new InvalidOperationException("CosmosDb:DatabaseName is required.");

        return _cosmosClient.GetContainer(databaseName, containerName);
    }
}
```

Registration:

```csharp
builder.Services.AddSingleton<CosmosContainerFactory>();
```

## Health Check

```csharp
/// <summary>
/// Health check that verifies Cosmos DB connectivity.
/// </summary>
public class CosmosDbHealthCheck : IHealthCheck
{
    private readonly CosmosClient _cosmosClient;
    private readonly string _databaseName;

    public CosmosDbHealthCheck(CosmosClient cosmosClient, IConfiguration configuration)
    {
        _cosmosClient = cosmosClient;
        _databaseName = configuration["CosmosDb:DatabaseName"]
            ?? throw new InvalidOperationException("CosmosDb:DatabaseName is required.");
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _cosmosClient.GetDatabase(_databaseName);
            await database.ReadAsync(cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy("Cosmos DB is reachable.");
        }
        catch (CosmosException ex)
        {
            return HealthCheckResult.Unhealthy("Cosmos DB is unreachable.", ex);
        }
    }
}
```

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<CosmosDbHealthCheck>("cosmosdb", tags: ["ready"]);
```

## Database and Container Initialization

```csharp
/// <summary>
/// Ensures database and container exist at startup.
/// </summary>
public static class CosmosDbInitializer
{
    public static async Task InitializeAsync(CosmosClient cosmosClient, IConfiguration configuration)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"]!;
        var containerName = configuration["CosmosDb:ContainerName"]!;
        var partitionKeyPath = configuration["CosmosDb:PartitionKeyPath"] ?? "/tenantId";

        var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);

        await database.Database.CreateContainerIfNotExistsAsync(
            new ContainerProperties(containerName, partitionKeyPath)
            {
                IndexingPolicy = new IndexingPolicy
                {
                    Automatic = true,
                    IndexingMode = IndexingMode.Consistent
                }
            });
    }
}
```

Call during startup:

```csharp
// After building the app, before app.Run()
using (var scope = app.Services.CreateScope())
{
    var cosmosClient = scope.ServiceProvider.GetRequiredService<CosmosClient>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await CosmosDbInitializer.InitializeAsync(cosmosClient, config);
}
```

## Official References

- [Cosmos DB .NET SDK](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/sdk-dotnet-v3)
- [CosmosClientOptions](https://learn.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.cosmosclientoptions)
- [Managed Identity with Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/managed-identity-based-authentication)
- [Cosmos DB Emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator)
