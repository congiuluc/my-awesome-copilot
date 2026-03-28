# Cosmos DB Implementation (Direct SDK)

Use this approach when you need direct Azure Cosmos DB SDK access instead of the EF Core Cosmos provider.
Choose direct SDK when you need: fine-grained RU control, point reads, cross-partition queries, or change feed access.

## Setup

- Use the **Azure Cosmos DB SDK for .NET** (`Microsoft.Azure.Cosmos`).
- Place in `{App}.Infrastructure/Repositories/CosmosDb/`.
- `CosmosClient` registered as **singleton** — it manages connections internally.

## Settings

```csharp
/// <summary>
/// Configuration settings for Azure Cosmos DB.
/// </summary>
public class CosmosDbSettings
{
    /// <summary>
    /// Cosmos DB connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Database name.
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Container name for product entities.
    /// </summary>
    public string ProductContainerName { get; set; } = "products";
}
```

## Repository

```csharp
/// <summary>
/// Cosmos DB repository implementation using the direct SDK.
/// </summary>
public class CosmosDbProductRepository : IRepository<Product>
{
    private readonly Container _container;

    public CosmosDbProductRepository(CosmosClient cosmosClient, IOptions<CosmosDbSettings> settings)
    {
        var database = cosmosClient.GetDatabase(settings.Value.DatabaseName);
        _container = database.GetContainer(settings.Value.ProductContainerName);
    }

    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<Product>(
                id,
                new PartitionKey(id),
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = _container.GetItemQueryIterator<Product>(
            new QueryDefinition("SELECT * FROM c ORDER BY c.CreatedAtUtc DESC"));

        var results = new List<Product>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            results.AddRange(response);
        }
        return results.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> FindAsync(
        Expression<Func<Product, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var queryable = _container.GetItemLinqQueryable<Product>()
            .Where(predicate);

        using var iterator = queryable.ToFeedIterator();
        var results = new List<Product>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            results.AddRange(response);
        }
        return results.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = Guid.NewGuid().ToString();
        }
        entity.CreatedAtUtc = DateTime.UtcNow;

        var response = await _container.CreateItemAsync(
            entity,
            new PartitionKey(entity.Id),
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return response.Resource;
    }

    /// <inheritdoc />
    public async Task<Product> UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAtUtc = DateTime.UtcNow;
        var response = await _container.ReplaceItemAsync(
            entity,
            entity.Id,
            new PartitionKey(entity.Id),
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return response.Resource;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _container.DeleteItemAsync<Product>(
                id,
                new PartitionKey(id),
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // Already deleted — idempotent
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _container.ReadItemAsync<Product>(
                id,
                new PartitionKey(id),
                new ItemRequestOptions { IfNoneMatchEtag = "*" },
                cancellationToken)
                .ConfigureAwait(false);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        Expression<Func<Product, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var queryText = predicate is null
            ? "SELECT VALUE COUNT(1) FROM c"
            : null;

        if (queryText is not null)
        {
            var countQuery = _container.GetItemQueryIterator<int>(new QueryDefinition(queryText));
            var response = await countQuery.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            return response.FirstOrDefault();
        }

        var queryable = _container.GetItemLinqQueryable<Product>();
        if (predicate is not null)
        {
            queryable = (IOrderedQueryable<Product>)queryable.Where(predicate);
        }
        return await queryable.CountAsync(cancellationToken).ConfigureAwait(false);
    }
}
```

## CosmosClient Best Practices

- Always specify `PartitionKey` in point reads — cross-partition queries are expensive.
- Use `ReadItemAsync` for single-item lookups (cheapest: 1 RU).
- Use parameterized queries to prevent injection.
- Handle `CosmosException` with status code matching for idempotent operations.
- Monitor RU consumption via response headers in production.
- Use `CosmosClientOptions` for connection tuning:

```csharp
var cosmosClient = new CosmosClient(connectionString, new CosmosClientOptions
{
    ApplicationName = "MyApp",
    SerializerOptions = new CosmosSerializationOptions
    {
        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
    },
    ConnectionMode = ConnectionMode.Direct,
    MaxRetryAttemptsOnRateLimitedRequests = 5,
    MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30)
});
```

## Official References

- [Azure Cosmos DB .NET SDK](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/sdk-dotnet-v3)
- [Cosmos DB Best Practices](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/best-practice-dotnet)
