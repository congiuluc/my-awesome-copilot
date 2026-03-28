// =============================================================================
// MongoDB Complete Sample
// Demonstrates: MongoClient setup, BSON mapping, repository pattern, indexes,
//               aggregation, health check, DI registration
// =============================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

// =============================================================================
// SECTION 1: Conventions and Class Maps
// =============================================================================

#region BSON Configuration

/// <summary>
/// Registers MongoDB conventions and BSON class maps.
/// Call once at startup before any collection access.
/// </summary>
public static class MongoBsonConfiguration
{
    public static void Register()
    {
        // Global conventions
        var conventionPack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true),
            new EnumRepresentationConvention(BsonType.String)
        };
        ConventionRegistry.Register("AppConventions", conventionPack, _ => true);

        // Class maps — register only if not already registered
        if (!BsonClassMap.IsClassMapRegistered(typeof(OrderDocument)))
        {
            BsonClassMap.RegisterClassMap<OrderDocument>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.String));
                cm.GetMemberMap(c => c.TotalAmount)
                    .SetSerializer(new DecimalSerializer(BsonType.Decimal128));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(OrderItemDoc)))
        {
            BsonClassMap.RegisterClassMap<OrderItemDoc>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(c => c.UnitPrice)
                    .SetSerializer(new DecimalSerializer(BsonType.Decimal128));
            });
        }
    }
}

#endregion

// =============================================================================
// SECTION 2: Document Models
// =============================================================================

#region Document Models

/// <summary>
/// Order document mapped to MongoDB.
/// </summary>
public class OrderDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string CustomerId { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";

    public decimal TotalAmount { get; set; }

    public List<OrderItemDoc> Items { get; set; } = [];

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Embedded order item (denormalized).
/// </summary>
public class OrderItemDoc
{
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

#endregion

// =============================================================================
// SECTION 3: Repository Interface and Implementation
// =============================================================================

#region Repository

/// <summary>
/// MongoDB repository for order documents.
/// </summary>
public interface IOrderRepository
{
    Task<OrderDocument?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<OrderDocument> CreateAsync(OrderDocument order, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(string id, string status, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<List<OrderDocument>> GetByCustomerAsync(
        string customerId, string? lastId = null, int pageSize = 50,
        CancellationToken cancellationToken = default);
    Task<List<CustomerSummary>> GetCustomerSummariesAsync(
        DateTime since, CancellationToken cancellationToken = default);
}

/// <summary>
/// MongoDB implementation of order repository with index creation.
/// </summary>
public class MongoOrderRepository : IOrderRepository
{
    private readonly IMongoCollection<OrderDocument> _collection;
    private readonly ILogger<MongoOrderRepository> _logger;

    public MongoOrderRepository(IMongoDatabase database, ILogger<MongoOrderRepository> logger)
    {
        _collection = database.GetCollection<OrderDocument>("orders");
        _logger = logger;
    }

    #region Index Creation

    /// <summary>
    /// Creates required indexes. Call once at startup.
    /// </summary>
    public async Task EnsureIndexesAsync()
    {
        var indexes = new List<CreateIndexModel<OrderDocument>>
        {
            // Most common query: orders by customer, newest first
            new(
                Builders<OrderDocument>.IndexKeys
                    .Ascending(x => x.CustomerId)
                    .Descending(x => x.CreatedAtUtc),
                new CreateIndexOptions { Name = "idx_customerId_createdAtUtc" }),

            // Status filter
            new(
                Builders<OrderDocument>.IndexKeys.Ascending(x => x.Status),
                new CreateIndexOptions { Name = "idx_status" })
        };

        await _collection.Indexes.CreateManyAsync(indexes);
        _logger.LogInformation("MongoDB indexes ensured for orders collection");
    }

    #endregion

    #region CRUD Operations

    /// <summary>
    /// Gets an order by ID.
    /// </summary>
    public async Task<OrderDocument?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(Builders<OrderDocument>.Filter.Eq(x => x.Id, id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Creates a new order document.
    /// </summary>
    public async Task<OrderDocument> CreateAsync(
        OrderDocument order,
        CancellationToken cancellationToken = default)
    {
        order.CreatedAtUtc = DateTime.UtcNow;
        order.UpdatedAtUtc = DateTime.UtcNow;

        await _collection.InsertOneAsync(order, cancellationToken: cancellationToken);

        _logger.LogInformation("Created order {OrderId} for customer {CustomerId}",
            order.Id, order.CustomerId);

        return order;
    }

    /// <summary>
    /// Updates order status using atomic $set.
    /// </summary>
    public async Task UpdateStatusAsync(
        string id,
        string status,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<OrderDocument>.Filter.Eq(x => x.Id, id);
        var update = Builders<OrderDocument>.Update
            .Set(x => x.Status, status)
            .Set(x => x.UpdatedAtUtc, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update,
            cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            _logger.LogWarning("Order {OrderId} not found for status update", id);
        }
    }

    /// <summary>
    /// Deletes an order by ID.
    /// </summary>
    public async Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        await _collection.DeleteOneAsync(
            Builders<OrderDocument>.Filter.Eq(x => x.Id, id),
            cancellationToken: cancellationToken);
    }

    #endregion

    #region Query Operations

    /// <summary>
    /// Keyset pagination — O(1) performance regardless of page depth.
    /// </summary>
    public async Task<List<OrderDocument>> GetByCustomerAsync(
        string customerId,
        string? lastId = null,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<OrderDocument>.Filter;
        var filter = filterBuilder.Eq(x => x.CustomerId, customerId);

        if (lastId is not null)
        {
            filter &= filterBuilder.Gt(x => x.Id, lastId);
        }

        return await _collection
            .Find(filter)
            .Sort(Builders<OrderDocument>.Sort.Ascending(x => x.Id))
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Aggregation pipeline: customer order summaries.
    /// </summary>
    public async Task<List<CustomerSummary>> GetCustomerSummariesAsync(
        DateTime since,
        CancellationToken cancellationToken = default)
    {
        return await _collection.Aggregate()
            .Match(Builders<OrderDocument>.Filter.And(
                Builders<OrderDocument>.Filter.Gte(x => x.CreatedAtUtc, since),
                Builders<OrderDocument>.Filter.Ne(x => x.Status, "Cancelled")))
            .Group(
                x => x.CustomerId,
                g => new CustomerSummary
                {
                    CustomerId = g.Key,
                    OrderCount = g.Count(),
                    TotalSpent = g.Sum(x => x.TotalAmount),
                    LastOrderDate = g.Max(x => x.CreatedAtUtc)
                })
            .Sort(Builders<CustomerSummary>.Sort.Descending(x => x.TotalSpent))
            .ToListAsync(cancellationToken);
    }

    #endregion
}

public class CustomerSummary
{
    public string CustomerId { get; set; } = string.Empty;
    public long OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime LastOrderDate { get; set; }
}

#endregion

// =============================================================================
// SECTION 4: Health Check
// =============================================================================

#region Health Check

/// <summary>
/// Health check that pings MongoDB to verify connectivity.
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

#endregion

// =============================================================================
// SECTION 5: DI Registration
// =============================================================================

#region DI Registration

/// <summary>
/// Extension methods for registering MongoDB services.
/// </summary>
public static class MongoDbServiceExtensions
{
    /// <summary>
    /// Registers MongoClient (singleton), IMongoDatabase, repositories, and health check.
    /// </summary>
    public static IServiceCollection AddMongoDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register BSON mappings
        MongoBsonConfiguration.Register();

        // Singleton MongoClient — thread-safe, never create per request
        services.AddSingleton<IMongoClient>(_ =>
        {
            var connectionString = configuration.GetConnectionString("MongoDB")
                ?? throw new InvalidOperationException("MongoDB connection string is required.");

            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            settings.MaxConnectionPoolSize = 100;
            settings.MinConnectionPoolSize = 10;
            settings.RetryWrites = true;
            settings.RetryReads = true;

            return new MongoClient(settings);
        });

        // Singleton IMongoDatabase
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var databaseName = configuration["MongoDB:DatabaseName"]
                ?? throw new InvalidOperationException("MongoDB:DatabaseName is required.");
            return client.GetDatabase(databaseName);
        });

        // Repositories
        services.AddScoped<IOrderRepository, MongoOrderRepository>();

        // Health check
        services.AddHealthChecks()
            .AddCheck<MongoDbHealthCheck>("mongodb", tags: ["ready"]);

        return services;
    }

    /// <summary>
    /// Ensures all required indexes are created. Call after app.Build().
    /// </summary>
    public static async Task EnsureMongoDbIndexesAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        if (orderRepo is MongoOrderRepository mongoRepo)
        {
            await mongoRepo.EnsureIndexesAsync();
        }
    }
}

#endregion

// =============================================================================
// SECTION 6: Usage in Program.cs
// =============================================================================

#region Program.cs Example

// MongoBsonConfiguration.Register();  // Before builder.Build()
//
// builder.Services.AddMongoDb(builder.Configuration);
//
// var app = builder.Build();
//
// await app.Services.EnsureMongoDbIndexesAsync();  // Create indexes at startup
//
// app.MapGet("/api/orders/{id}", async (string id, IOrderRepository repo) =>
// {
//     var order = await repo.GetByIdAsync(id);
//     return order is not null ? Results.Ok(order) : Results.NotFound();
// });
//
// app.MapGet("/api/orders", async (
//     string customerId, string? lastId, int pageSize,
//     IOrderRepository repo) =>
// {
//     var orders = await repo.GetByCustomerAsync(customerId, lastId, pageSize);
//     return Results.Ok(orders);
// });
//
// app.MapGet("/api/orders/summaries", async (
//     DateTime since, IOrderRepository repo) =>
// {
//     var summaries = await repo.GetCustomerSummariesAsync(since);
//     return Results.Ok(summaries);
// });

#endregion
