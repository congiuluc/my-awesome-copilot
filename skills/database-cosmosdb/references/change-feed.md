# Cosmos DB Change Feed

## Overview

The change feed provides a sorted list of documents within a container in the order they were modified. It enables event-driven architectures and real-time processing.

## Change Feed Processor

The recommended approach for consuming the change feed in .NET applications.

### Setup

```csharp
/// <summary>
/// Builds and starts a change feed processor for processing document changes.
/// </summary>
public static class ChangeFeedProcessorFactory
{
    public static ChangeFeedProcessor Create(
        Container monitoredContainer,
        Container leaseContainer,
        IServiceProvider serviceProvider,
        string processorName = "MyProcessor",
        string instanceName = "Instance1")
    {
        return monitoredContainer
            .GetChangeFeedProcessorBuilder<CosmosDocument>(processorName, async (
                IReadOnlyCollection<CosmosDocument> changes,
                CancellationToken cancellationToken) =>
            {
                using var scope = serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IChangeFeedHandler>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<ChangeFeedProcessor>>();

                logger.LogInformation("Processing {Count} changes", changes.Count);

                foreach (var document in changes)
                {
                    try
                    {
                        await handler.HandleAsync(document, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to process document {Id}", document.Id);
                        // Do NOT rethrow — the change feed processor will retry the entire batch
                        // Log to dead-letter or retry queue instead
                    }
                }
            })
            .WithInstanceName(instanceName)
            .WithLeaseContainer(leaseContainer)
            .WithStartTime(DateTime.MinValue.ToUniversalTime())  // Start from beginning
            .Build();
    }
}
```

### Handler Interface

```csharp
/// <summary>
/// Handles individual document changes from the change feed.
/// </summary>
public interface IChangeFeedHandler
{
    Task HandleAsync(CosmosDocument document, CancellationToken cancellationToken);
}

/// <summary>
/// Routes changes by document type to specialized handlers.
/// </summary>
public class TypedChangeFeedHandler : IChangeFeedHandler
{
    private readonly ILogger<TypedChangeFeedHandler> _logger;

    public TypedChangeFeedHandler(ILogger<TypedChangeFeedHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(CosmosDocument document, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Processing {Type} document {Id}", document.Type, document.Id);

        switch (document.Type)
        {
            case "order":
                await HandleOrderChangeAsync(document, cancellationToken);
                break;
            case "product":
                await HandleProductChangeAsync(document, cancellationToken);
                break;
            default:
                _logger.LogWarning("Unhandled document type: {Type}", document.Type);
                break;
        }
    }

    private Task HandleOrderChangeAsync(CosmosDocument document, CancellationToken cancellationToken)
    {
        // Process order change (e.g., send notification, update cache)
        return Task.CompletedTask;
    }

    private Task HandleProductChangeAsync(CosmosDocument document, CancellationToken cancellationToken)
    {
        // Process product change (e.g., update search index)
        return Task.CompletedTask;
    }
}
```

### Registration and Lifecycle (Hosted Service)

```csharp
/// <summary>
/// Background service that manages the change feed processor lifecycle.
/// </summary>
public class ChangeFeedHostedService : IHostedService
{
    private readonly ChangeFeedProcessor _processor;
    private readonly ILogger<ChangeFeedHostedService> _logger;

    public ChangeFeedHostedService(
        CosmosClient cosmosClient,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<ChangeFeedHostedService> logger)
    {
        _logger = logger;

        var databaseName = configuration["CosmosDb:DatabaseName"]!;
        var containerName = configuration["CosmosDb:ContainerName"]!;

        var monitoredContainer = cosmosClient.GetContainer(databaseName, containerName);
        var leaseContainer = cosmosClient.GetContainer(databaseName, "leases");

        _processor = ChangeFeedProcessorFactory.Create(
            monitoredContainer, leaseContainer, serviceProvider);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting change feed processor");
        await _processor.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping change feed processor");
        await _processor.StopAsync();
    }
}
```

```csharp
// Program.cs
builder.Services.AddScoped<IChangeFeedHandler, TypedChangeFeedHandler>();
builder.Services.AddHostedService<ChangeFeedHostedService>();
```

### Lease Container

The lease container tracks processing progress per partition. Create it once:

```csharp
await database.CreateContainerIfNotExistsAsync(
    new ContainerProperties("leases", "/id"));
```

## Change Feed Pull Model

For simpler scenarios or Azure Functions:

```csharp
/// <summary>
/// Pulls changes from the feed manually (useful for batch processing).
/// </summary>
public async Task<List<T>> PullChangesAsync<T>(
    Container container,
    FeedRange feedRange,
    string? continuationToken = null)
{
    var iterator = container.GetChangeFeedIterator<T>(
        ChangeFeedStartFrom.ContinuationToken(continuationToken
            ?? ChangeFeedStartFrom.Beginning().ToString()),
        ChangeFeedMode.LatestVersion);

    var results = new List<T>();

    while (iterator.HasMoreResults)
    {
        var response = await iterator.ReadNextAsync();

        if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
        {
            // No more changes
            break;
        }

        results.AddRange(response);
    }

    return results;
}
```

## Common Patterns

### Materialized View

Use the change feed to project data into a read-optimized view:

```
Source Container (orders by customerId)
    → Change Feed
        → Materialized View Container (order summaries by status)
```

### Event Sourcing

Store events as documents and use the change feed as the event stream:

```
Events Container (append-only, partition by aggregateId)
    → Change Feed
        → Read Model Container (projected aggregate state)
```

### Cross-Container Sync

Replicate data between containers with different partition keys:

```
Orders Container (partition: /customerId)
    → Change Feed
        → OrdersByDate Container (partition: /orderDate)
```

## Error Handling

- **Never throw** from the change feed delegate — it retries the entire batch
- Use a dead-letter pattern for poison messages:

```csharp
catch (Exception ex)
{
    logger.LogError(ex, "Poison document {Id}, sending to dead-letter", document.Id);
    await deadLetterContainer.UpsertItemAsync(new
    {
        id = $"dl-{document.Id}-{DateTime.UtcNow.Ticks}",
        originalId = document.Id,
        error = ex.Message,
        document,
        failedAtUtc = DateTime.UtcNow
    }, new PartitionKey(document.Id));
}
```

## Performance Considerations

- Change feed reads consume RUs — monitor RU cost on both monitored and lease containers
- Scale out by adding more processor instances (same processor name, different instance names)
- Use `WithMaxItems()` to control batch size
- The lease container requires minimal throughput (400 RU/s typically sufficient)
- Processing latency depends on polling interval (default ~5 seconds)

## Official References

- [Change Feed Overview](https://learn.microsoft.com/en-us/azure/cosmos-db/change-feed)
- [Change Feed Processor](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/change-feed-processor)
- [Change Feed Design Patterns](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/change-feed-design-patterns)
