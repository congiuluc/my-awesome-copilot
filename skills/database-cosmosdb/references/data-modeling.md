# Cosmos DB Data Modeling

## Partition Key Design

The partition key determines data distribution and query efficiency.

### Rules for Choosing

| Rule | Explanation |
|------|-------------|
| High cardinality | Many distinct values = even distribution |
| Even distribution | No "hot" partitions consuming all RUs |
| Appears in WHERE | Most queries should filter by partition key |
| Single-partition queries | Keep related data in the same partition |

### Common Patterns

```
// Multi-tenant SaaS: partition by tenant
Partition Key: /tenantId

// E-commerce orders: partition by customer
Partition Key: /customerId

// IoT telemetry: partition by device
Partition Key: /deviceId

// Social feed: partition by user
Partition Key: /userId
```

### Hierarchical Partition Keys (Preview→GA)

```csharp
// For multi-dimensional data (tenant + user + category)
var containerProperties = new ContainerProperties("Items", new List<string>
{
    "/tenantId",
    "/userId",
    "/categoryId"
});
```

## Document Design

### Embed vs Reference

| Pattern | When to Use |
|---------|-------------|
| **Embed** (denormalize) | Data read together, 1:few relationship, bounded size |
| **Reference** (normalize) | Data changes independently, 1:many unbounded, large sub-documents |

### Embedding Example

```json
{
    "id": "order-001",
    "customerId": "cust-123",
    "type": "order",
    "status": "Shipped",
    "items": [
        { "productId": "prod-1", "name": "Widget", "quantity": 2, "price": 9.99 },
        { "productId": "prod-2", "name": "Gadget", "quantity": 1, "price": 24.99 }
    ],
    "shippingAddress": {
        "street": "123 Main St",
        "city": "Springfield",
        "zip": "62704"
    },
    "createdAtUtc": "2026-03-28T10:00:00Z"
}
```

### Type Discriminator Pattern

Store multiple entity types in the same container:

```json
// Product document
{ "id": "prod-1", "type": "product", "name": "Widget", "price": 9.99, "categoryId": "cat-1" }

// Category document (same container)
{ "id": "cat-1", "type": "category", "name": "Electronics", "productCount": 42 }
```

```csharp
/// <summary>
/// Base class for typed documents in a shared container.
/// </summary>
public abstract class CosmosDocument
{
    /// <summary>Unique identifier.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Document type discriminator.</summary>
    [JsonPropertyName("type")]
    public abstract string Type { get; }

    /// <summary>Partition key value.</summary>
    [JsonPropertyName("tenantId")]
    public string TenantId { get; set; } = string.Empty;
}

public class ProductDocument : CosmosDocument
{
    public override string Type => "product";
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

## Document Size

- Max document size: **2 MB**
- Keep documents under 100 KB for optimal performance
- If a document grows unbounded (e.g., comments array), use the reference pattern
- Use TTL (Time to Live) for auto-expiring documents:

```csharp
containerProperties.DefaultTimeToLive = 60 * 60 * 24 * 30;  // 30 days
```

## Official References

- [Data Modeling in Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/modeling-data)
- [Partition Key Design](https://learn.microsoft.com/en-us/azure/cosmos-db/partitioning-overview)
- [Hierarchical Partition Keys](https://learn.microsoft.com/en-us/azure/cosmos-db/hierarchical-partition-keys)
