# MongoDB Connection Setup

## NuGet Package

```xml
<PackageReference Include="MongoDB.Driver" Version="3.*" />
```

## Connection Strings

### Local Development

```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017"
  },
  "MongoDB": {
    "DatabaseName": "MyDatabase"
  }
}
```

### MongoDB Atlas (Production)

```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb+srv://username:password@cluster0.xxxxx.mongodb.net/?retryWrites=true&w=majority"
  },
  "MongoDB": {
    "DatabaseName": "MyDatabase"
  }
}
```

### Azure Cosmos DB for MongoDB (vCore)

```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb+srv://username:password@myaccount.mongocluster.cosmos.azure.com/?tls=true&authMechanism=SCRAM-SHA-256&retrywrites=false&maxIdleTimeMS=120000"
  }
}
```

## MongoClient Singleton Registration

**CRITICAL**: `MongoClient` is thread-safe. Register as singleton — never create per request.

```csharp
// Program.cs — Register singleton MongoClient and IMongoDatabase
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDB")
        ?? throw new InvalidOperationException("MongoDB connection string is required.");

    var settings = MongoClientSettings.FromConnectionString(connectionString);
    settings.ServerApi = new ServerApi(ServerApiVersion.V1);
    settings.MaxConnectionPoolSize = 100;
    settings.MinConnectionPoolSize = 10;
    settings.ConnectTimeout = TimeSpan.FromSeconds(10);
    settings.SocketTimeout = TimeSpan.FromSeconds(30);
    settings.RetryWrites = true;
    settings.RetryReads = true;

    // Enable command logging in development
    if (builder.Environment.IsDevelopment())
    {
        settings.ClusterConfigurator = cb =>
        {
            cb.Subscribe<MongoDB.Driver.Core.Events.CommandStartedEvent>(e =>
            {
                var logger = sp.GetRequiredService<ILogger<MongoClient>>();
                logger.LogDebug("MongoDB Command: {CommandName} {Command}",
                    e.CommandName, e.Command.ToJson());
            });
        };
    }

    return new MongoClient(settings);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration["MongoDB:DatabaseName"]
        ?? throw new InvalidOperationException("MongoDB:DatabaseName is required.");
    return client.GetDatabase(databaseName);
});
```

## Convention Registration

Register conventions **once at startup** before any collection access:

```csharp
// Program.cs — Before builder.Build()
var conventionPack = new ConventionPack
{
    new CamelCaseElementNameConvention(),       // MongoDB fields use camelCase
    new IgnoreExtraElementsConvention(true),    // Ignore unknown fields during deserialization
    new EnumRepresentationConvention(BsonType.String)  // Store enums as strings
};

ConventionRegistry.Register("AppConventions", conventionPack, _ => true);
```

## Health Check

```csharp
/// <summary>
/// Health check that verifies MongoDB connectivity by running a ping command.
/// </summary>
public class MongoDbHealthCheck : IHealthCheck
{
    private readonly IMongoClient _mongoClient;

    public MongoDbHealthCheck(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _mongoClient.GetDatabase("admin");
            var command = new BsonDocumentCommand<BsonDocument>(new BsonDocument("ping", 1));
            await database.RunCommandAsync(command, cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy("MongoDB is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("MongoDB is unreachable.", ex);
        }
    }
}
```

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<MongoDbHealthCheck>("mongodb", tags: ["ready"]);
```

## Docker Compose (Local Development)

```yaml
services:
  mongodb:
    image: mongo:8
    ports:
      - "27017:27017"
    environment:
      MONGO_INITDB_ROOT_USERNAME: admin
      MONGO_INITDB_ROOT_PASSWORD: devpassword
    volumes:
      - mongodb_data:/data/db

volumes:
  mongodb_data:
```

## Official References

- [MongoClient Settings](https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/connection/connect/)
- [Connection String Options](https://www.mongodb.com/docs/manual/reference/connection-string/)
- [Azure Cosmos DB for MongoDB](https://learn.microsoft.com/en-us/azure/cosmos-db/mongodb/)
