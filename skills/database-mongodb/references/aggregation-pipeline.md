# MongoDB Aggregation Pipeline

## Overview

The aggregation pipeline processes documents through sequential stages. Each stage transforms the documents as they pass through. Always prefer server-side aggregation over client-side processing.

## Common Stages

| Stage | Purpose | Example |
|-------|---------|---------|
| `$match` | Filter documents (use first!) | `{ $match: { status: "Active" } }` |
| `$group` | Group and aggregate | `{ $group: { _id: "$customerId", total: { $sum: "$amount" } } }` |
| `$project` | Shape output fields | `{ $project: { name: 1, total: 1 } }` |
| `$sort` | Order results | `{ $sort: { createdAtUtc: -1 } }` |
| `$limit` / `$skip` | Paginate results | `{ $limit: 50 }` |
| `$lookup` | Left outer join | Join from another collection |
| `$unwind` | Deconstruct arrays | `{ $unwind: "$items" }` |
| `$addFields` | Add computed fields | `{ $addFields: { total: { $multiply: ["$qty", "$price"] } } }` |
| `$facet` | Multiple pipelines | Run parallel sub-pipelines |
| `$bucket` | Range-based grouping | Group by price ranges |

## Typed Builders in .NET

### Basic Aggregation

```csharp
/// <summary>
/// Gets order totals per customer using the aggregation pipeline.
/// </summary>
public async Task<List<CustomerOrderSummary>> GetOrderTotalsByCustomerAsync(
    IMongoCollection<OrderDocument> collection,
    DateTime since)
{
    var pipeline = collection.Aggregate()
        .Match(Builders<OrderDocument>.Filter.And(
            Builders<OrderDocument>.Filter.Gte(x => x.CreatedAtUtc, since),
            Builders<OrderDocument>.Filter.Ne(x => x.Status, OrderStatus.Cancelled)))
        .Group(
            x => x.CustomerId,
            g => new CustomerOrderSummary
            {
                CustomerId = g.Key,
                OrderCount = g.Count(),
                TotalAmount = g.Sum(x => x.TotalAmount),
                AverageAmount = g.Average(x => x.TotalAmount),
                LastOrderDate = g.Max(x => x.CreatedAtUtc)
            })
        .Sort(Builders<CustomerOrderSummary>.Sort.Descending(x => x.TotalAmount));

    return await pipeline.ToListAsync();
}

public class CustomerOrderSummary
{
    public string CustomerId { get; set; } = string.Empty;
    public long OrderCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public DateTime LastOrderDate { get; set; }
}
```

### $lookup (Join)

```csharp
/// <summary>
/// Joins orders with customer details using $lookup.
/// </summary>
public async Task<List<BsonDocument>> GetOrdersWithCustomerAsync(
    IMongoCollection<OrderDocument> ordersCollection,
    string status)
{
    var pipeline = ordersCollection.Aggregate()
        .Match(Builders<OrderDocument>.Filter.Eq(x => x.Status, status))
        .Lookup(
            foreignCollectionName: "customers",
            localField: "customerId",
            foreignField: "_id",
            @as: "customer")
        .Unwind("customer")   // Convert 1-element array to object
        .Project(new BsonDocument
        {
            { "_id", 1 },
            { "status", 1 },
            { "totalAmount", 1 },
            { "customerName", "$customer.name" },
            { "customerEmail", "$customer.email" }
        })
        .Sort(Builders<BsonDocument>.Sort.Descending("totalAmount"));

    return await pipeline.ToListAsync();
}
```

### $unwind + $group (Array Analysis)

```csharp
/// <summary>
/// Finds the top-selling products by unwinding order items.
/// </summary>
public async Task<List<ProductSalesSummary>> GetTopProductsAsync(
    IMongoCollection<OrderDocument> collection,
    int topN = 10)
{
    var pipeline = collection.Aggregate()
        .Match(Builders<OrderDocument>.Filter.Ne(x => x.Status, OrderStatus.Cancelled))
        .Unwind<OrderDocument, UnwoundOrder>(x => x.Items)
        .Group(
            x => x.Items.ProductId,
            g => new ProductSalesSummary
            {
                ProductId = g.Key,
                ProductName = g.First().Items.Name,
                TotalQuantity = g.Sum(x => x.Items.Quantity),
                TotalRevenue = g.Sum(x => x.Items.UnitPrice * x.Items.Quantity)
            })
        .Sort(Builders<ProductSalesSummary>.Sort.Descending(x => x.TotalRevenue))
        .Limit(topN);

    return await pipeline.ToListAsync();
}

/// <summary>Helper model for $unwind result.</summary>
public class UnwoundOrder
{
    [BsonId]
    public string Id { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public OrderItem Items { get; set; } = null!;  // Singular after unwind
}

public class ProductSalesSummary
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public long TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}
```

### $facet (Parallel Pipelines)

```csharp
/// <summary>
/// Returns paginated data with total count in a single query using $facet.
/// </summary>
public async Task<(List<OrderDocument> Items, long TotalCount)> GetPagedOrdersAsync(
    IMongoCollection<OrderDocument> collection,
    string customerId,
    int page,
    int pageSize)
{
    var matchStage = new BsonDocument("$match",
        new BsonDocument("customerId", customerId));

    var facetStage = new BsonDocument("$facet", new BsonDocument
    {
        { "data", new BsonArray
            {
                new BsonDocument("$sort", new BsonDocument("createdAtUtc", -1)),
                new BsonDocument("$skip", (page - 1) * pageSize),
                new BsonDocument("$limit", pageSize)
            }
        },
        { "totalCount", new BsonArray
            {
                new BsonDocument("$count", "count")
            }
        }
    });

    var result = await collection
        .Aggregate()
        .AppendStage<BsonDocument>(matchStage)
        .AppendStage<BsonDocument>(facetStage)
        .FirstOrDefaultAsync();

    if (result is null)
    {
        return ([], 0);
    }

    var items = result["data"].AsBsonArray
        .Select(x => BsonSerializer.Deserialize<OrderDocument>(x.AsBsonDocument))
        .ToList();

    var totalCount = result["totalCount"].AsBsonArray.Count > 0
        ? result["totalCount"].AsBsonArray[0]["count"].AsInt64
        : 0;

    return (items, totalCount);
}
```

### $bucket (Range Grouping)

```csharp
/// <summary>
/// Groups orders into price range buckets for distribution analysis.
/// </summary>
public async Task<List<BsonDocument>> GetOrderDistributionAsync(
    IMongoCollection<OrderDocument> collection)
{
    var bucketStage = new BsonDocument("$bucket", new BsonDocument
    {
        { "groupBy", "$totalAmount" },
        { "boundaries", new BsonArray { 0, 10, 50, 100, 500, 1000, 10000 } },
        { "default", "10000+" },
        { "output", new BsonDocument
            {
                { "count", new BsonDocument("$sum", 1) },
                { "avgAmount", new BsonDocument("$avg", "$totalAmount") }
            }
        }
    });

    return await collection
        .Aggregate()
        .AppendStage<BsonDocument>(bucketStage)
        .ToListAsync();
}
```

## Pipeline Performance Rules

1. **$match first** — filter early to reduce documents flowing through later stages
2. **Use indexes** — `$match` and `$sort` at the beginning use indexes
3. **$project early** — drop unneeded fields to reduce memory
4. **Avoid $lookup on large collections** — denormalize for read-heavy patterns
5. **Set `allowDiskUse: true`** for large aggregations that exceed 100 MB memory limit:

```csharp
var options = new AggregateOptions { AllowDiskUse = true };
var pipeline = collection.Aggregate(options)
    .Match(filter)
    .Group(/* ... */);
```

## Pipeline Explain

```csharp
// Check pipeline execution plan
var explainCommand = new BsonDocument
{
    { "explain", new BsonDocument
        {
            { "aggregate", "orders" },
            { "pipeline", new BsonArray { matchStage, groupStage } },
            { "cursor", new BsonDocument() }
        }
    },
    { "verbosity", "executionStats" }
};

var explanation = await database.RunCommandAsync<BsonDocument>(explainCommand);
```

## Official References

- [Aggregation Pipeline](https://www.mongodb.com/docs/manual/core/aggregation-pipeline/)
- [Aggregation Stages](https://www.mongodb.com/docs/manual/reference/operator/aggregation-pipeline/)
- [Aggregation with .NET](https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/linq/aggregation/)
- [Pipeline Optimization](https://www.mongodb.com/docs/manual/core/aggregation-pipeline-optimization/)
