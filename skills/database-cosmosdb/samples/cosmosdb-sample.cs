// =============================================================================
// Cosmos DB Complete Sample
// Demonstrates: SDK setup, repository pattern, change feed, health check
// =============================================================================

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json.Serialization;

// =============================================================================
// SECTION 1: Document Models
// =============================================================================

#region Document Models

/// <summary>
/// Base class for all Cosmos DB documents with type discriminator.
/// </summary>
public abstract class CosmosDocument
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public abstract string Type { get; }

    [JsonPropertyName("tenantId")]
    public string TenantId { get; set; } = string.Empty;

    [JsonPropertyName("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAtUtc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("_etag")]
    public string? ETag { get; set; }
}

/// <summary>
/// Order document stored in Cosmos DB.
/// </summary>
public class OrderDocument : CosmosDocument
{
    public override string Type => "order";

    [JsonPropertyName("customerId")]
    public string CustomerId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Pending";

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("items")]
    public List<OrderItem> Items { get; set; } = [];
}

/// <summary>
/// Embedded order item (denormalized).
/// </summary>
public class OrderItem
{
    [JsonPropertyName("productId")]
    public string ProductId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }
}

#endregion

// =============================================================================
// SECTION 2: Repository Interface and Implementation
// =============================================================================

#region Repository

/// <summary>
/// Generic Cosmos DB repository with partition-key-aware operations.
/// </summary>
public interface ICosmosRepository<T> where T : CosmosDocument
{
    Task<T?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default);
    Task<T> CreateAsync(T document, CancellationToken cancellationToken = default);
    Task<T> UpsertAsync(T document, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default);
    Task<(List<T> Items, string? ContinuationToken)> QueryAsync(
        QueryDefinition query,
        string partitionKey,
        int pageSize = 50,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Cosmos DB repository implementation with logging and diagnostics.
/// </summary>
public class CosmosRepository<T> : ICosmosRepository<T> where T : CosmosDocument
{
    private readonly Container _container;
    private readonly ILogger<CosmosRepository<T>> _logger;

    public CosmosRepository(Container container, ILogger<CosmosRepository<T>> logger)
    {
        _container = container;
        _logger = logger;
    }

    /// <summary>
    /// Point read by id and partition key — most efficient operation (1 RU for 1KB doc).
    /// </summary>
    public async Task<T?> GetByIdAsync(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(
                id, new PartitionKey(partitionKey), cancellationToken: cancellationToken);

            _logger.LogDebug(
                "ReadItem {Id} — {RequestCharge} RUs",
                id, response.RequestCharge);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <summary>
    /// Creates a new document. Fails if id already exists in partition.
    /// </summary>
    public async Task<T> CreateAsync(T document, CancellationToken cancellationToken = default)
    {
        document.CreatedAtUtc = DateTime.UtcNow;
        document.UpdatedAtUtc = DateTime.UtcNow;

        var response = await _container.CreateItemAsync(
            document, new PartitionKey(document.TenantId), cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Created {Type} {Id} — {RequestCharge} RUs",
            document.Type, document.Id, response.RequestCharge);

        return response.Resource;
    }

    /// <summary>
    /// Creates or replaces a document. Uses ETag for optimistic concurrency when available.
    /// </summary>
    public async Task<T> UpsertAsync(T document, CancellationToken cancellationToken = default)
    {
        document.UpdatedAtUtc = DateTime.UtcNow;

        var options = document.ETag is not null
            ? new ItemRequestOptions { IfMatchEtag = document.ETag }
            : null;

        var response = await _container.UpsertItemAsync(
            document, new PartitionKey(document.TenantId),
            options, cancellationToken);

        _logger.LogInformation(
            "Upserted {Type} {Id} — {RequestCharge} RUs",
            document.Type, document.Id, response.RequestCharge);

        return response.Resource;
    }

    /// <summary>
    /// Deletes a document by id and partition key.
    /// </summary>
    public async Task DeleteAsync(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        var response = await _container.DeleteItemAsync<T>(
            id, new PartitionKey(partitionKey), cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Deleted {Id} — {RequestCharge} RUs",
            id, response.RequestCharge);
    }

    /// <summary>
    /// Executes a parameterized query with continuation-token pagination.
    /// Always specify partition key for single-partition queries.
    /// </summary>
    public async Task<(List<T> Items, string? ContinuationToken)> QueryAsync(
        QueryDefinition query,
        string partitionKey,
        int pageSize = 50,
        string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        var options = new QueryRequestOptions
        {
            PartitionKey = new PartitionKey(partitionKey),
            MaxItemCount = pageSize
        };

        using var iterator = _container.GetItemQueryIterator<T>(
            query, continuationToken, options);

        var items = new List<T>();
        double totalRu = 0;

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            items.AddRange(response);
            totalRu += response.RequestCharge;

            _logger.LogDebug(
                "Query returned {Count} items — {RequestCharge} RUs",
                response.Count, totalRu);

            return (items, response.ContinuationToken);
        }

        return (items, null);
    }
}

#endregion

// =============================================================================
// SECTION 3: Health Check
// =============================================================================

#region Health Check

/// <summary>
/// Health check verifying Cosmos DB connectivity.
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
            return HealthCheckResult.Unhealthy("Cosmos DB unreachable.", ex);
        }
    }
}

#endregion

// =============================================================================
// SECTION 4: DI Registration
// =============================================================================

#region DI Registration

/// <summary>
/// Extension methods for registering Cosmos DB services.
/// </summary>
public static class CosmosDbServiceExtensions
{
    /// <summary>
    /// Registers CosmosClient as singleton, container factories, repositories, and health check.
    /// </summary>
    public static IServiceCollection AddCosmosDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Singleton CosmosClient — NEVER create multiple instances
        services.AddSingleton<CosmosClient>(_ =>
        {
            var connectionString = configuration.GetConnectionString("CosmosDb")
                ?? throw new InvalidOperationException("CosmosDb connection string is required.");

            return new CosmosClient(connectionString, new CosmosClientOptions
            {
                ApplicationName = "MyApp",
                ConnectionMode = ConnectionMode.Direct,
                MaxRetryAttemptsOnRateLimitedRequests = 9,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),
                CosmosClientTelemetryOptions = new CosmosClientTelemetryOptions
                {
                    DisableDistributedTracing = false
                },
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });
        });

        // Container reference
        services.AddSingleton<Container>(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            var databaseName = configuration["CosmosDb:DatabaseName"]!;
            var containerName = configuration["CosmosDb:ContainerName"]!;
            return client.GetContainer(databaseName, containerName);
        });

        // Repository
        services.AddScoped(typeof(ICosmosRepository<>), typeof(CosmosRepository<>));

        // Health check
        services.AddHealthChecks()
            .AddCheck<CosmosDbHealthCheck>("cosmosdb", tags: ["ready"]);

        return services;
    }
}

#endregion

// =============================================================================
// SECTION 5: Usage in Minimal API Endpoint
// =============================================================================

#region API Endpoint Example

// In Program.cs:
// builder.Services.AddCosmosDb(builder.Configuration);
//
// var app = builder.Build();
//
// app.MapGet("/api/orders/{id}", async (
//     string id,
//     string tenantId,
//     ICosmosRepository<OrderDocument> repository) =>
// {
//     var order = await repository.GetByIdAsync(id, tenantId);
//     return order is not null ? Results.Ok(order) : Results.NotFound();
// });
//
// app.MapGet("/api/orders", async (
//     string tenantId,
//     int pageSize,
//     string? continuationToken,
//     ICosmosRepository<OrderDocument> repository) =>
// {
//     var query = new QueryDefinition(
//         "SELECT * FROM c WHERE c.type = 'order' ORDER BY c.createdAtUtc DESC");
//
//     var (items, nextToken) = await repository.QueryAsync(
//         query, tenantId, pageSize, continuationToken);
//
//     return Results.Ok(new { items, continuationToken = nextToken });
// });

#endregion
