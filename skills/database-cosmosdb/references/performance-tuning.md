# Cosmos DB Performance Tuning

## Request Units (RU) Optimization

Every operation consumes Request Units. Understanding and minimizing RU cost is critical.

### RU Cost Factors

| Factor | Impact |
|--------|--------|
| Document size | Larger = more RUs |
| Property count | More indexed properties = more write RUs |
| Consistency level | Strong > Bounded Staleness > Session > Eventual |
| Query complexity | Cross-partition > single-partition; scans > seeks |
| Indexed properties | More indexes = higher write cost, lower read cost |

### Always Read RU Charge

```csharp
/// <summary>
/// Reads an item and logs the RU charge for monitoring.
/// </summary>
public async Task<T?> ReadItemWithDiagnosticsAsync<T>(
    Container container,
    string id,
    PartitionKey partitionKey,
    ILogger logger)
{
    var response = await container.ReadItemAsync<T>(id, partitionKey);

    logger.LogInformation(
        "ReadItem {Id} cost {RequestCharge} RUs, StatusCode={StatusCode}",
        id, response.RequestCharge, response.StatusCode);

    return response.Resource;
}
```

### Point Reads vs Queries

**Always prefer point reads** (ReadItemAsync) over queries when you have the id + partition key:

```csharp
// GOOD: Point read — 1 RU for a 1KB document
var response = await container.ReadItemAsync<Order>(orderId, new PartitionKey(tenantId));

// BAD: Query to find by ID — 2.5+ RUs minimum
var query = container.GetItemQueryIterator<Order>(
    new QueryDefinition("SELECT * FROM c WHERE c.id = @id").WithParameter("@id", orderId));
```

## Indexing Policy

### Default Policy (Index Everything)

```json
{
    "indexingMode": "consistent",
    "automatic": true,
    "includedPaths": [{ "path": "/*" }],
    "excludedPaths": [{ "path": "/\"_etag\"/?" }]
}
```

### Optimized Policy (Index Only What You Query)

```csharp
var indexingPolicy = new IndexingPolicy
{
    IndexingMode = IndexingMode.Consistent,
    Automatic = true
};

// Exclude everything first
indexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = "/*" });

// Include only queried paths
indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/type/?" });
indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/status/?" });
indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/createdAtUtc/?" });

// Composite index for ORDER BY with multiple fields
indexingPolicy.CompositeIndexes.Add(new Collection<CompositePath>
{
    new CompositePath { Path = "/type", Order = CompositePathSortOrder.Ascending },
    new CompositePath { Path = "/createdAtUtc", Order = CompositePathSortOrder.Descending }
});
```

### When to Use Composite Indexes

Required for:
- `ORDER BY` with multiple properties
- Filters on multiple properties combined with `ORDER BY`
- Filters using `AND` on multiple properties (improves RU cost)

```sql
-- Requires composite index on (type ASC, createdAtUtc DESC)
SELECT * FROM c WHERE c.type = 'order' ORDER BY c.createdAtUtc DESC
```

## Query Best Practices

### 1. Always Specify Partition Key

```csharp
// GOOD: Single-partition query
var query = new QueryDefinition("SELECT * FROM c WHERE c.type = @type")
    .WithParameter("@type", "order");

var options = new QueryRequestOptions { PartitionKey = new PartitionKey(tenantId) };
var iterator = container.GetItemQueryIterator<Order>(query, requestOptions: options);
```

### 2. Project Only Needed Fields

```csharp
// GOOD: Reduced RU cost and network transfer
var query = new QueryDefinition(
    "SELECT c.id, c.status, c.createdAtUtc FROM c WHERE c.type = 'order'");

// BAD: SELECT * transfers entire documents
```

### 3. Avoid Cross-Partition Queries

```csharp
// Set MaxItemCount to control page size
var options = new QueryRequestOptions
{
    PartitionKey = new PartitionKey(tenantId),  // REQUIRED for performance
    MaxItemCount = 50                            // Control batch size
};
```

### 4. Use Continuation Tokens for Pagination

```csharp
/// <summary>
/// Paginates query results using continuation tokens.
/// </summary>
public async Task<(List<T> Items, string? ContinuationToken)> QueryPageAsync<T>(
    Container container,
    QueryDefinition query,
    PartitionKey partitionKey,
    int pageSize,
    string? continuationToken = null)
{
    var options = new QueryRequestOptions
    {
        PartitionKey = partitionKey,
        MaxItemCount = pageSize
    };

    var iterator = container.GetItemQueryIterator<T>(query, continuationToken, options);
    var items = new List<T>();

    if (iterator.HasMoreResults)
    {
        var response = await iterator.ReadNextAsync();
        items.AddRange(response);
        return (items, response.ContinuationToken);
    }

    return (items, null);
}
```

## Consistency Levels

| Level | Latency | Throughput | Use Case |
|-------|---------|------------|----------|
| **Strong** | Highest | Lowest | Financial transactions |
| **Bounded Staleness** | High | Low | Leader boards, counts |
| **Session** (default) | Medium | Medium | User-facing apps (recommended default) |
| **Consistent Prefix** | Low | High | Updates with ordering |
| **Eventual** | Lowest | Highest | Non-critical counters, logs |

Override consistency per request:

```csharp
var options = new ItemRequestOptions
{
    ConsistencyLevel = ConsistencyLevel.Eventual  // Lower cost for non-critical reads
};
var response = await container.ReadItemAsync<Log>(id, partitionKey, options);
```

## Bulk Operations

For high-volume ingestion, use bulk mode:

```csharp
var bulkClient = new CosmosClient(connectionString, new CosmosClientOptions
{
    AllowBulkExecution = true  // Enable bulk mode
});

var container = bulkClient.GetContainer(databaseName, containerName);
var tasks = new List<Task>(items.Count);

foreach (var item in items)
{
    tasks.Add(container.CreateItemAsync(item, new PartitionKey(item.TenantId))
        .ContinueWith(t =>
        {
            if (!t.IsCompletedSuccessfully)
            {
                // Log individual failures
            }
        }));
}

await Task.WhenAll(tasks);
```

## Throughput Management

### Auto-scale (Recommended)

```csharp
// Auto-scale between 400 and 4000 RU/s
var throughput = ThroughputProperties.CreateAutoscaleThroughput(4000);
await database.Database.CreateContainerIfNotExistsAsync(
    new ContainerProperties("Items", "/tenantId"),
    throughput);
```

### Monitor 429 (Rate Limited) Responses

```csharp
try
{
    await container.CreateItemAsync(item, new PartitionKey(item.TenantId));
}
catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
{
    logger.LogWarning(
        "Rate limited. Retry after {RetryAfter}ms",
        ex.RetryAfter?.TotalMilliseconds);
    // SDK auto-retries; this catch is for monitoring/alerting
}
```

## Performance Checklist

- [ ] Partition key has high cardinality and appears in WHERE clauses
- [ ] Point reads used instead of queries when id + partition key are known
- [ ] Indexing policy excludes unused paths to reduce write RUs
- [ ] Composite indexes defined for multi-field ORDER BY
- [ ] Queries project only needed fields (no SELECT *)
- [ ] All queries specify partition key (no cross-partition fan-out)
- [ ] Continuation tokens used for pagination (not OFFSET/LIMIT)
- [ ] CosmosClient is a singleton with Direct connection mode
- [ ] Session consistency used unless stronger guarantees needed
- [ ] Auto-scale throughput configured for variable workloads
- [ ] RU charge logged and monitored for all operations
- [ ] Bulk mode enabled for batch ingestion scenarios

## Official References

- [Request Units](https://learn.microsoft.com/en-us/azure/cosmos-db/request-units)
- [Indexing Policies](https://learn.microsoft.com/en-us/azure/cosmos-db/index-policy)
- [Query Performance Tips](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/performance-tips-query-sdk)
- [.NET SDK Performance Tips](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/performance-tips-dotnet-sdk-v3)
- [Consistency Levels](https://learn.microsoft.com/en-us/azure/cosmos-db/consistency-levels)
