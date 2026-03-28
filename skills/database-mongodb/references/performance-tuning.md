# MongoDB Performance Tuning

## Indexing Strategy

### Rules

1. **Every query MUST use an index** — unindexed queries perform collection scans
2. **Create indexes that cover common queries** — compound indexes for multi-field filters
3. **Index order matters** — Equality → Sort → Range (ESR rule)
4. **Don't over-index** — each index consumes RAM and slows writes

### Index Types

| Type | Syntax | Use Case |
|------|--------|----------|
| **Single field** | `{ status: 1 }` | Simple equality/range queries |
| **Compound** | `{ customerId: 1, createdAtUtc: -1 }` | Multi-field queries |
| **Text** | `{ name: "text", description: "text" }` | Full-text search |
| **Wildcard** | `{ "metadata.$**": 1 }` | Schema-flexible fields |
| **TTL** | `{ expiresAt: 1 }, { expireAfterSeconds: 0 }` | Auto-delete expired documents |
| **Unique** | `{ email: 1 }, { unique: true }` | Enforce uniqueness |
| **Partial** | `{ status: 1 }, { partialFilterExpression: { status: "Active" } }` | Index only matching docs |

### Creating Indexes in .NET

```csharp
/// <summary>
/// Creates indexes for the orders collection at startup.
/// </summary>
public static class OrderIndexes
{
    public static async Task CreateAsync(IMongoCollection<OrderDocument> collection)
    {
        var indexModels = new List<CreateIndexModel<OrderDocument>>
        {
            // Compound: customerId + createdAtUtc (descending) — most common query
            new(
                Builders<OrderDocument>.IndexKeys
                    .Ascending(x => x.CustomerId)
                    .Descending(x => x.CreatedAtUtc),
                new CreateIndexOptions { Name = "idx_customerId_createdAtUtc" }
            ),

            // Single field: status (with partial filter for active orders)
            new(
                Builders<OrderDocument>.IndexKeys.Ascending(x => x.Status),
                new CreateIndexOptions
                {
                    Name = "idx_status_active",
                    PartialFilterExpression = Builders<OrderDocument>.Filter.In(
                        x => x.Status,
                        new[] { OrderStatus.Pending, OrderStatus.Processing })
                }
            ),

            // TTL: auto-delete cancelled orders after 90 days
            new(
                Builders<OrderDocument>.IndexKeys.Ascending(x => x.UpdatedAtUtc),
                new CreateIndexOptions
                {
                    Name = "idx_ttl_cancelled",
                    ExpireAfter = TimeSpan.FromDays(90),
                    PartialFilterExpression = Builders<OrderDocument>.Filter.Eq(
                        x => x.Status, OrderStatus.Cancelled)
                }
            ),

            // Unique: prevent duplicate external order references
            new(
                Builders<OrderDocument>.IndexKeys.Ascending("externalOrderId"),
                new CreateIndexOptions
                {
                    Name = "idx_externalOrderId_unique",
                    Unique = true,
                    Sparse = true  // Only index documents that have this field
                }
            )
        };

        await collection.Indexes.CreateManyAsync(indexModels);
    }
}
```

### ESR Rule (Equality → Sort → Range)

Design compound indexes following the ESR rule for optimal performance:

```csharp
// Query: WHERE status = "Shipped" AND createdAtUtc > lastWeek ORDER BY totalAmount DESC
// Index: { status: 1, totalAmount: -1, createdAtUtc: -1 }
//         ^Equality    ^Sort            ^Range

var index = Builders<OrderDocument>.IndexKeys
    .Ascending(x => x.Status)          // E: Equality
    .Descending(x => x.TotalAmount)    // S: Sort
    .Descending(x => x.CreatedAtUtc);  // R: Range
```

## Query Optimization

### Use Explain to Analyze Queries

```csharp
/// <summary>
/// Runs explain on a query to check index usage and execution stats.
/// </summary>
public async Task<BsonDocument> ExplainQueryAsync<T>(
    IMongoCollection<T> collection,
    FilterDefinition<T> filter)
{
    var command = new BsonDocument
    {
        { "explain", new BsonDocument
            {
                { "find", collection.CollectionNamespace.CollectionName },
                { "filter", filter.Render(
                    collection.DocumentSerializer,
                    collection.Settings.SerializerRegistry) }
            }
        },
        { "verbosity", "executionStats" }
    };

    return await collection.Database.RunCommandAsync<BsonDocument>(command);
}
```

Key explain output to check:
- `winningPlan.stage` should be `IXSCAN`, not `COLLSCAN`
- `executionStats.totalKeysExamined` should be close to `nReturned`
- `executionStats.totalDocsExamined` should be close to `nReturned`

### Projection — Return Only Needed Fields

```csharp
// GOOD: Project only needed fields
var orders = await collection
    .Find(filter)
    .Project(Builders<OrderDocument>.Projection
        .Include(x => x.Id)
        .Include(x => x.Status)
        .Include(x => x.TotalAmount)
        .Include(x => x.CreatedAtUtc))
    .ToListAsync();

// Or use typed projection
var summaries = await collection
    .Find(filter)
    .Project(x => new OrderSummary
    {
        Id = x.Id,
        Status = x.Status,
        TotalAmount = x.TotalAmount
    })
    .ToListAsync();
```

### Pagination — Use Keyset (Cursor-Based)

```csharp
/// <summary>
/// Keyset pagination using _id as cursor — O(1) vs OFFSET's O(n).
/// </summary>
public async Task<List<OrderDocument>> GetPageAsync(
    IMongoCollection<OrderDocument> collection,
    string customerId,
    string? lastId = null,
    int pageSize = 50)
{
    var filterBuilder = Builders<OrderDocument>.Filter;
    var filter = filterBuilder.Eq(x => x.CustomerId, customerId);

    if (lastId is not null)
    {
        filter &= filterBuilder.Gt(x => x.Id, lastId);
    }

    return await collection
        .Find(filter)
        .Sort(Builders<OrderDocument>.Sort.Ascending(x => x.Id))
        .Limit(pageSize)
        .ToListAsync();
}
```

### Avoid Anti-Patterns

```csharp
// BAD: Skip-based pagination — degrades as offset increases
var results = await collection.Find(filter).Skip(10000).Limit(50).ToListAsync();

// BAD: Negation queries — can't use indexes efficiently
var filter = Builders<T>.Filter.Ne(x => x.Status, "Deleted");

// BAD: Regex without prefix — causes collection scan
var filter = Builders<T>.Filter.Regex(x => x.Name, new BsonRegularExpression(".*widget.*"));

// GOOD: Prefix regex — can use index
var filter = Builders<T>.Filter.Regex(x => x.Name, new BsonRegularExpression("^Widget"));
```

## Read/Write Concerns

### Read Concern

| Level | Guarantee | Use Case |
|-------|-----------|----------|
| `local` (default) | Most recent data on primary | General reads |
| `majority` | Data acknowledged by majority | After failover safety |
| `linearizable` | Reflects all successful writes | Strongest consistency |

### Write Concern

| Level | Guarantee | Use Case |
|-------|-----------|----------|
| `w: 1` (default) | Acknowledged by primary | General writes |
| `w: "majority"` | Acknowledged by majority | Critical writes |
| `w: 0` | Fire and forget | Logging, metrics |

```csharp
// Per-operation write concern
var options = new InsertOneOptions();
await collection
    .WithWriteConcern(WriteConcern.WMajority)
    .InsertOneAsync(document, options);

// Per-operation read concern
var result = await collection
    .WithReadConcern(ReadConcern.Majority)
    .Find(filter)
    .FirstOrDefaultAsync();
```

## Connection Pool Tuning

```csharp
var settings = MongoClientSettings.FromConnectionString(connectionString);
settings.MaxConnectionPoolSize = 100;    // Max concurrent connections (default: 100)
settings.MinConnectionPoolSize = 10;     // Keep warm connections
settings.WaitQueueTimeout = TimeSpan.FromSeconds(10);  // Fail fast if pool exhausted
settings.ConnectTimeout = TimeSpan.FromSeconds(10);
settings.SocketTimeout = TimeSpan.FromSeconds(30);
settings.ServerSelectionTimeout = TimeSpan.FromSeconds(15);
```

## Performance Checklist

- [ ] Every query uses an index (verify with explain)
- [ ] Compound indexes follow ESR rule (Equality → Sort → Range)
- [ ] Projections used to return only needed fields
- [ ] Keyset pagination instead of skip/limit
- [ ] MongoClient registered as singleton
- [ ] Connection pool sized for expected concurrency
- [ ] TTL indexes configured for auto-cleanup of expired documents
- [ ] Write concern set to "majority" for critical data
- [ ] No regex queries without anchored prefix
- [ ] No negation queries ($ne, $nin) on large collections
- [ ] Documents kept under 100 KB for optimal performance
- [ ] Unbounded arrays avoided (use bucket pattern)

## Official References

- [Indexes](https://www.mongodb.com/docs/manual/indexes/)
- [Query Optimization](https://www.mongodb.com/docs/manual/core/query-optimization/)
- [Explain Results](https://www.mongodb.com/docs/manual/reference/explain-results/)
- [Read/Write Concern](https://www.mongodb.com/docs/manual/reference/read-concern/)
- [Connection Pool](https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/connection/connect/#connection-pool)
